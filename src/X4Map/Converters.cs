using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace X4Map.Converters
{
  public class HexagonPointsConverter : IValueConverter
  {
    // Converts VisualSizePx (double) to PointCollection for a horizontally oriented hexagon
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is double size && size > 0)
      {
        return Converter(size);
      }
      return DependencyProperty.UnsetValue;
    }

    public static PointCollection Converter(double size)
    {
      double radius = size / 2;
      PointCollection points = [];

      for (int i = 0; i < 6; i++)
      {
        double angle_deg = 60 * i; // Start at 0 degrees for flat-top
        double angle_rad = Math.PI / 180 * angle_deg;
        double x = radius + radius * Math.Cos(angle_rad);
        double y = radius + radius * Math.Sin(angle_rad);
        points.Add(new System.Windows.Point(x, y - size * 0.067));
      }
      return points;
    }

    // Not implemented as conversion back is not required
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
