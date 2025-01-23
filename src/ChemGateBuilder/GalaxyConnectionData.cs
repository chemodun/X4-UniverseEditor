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
        private int _gateDirectX;
        private int _gateDirectY;
        private int _gateDirectZ;
        private int _gateOppositeX;
        private int _gateOppositeY;
        private int _gateOppositeZ;
        public Coordinates DirectPosition;
        public Rotation DirectRotation;
        public Coordinates OppositePosition;
        public Rotation OppositeRotation;
        public string SectorDirectName => _connection.PathDirect.Sector.Name;
        public int GateDirectX => (int)_connection.PathDirect.Zone.Position.x;
        public int GateDirectY => (int)_connection.PathDirect.Zone.Position.y;
        public int GateDirectZ => (int)_connection.PathDirect.Zone.Position.z;
        public bool GateDirectActive => _connection.PathDirect.Gate != null && _connection.PathDirect.Gate.IsActive;
        public string SectorOppositeName => _connection.PathOpposite.Sector.Name;
        public int GateOppositeX => (int)_connection.PathOpposite.Zone.Position.x;
        public int GateOppositeY => (int)_connection.PathOpposite.Zone.Position.y;
        public int GateOppositeZ => (int)_connection.PathOpposite.Zone.Position.z;
        public bool GateOppositeActive => _connection.PathOpposite.Gate != null && _connection.PathOpposite.Gate.IsActive;

        public GalaxyConnection Connection
        {
            get => _connection;
            set
            {
                _connection = value;
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
        }

        public GalaxyConnectionData(GalaxyConnection connection, GatesConnectionData connectionData)
        {
            Update(connection, connectionData);
        }

        public void Update(GalaxyConnection connection, GatesConnectionData connectionData)
        {
            Connection = connection;
            DirectPosition = connectionData.GateDirect.Position;
            DirectRotation = connectionData.GateDirect.Rotation;
            OppositePosition = connectionData.GateOpposite.Position;
            OppositeRotation = connectionData.GateOpposite.Rotation;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}