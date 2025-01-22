using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using X4DataLoader;

namespace ChemGateBuilder
{
    public class GalaxyConnectionData : INotifyPropertyChanged

    {
        private string _sectorDirectName;
        private int _gateDirectX;
        private int _gateDirectY;
        private int _gateDirectZ;
        private bool _gateDirectActive;
        private string _sectorOppositeName;
        private int _gateOppositeX;
        private int _gateOppositeY;
        private int _gateOppositeZ;
        private bool _gateOppositeActive;
        private GalaxyConnection _connection;

        public string SectorDirectName
        {
            get => _sectorDirectName;
            set { _sectorDirectName = value; OnPropertyChanged(nameof(SectorDirectName)); }
        }
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
        public bool GateDirectActive
        {
            get => _gateDirectActive;
            set { _gateDirectActive = value; OnPropertyChanged(nameof(GateDirectActive)); }
        }
        public string SectorOppositeName
        {
            get => _sectorOppositeName;
            set { _sectorOppositeName = value; OnPropertyChanged(nameof(SectorOppositeName)); }
        }
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
        public bool GateOppositeActive
        {
            get => _gateOppositeActive;
            set { _gateOppositeActive = value; OnPropertyChanged(nameof(GateOppositeActive)); }
        }

        public string ConnectionId {
            get {
                if (_connection == null || _connection.Name == null)
                    return "";
                return _connection.Name;
            }
        }

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