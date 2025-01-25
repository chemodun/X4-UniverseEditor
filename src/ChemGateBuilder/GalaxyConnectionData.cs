using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using X4DataLoader;

namespace ChemGateBuilder
{
    public class GalaxyConnectionData : INotifyPropertyChanged

    {
        private GalaxyConnection _connection;
        public Coordinates DirectPosition;
        public Rotation DirectRotation;
        public Coordinates OppositePosition;
        public Rotation OppositeRotation;
        public string SectorDirectName => _connection?.PathDirect?.Sector?.Name ?? string.Empty;
        public int GateDirectX => (int)(_connection?.PathDirect?.Zone?.Position?.X ?? 0);
        public int GateDirectY => (int)(_connection?.PathDirect?.Zone?.Position?.Y ?? 0);
        public int GateDirectZ => (int)(_connection?.PathDirect?.Zone?.Position?.Z ?? 0);
        public bool GateDirectActive => _connection?.PathDirect?.Gate?.IsActive ?? false;
        public string SectorOppositeName => _connection?.PathOpposite?.Sector?.Name ?? string.Empty;
        public int GateOppositeX => (int)(_connection?.PathOpposite?.Zone?.Position?.X ?? 0);
        public int GateOppositeY => (int)(_connection?.PathOpposite?.Zone?.Position?.Y ?? 0);
        public int GateOppositeZ => (int)(_connection?.PathOpposite?.Zone?.Position?.Z ?? 0);
        public bool GateOppositeActive => _connection?.PathOpposite?.Gate?.IsActive ?? false;

        public GalaxyConnection Connection
        {
            get => _connection;
            set
            {
                _connection = value;
                OnPropertyChanged(nameof(Connection));
            }
        }

        public GalaxyConnectionData(GalaxyConnection connection, GatesConnectionData connectionData)
        {
            _connection = connection;
            Connection = connection;
            DirectPosition = connectionData.GateDirect.Position;
            DirectRotation = connectionData.GateDirect.Rotation;
            OppositePosition = connectionData.GateOpposite.Position;
            OppositeRotation = connectionData.GateOpposite.Rotation;
            PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(Connection))
                {
                    OnPropertyChanged(nameof(SectorDirectName));
                    OnPropertyChanged(nameof(GateDirectX));
                    OnPropertyChanged(nameof(GateDirectY));
                    OnPropertyChanged(nameof(GateDirectZ));
                    OnPropertyChanged(nameof(GateDirectActive));
                    OnPropertyChanged(nameof(SectorOppositeName));
                    OnPropertyChanged(nameof(GateOppositeX));
                    OnPropertyChanged(nameof(GateOppositeY));
                    OnPropertyChanged(nameof(GateOppositeZ));
                    OnPropertyChanged(nameof(GateOppositeActive));
                }
            };
        }

        public void Update(GalaxyConnection connection, GatesConnectionData connectionData)
        {
            Connection = connection;
            DirectPosition = connectionData.GateDirect.Position;
            DirectRotation = connectionData.GateDirect.Rotation;
            OppositePosition = connectionData.GateOpposite.Position;
            OppositeRotation = connectionData.GateOpposite.Rotation;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}