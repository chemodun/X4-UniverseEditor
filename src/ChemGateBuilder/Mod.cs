using System.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;
using System.Data;
using System.Windows;
using X4DataLoader;
using System.Xml.Linq;
using System.DirectoryServices;

namespace ChemGateBuilder
{
    public class ChemGateKeeper : INotifyPropertyChanged
    {
        private string _modFolderPath = "";
        private string _id = "chem_gate_keeper";
        private string _name = "Chem Gate Keeper";
        private string _description = "This extension adds new gate connections between sectors";
        private string _author = "Chem O`Dun";
        private int _version = 0;
        private int _versionInitial = 100;
        private string _date = "2021-09-01";
        private int _gameVersion = 710;
        private readonly string _save = "false";
        private readonly string _sync = "false";
        private readonly List<string> _dlcRequired = [];
        private readonly Dictionary<string, List<GalaxyConnectionPath>> _paths = [];
        public int Version
        {
            get => _version;
            set { _version = value; OnPropertyChanged(nameof(Version)); }
        }
        public string Date
        {
            get => _date;
            set { _date = value; OnPropertyChanged(nameof(Date)); }
        }
        public int GameVersion
        {
            get => _gameVersion;
            set { _gameVersion = value; OnPropertyChanged(nameof(GameVersion)); }
        }

        public List<GalaxyConnection> Connections = [];
        public XElement? XML = null;

        public ChemGateKeeper()
        {
            XML = null;
        }

        public bool LoadData(Galaxy galaxy, ObservableCollection<GalaxyConnectionData> GalaxyConnections)
        {
            string currentPath = _modFolderPath;

            System.Windows.Forms.OpenFileDialog dialog = new()
            {
                InitialDirectory = string.IsNullOrEmpty(currentPath) ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : currentPath,
                Filter = "Mod Content File|content.xml",
                Title = "Select a File"
            };

            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.FileName))
            {
                currentPath = Path.GetDirectoryName(dialog.FileName) ?? "";
            }
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

            _versionInitial = _version;
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
            _date = DateTime.Now.ToString("yyyy-MM-dd");
            _versionInitial = _version;
            SaveModXMLs();
            return true;
        }

        private void SaveModXMLs()
        {
            _dlcRequired.Clear();
            _paths.Clear();
            string connectionsText = "";
            XElement diff = new("diff");
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
                diff.Add(connection.XML);
            }
            XElement content = new("content");
            content.SetAttributeValue("id", _id);
            content.SetAttributeValue("name", _name);
            content.SetAttributeValue("version", (_version / 100.0).ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
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
            XDocument docGalaxy = new(new XDeclaration("1.0", "utf-8", null), diff);
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
                    universePath = Path.Combine(universePath, path.Key);
                }
                universePath = Path.Combine(universePath, "maps", "xu_ep2_universe");
                Directory.CreateDirectory(universePath);
                XElement sectors = new("diff");
                XElement zones = new("diff");
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
                    }
                }
                XDocument docSectors = new(new XDeclaration("1.0", "utf-8", null), sectors);
                docSectors.Save(Path.Combine(universePath, "sectors.xml"));
                XDocument docZones = new(new XDeclaration("1.0", "utf-8", null), zones);
                docZones.Save(Path.Combine(universePath, "zones.xml"));
            }
        }

        public bool IsModChanged(ObservableCollection<GalaxyConnectionData> GalaxyConnections)
        {
            if (Connections.Count != GalaxyConnections.Count)
            {
                return true;
            }
            else
            {
                for (int i = 0; i < Connections.Count; i++)
                {
                    if (!GalaxyConnections.Any(gc => gc.Connection != null && gc.Connection.Name == Connections[i].Name))
                    {
                        return true;
                    }

                }
                for (int i = 0; i < GalaxyConnections.Count; i++)
                {
                    if(!Connections.Any(c => c.Name == GalaxyConnections[i].Connection.Name))
                    {
                        return true;
                    }
                }
                for (int i = 0; i < Connections.Count; i++)
                {
                    GalaxyConnection newConnection = GalaxyConnections.First(gc => gc.Connection.Name == Connections[i].Name).Connection;
                    if (Connections[i].PathDirect?.Zone?.PositionXML?.ToString() != newConnection.PathDirect?.Zone?.PositionXML?.ToString())
                    {
                        return true;
                    }
                    else if (Connections[i].PathOpposite?.Zone?.XML?.ToString() != newConnection.PathOpposite?.Zone?.XML?.ToString())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}