using System.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;
using System.Data;
using System.Windows;
using X4DataLoader;
using System.Xml.Linq;
using System.Xml.XPath;
using System.DirectoryServices;
using Utilities.Logging;
using System.Text.RegularExpressions;


namespace ChemGateBuilder
{
    public class ChemGateKeeper : INotifyPropertyChanged
    {
        private string _modFolderPath = "";
        private string _id = "chem_gate_keeper";
        private string _name = "Chem Gate Keeper";
        private string _description = "This extension adds new gate connections between sectors";
        private string _author = "Chem O`Dun";
        private int _version = 100;
        private int _versionInitial = 0;
        private string _date = "2021-09-01";
        private int _gameVersion = 710;
        private readonly string _save = "false";
        private readonly string _sync = "false";
        private readonly List<string> _dlcRequired = [];
        private readonly Dictionary<string, List<GalaxyConnectionPath>> _paths = [];
        private static readonly Regex _regex = new(@"/macros/macro\[@name='([^']+)'\]/connections");
        public int Version
        {
            get => _version;
            set { _version = value; OnPropertyChanged(nameof(Version)); OnPropertyChanged(nameof(Title)); }
        }
        public string Date
        {
            get => _date;
            set { _date = value; OnPropertyChanged(nameof(Date)); OnPropertyChanged(nameof(Title)); }
        }
        public int GameVersion
        {
            get => _gameVersion;
            set { _gameVersion = value; OnPropertyChanged(nameof(GameVersion)); OnPropertyChanged(nameof(Title)); }
        }
        public string Title
        {
            get
            {
                string versionString = $" {_version / 100.0:F2}".Replace(',', '.');
                versionString += _isModChanged ? "*" : "";
                string gameVersion = $"{_gameVersion / 100.0:F2}".Replace(',', '.');
                return $"{_name} v.{versionString} built {_date} for X4: Foundations v.{gameVersion}";
            }
        }
        private bool _isModChanged = false;
        public List<GalaxyConnection> Connections = [];
        public XElement? XML = null;

        public ChemGateKeeper()
        {
            XML = null;
            Date = DateTime.Now.ToString("yyyy-MM-dd");
        }

        public void SetGameVersion(int version)
        {
            GameVersion = version;
        }

        public bool LoadData(Galaxy galaxy)
        {
            string currentPath = _modFolderPath;

            System.Windows.Forms.OpenFileDialog dialog = new()
            {
                InitialDirectory = string.IsNullOrEmpty(currentPath) ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : currentPath,
                Filter = "Mod Content File|content.xml",
                Title = "Select a File"
            };

            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK || string.IsNullOrWhiteSpace(dialog.FileName))
            {
                return false;
            }
            try
            {
                currentPath = Path.GetDirectoryName(dialog.FileName) ?? "";
                Dictionary<string, (string path, string fileName)> relativePaths = new()
                {
                    { "galaxy", ("maps/xu_ep2_universe", "galaxy.xml") },
                    { "sectors", ("maps/xu_ep2_universe", "sectors.xml") },
                    { "zones", ("maps/xu_ep2_universe", "zones.xml") },
                };
                var files = DataLoader.GatherFiles(currentPath, relativePaths);
                if (files.Count == 0)
                {
                    MessageBox.Show("The selected folder does not contain a valid mod", "Invalid Folder", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                List<ZoneConnection> zonesConnections = [];
                List<Zone> zones = [];
                foreach (var filesGroup in files)
                {
                    string source = filesGroup.Key;
                    Log.Debug($"Loading {source} files");
                    if (filesGroup.Value.TryGetValue("sectors", out var sectorsFile))
                    {
                        XDocument sectorsDoc;
                        try
                        {
                            sectorsDoc = XDocument.Load(sectorsFile.fullPath);
                        }
                        catch (ArgumentException e)
                        {
                            Log.Warn($"Error loading sectors file {sectorsFile.fullPath}: {e.Message}");
                            continue;
                        }
                        foreach (var macroElement in sectorsDoc.XPathSelectElements("/diff/add"))
                        {
                            string sel = macroElement.Attribute("sel")?.Value ?? "";
                            if (_regex.IsMatch(sel))
                            {
                                string sectorMacro = _regex.Match(sel).Groups[1].Value;
                                var sector = galaxy.GetSectorByMacro(sectorMacro);
                                if (sector == null)
                                {
                                    Log.Error($"Sector not found: {sectorMacro}");
                                    continue;
                                }
                                var connections = macroElement.Elements("connection");
                                if (connections == null || !connections.Any())
                                {
                                    Log.Error($"Sector {sector.Name} has no added connection");
                                    continue;
                                }
                                foreach (XElement connectionElement in connections)
                                {
                                    string reference = connectionElement.Attribute("ref")?.Value ?? "";
                                    string connectionName = connectionElement.Attribute("name")?.Value ?? "";
                                    if (reference == "zones" && !string.IsNullOrEmpty(connectionName))
                                    {
                                        ZoneConnection zoneConnection = new();
                                        try
                                        {
                                            zoneConnection.Load(connectionElement, source, sectorsFile.fileName);
                                        }
                                        catch (ArgumentException e)
                                        {
                                            Log.Error($"Error loading zone connection: {e.Message}");
                                            continue;
                                        }
                                        zonesConnections.Add(zoneConnection);
                                    }
                                }
                            }
                        }
                        Log.Debug($"Sectors loaded from: {sectorsFile.fileName} for {source}");
                    }
                    if (filesGroup.Value.TryGetValue("zones", out var zoneFileInfo))
                    {
                        Log.Debug($"Loading zone file {zoneFileInfo.fileName} from {zoneFileInfo.fullPath}");
                        XDocument docZones;
                        try
                        {
                            docZones = XDocument.Load(zoneFileInfo.fullPath);
                        }
                        catch (ArgumentException e)
                        {
                            Log.Warn($"Error loading zone file {zoneFileInfo.fullPath}: {e.Message}");
                            continue;
                        }
                        foreach (XElement addElement in docZones.XPathSelectElements("/diff/add"))
                        {
                            var zoneElements = addElement.Elements("macro");
                            if (zoneElements == null || !zoneElements.Any())
                            {
                                Log.Warn($"No Zone macro is found in this Add element");
                                continue;
                            }
                            foreach (XElement zoneElement in zoneElements)
                            {
                                Zone newZone = new();
                                try
                                {
                                    newZone.Load(zoneElement, source, zoneFileInfo.fileName);
                                }
                                catch (ArgumentException e)
                                {
                                    Log.Error($"Error loading zone: {e.Message}");
                                    continue;
                                }
                                string zoneId = newZone.Name.Replace("_macro", "_connection");
                                ZoneConnection? zoneConnection = zonesConnections.FirstOrDefault(zc => zc.Name == zoneId);
                                if (zoneConnection != null)
                                {
                                    newZone.SetPosition(zoneConnection.Position, zoneConnection.Name, zoneConnection.XML);
                                    zones.Add(newZone);
                                }
                                else
                                {
                                    Log.Warn($"Zone connection not found for {newZone.Name}");
                                }
                            }
                        }
                    }
                }
                if (zones.Count == 0)
                {
                    Log.Error("No zones loaded");
                    return false;
                }
                var vanillaFiles = files["vanilla"];
                if (vanillaFiles == null || !vanillaFiles.TryGetValue("galaxy", out var galaxyFile))
                {
                    Log.Error("Vanilla galaxy file not found");
                    return false;
                }
                Galaxy modGalaxy = new();
                XDocument? docGalaxy;
                try
                {
                    docGalaxy = XDocument.Load(galaxyFile.fullPath);
                }
                catch (ArgumentException e)
                {
                    Log.Error($"Error loading galaxy file {galaxyFile.fullPath}: {e.Message}");
                    return false;
                }
                foreach (XElement addElement in docGalaxy.XPathSelectElements("/diff/add"))
                {
                    if (addElement.Attribute("sel")?.Value != "/macros/macro[@name='XU_EP2_universe_macro']/connections")
                    {
                        Log.Warn("No connections found in thi Add element");
                        continue;
                    }
                    modGalaxy.LoadConnections(addElement, galaxy.Clusters, "vanilla", galaxyFile.fileName, zones);
                }
                if (modGalaxy.Connections.Count == 0)
                {
                    Log.Error("No Galaxy Connections loaded");
                    return false;
                }
                XDocument? docContent;
                try
                {
                    docContent = XDocument.Load(Path.Combine(currentPath, "content.xml"));
                }
                catch (ArgumentException e)
                {
                    Log.Error($"Error loading content file: {e.Message}");
                    return false;
                }
                XElement? contentElement = docContent.Element("content");
                if (contentElement == null)
                {
                    Log.Error("No content element found in content file");
                    return false;
                }
                _id = contentElement.Attribute("id")?.Value ?? _id;
                _name = contentElement.Attribute("name")?.Value ?? _name;
                Version = int.Parse(contentElement.Attribute("version")?.Value ?? "0", System.Globalization.CultureInfo.InvariantCulture);
                _author = contentElement.Attribute("author")?.Value ?? _author;
                Date = contentElement.Attribute("date")?.Value ?? _date;
                _description = contentElement.Attribute("description")?.Value ?? _description;
                GameVersion = int.Parse(contentElement.Element("dependency")?.Attribute("version")?.Value ?? "710", System.Globalization.CultureInfo.InvariantCulture);
                Connections = modGalaxy.Connections;
                _versionInitial = _version;
            }
            catch (Exception e)
            {
                Log.Error($"Error loading mod data: {e.Message}");
                return false;
            }
            return true;
        }

        public bool SaveData(ObservableCollection<GalaxyConnectionData> GalaxyConnections)
        {
            string currentPath = _modFolderPath;
            if (string.IsNullOrEmpty(currentPath))
            {
                System.Windows.Forms.FolderBrowserDialog dialog = new();
                System.Windows.Forms.DialogResult folderSelect = dialog.ShowDialog();

                if (folderSelect == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {
                    currentPath = Path.Combine(dialog.SelectedPath, _id);
                }
            }
            if (Directory.Exists(currentPath))
            {
                string pathToContext = Path.Combine(currentPath, "content.xml");
                if (File.Exists(pathToContext))
                {
                    var confirmOverwrite = MessageBox.Show("Do you want to overwrite the existing Mod?", "Confirm Overwrite", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (confirmOverwrite != MessageBoxResult.Yes)
                    {
                        return false;
                    }
                    // Clear the currentPath folder
                    foreach (var directory in Directory.GetDirectories(currentPath))
                    {
                        Directory.Delete(directory, true);
                    }
                    foreach (var file in Directory.GetFiles(currentPath))
                    {
                        File.Delete(file);
                    }
                }
                else
                {
                    if (Directory.EnumerateFileSystemEntries(currentPath).Any())
                    {
                        MessageBox.Show("Folder not contains a mod, but not empty!", "Invalid Folder", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }
            }
            else
            {
                Directory.CreateDirectory(currentPath);
            }
            _modFolderPath = currentPath;
            Connections = GalaxyConnections.Select(gc => gc.Connection).Where(c => c != null).ToList();
            Date = DateTime.Now.ToString("yyyy-MM-dd");
            _versionInitial = _version;
            SaveModXMLs();
            Version = _versionInitial;
            return true;
        }

        private void SaveModXMLs()
        {
            _dlcRequired.Clear();
            _paths.Clear();
            string connectionsText = "";
            XElement diffElement = new("diff");
            XElement addElement = new("add", new XAttribute("sel", "/macros/macro[@name='XU_EP2_universe_macro']/connections"));
            diffElement.Add(addElement);
            foreach (GalaxyConnection connection in Connections)
            {
                if (connection == null)
                {
                    continue;
                }
                if (connection.PathDirect != null && connection.PathDirect.Sector != null)
                {
                    if (!_dlcRequired.Contains(connection.PathDirect.Sector.Source))
                    {
                        _dlcRequired.Add(connection.PathDirect.Sector.Source);
                    }
                    connectionsText += $"\n - {connection.PathDirect.Sector.Name} and";
                    if (!_paths.TryGetValue(connection.PathDirect.Sector.Source, out List<GalaxyConnectionPath>? pathItems))
                    {
                        pathItems = [];
                        _paths.Add(connection.PathDirect.Sector.Source, pathItems);
                    }

                    pathItems.Add(connection.PathDirect);
                }
                if (connection.PathOpposite != null && connection.PathOpposite.Sector != null)
                {
                    if (!_dlcRequired.Contains(connection.PathOpposite.Sector.Source))
                    {
                        _dlcRequired.Add(connection.PathOpposite.Sector.Source);
                    }
                    connectionsText += $" {connection.PathOpposite.Sector.Name}";
                    if (!_paths.TryGetValue(connection.PathOpposite.Sector.Source, out List<GalaxyConnectionPath>? pathItems))
                    {
                        pathItems = [];
                        _paths.Add(connection.PathOpposite.Sector.Source, pathItems);
                    }

                    pathItems.Add(connection.PathOpposite);
                }
                addElement.Add(connection.XML);
            }
            XElement content = new("content");
            content.SetAttributeValue("id", _id);
            content.SetAttributeValue("name", _name);
            content.SetAttributeValue("version", _version);
            content.SetAttributeValue("author", _author);
            content.SetAttributeValue("date", _date);
            content.SetAttributeValue("save", _save);
            content.SetAttributeValue("sync", _sync);
            content.SetAttributeValue("description", _description + connectionsText);
            List<int> languages = [7, 33, 37, 39, 44, 49, 55, 81, 82, 86, 88, 380];
            foreach (int language in languages)
            {
                XElement text = new("text");
                text.SetAttributeValue("language", $"{language}");
                text.SetAttributeValue("description", _description + connectionsText);
                content.Add(text);
            }
            content.Add(new XElement("dependency", new XAttribute("version", $"{GameVersion}")));
            _dlcRequired.Sort();
            foreach (string dlc in _dlcRequired)
            {
                if (dlc == "vanilla")
                {
                    continue;
                }
                content.Add(new XElement("dependency", new XAttribute("id", dlc), new XAttribute("optional", "false")));
            }
            XDocument docContent = new(new XDeclaration("1.0", "utf-8", null), content);
            docContent.Save(Path.Combine(_modFolderPath, "content.xml"));
            XDocument docGalaxy = new(new XDeclaration("1.0", "utf-8", null), diffElement);
            string galaxyPath = Path.Combine(_modFolderPath, "maps", "xu_ep2_universe");
            Directory.CreateDirectory(galaxyPath);
            docGalaxy.Save(Path.Combine(galaxyPath, "galaxy.xml"));
            SaveModSectorsAndZones();
        }

        private void SaveModSectorsAndZones()
        {
            foreach (KeyValuePair<string, List<GalaxyConnectionPath>> path in _paths)
            {
                string universePath = _modFolderPath;
                if (path.Key != "vanilla")
                {
                    universePath = Path.Combine(universePath, "extensions", path.Key);
                }
                universePath = Path.Combine(universePath, "maps", "xu_ep2_universe");
                Directory.CreateDirectory(universePath);
                XElement sectors = new("diff");
                XElement zones = new("diff");
                string files_prefix = "";
                foreach (GalaxyConnectionPath connectionPath in path.Value)
                {
                    if (connectionPath.Sector != null && connectionPath.Zone != null)
                    {
                        XElement sector = new("add", new XAttribute("sel", $"/macros/macro[@name='{connectionPath.Sector.Macro}']/connections"));
                        sector.Add(connectionPath.Zone.PositionXML);
                        sectors.Add(sector);
                        XElement zone = new("add", new XAttribute("sel", $"/macros"));
                        zone.Add(connectionPath.Zone.XML);
                        zones.Add(zone);
                        if (path.Key != "vanilla" && files_prefix == "")
                        {
                            Zone? zoneItem = connectionPath.Sector.Zones.FirstOrDefault();
                            if (zoneItem != null)
                            {
                                files_prefix = zoneItem.FileName.Replace("zones.xml", "");
                            }
                        }
                    }
                }
                XDocument docSectors = new(new XDeclaration("1.0", "utf-8", null), sectors);
                docSectors.Save(Path.Combine(universePath, $"{files_prefix}sectors.xml"));
                XDocument docZones = new(new XDeclaration("1.0", "utf-8", null), zones);
                docZones.Save(Path.Combine(universePath, $"{files_prefix}zones.xml"));
            }
        }

        public bool IsModChanged(ObservableCollection<GalaxyConnectionData> GalaxyConnections)
        {
            bool result = false;
            if (Connections.Count != GalaxyConnections.Count)
            {
                result = true;
            }
            else
            {
                for (int i = 0; i < Connections.Count; i++)
                {
                    if (!GalaxyConnections.Any(gc => gc.Connection != null && gc.Connection.Name == Connections[i].Name))
                    {
                        result = true;
                        break;
                    }

                }
                for (int i = 0; i < GalaxyConnections.Count; i++)
                {
                    if (!Connections.Any(c => c.Name == GalaxyConnections[i].Connection.Name))
                    {
                        result = true;
                        break;
                    }
                }
                for (int i = 0; i < Connections.Count; i++)
                {
                    GalaxyConnection newConnection = GalaxyConnections.First(gc => gc.Connection.Name == Connections[i].Name).Connection;
                    if (Connections[i].PathDirect?.Zone?.PositionXML?.ToString() != newConnection.PathDirect?.Zone?.PositionXML?.ToString())
                    {
                        result = true;
                        break;
                    }
                    else if (Connections[i].PathOpposite?.Zone?.XML?.ToString() != newConnection.PathOpposite?.Zone?.XML?.ToString())
                    {
                        result = true;
                        break;
                    }
                    else if (Connections[i].PathDirect?.Gate?.XML?.ToString() != newConnection.PathDirect?.Gate?.XML?.ToString())
                    {
                        result = true;
                        break;
                    }
                    else if (Connections[i].PathOpposite?.Gate?.XML?.ToString() != newConnection.PathOpposite?.Gate?.XML?.ToString())
                    {
                        result = true;
                        break;
                    }
                    else if (Connections[i].PathOpposite?.Gate?.XML?.ToString() != newConnection.PathOpposite?.Gate?.XML?.ToString())
                    {
                        result = true;
                        break;
                    }
                }
            }
            if (result)
            {
                Log.Debug("Mod has been changed");
                Version = _versionInitial == 0 ? 100 : (_versionInitial + 1);
            }
            else
            {
                Log.Debug("Mod has not been changed");
                Version = _versionInitial == 0 ? 100 : _versionInitial;
            }
            _isModChanged = result;
            return result;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}