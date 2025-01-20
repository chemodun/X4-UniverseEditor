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

        public ObservableCollection<SectorMapItem> Items { get; set; } = new ObservableCollection<SectorMapItem>();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void ClearItems()
        {
            Items.Clear();
        }
        public void AddItem(int x, int y, int z, string type, string status, bool isNew = false)
        {
            Items.Add(new SectorMapItem
            {
                ExternalX = x,
                ExternalZ = z,
                X = (x * VisualSizePx / InternalSizeKm + VisualSizePx) / 2,
                Y = (z * VisualSizePx / InternalSizeKm + VisualSizePx) / 2,
                Type = type,
                Status = status,
                IsNew = isNew
            });
        }
    }

    public class SectorMapItem : INotifyPropertyChanged
    {
        private double _x;
        private double _y;
        private bool _isNew;

        public int ExternalX {get; set;}
        public int ExternalY {get; set;}
        public int ExternalZ {get; set;}
        public string Type { get; set; } // e.g., "empty", "gate", "highway", etc.
        public string Status { get; set; } // e.g., "active", "inactive", "unknown"
        public string ToolTip => $"{Type} ({ExternalX} km, {ExternalY} km, {ExternalZ} km) - {Status}";

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

        // Colors based on gate type and status
        public Brush BorderColor
        {
            get
            {
                return Status switch
                {
                    "active" => Brushes.LimeGreen,
                    "inactive" => Brushes.Red,
                    _ => Brushes.DarkGray
                };
            }
        }

        public Brush FillColor
        {
            get
            {
                if (IsNew)
                    return Brushes.Yellow;

                return Type switch
                {
                    "empty" => Brushes.DarkGray,
                    "gate" => Brushes.Blue,
                    "highway" => Brushes.Olive,
                    "mod" => Brushes.DarkGreen,
                    _ => Brushes.LightGray
                };
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
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