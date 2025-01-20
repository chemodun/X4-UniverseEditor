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

        // public ObservableCollection<Gate> Gates { get; set; } = new ObservableCollection<Gate>();

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