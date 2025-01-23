using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ChemGateBuilder
{
    public class SectorMap : INotifyPropertyChanged
    {
        private double _visualX;
        private double _visualY;
        private double _visualSizePx = 100; // Default size
        private string?  _selectedItemId = "";
        public double InternalSizeKm { get; set; } = 400;

        public double VisualSizePx
        {
            get => _visualSizePx;
            set { _visualSizePx = value; OnPropertyChanged(); }
        }

        public double MinVisualSectorSize { get; set; } = 50;
        public double MaxVisualSectorSize { get; set; } = 300;

        public double VisualX
        {
            get => _visualX;
            set { _visualX = value; OnPropertyChanged(); }
        }

        public double VisualY
        {
            get => _visualY;
            set { _visualY = value; OnPropertyChanged(); }
        }

        public string? SelectedItemId
        {
            get => _selectedItemId;
            set { _selectedItemId = value; OnPropertyChanged(); }
        }
        public ObservableCollection<SectorMapItem> Items { get; set; } = new ObservableCollection<SectorMapItem>();

        public bool IsDragging = false;
        public SectorMapItem? SelectedItem = null;
        public Point MouseOffset;
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void ClearItems()
        {
            Items.Clear();
        }
        public void AddItem(SectorConnectionData connectionData)
        {
            Items.Add(new SectorMapItem
            {
                SectorMap = this,
                ConnectionData = connectionData,
                IsNew = connectionData.Id == "New"
            });
        }

        public void SelectItem(string? ItemId)
        {
            string? selectedItemId = SelectedItemId;
            SelectedItemId = ItemId;
            if (selectedItemId != ItemId)
            {
                if (selectedItemId != null && selectedItemId != "")
                {
                    RefreshItem(selectedItemId);
                }
                if (ItemId != null && ItemId != "")
                {
                    RefreshItem(ItemId);
                }
            }
        }
        public bool RefreshItem(string ItemId)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].Id == ItemId)
                {
                    var Item = Items[i];
                    Items.RemoveAt(i);
                    Items.Add(Item);
                    return true;
                }
            }
            return false;
        }
        public void UpdateItem(SectorConnectionData? connectionData)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].Id == connectionData?.Id)
                {
                    Items.RemoveAt(i);
                }
            }
            if (connectionData != null) AddItem(connectionData);
        }

        private void UpdateItems()
        {
            foreach (var item in Items.ToArray())
            {
                UpdateItem(item.ConnectionData);
            }
        }
        public void OnSizeChanged(double newSize)
        {
            VisualSizePx = Math.Min(Math.Max(newSize, MinVisualSectorSize), MaxVisualSectorSize);
            UpdateItems();
        }
        public void SetInternalSize (int sizeKm)
        {
            InternalSizeKm = sizeKm;
            UpdateItems();
        }
    }

    public class SectorMapItem : INotifyPropertyChanged
    {
        private double _x;
        private double _y;
        private double _itemSizePx = 12;
        private bool _isNew;
        private SectorConnectionData? _connectionData;
        private SectorMap? _sectorMap;
        public SectorMap? SectorMap {
            get => _sectorMap;
            set {
                _sectorMap = value;
                UpdatePosition();
            }
        }
        public SectorConnectionData? ConnectionData {
            get => _connectionData;
            set {
                _connectionData = value;
                UpdatePosition();
            }
        }
        public string Type { get {
            if (_connectionData == null || _connectionData?.Type == null)
                return "empty";
            return _connectionData.Type;
        } } // e.g., "empty", "gate", "highway", etc.
        public string Status { get {
            if (_connectionData == null || _connectionData?.Active == null)
                return "unknown";
            return _connectionData.Active ? "active" : "inactive";
        } } // e.g., "active", "inactive", "unknown"
        public string? Id { get {
            if (_connectionData == null)
                return "unknown";
            return _connectionData.Id;
        } }
        public string ToolTip { get {
            if (_connectionData == null)
                return "No connection data";
            string result = $"Type: {Type}, Status: {Status}\n";
            if (Type == "gate" || Type == "highway")
                result += $"To: {_connectionData.ToSector ?? ""}\n";
            result += $" X: {_connectionData.X}, Y: {_connectionData.Y}, Z: {_connectionData.Z}";
            return result;
        } }
        public double ItemSizePx {
            get => _itemSizePx;
            set { _itemSizePx = value; OnPropertyChanged(); }
        }
        public double X
        {
            get => _x;
            set { _x = value; OnPropertyChanged(); }
        }

        public double Y
        {
            get => _y;
            set { _y = value; OnPropertyChanged(); }
        }

        public bool IsNew
        {
            get => _isNew;
            set { _isNew = value; OnPropertyChanged(); }
        }

        private void UpdatePosition()
        {
            if (SectorMap == null || ConnectionData == null)
                return;
            X = (ConnectionData.X * SectorMap.VisualSizePx / SectorMap.InternalSizeKm + SectorMap.VisualSizePx - ItemSizePx) / 2;
            Y = (- ConnectionData.Z * SectorMap.VisualSizePx / SectorMap.InternalSizeKm + SectorMap.VisualSizePx - ItemSizePx) / 2;
            OnPropertyChanged(nameof(X));
            OnPropertyChanged(nameof(Y));
        }

        public void UpdateInternalCoordinates(Coordinates coordinates)
        {
            if (SectorMap == null)
                return;
            coordinates.X = (int)((X * 2 + ItemSizePx - SectorMap.VisualSizePx) * SectorMap.InternalSizeKm / SectorMap.VisualSizePx);
            coordinates.Z = (int)((SectorMap.VisualSizePx - Y * 2 - ItemSizePx) * SectorMap.InternalSizeKm / SectorMap.VisualSizePx);
            if (_connectionData == null)
                return;
            _connectionData.X = coordinates.X;
            _connectionData.Z = coordinates.Z;
        }
        // Colors based on gate type and status
        public Brush BorderColor
        {
            get
            {
                if (ConnectionData != null && SectorMap != null && ConnectionData.Id == SectorMap.SelectedItemId)
                    return Brushes.Yellow;
                return Status switch
                {
                    "active" => Brushes.LimeGreen,
                    "inactive" => Brushes.Brown,
                    _ => Brushes.DarkGray
                };
            }
        }

        public Brush FillColor
        {
            get
            {
                if (IsNew)
                    return Brushes.LightGreen;

                return Type switch
                {
                    "empty" => Brushes.DarkGray,
                    "gate" => Brushes.DarkOrange,
                    "highway" => Brushes.Olive,
                    "mod" => Brushes.DarkGreen,
                    _ => Brushes.LightGray
                };
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

namespace ChemGateBuilder.Core.Converters
{
    public class HexagonPointsConverter : IValueConverter
    {
        // Converts VisualSizePx (double) to PointCollection for a horizontally oriented hexagon
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double size && size > 0)
            {
                double radius = size / 2;
                PointCollection points = new PointCollection();

                for (int i = 0; i < 6; i++)
                {
                    double angle_deg = 60 * i; // Start at 0 degrees for flat-top
                    double angle_rad = Math.PI / 180 * angle_deg;
                    double x = radius + radius * Math.Cos(angle_rad);
                    double y = radius + radius * Math.Sin(angle_rad);
                    points.Add(new Point(x, y));
                }
                return points;
            }
            return DependencyProperty.UnsetValue;
        }

        // Not implemented as conversion back is not required
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}