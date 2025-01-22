using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using X4DataLoader;

namespace ChemGateBuilder
{
    public class GalaxyConnectionData : INotifyPropertyChanged

    {
        private int _gateDirectX;
        private int _gateDirectY;
        private int _gateDirectZ;
        private int _gateOppositeX;
        private int _gateOppositeY;
        private int _gateOppositeZ;
        private GalaxyConnection _connection;

        public string SectorDirectName => _connection.PathDirect.Sector.Name;

        public int GateDirectX
        {
            get => _gateDirectX;
            set { _gateDirectX = value; OnPropertyChanged(nameof(GateDirectX)); }
        }
        public int GateDirectY
        {
            get => _gateDirectY;
            set { _gateDirectY = value; OnPropertyChanged(nameof(GateDirectY)); }
        }
        public int GateDirectZ
        {
            get => _gateDirectZ;
            set { _gateDirectZ = value; OnPropertyChanged(nameof(GateDirectZ)); }
        }
        public bool GateDirectActive => _connection.PathDirect.Gate != null && _connection.PathDirect.Gate.IsActive;
        public string SectorOppositeName => _connection.PathOpposite.Sector.Name;
        public int GateOppositeX
        {
            get => _gateOppositeX;
            set { _gateOppositeX = value; OnPropertyChanged(nameof(GateOppositeX)); }
        }
        public int GateOppositeY
        {
            get => _gateOppositeY;
            set { _gateOppositeY = value; OnPropertyChanged(nameof(GateOppositeY)); }
        }
        public int GateOppositeZ
        {
            get => _gateOppositeZ;
            set { _gateOppositeZ = value; OnPropertyChanged(nameof(GateOppositeZ)); }
        }
        public bool GateOppositeActive => _connection.PathOpposite.Gate != null && _connection.PathOpposite.Gate.IsActive;

        public GalaxyConnection Connection
        {
            get => _connection;
            set { _connection = value; OnPropertyChanged(nameof(Connection)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}