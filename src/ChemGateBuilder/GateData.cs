using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using X4DataLoader;

namespace ChemGateBuilder
{
    public class GatesConnectionData : INotifyPropertyChanged
    {
        private SectorItem _sectorDirect = new SectorItem();
        private SectorConnectionData _sectorDirectSelectedConnection = new SectorConnectionData();
        private SectorMap _sectorDirectMap = new SectorMap();
        private ObservableCollection<SectorConnectionData> _sectorDirectConnections = new ObservableCollection<SectorConnectionData>();
        private GateData _gateDirect = new GateData();
        private SectorItem _sectorOpposite = new SectorItem();
        private SectorConnectionData _sectorOppositeSelectedConnection = new SectorConnectionData();
        private SectorMap _sectorOppositeMap = new SectorMap();
        private ObservableCollection<SectorConnectionData>  _SectorOppositeConnections = new ObservableCollection<SectorConnectionData>();
        private GateData _gateOpposite = new GateData();

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
                        _sectorDirectMap.SelectItem(value.Id);
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
                    _gateDirect = value;
                    OnPropertyChanged(nameof(GateDirect));
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
                    _gateOpposite = value;
                    OnPropertyChanged(nameof(GateOpposite));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            if (propertyName == nameof(SectorDirect) || propertyName == nameof(SectorOpposite))
            {
                SectorItem sectorCurrent = propertyName == nameof(SectorDirect) ? SectorDirect : SectorOpposite;
                var sectorConnections = propertyName == nameof(SectorDirect) ? SectorDirectConnections : SectorOppositeConnections;
                sectorConnections.Clear();
                SectorMap sectorMap = propertyName == nameof(SectorDirect) ? SectorDirectMap : SectorOppositeMap;
                sectorMap.ClearItems();
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

                foreach (var zone in sector.Zones)
                {
                    if (zone.Connections == null || zone.Connections.Count == 0) continue;
                    foreach (var connection in zone.Connections.Values)
                    {
                        if (connection is GateConnection gateConnection)
                        {
                            bool active = gateConnection.IsActive;
                            string sectorTo = active ? galaxy.GetOppositeSectorForGateConnection(gateConnection)?.Name : "";
                            Connection? zoneConnection = sector.GetConnection(zone.ConnectionId);
                            if (zoneConnection == null) continue;
                            float[] zoneCoordinates = zoneConnection.GetCoordinates();
                            if (zoneCoordinates == null) continue;
                            float[] gateCoordinates = gateConnection.GetCoordinates();
                            if (gateCoordinates == null) continue;
                            SectorConnectionData newConnection = new SectorConnectionData
                            {
                                Active = active && !string.IsNullOrEmpty(sectorTo),
                                ToSector = sectorTo,
                                X = 0, // Update as needed
                                Y = 0, // Update as needed
                                Z = 0, // Update as needed
                                Type = "gate",
                                Id = gateConnection.Name
                            };
                            for (int i = 0; i < 3; i++)
                            {
                                int value = (int)((zoneCoordinates[i] + gateCoordinates[i]) / 1000);
                                switch (i)
                                {
                                    case 0:
                                        newConnection.X = value;
                                        break;
                                    case 1:
                                        newConnection.Y = value;
                                        break;
                                    case 2:
                                        newConnection.Z = value;
                                        break;
                                }
                            }
                            sectorConnections.Add(newConnection);
                            sectorMap.AddItem(newConnection);
                        }
                    }
                }
            }
        }

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

        public Coordinates Coordinates
        {
            get => _coordinates;
            set
            {
                if (_coordinates != value)
                {
                    _coordinates = value;
                    OnPropertyChanged(nameof(Coordinates));
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
                    OnPropertyChanged(nameof(_position));
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }



    public class Coordinates : INotifyPropertyChanged
    {
        private int _x;
        private int _y;
        private int _z;

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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Rotation : INotifyPropertyChanged
    {
        private int _roll;
        private int _pitch;
        private int _yaw;

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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}