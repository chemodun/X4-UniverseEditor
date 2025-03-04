using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Utilities.Logging;
using X4DataLoader;

namespace X4Map
{
  public class GalaxyMapSector(
    double x,
    double y,
    GalaxyMapCluster owner,
    Canvas canvas,
    Cluster cluster,
    Sector sector,
    double hexagonWidth,
    double hexagonHeight,
    double scaleFactor,
    bool isHalf = false
  ) : GalaxyMapCluster(x, y, new MapPosition(0, 0), canvas, cluster, sector.Position, hexagonWidth, hexagonHeight, scaleFactor)
  {
    protected override double Modifier { get; set; } = isHalf ? 0.5 : 1;
    public GalaxyMapCluster Owner = owner;
    public Sector? Sector = sector;
    public override string Macro => Sector?.Macro ?? "";
    protected Grid? Grid = null;
    protected TextBlock? TextBlock = null;
    private readonly SectorMap SectorMapHelper = new();
    private readonly double FrontSizeProportion = 0.12;
    private readonly double FrontSizeMax = 22;

    private static readonly string[] _toolTipItems =
    [
      "Name",
      "Owner",
      "Sunlight",
      "Economy",
      "Security",
      "Source",
      "Macro",
      "Coordinates",
      "X",
      "Y",
      "Z",
      "Cluster",
    ];

    protected override string[] ToolTipItems => [.. _toolTipItems, .. base.ToolTipItems]; /* Implement setter if needed, or leave it empty if not applicable */

    public override double Create(GalaxyMapViewer map)
    {
      if (Cluster == null || Sector == null || Canvas == null || map == null)
      {
        return 0;
      }
      UpdatePoints();
      SolidColorBrush brush;
      Log.Debug(
        $"Creating sector {Sector.Name} at {X}, {Y}. Source: {Sector.Source}. Owner: '{Sector.DominantOwner}', Faction: {Sector.DominantOwnerFaction?.Id}, Color: {Sector.Color}, Faction Color: {Sector.DominantOwnerFaction?.Color}"
      );
      if (Sector.Color != null)
      {
        brush = new SolidColorBrush(
          Color.FromArgb(
            (byte)(Sector.Color.Alpha * map.MapColorsOpacity),
            (byte)Sector.Color.Red,
            (byte)Sector.Color.Green,
            (byte)Sector.Color.Blue
          )
        );
      }
      else
      {
        brush = Brushes.LightGray;
      }
      Hexagon = new()
      {
        Stroke = Brushes.Black,
        StrokeThickness = 1,
        Fill = brush,
        // Fill.Opacity = 0.5,
        Tag = Sector.Name,
        DataContext = Sector,
        Points = Points,
        ToolTip = ToolTipCreator(Cluster, Sector, null, Owner.MapPosition),
      };
      Grid = new()
      {
        Width = Width,
        Height = Height,
        DataContext = Sector,
      };
      Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(value: 25, GridUnitType.Star) });
      Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(value: 50, GridUnitType.Star) });
      Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(value: 25, GridUnitType.Star) });
      Grid.SetColumn(Hexagon, value: 0);
      Grid.SetColumnSpan(Hexagon, value: 3);
      Grid.Children.Add(Hexagon);
      // Create TextBlock
      TextBlock = new()
      {
        Text = Sector.Name,
        Foreground = Brushes.Black,
        FontSize = SetFontSize(), // Initial proportional font size
        TextAlignment = TextAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Top,
        TextWrapping = TextWrapping.Wrap,
        Background = Brushes.Transparent, // Make the background transparent
        DataContext = Sector,
        FontWeight = FontWeights.DemiBold, // Make the text bold
      };
      Grid.SetColumn(TextBlock, value: 1);
      Grid.Children.Add(TextBlock);
      // Position the Hexagon on the Canvas
      Canvas.SetLeft(Grid, X);
      Canvas.SetTop(Grid, Y);
      Canvas.Children.Add(Grid);
      SectorMapHelper.Connect(Canvas, Hexagon, mapMode: true);
      SectorMapHelper.VisualX = X;
      SectorMapHelper.VisualY = Y;
      SectorMapHelper.VisualSizePx = Width;
      SectorMapHelper.SetInternalSize(map.SectorRadius);
      SectorMapHelper.ItemSizeMinPx = 4;
      SectorMapHelper.SetSector(Sector, map.GalaxyData);
      List<ObjectInSector>? extraObjects = map.GetExtraObjects(Sector.Macro);
      foreach (ObjectInSector modObject in extraObjects)
      {
        SectorMapHelper.AddItem(modObject);
      }
      foreach (SectorMapItem item in SectorMapHelper.Items)
      {
        map.SectorsItems.Add(item);
        Image image = new() { DataContext = item };
        // Binding for Width
        Binding widthBinding = new(path: "ItemSizePx") { Source = item };
        image.SetBinding(Image.WidthProperty, widthBinding);

        // Binding for Height
        Binding heightBinding = new(path: "ItemSizePx") { Source = item };
        image.SetBinding(Image.HeightProperty, heightBinding);
        // Binding for Source
        Binding sourceBinding = new(path: "ObjectImage") { Source = item };
        image.SetBinding(Image.SourceProperty, sourceBinding);
        // Create TranslateTransform
        TranslateTransform translateTransform = new();

        // Binding for TranslateTransform.X
        Binding translateXBinding = new(path: "X") { Source = item };
        BindingOperations.SetBinding(translateTransform, TranslateTransform.XProperty, translateXBinding);

        // Binding for TranslateTransform.Y
        Binding translateYBinding = new(path: "Y") { Source = item };
        BindingOperations.SetBinding(translateTransform, TranslateTransform.YProperty, translateYBinding);

        // Assign the transform to the Image
        image.RenderTransform = translateTransform;
        // Binding for ToolTip
        Binding toolTipBinding = new(path: "ToolTip") { Source = item };
        image.SetBinding(Image.ToolTipProperty, toolTipBinding);

        Canvas.Children.Add(image);
        item.ConnectImage(image);
        item.SetVisible(true);
      }
      return SectorMapHelper.InternalSizeKm;
    }

    public override void Remove(Canvas canvas)
    {
      if (Grid != null)
      {
        canvas.Children.Remove(Grid);
      }
    }

    public override void Update(GalaxyMapViewer map)
    {
      if (Cluster == null || Sector == null || Canvas == null || Hexagon == null || Grid == null || TextBlock == null)
      {
        return;
      }
      HexagonWidth = map.HexagonWidth;
      HexagonHeight = map.HexagonHeight;
      ScaleFactor = map.ScaleFactor;
      SectorMapHelper.SetInternalSize(map.SectorRadius);
      UpdatePoints();
      if (Sector.Color != null)
      {
        SolidColorBrush brush = new(
          Color.FromArgb(
            (byte)(Sector.Color.Alpha * map.MapColorsOpacity),
            (byte)Sector.Color.Red,
            (byte)Sector.Color.Green,
            (byte)Sector.Color.Blue
          )
        );
        Hexagon.Fill = brush;
      }
      Hexagon.Points = Points;
      // Update TextBlock
      TextBlock.FontSize = SetFontSize();
      Grid.Width = Width;
      Grid.Height = Height;
      // Position the Hexagon on the Canvas
      bool isVisible = map == null || map.IsVisibleBySource(Sector.Source);
      Grid.Visibility = isVisible ? Visibility.Visible : Visibility.Hidden;
      Canvas.SetLeft(Grid, X);
      Canvas.SetTop(Grid, Y);
      SectorMapHelper.VisualX = X;
      SectorMapHelper.VisualY = Y;
      SectorMapHelper.VisualSizePx = Width;
      foreach (SectorMapItem item in SectorMapHelper.Items)
      {
        item.Update();
        item.SetVisible(isVisible);
      }
    }

    private double SetFontSize()
    {
      return Math.Min(Height * FrontSizeProportion, FrontSizeMax);
    }
  }

  class HexagonCornersTriplet(HexagonCorner top, HexagonCorner center, HexagonCorner bottom)
  {
    public HexagonCorner Top = top;
    public HexagonCorner Center = center;
    public HexagonCorner Bottom = bottom;

    public static HexagonCorner GetCornerByTwoOther(HexagonCorner corner1, HexagonCorner corner2, List<HexagonCornersTriplet> triplets)
    {
      foreach (HexagonCornersTriplet triplet in triplets)
      {
        if ((triplet.Top == corner1 && triplet.Center == corner2) || (triplet.Top == corner2 && triplet.Center == corner1))
        {
          return triplet.Bottom;
        }
        else if ((triplet.Center == corner1 && triplet.Bottom == corner2) || (triplet.Center == corner2 && triplet.Bottom == corner1))
        {
          return triplet.Top;
        }
        else if ((triplet.Top == corner1 && triplet.Bottom == corner2) || (triplet.Top == corner2 && triplet.Bottom == corner1))
        {
          return triplet.Center;
        }
      }
      return HexagonCorner.Unknown;
    }
  }

  enum HexagonCorner
  {
    RightCenter,
    RightBottom,
    LeftBottom,
    LeftCenter,
    LeftTop,
    RightTop,
    Unknown,
  }
}
