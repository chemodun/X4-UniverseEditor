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

        public double MapColorsOpacity { get; set; } = 0.5;

        public SectorMapExpandedWindow(Window owner, string title, int sectorRadius, SectorMap sectorMap, double mapColorsOpacity)
        {
            Owner = owner;
            Title = title;
            var minSize = Math.Min(Owner.ActualWidth, Owner.ActualHeight) * 0.9;
            Width = minSize;
            Height = minSize;
            Left = WindowHelper.GetWindowLeft(Owner) + (Owner.ActualWidth - minSize) / 2;
            Top = WindowHelper.GetWindowTop(Owner) + (Owner.ActualHeight - minSize) / 2;
            MapColorsOpacity = mapColorsOpacity;
            InitializeComponent();
            DataContext = this;
            _sectorMapExpanded.InternalSizeKm = sectorRadius;
            _sectorMapExpanded.Connect(SectorMapExpandedCanvas, SectorHexagon);
            _sectorMapExpanded.From(sectorMap);
            SectorMapItem? newItem = _sectorMapExpanded.GetItem(SectorMap.NewGateId);
            if (newItem != null && newItem.ObjectData != null)
            {
                NewGateCoordinates.X = newItem.ObjectData.X;
                NewGateCoordinates.Y = newItem.ObjectData.Y;
                NewGateCoordinates.Z = newItem.ObjectData.Z;
            }
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