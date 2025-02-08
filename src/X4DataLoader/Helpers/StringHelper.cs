using System.Globalization;

namespace X4DataLoader.Helpers
{
  public static class StringHelper
  {
    public static bool EqualsIgnoreCase(string str1, string str2)
    {
      return string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase);
    }

    public static int ParseInt(string? str, int defaultValue = 0)
    {
      if (string.IsNullOrEmpty(str))
      {
        return defaultValue;
      }
      return int.TryParse(str, CultureInfo.InvariantCulture, out int result) ? result : defaultValue;
    }

    public static double ParseDouble(string? str, double defaultValue = 0.0)
    {
      if (string.IsNullOrEmpty(str))
      {
        return defaultValue;
      }
      return double.TryParse(str, CultureInfo.InvariantCulture, out double result) ? result : defaultValue;
    }
  }
}
