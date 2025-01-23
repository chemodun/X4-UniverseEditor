using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using X4DataLoader;

namespace ChemGateBuilder
{
    public class GatesConnectionData : INotifyPropertyChanged
    {
        private GalaxyConnectionData? _reference;
        private SectorItem _sectorDirect;
        private SectorItem _sectorDirectDefault;
        private SectorConnectionData _sectorDirectSelectedConnection = new SectorConnectionData();
        private SectorMap _sectorDirectMap = new SectorMap();
        private ObservableCollection<SectorConnectionData> _sectorDirectConnections = new ObservableCollection<SectorConnectionData>();
        private GateData _gateDirect ;
        private SectorItem _sectorOpposite;
        private SectorItem _sectorOppositeDefault;
        private SectorConnectionData _sectorOppositeSelectedConnection = new SectorConnectionData();
        private SectorMap _sectorOppositeMap = new SectorMap() ;
        private ObservableCollection<SectorConnectionData>  _SectorOppositeConnections = new ObservableCollection<SectorConnectionData>();
        private GateData _gateOpposite;
        private bool _isChanged = false;
        private bool _isReadyToSave = false;

        private bool _isDataChanged => _sectorDirect != _sectorDirectDefault || _sectorOpposite != _sectorOppositeDefault || _gateDirect.IsChanged || _gateOpposite.IsChanged;
        private bool _isDataReadyToSave => _sectorDirect != null && _sectorOpposite != null && _gateDirect.Active && _gateOpposite.Active && _isDataChanged;

        public SectorItem SectorDirect
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
        public SectorConnectionData SectorDirectSelectedConnection
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
                            _sectorDirectMap.SelectItem(value.Id);
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

        public SectorItem SectorOpposite
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
        public SectorConnectionData SectorOppositeSelectedConnection
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

        public bool IsChanged { get => _isChanged; set { _isChanged = value; OnPropertyChanged(nameof(IsChanged)); }}
        public bool IsReadyToSave { get => _isReadyToSave; set { _isReadyToSave = value; OnPropertyChanged(nameof(IsReadyToSave)); }}
        public GatesConnectionData(bool gateActiveDefault, string gateMacroDefault, SectorItem sectorDirectDefault = null, SectorItem sectorOppositeDefault = null)
        {
            _reference = null;
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
            IsChanged = _isDataChanged;
            IsReadyToSave = _isDataReadyToSave;
            OnPropertyChanged(nameof(SectorDirect));
            OnPropertyChanged(nameof(SectorOpposite));
            OnPropertyChanged(nameof(GateDirect));
            OnPropertyChanged(nameof(GateOpposite));
            UpdateCurrentGateOnMap(nameof(GateDirect));
            UpdateCurrentGateOnMap(nameof(GateOpposite));
        }

        public void SetDefaults(bool gateActiveDefault)
        {
            _gateDirect.SetDefaults(gateActiveDefault, "");
            _gateOpposite.SetDefaults(gateActiveDefault, "");
            OnPropertyChanged("");
        }
        public void SetReference(GalaxyConnectionData reference)
        {
            _reference = reference;
            if (reference == null) return;
            _sectorDirectDefault = new SectorItem
            {
                Name = _reference.SectorDirectName,
                Source = _reference.Connection.PathDirect.Sector.Source,
                Macro = _reference.Connection.PathDirect.Sector.Macro,
                Selectable = true
            };
            _sectorOppositeDefault = new SectorItem
            {
                Name = _reference.SectorOppositeName,
                Source = _reference.Connection.PathOpposite.Sector.Source,
                Macro = _reference.Connection.PathOpposite.Sector.Macro,
                Selectable = true
            };
            GateDirect = new GateData(_reference.GateDirectActive, _reference.Connection.PathDirect.Gate.GateMacro);
            GateOpposite = new GateData(_reference.GateOppositeActive, _reference.Connection.PathOpposite.Gate.GateMacro);
            UpdateCurrentGateOnMap(nameof(GateDirect));
            UpdateCurrentGateOnMap(nameof(GateOpposite));
        }
        private void ChildPropertyChanged(object sender, PropertyChangedEventArgs e)
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
        public event PropertyChangedEventHandler PropertyChanged;

        private void UpdateCurrentGateOnMap(string propertyName)
        {
            GateData gateCurrent = propertyName == nameof(GateDirect) ? GateDirect : GateOpposite;
            if (gateCurrent == null) return;
            SectorItem sectorTo = propertyName == nameof(GateDirect) ? SectorOpposite : SectorDirect;
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

        public void UpdateCurrentGateCoordinates(int X, int Y, int Z, string propertyName)
        {
            GateData gateCurrent = propertyName == nameof(GateDirect) ? GateDirect : GateOpposite;
            if (gateCurrent == null) return;
            gateCurrent.Coordinates = new Coordinates(X, Y ,Z);
        }
        protected void OnPropertyChanged(string propertyName)
        {
            if (_isChanged != _isDataChanged) IsChanged = _isDataChanged;
            if (_isReadyToSave != _isDataReadyToSave) IsReadyToSave = _isDataReadyToSave;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            if (propertyName == nameof(SectorDirect) || propertyName == nameof(SectorOpposite))
            {
                SectorItem sectorCurrent = propertyName == nameof(SectorDirect) ? SectorDirect : SectorOpposite;
                var sectorConnections = propertyName == nameof(SectorDirect) ? SectorDirectConnections : SectorOppositeConnections;
                sectorConnections.Clear();
                SectorMap sectorMap = propertyName == nameof(SectorDirect) ? SectorDirectMap : SectorOppositeMap;
                sectorMap.ClearItems();
                UpdateCurrentGateOnMap(propertyName == nameof(SectorDirect) ? nameof(GateDirect) : nameof(GateOpposite));
                if (sectorCurrent == null) return;


                MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow == null) return;
                var sectorsViewSource = propertyName == nameof(SectorDirect) ? mainWindow.SectorsOppositeViewSource : mainWindow.SectorsDirectViewSource;
                if (sectorsViewSource != null)
                {
                    sectorsViewSource.View.Refresh();
                }

                // Access the Galaxy property
                Galaxy galaxy = mainWindow.Galaxy;
                Sector sector = galaxy.GetSectorByMacro(sectorCurrent.Macro);
                if (sector == null || sector.Zones == null || sector.Zones.Count == 0) return;
                // Update opposite Gate details
                UpdateCurrentGateOnMap(propertyName == nameof(SectorDirect) ? nameof(GateOpposite) : nameof(GateDirect));
                foreach (var zone in sector.Zones)
                {
                    if (zone.Connections == null || zone.Connections.Count == 0) continue;
                    foreach (var connection in zone.Connections.Values)
                    {
                        if (connection is GateConnection gateConnection)
                        {
                            bool active = gateConnection.IsActive;
                            string sectorTo = active ? galaxy.GetOppositeSectorForGateConnection(gateConnection)?.Name : "";
                            Position zoneCoordinates = zone.Position;
                            if (zoneCoordinates == null) continue;
                            Position gateCoordinates = gateConnection.Position;
                            if (gateCoordinates == null) continue;
                            SectorConnectionData newConnection = new SectorConnectionData
                            {
                                Active = active && !string.IsNullOrEmpty(sectorTo),
                                ToSector = sectorTo,
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
            else if (propertyName == nameof(GateDirect) || propertyName == nameof(GateOpposite))
            {
                UpdateCurrentGateOnMap(propertyName);
            }
        }

    }

    public class SectorItem
    {
        public string Name { get; set; }
        public string Source { get; set; }
        public string Macro { get; set; }
        public bool Selectable { get; set; }
    }

    public class SectorConnectionData : INotifyPropertyChanged
    {
        private string _toSector;
        private int _x;
        private int _y;
        private int _z;
        private string _type;
        private string _id;
        private bool _active;

        public string ToSector
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

        public string Type
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

        public string Id
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

        public event PropertyChangedEventHandler PropertyChanged;

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

        public void SetDefaults(bool activeDefault, string gateMacroDefault = "")
        {
            _activeDefault = activeDefault;
            if (!string.IsNullOrEmpty(gateMacroDefault)) _gateMacroDefault = gateMacroDefault;
        }
        private void ChildPropertyChanged(object sender, PropertyChangedEventArgs e)
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
        public event PropertyChangedEventHandler PropertyChanged;

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
        public event PropertyChangedEventHandler PropertyChanged;

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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}