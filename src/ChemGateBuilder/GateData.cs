using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using X4DataLoader;

namespace ChemGateBuilder
{
    public class GatesConnectionData : INotifyPropertyChanged
    {
        private SectorItem? _sectorDirect;
        private SectorItem? _sectorDirectDefault;
        private SectorConnectionData? _sectorDirectSelectedConnection = new SectorConnectionData();
        private SectorMap _sectorDirectMap = new SectorMap();
        private ObservableCollection<SectorConnectionData> _sectorDirectConnections = new ObservableCollection<SectorConnectionData>();
        private GateData _gateDirect ;
        private SectorItem? _sectorOpposite;
        private SectorItem? _sectorOppositeDefault;
        private SectorConnectionData? _sectorOppositeSelectedConnection = new SectorConnectionData();
        private SectorMap _sectorOppositeMap = new SectorMap() ;
        private ObservableCollection<SectorConnectionData>  _SectorOppositeConnections = new ObservableCollection<SectorConnectionData>();
        private GateData _gateOpposite;
        private bool _isChanged = false;
        private bool _isReadyToSave = false;

        private bool IsDataChanged => _sectorDirect != _sectorDirectDefault || _sectorOpposite != _sectorOppositeDefault || _gateDirect.IsChanged || _gateOpposite.IsChanged;
        private bool IsDataReadyToSave => _sectorDirect != null && _sectorOpposite != null && IsDataChanged;

        public SectorItem? SectorDirect
        {
            get => _sectorDirect;
            set
            {
                if (_sectorDirect != value)
                {
                    _sectorDirect = value;
                    OnPropertyChanged(nameof(SectorDirect));
                }
            }
        }
        public List<string> SectorDirectExistingConnectionsMacros = new List<string>();
        public SectorConnectionData? SectorDirectSelectedConnection
        {
            get => _sectorDirectSelectedConnection;
            set
            {
                if (_sectorDirectSelectedConnection != value)
                {
                    _sectorDirectSelectedConnection = value;
                    if (_sectorDirectMap != null)
                    {
                        if (value != null)
                        {
                            _sectorDirectMap.SelectItem(value?.Id);
                        }
                        else
                        {
                            _sectorDirectMap.SelectItem("");
                        }
                    }
                    OnPropertyChanged(nameof(SectorDirectSelectedConnection));
                }
            }
        }
        public SectorMap SectorDirectMap
        {
            get => _sectorDirectMap;
            set
            {
                if (_sectorDirectMap != value)
                {
                    _sectorDirectMap = value;
                    OnPropertyChanged(nameof(SectorDirectMap));
                }
            }
        }

        public ObservableCollection<SectorConnectionData> SectorDirectConnections
        {
            get => _sectorDirectConnections;
            set
            {
                if (_sectorDirectConnections != value)
                {
                    _sectorDirectConnections = value;
                    OnPropertyChanged(nameof(SectorDirectConnections));
                }
            }
        }
        public GateData GateDirect
        {
            get => _gateDirect;
            set
            {
                if (_gateDirect != value)
                {
                    if (_gateDirect != null)
                    {
                        _gateDirect.PropertyChanged -= ChildPropertyChanged;
                    }
                    _gateDirect = value;
                    OnPropertyChanged(nameof(GateDirect));
                    if (_gateDirect != null)
                    {
                        _gateDirect.PropertyChanged += ChildPropertyChanged;
                    }
                }
            }
        }

        public SectorItem? SectorOpposite
        {
            get => _sectorOpposite;
            set
            {
                if (_sectorOpposite != value)
                {
                    _sectorOpposite = value;
                    OnPropertyChanged(nameof(SectorOpposite));
                }
            }
        }
        public List<string> SectorOppositeExistingConnectionsMacros = new List<string>();
        public SectorConnectionData? SectorOppositeSelectedConnection
        {
            get => _sectorOppositeSelectedConnection;
            set
            {
                if (_sectorOppositeSelectedConnection != value)
                {
                    _sectorOppositeSelectedConnection = value;
                    if (_sectorOppositeMap != null)
                    {
                        if (value != null)
                        {
                            _sectorOppositeMap.SelectItem(value.Id);
                        }
                        else
                        {
                            _sectorOppositeMap.SelectItem("");
                        }
                    }
                    OnPropertyChanged(nameof(SectorOppositeSelectedConnection));
                }
            }
        }
        public SectorMap SectorOppositeMap
        {
            get => _sectorOppositeMap;
            set
            {
                if (_sectorOppositeMap != value)
                {
                    _sectorOppositeMap = value;
                    OnPropertyChanged(nameof(SectorOppositeMap));
                }
            }
        }

        public ObservableCollection<SectorConnectionData> SectorOppositeConnections
        {
            get => _SectorOppositeConnections;
            set
            {
                if (_SectorOppositeConnections != value)
                {
                    _SectorOppositeConnections = value;
                    OnPropertyChanged(nameof(SectorOppositeConnections));
                }
            }
        }


        public GateData GateOpposite
        {
            get => _gateOpposite;
            set
            {
                if (_gateOpposite != value)
                {
                    if (_gateOpposite != null)
                    {
                        _gateOpposite.PropertyChanged -= ChildPropertyChanged;
                    }
                    _gateOpposite = value;
                    OnPropertyChanged(nameof(GateOpposite));
                    if (_gateOpposite != null)
                    {
                        _gateOpposite.PropertyChanged += ChildPropertyChanged;
                    }
                }
            }
        }

        public bool IsChanged {
            get => _isChanged;
            set {
                _isChanged = value;
                OnPropertyChanged(nameof(IsChanged));
                MainWindow? mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.ChangingGalaxyConnectionIsPossible = !value;
                }
            }
        }
        public bool IsReadyToSave { get => _isReadyToSave; set { _isReadyToSave = value; OnPropertyChanged(nameof(IsReadyToSave)); }}
        public GatesConnectionData(bool gateActiveDefault, string gateMacroDefault, SectorItem? sectorDirectDefault = null, SectorItem? sectorOppositeDefault = null)
        {
            // Subscribe to child PropertyChanged events
            _gateDirect = new GateData(gateActiveDefault, gateMacroDefault);
            _gateOpposite = new GateData(gateActiveDefault, gateMacroDefault);
            _sectorDirectDefault = sectorDirectDefault;
            _sectorOppositeDefault = sectorOppositeDefault;
            Reset();
            _gateOpposite.PropertyChanged += ChildPropertyChanged;
            _gateDirect.PropertyChanged += ChildPropertyChanged;
        }

        public void Reset()
        {
            _sectorDirect = _sectorDirectDefault;
            _sectorOpposite = _sectorOppositeDefault;
            _gateDirect.Reset();
            _gateOpposite.Reset();
            IsChanged = IsDataChanged;
            IsReadyToSave = IsDataReadyToSave;
            OnPropertyChanged(nameof(SectorDirect));
            OnPropertyChanged(nameof(SectorOpposite));
            OnPropertyChanged(nameof(GateDirect));
            OnPropertyChanged(nameof(GateOpposite));
            UpdateCurrentGateOnMap(nameof(GateDirect));
            UpdateCurrentGateOnMap(nameof(GateOpposite));
        }

        public void SetGateStatusDefaults(bool gateActiveDefault)
        {
            _gateDirect.SetDefaults(gateActiveDefault, "");
            _gateOpposite.SetDefaults(gateActiveDefault, "");
            OnPropertyChanged("");
        }

        public void ResetToInitial(bool gateActiveDefault, string gateMacroDefault)
        {
            _gateDirect = new GateData(gateActiveDefault, gateMacroDefault);
            _gateOpposite = new GateData(gateActiveDefault, gateMacroDefault);
            _sectorDirectDefault = null;
            _sectorOppositeDefault = null;
            Reset();
            _gateOpposite.PropertyChanged += ChildPropertyChanged;
            _gateDirect.PropertyChanged += ChildPropertyChanged;
        }

        public void SetDefaultsFromReference(GalaxyConnectionData reference)
        {
            if (reference == null) return;
            GalaxyConnection connection = reference.Connection;
            _sectorDirectDefault = new SectorItem
            {
                Name = reference.SectorDirectName,
                Source = connection.PathDirect?.Sector?.Source ?? "",
                Macro = connection.PathDirect?.Sector?.Macro ?? "",
                Selectable = true
            };
            _sectorOppositeDefault = new SectorItem
            {
                Name = reference.SectorOppositeName,
                Source = connection.PathOpposite?.Sector?.Source ?? "",
                Macro = connection.PathOpposite?.Sector?.Macro ?? "",
                Selectable = true
            };
            _gateDirect.SetDefaults(reference.GateDirectActive, connection.PathDirect?.Gate?.GateMacro ?? "",
                new Coordinates(reference.GateDirectX, reference.GateDirectY, reference.GateDirectZ),
                reference.DirectPosition,
                reference.DirectRotation);
            _gateOpposite.SetDefaults(reference.GateOppositeActive, connection.PathOpposite?.Gate?.GateMacro ?? "",
                new Coordinates(reference.GateOppositeX, reference.GateOppositeY, reference.GateOppositeZ),
                reference.OppositePosition,
                reference.OppositeRotation);
        }
        private void ChildPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Propagate the change
            if (sender == GateDirect)
            {
                OnPropertyChanged(nameof(GateDirect));
            }
            else if (sender == GateOpposite)
            {
                OnPropertyChanged(nameof(GateOpposite));
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;

        private void UpdateCurrentGateOnMap(string propertyName)
        {
            GateData gateCurrent = propertyName == nameof(GateDirect) ? GateDirect : GateOpposite;
            if (gateCurrent == null) return;
            SectorItem? sectorTo = propertyName == nameof(GateDirect) ? SectorOpposite : SectorDirect;
            SectorMap sectorMap = propertyName == nameof(GateDirect) ? SectorDirectMap : SectorOppositeMap;
            if (sectorMap == null || sectorMap.IsDragging) return;
            SectorConnectionData newConnection = new SectorConnectionData
            {
                Active = gateCurrent.Active && sectorTo != null,
                ToSector = sectorTo != null ? sectorTo.Name : "",
                X = gateCurrent.Coordinates.X,
                Y = gateCurrent.Coordinates.Y,
                Z = gateCurrent.Coordinates.Z,
                Type = "gate",
                Id = "New"
            };
            sectorMap.UpdateItem(newConnection);
        }

        public void SetSectorMapInternalSize(int sizeKm)
        {
            SectorDirectMap.SetInternalSize(sizeKm);
            SectorOppositeMap.SetInternalSize(sizeKm);
        }

        public void UpdateCurrentGateCoordinates(int X, int Y, int Z, string propertyName)
        {
            GateData gateCurrent = propertyName == nameof(GateDirect) ? GateDirect : GateOpposite;
            if (gateCurrent == null) return;
            gateCurrent.Coordinates = new Coordinates(X, Y ,Z);
        }
        protected void OnPropertyChanged(string propertyName)
        {
            if (_isChanged != IsDataChanged) IsChanged = IsDataChanged;
            if (_isReadyToSave != IsDataReadyToSave) IsReadyToSave = IsDataReadyToSave;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            if (propertyName == nameof(SectorDirect) || propertyName == nameof(SectorOpposite))
            {
                SectorItem? sectorCurrent = propertyName == nameof(SectorDirect) ? SectorDirect : SectorOpposite;
                var sectorConnections = propertyName == nameof(SectorDirect) ? SectorDirectConnections : SectorOppositeConnections;
                sectorConnections.Clear();
                SectorMap sectorMap = propertyName == nameof(SectorDirect) ? SectorDirectMap : SectorOppositeMap;
                sectorMap.ClearItems();
                if (sectorCurrent != null) {
                    MainWindow? mainWindow = Application.Current.MainWindow as MainWindow;
                    if (mainWindow != null && mainWindow.Galaxy != null && sectorCurrent?.Macro != null)
                    {
                        Galaxy galaxy = mainWindow.Galaxy;
                        Sector? sector = galaxy.GetSectorByMacro(sectorCurrent.Macro);
                        var sectorsViewSource = propertyName == nameof(SectorDirect) ? mainWindow.SectorsOppositeViewSource : mainWindow.SectorsDirectViewSource;
                        if (sectorsViewSource != null)
                        {
                            if (sector != null)
                            {
                                List<string> sectorMacros = galaxy.GetOppositeSectorsFromConnections(sector)
                                    .Select(s => s.Macro)
                                    .ToList();
                                if (propertyName == nameof(SectorDirect))
                                {
                                    SectorDirectExistingConnectionsMacros = sectorMacros;
                                }
                                else
                                {
                                    SectorOppositeExistingConnectionsMacros = sectorMacros;
                                }
                            }
                            sectorsViewSource.View.Refresh();
                        }
                        if (sector != null && sector.Zones != null && sector.Zones.Count != 0)
                        {
                            foreach (var zone in sector.Zones)
                            {
                                if (zone.Connections == null || zone.Connections.Count == 0) continue;
                                foreach (var connection in zone.Connections.Values)
                                {
                                    if (connection is GateConnection gateConnection)
                                    {
                                        bool active = gateConnection.IsActive;
                                        string? sectorTo = active ? galaxy.GetOppositeSectorForGateConnection(gateConnection)?.Name : "";
                                        Position zoneCoordinates = zone.Position;
                                        if (zoneCoordinates == null) continue;
                                        Position? gateCoordinates = gateConnection.Position;
                                        if (gateCoordinates == null) continue;
                                        SectorConnectionData newConnection = new SectorConnectionData
                                        {
                                            Active = active && !string.IsNullOrEmpty(sectorTo),
                                            ToSector = sectorTo ?? "",
                                            X =  (int)((zoneCoordinates.x + gateCoordinates.x) / 1000),
                                            Y = (int)((zoneCoordinates.y + gateCoordinates.y) / 1000),
                                            Z = (int)((zoneCoordinates.z + gateCoordinates.z) / 1000),
                                            Type = "gate",
                                            Id = gateConnection.Name
                                        };
                                        sectorConnections.Add(newConnection);
                                        sectorMap.AddItem(newConnection);
                                    }
                                }
                            }
                        }
                    }
                }
                UpdateCurrentGateOnMap(propertyName == nameof(SectorDirect) ? nameof(GateOpposite) : nameof(GateDirect));
                UpdateCurrentGateOnMap(propertyName == nameof(SectorDirect) ? nameof(GateDirect) : nameof(GateOpposite));
            }
            else if (propertyName == nameof(GateDirect) || propertyName == nameof(GateOpposite))
            {
                UpdateCurrentGateOnMap(propertyName);
            }
        }

    }

    public class SectorItem
    {
        public string? Name { get; set; }
        public string? Source { get; set; }
        public string? Macro { get; set; }
        public bool Selectable { get; set; }
    }

    public class SectorConnectionData : INotifyPropertyChanged
    {
        private string? _toSector;
        private int _x;
        private int _y;
        private int _z;
        private string? _type;
        private string? _id;
        private bool _active;

        public string? ToSector
        {
            get => _toSector;
            set
            {
                if (_toSector != value)
                {
                    _toSector = value;
                    OnPropertyChanged(nameof(ToSector));
                }
            }
        }

        public int X
        {
            get => _x;
            set
            {
                if (_x != value)
                {
                    _x = value;
                    OnPropertyChanged(nameof(X));
                }
            }
        }

        public int Y
        {
            get => _y;
            set
            {
                if (_y != value)
                {
                    _y = value;
                    OnPropertyChanged(nameof(Y));
                }
            }
        }

        public int Z
        {
            get => _z;
            set
            {
                if (_z != value)
                {
                    _z = value;
                    OnPropertyChanged(nameof(Z));
                }
            }
        }

        public string? Type
        {
            get => _type;
            set
            {
                if (_type != value)
                {
                    _type = value;
                    OnPropertyChanged(nameof(Type));
                }
            }
        }

        public string? Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }

        public bool Active
        {
            get => _active;
            set
            {
                if (_active != value)
                {
                    _active = value;
                    OnPropertyChanged(nameof(Active));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class GateData : INotifyPropertyChanged
    {
        private Coordinates _coordinates = new Coordinates();
        private Coordinates _position = new Coordinates();
        private Rotation _rotation = new Rotation();
        private bool _active;
        private string _gateMacro;

        private bool _activeDefault = true;
        private string _gateMacroDefault = "";

        public Coordinates Coordinates
        {
            get => _coordinates;
            set
            {
                if (_coordinates != null)
                {
                    // Unsubscribe from previous Coordinates PropertyChanged
                    _coordinates.PropertyChanged -= ChildPropertyChanged;
                }

                _coordinates = value;
                OnPropertyChanged(nameof(Coordinates));

                if (_coordinates != null)
                {
                    // Subscribe to new Coordinates PropertyChanged
                    _coordinates.PropertyChanged += ChildPropertyChanged;
                }
            }
        }

        public Coordinates Position
        {
            get => _position;
            set
            {
                if (_position != value)
                {
                    _position = value;
                    OnPropertyChanged(nameof(Position));
                }
            }
        }

        public Rotation Rotation
        {
            get => _rotation;
            set
            {
                if (_rotation != value)
                {
                    _rotation = value;
                    OnPropertyChanged(nameof(Rotation));
                }
            }
        }

        public bool Active
        {
            get => _active;
            set
            {
                if (_active != value)
                {
                    _active = value;
                    OnPropertyChanged(nameof(Active));
                }
            }
        }
        public string GateMacro
        {
            get => _gateMacro;
            set
            {
                if (_gateMacro != value)
                {
                    _gateMacro = value;
                    OnPropertyChanged(nameof(GateMacro));
                }
            }
        }
        public bool IsChanged => _active != _activeDefault || _gateMacro != _gateMacroDefault || _coordinates.IsChanged || _position.IsChanged || _rotation.IsChanged;

        public GateData(bool activeDefault, string gateMacroDefault)
        {
            SetDefaults(activeDefault, gateMacroDefault);
            _active = activeDefault;
            _gateMacro = gateMacroDefault;
            // Subscribe to initial child PropertyChanged events
            if (_coordinates != null)
                _coordinates.PropertyChanged += ChildPropertyChanged;

            if (_position != null)
                _position.PropertyChanged += ChildPropertyChanged;

            if (_rotation != null)
                _rotation.PropertyChanged += ChildPropertyChanged;
        }
        public void Reset()
        {
            _active = _activeDefault;
            _gateMacro = _gateMacroDefault;
            OnPropertyChanged(nameof(Active));
            OnPropertyChanged(nameof(GateMacro));
            Coordinates.Reset();
            Position.Reset();
            Rotation.Reset();
            OnPropertyChanged(nameof(Coordinates));
            OnPropertyChanged(nameof(Position));
            OnPropertyChanged(nameof(Rotation));
        }

        public void SetDefaults(bool activeDefault, string gateMacroDefault = "", Coordinates? coordinatesDefault = null, Coordinates? positionDefault = null, Rotation? rotationDefault = null)
        {
            _activeDefault = activeDefault;
            if (!string.IsNullOrEmpty(gateMacroDefault)) _gateMacroDefault = gateMacroDefault;
            if (coordinatesDefault != null) Coordinates.SetDefaults(coordinatesDefault.X, coordinatesDefault.Y, coordinatesDefault.Z);
            if (positionDefault != null) Position.SetDefaults(positionDefault.X, positionDefault.Y, positionDefault.Z);
            if (rotationDefault != null) Rotation.SetDefaults(rotationDefault.Roll, rotationDefault.Pitch, rotationDefault.Yaw);
        }
        private void ChildPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Determine which child sent the event and handle accordingly
            if (sender == Coordinates)
            {
                // Optionally, you can specify which property changed
                // For simplicity, notify that Coordinates has changed
                OnPropertyChanged(nameof(Coordinates));
            }
            else if (sender == Position)
            {
                OnPropertyChanged(nameof(Position));
            }
            else if (sender == Rotation)
            {
                OnPropertyChanged(nameof(Rotation));
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }



    public class Coordinates : INotifyPropertyChanged
    {
        private int _x;
        private int _xDefault = 0;
        private int _y;
        private int _yDefault = 0;
        private int _z;
        private int _zDefault = 0;

        public int X
        {
            get => _x;
            set
            {
                if (_x != value)
                {
                    _x = value;
                    OnPropertyChanged(nameof(X));
                }
            }
        }

        public int Y
        {
            get => _y;
            set
            {
                if (_y != value)
                {
                    _y = value;
                    OnPropertyChanged(nameof(Y));
                }
            }
        }

        public int Z
        {
            get => _z;
            set
            {
                if (_z != value)
                {
                    _z = value;
                    OnPropertyChanged(nameof(Z));
                }
            }
        }
        public bool IsChanged => _x != _xDefault || _y != _yDefault || _z != _zDefault;

        public Coordinates(int xDefault = 0, int yDefault = 0, int zDefault = 0)
        {
            _x = xDefault;
            _xDefault = xDefault;
            _y = yDefault;
            _yDefault = yDefault;
            _z = zDefault;
            _zDefault = zDefault;
        }
        public void Reset()
        {
            _x = _xDefault;
            _y = _yDefault;
            _z = _zDefault;
            OnPropertyChanged(nameof(X));
            OnPropertyChanged(nameof(Y));
            OnPropertyChanged(nameof(Z));
        }

        public void SetDefaults(int xDefault = 0, int yDefault = 0, int zDefault = 0)
        {
            _xDefault = xDefault;
            _yDefault = yDefault;
            _zDefault = zDefault;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Rotation : INotifyPropertyChanged
    {
        private int _roll;
        private int _rollDefault = 0;
        private int _pitch;
        private int _pitchDefault = 0;
        private int _yaw;
        private int _yawDefault = 0;

        public int Roll
        {
            get => _roll;
            set
            {
                if (_roll != value)
                {
                    _roll = value;
                    OnPropertyChanged(nameof(Roll));
                }
            }
        }

        public int Pitch
        {
            get => _pitch;
            set
            {
                if (_pitch != value)
                {
                    _pitch = value;
                    OnPropertyChanged(nameof(Pitch));
                }
            }
        }

        public int Yaw
        {
            get => _yaw;
            set
            {
                if (_yaw != value)
                {
                    _yaw = value;
                    OnPropertyChanged(nameof(Yaw));
                }
            }
        }
        public bool IsChanged => _roll != _rollDefault || _pitch != _pitchDefault || _yaw != _yawDefault;

        public Rotation(int rollDefault = 0, int pitchDefault = 0, int yawDefault = 0)
        {
            _roll = rollDefault;
            _rollDefault = rollDefault;
            _pitch = pitchDefault;
            _pitchDefault = pitchDefault;
            _yaw = yawDefault;
            _yawDefault = yawDefault;
        }
        public void Reset()
        {
            _roll = _rollDefault;
            _pitch = _pitchDefault;
            _yaw = _yawDefault;
            OnPropertyChanged(nameof(Roll));
            OnPropertyChanged(nameof(Pitch));
            OnPropertyChanged(nameof(Yaw));
        }

        public void SetDefaults(int rollDefault = 0, int pitchDefault = 0, int yawDefault = 0)
        {
            _rollDefault = rollDefault;
            _pitchDefault = pitchDefault;
            _yawDefault = yawDefault;
        }

        public Quaternion ToQuaternion()
        {
            double rollRad = Roll * Math.PI / 180.0;
            double pitchRad = Pitch * Math.PI / 180.0;
            double yawRad = Yaw * Math.PI / 180.0;

            double cy = Math.Cos(yawRad * 0.5);
            double sy = Math.Sin(yawRad * 0.5);
            double cp = Math.Cos(pitchRad * 0.5);
            double sp = Math.Sin(pitchRad * 0.5);
            double cr = Math.Cos(rollRad * 0.5);
            double sr = Math.Sin(rollRad * 0.5);

            double qw = cr * cp * cy + sr * sp * sy;
            double qx = sr * cp * cy - cr * sp * sy;
            double qy = cr * sp * cy + sr * cp * sy;
            double qz = cr * cp * sy - sr * sp * cy;

            return new Quaternion(qx, qy, qz, qw);
        }

        /// <summary>
        /// Converts a Quaternion to a Rotation (Roll, Pitch, Yaw) in degrees.
        /// </summary>
        /// <param name="q">The Quaternion to convert.</param>
        /// <returns>A Rotation instance representing the equivalent Roll, Pitch, and Yaw.</returns>
        public static Rotation FromQuaternion(Quaternion q)
        {
            // Normalize the quaternion to ensure accurate calculations
            double norm = Math.Sqrt(q.qx * q.qx + q.qy * q.qy + q.qz * q.qz + q.qw * q.qw);
            double x = q.qx / norm;
            double y = q.qy / norm;
            double z = q.qz / norm;
            double w = q.qw / norm;

            // Calculate Roll (x-axis rotation)
            double sinr_cosp = 2 * (w * x + y * z);
            double cosr_cosp = 1 - 2 * (x * x + y * y);
            double rollRad = Math.Atan2(sinr_cosp, cosr_cosp);

            // Calculate Pitch (y-axis rotation)
            double sinp = 2 * (w * y - z * x);
            double pitchRad;
            if (Math.Abs(sinp) >= 1)
                pitchRad = Math.CopySign(Math.PI / 2, sinp); // Use 90 degrees if out of range
            else
                pitchRad = Math.Asin(sinp);

            // Calculate Yaw (z-axis rotation)
            double siny_cosp = 2 * (w * z + x * y);
            double cosy_cosp = 1 - 2 * (y * y + z * z);
            double yawRad = Math.Atan2(siny_cosp, cosy_cosp);

            // Convert radians to degrees
            double rollDeg = rollRad * 180.0 / Math.PI;
            double pitchDeg = pitchRad * 180.0 / Math.PI;
            double yawDeg = yawRad * 180.0 / Math.PI;

            return new Rotation
            {
                Roll = (int)Math.Round(rollDeg),
                Pitch = (int)Math.Round(pitchDeg),
                Yaw = (int)Math.Round(yawDeg)
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}