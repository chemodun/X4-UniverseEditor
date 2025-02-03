using System.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;
using System.Data;
using System.Windows;
using X4DataLoader;
using System.Xml.Linq;
using System.Xml.XPath;
using System.DirectoryServices;
using Utilities.Logging;
using System.Text.RegularExpressions;
using System.Windows.Media;


namespace ChemGateBuilder
{
    public class FactionColors()
    {
        private Dictionary<string,Color> MappedColors = new Dictionary<string, Color>();
        private Dictionary<string, SolidColorBrush> MappedBrushes = new Dictionary<string, SolidColorBrush>();

        public void Load(List<Faction> allFactions, List<X4MappedColor> mappedColors)
        {
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
            if (MappedBrushes.ContainsKey(id))
            {
                return MappedBrushes[id];
            }
            return null;
        }

        public Color? GetColor(string id)
        {
            if (MappedColors.ContainsKey(id))
            {
                return MappedColors[id];
            }
            return null;
        }
    }

}