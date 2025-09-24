using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SharedWindows.Converters
{
  public class BoolInverseConverter : IValueConverter
  {
    // Converts a boolean value to its opposite
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is bool booleanValue)
      {
        return !booleanValue;
      }
      return DependencyProperty.UnsetValue;
    }

    // Converts back the value to its original form
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is bool booleanValue)
      {
        return !booleanValue;
      }
      return DependencyProperty.UnsetValue;
    }
  }

  public class BoolToVisibilityConverter : IValueConverter
  {
    // Converts a boolean value to a Visibility value
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is bool booleanValue)
      {
        return booleanValue ? Visibility.Visible : Visibility.Collapsed;
      }
      return DependencyProperty.UnsetValue;
    }

    // Converts back the value to its original form
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is Visibility visibilityValue)
      {
        return visibilityValue == Visibility.Visible;
      }
      return DependencyProperty.UnsetValue;
    }
  }

  public class BoolInverseToVisibilityConverter : IValueConverter
  {
    // Converts a boolean value to the opposite Visibility value
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is bool booleanValue)
      {
        return booleanValue ? Visibility.Collapsed : Visibility.Visible;
      }
      return DependencyProperty.UnsetValue;
    }

    // Converts back the value to its original form
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is Visibility visibilityValue)
      {
        return visibilityValue == Visibility.Collapsed;
      }
      return DependencyProperty.UnsetValue;
    }
  }

  public class NumberToBrushConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is double d)
      {
        if (d < 0)
          return System.Windows.Media.Brushes.SaddleBrown; // dark brown
        if (d > 0)
          return System.Windows.Media.Brushes.DarkGreen;
      }
      return System.Windows.Media.Brushes.Black;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
  }
}
