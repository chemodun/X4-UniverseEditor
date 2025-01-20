using System.ComponentModel;

namespace ChemGateBuilder
{
    public class GatesConnectionData : INotifyPropertyChanged
    {
        private SectorItem _sectorDirect = new SectorItem();
        private SectorItem _sectorOpposite = new SectorItem();

        private GateData _gateDirect = new GateData();
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