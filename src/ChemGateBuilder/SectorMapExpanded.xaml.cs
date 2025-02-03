using System.Windows;
using Utilities.Logging;
using System.Windows.Input;
using System.Windows.Controls;
using System.ComponentModel;

namespace ChemGateBuilder
{
    public partial class SectorMapExpandedWindow : Window, INotifyPropertyChanged
    {
        // Rename to avoid conflict
        public SectorMap _sectorMapExpanded = new();

        public SectorMap SectorMapExpanded
        {
            get => _sectorMapExpanded;
            set
            {
                // _sectorMapExpanded.PropertyChanged -= ChildPropertyChanged;
                _sectorMapExpanded = value;
                OnPropertyChanged(nameof(SectorMapExpanded));
            }
        }
        private Coordinates _newGateCoordinates = new();
        public Coordinates NewGateCoordinates
        {
            get => _newGateCoordinates;
            set
            {
                _newGateCoordinates = value;
                OnPropertyChanged(nameof(NewGateCoordinates));
            }
        }
        public SectorMapExpandedWindow(int sectorRadius, FactionColors factionColors)
        {
            InitializeComponent();
            DataContext = this;
            _sectorMapExpanded.InternalSizeKm = sectorRadius;
            _sectorMapExpanded.Connect(SectorMapExpandedCanvas, SectorHexagon);
            _sectorMapExpanded.SetColors(factionColors);
        }

        private void SectorMapExpandedCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SectorMapExpanded?.OnSizeChanged(e.NewSize.Width, e.NewSize.Height);
        }

        private void SectorMapExpandedItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SectorMapExpanded.MouseLeftButtonDown(sender, e);
        }

        private void SectorMapExpandedItem_MouseMove(object sender, MouseEventArgs e)
        {
            SectorMapExpanded.MouseMove(sender, e, NewGateCoordinates);
        }

        private void SectorMapExpandedItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
           SectorMapExpanded.MouseLeftButtonUp(sender, e);
        }

        public void SetMapItems(List<SectorMapItem> mapItems)
        {
            foreach (SectorMapItem item in mapItems)
            {
                if (item.ConnectionData != null)
                {
                    SectorMapExpanded.AddItem(item.ConnectionData);
                    if (item.IsNew)
                    {
                        NewGateCoordinates.X = item.ConnectionData.X;
                        NewGateCoordinates.Y = item.ConnectionData.Y;
                        NewGateCoordinates.Z = item.ConnectionData.Z;
                    }
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }
    }
}