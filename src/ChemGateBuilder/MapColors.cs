using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.DirectoryServices;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;
using System.Xml.XPath;
using Utilities.Logging;
using X4DataLoader;

namespace ChemGateBuilder
{
  public class FactionColors()
  {
    private readonly Dictionary<string, Color> MappedColors = [];
    private readonly Dictionary<string, SolidColorBrush> MappedBrushes = [];

    public void Load(List<Faction> allFactions, List<X4MappedColor> mappedColors)
    {
      MappedBrushes.Clear();
      MappedColors.Clear();
      foreach (Faction faction in allFactions)
      {
        X4MappedColor? mappedColor = mappedColors.FirstOrDefault(x => x.Id == faction.ColorId);
        if (mappedColor != null)
        {
          Color color = Color.FromArgb((byte)mappedColor.Alpha, (byte)mappedColor.Red, (byte)mappedColor.Green, (byte)mappedColor.Blue);
          MappedColors.Add(faction.Id, color);
          MappedBrushes.Add(faction.Id, new SolidColorBrush(color));
        }
      }
      Color colorEmpty = (Color)ColorConverter.ConvertFromString("#B0B0B0");
      MappedColors.Add("", colorEmpty);
      MappedBrushes.Add("", new SolidColorBrush(colorEmpty));
    }

    public SolidColorBrush? GetBrush(string id)
    {
      if (MappedBrushes.TryGetValue(id, out SolidColorBrush? value))
      {
        return value;
      }
      return null;
    }

    public Color? GetColor(string id)
    {
      if (MappedColors.TryGetValue(id, out Color value))
      {
        return value;
      }
      return null;
    }

    public string GetColorString(string id)
    {
      if (MappedColors.TryGetValue(id, out Color value))
      {
        Color color = value;
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
      }
      return string.Empty;
    }
  }
}
