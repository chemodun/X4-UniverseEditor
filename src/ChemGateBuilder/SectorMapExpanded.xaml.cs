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
        public SectorMapExpandedWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void SectorMapExpandedCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double newSize = Math.Min(e.NewSize.Width, e.NewSize.Height) ;//* 0.8; // 80% of smaller dimension
            SectorMapExpanded?.OnSizeChanged(newSize);
        }

        private void SectorMapExpandedItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Image image && image.DataContext is SectorMapItem item && SectorMapExpanded != null)
            {
                SectorMapExpanded.SelectedItem = item;
                if (item.IsNew) // Only allow dragging for "new" gates
                {
                    SectorMapExpanded.IsDragging = true;

                    // Get the mouse position relative to the gate
                    SectorMapExpanded.MouseOffset = e.GetPosition(image);

                    // Capture the mouse to receive MouseMove events even if the cursor leaves the image
                    image.CaptureMouse();
                }
                Log.Debug($"[MouseLeftButtonDown] Selected Item: {SectorMapExpanded.SelectedItem?.ConnectionData?.Id}, IsDragging: {SectorMapExpanded.IsDragging}, MouseOffset: {SectorMapExpanded.MouseOffset}");
            }
        }

        private void SectorMapExpandedItem_MouseMove(object sender, MouseEventArgs e)
        {
            if (SectorMapExpanded != null && SectorMapExpanded.SelectedItem != null && SectorMapExpanded.IsDragging)
            {
                double halfSize = SectorMapExpanded.SelectedItem.ItemSizePx / 2;
                SectorMapItem selectedItem = SectorMapExpanded.SelectedItem;
                Log.Debug($"[MouseMove] Selected Item: {SectorMapExpanded.SelectedItem?.ConnectionData?.Id}, IsDragging: {SectorMapExpanded.IsDragging}, MouseOffset: {SectorMapExpanded.MouseOffset}, sender: {sender}, isImage: {sender is Image}");
                if (sender is Image image)
                {
                    // Get the current mouse position relative to the SectorCanvas
                    Point mousePosition = e.GetPosition(SectorMapExpandedCanvas);
                    // Calculate new position by subtracting the offset
                    double newX = mousePosition.X - SectorMapExpanded.MouseOffset.X;
                    double newY = mousePosition.Y - SectorMapExpanded.MouseOffset.Y;
                    // Account the size of the item
                    Point newPoint = new(newX + halfSize , newY + halfSize);
                    // Check if the new position is inside the hexagon
                    bool isInside = SectorHexagon.RenderedGeometry.FillContains(newPoint);
                    Log.Debug($"[MouseMove] IsInside: {isInside}");
                    if (isInside)
                    {
                        // Update the SectorMapItem's coordinates
                        selectedItem.X = newX;
                        selectedItem.Y = newY;
                        // selectedItem.UpdateInternalCoordinates(GatesConnectionCurrent.GateDirect.Coordinates);
                        Log.Debug($"[MouseMove] New X: {newX}, New Y: {newY}");
                        selectedItem.UpdateInternalCoordinates(NewGateCoordinates);
                    }
                }
            }
        }

        private void SectorMapExpandedItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (SectorMapExpanded != null && SectorMapExpanded.SelectedItem != null)
            {
                SectorMapExpanded.IsDragging = false;
                SectorMapExpanded.SelectedItem = null;

                if (sender is Image image && image.DataContext is SectorMapItem item && item != null)
                {
                    image.ReleaseMouseCapture();
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