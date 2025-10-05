using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Security;
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
  public class GalaxyMapCluster(
    double x,
    double y,
    MapPosition mapPosition,
    Canvas canvas,
    Cluster? cluster,
    Position? position,
    double hexagonWidth,
    double hexagonHeight,
    double scaleFactor
  ) : INotifyPropertyChanged
  {
    public Cluster? Cluster = cluster;
    public virtual string Macro => Cluster?.Macro ?? "";
    protected virtual double Modifier { get; set; } = 1.0;
    protected virtual double HexagonWidth { get; set; } = hexagonWidth;
    protected virtual double HexagonHeight { get; set; } = hexagonHeight;
    protected virtual double ScaleFactor { get; set; } = scaleFactor;

    private readonly double _x = x;
    protected double X
    {
      get { return _x * HexagonWidth * ScaleFactor; }
    }
    private readonly double _y = y;
    protected double Y
    {
      get { return _y * HexagonHeight * ScaleFactor; }
    }

    public virtual Position Position { get; set; } = cluster?.Position ?? position ?? new();
    public double OriginalX
    {
      get
      {
        if (Position != null)
        {
          return Position.X;
        }
        return 0;
      }
    }

    public double OriginalY
    {
      get
      {
        if (Position != null)
        {
          return Position.Y;
        }
        return 0;
      }
    }

    public double OriginalZ
    {
      get
      {
        if (Position != null)
        {
          return Position.Z;
        }
        return 0;
      }
    }

    public MapPosition MapPosition { get; set; } = mapPosition;

    private static readonly string[] _toolTipItems = ["Name", "Source", "Macro", "Coordinates", "X", "Y", "Z", "Column", "Row"];
    protected virtual string[] ToolTipItems
    {
      get => _toolTipItems;
    }

    protected double Width
    {
      get { return HexagonWidth * ScaleFactor * Modifier; }
    }
    protected double Height
    {
      get { return HexagonHeight * ScaleFactor * Modifier; }
    }
    public string Source => Cluster?.Source ?? "EmptyCell";
    protected Canvas? Canvas = canvas;
    protected PointCollection Points = [];
    public Polygon? Hexagon { get; protected set; } = null;
    public readonly List<GalaxyMapSector> Sectors = [];

    private static readonly List<HexagonCornersTriplet> Triplets =
    [
      new HexagonCornersTriplet(HexagonCorner.LeftTop, HexagonCorner.RightCenter, HexagonCorner.LeftBottom),
      new HexagonCornersTriplet(HexagonCorner.RightTop, HexagonCorner.LeftCenter, HexagonCorner.RightBottom),
    ];

    private static readonly List<(HexagonCorner, HexagonCorner)> HorizontalPairs =
    [
      (HexagonCorner.LeftTop, HexagonCorner.RightTop),
      (HexagonCorner.LeftCenter, HexagonCorner.RightCenter),
      (HexagonCorner.LeftBottom, HexagonCorner.RightBottom),
    ];

    protected Brush DefaultStroke
    {
      get
      {
        if (Cluster == null)
        {
          return GalaxyMapViewer.BrushClusterEmpty;
        }
        return Cluster.Source == "New" ? Brushes.DarkGreen : GalaxyMapViewer.BrushClusterDefault;
      }
    }

    public virtual double Create(GalaxyMapViewer map)
    {
      double maxInternalSizeKm = 0;
      if (map == null || Canvas == null)
      {
        return maxInternalSizeKm;
      }
      // Sync geometry with current map zoom/layout
      HexagonWidth = map.HexagonWidth;
      HexagonHeight = map.HexagonHeight;
      ScaleFactor = map.ScaleFactor;
      if (Cluster != null && Cluster.Sectors.Count == 1)
      {
        Sector sector = Cluster.Sectors[0];
        Log.Debug(
          $"Col: {MapPosition.Column, 3} Row: {MapPosition.Row, 3}: Creating a single Sector '{sector.Name}' in Cluster '({Cluster.Name})'"
        );
        GalaxyMapSector clusterMapSector = map.CreateMapSector(
          _x,
          _y,
          this,
          Canvas,
          Cluster,
          sector,
          HexagonWidth,
          HexagonHeight,
          ScaleFactor
        );
        maxInternalSizeKm = clusterMapSector.Create(map);
        Sectors.Add(clusterMapSector);
      }
      else
      {
        UpdatePoints();
        Log.Debug(
          $"Col: {MapPosition.Column, 3} Row: {MapPosition.Row, 3}: Creating a {(Cluster != null ? "Cluster '" + Cluster.Name + "'" : "'Empty Map Cell'")}"
        );
        Hexagon = new()
        {
          Stroke = Cluster != null ? GalaxyMapViewer.BrushClusterDefault : GalaxyMapViewer.BrushClusterEmpty,
          StrokeThickness = 1,
          Fill = Brushes.Transparent,
          Tag = Cluster != null ? Cluster.Name : "Empty Map Cell",
          DataContext = Cluster == null ? this : Cluster,
          Points = Points,
          ToolTip =
            Cluster != null
              ? ToolTipCreator(Cluster, null, null, MapPosition)
              : ToolTipCreator(null, null, new Position(OriginalX, 0, OriginalZ), MapPosition),
        };
        if (Cluster == null)
        {
          Hexagon.StrokeDashArray = [2, 2]; // Dash pattern: 2 units dash, 2 units gap
          Hexagon.Visibility = map.ShowEmptyClusterPlaces.IsChecked ? Visibility.Visible : Visibility.Hidden;
        }
        // Position the Hexagon on the Canvas
        Canvas.SetLeft(Hexagon, X);
        Canvas.SetTop(Hexagon, Y);
        Canvas.Children.Add(Hexagon);
        List<HexagonCorner> corners = [];
        List<double?> angles = [];
        if (Cluster != null)
        {
          Log.Debug($"Cluster '{Cluster.Name}' has {Cluster.Sectors.Count} sectors");
          for (int i = 0; i < Cluster.Sectors.Count; i++)
          {
            double? angle = null;
            Sector sector = Cluster.Sectors[i];
            if (i == 0)
            {
              corners.Add(HexagonCorner.Unknown);
              angles.Add(0);
            }
            else
            {
              angle = Math.Atan2(sector.Position.Z, sector.Position.X) * (180 / Math.PI);
              angles.Add(angle);
              if (angle <= 30 && angle > -30)
              {
                corners.Add(HexagonCorner.RightCenter);
              }
              else if (angle <= -30 && angle > -90)
              {
                corners.Add(HexagonCorner.RightBottom);
              }
              else if (angle <= -90 && angle > -120)
              {
                corners.Add(HexagonCorner.LeftBottom);
              }
              else if (angle <= -120 || angle > 120)
              {
                corners.Add(HexagonCorner.LeftCenter);
              }
              else if (angle <= 120 && angle > 90)
              {
                corners.Add(HexagonCorner.LeftTop);
              }
              else if (angle <= 90 && angle > 30)
              {
                corners.Add(HexagonCorner.RightTop);
              }
            }
            Log.Debug(
              $"Sector {sector.Name}  with Position: X = {sector.Position.X}, Y = {sector.Position.Y}, Z = {sector.Position.Z}. Angle: {angle}, Corner: {corners[i]}"
            );
          }
          Log.Debug($"Angles({angles.Count}): {string.Join(", ", angles)}, Corners({corners.Count}): {string.Join(", ", corners)}");
          if (corners.Count == 3)
          {
            corners[0] = HexagonCornersTriplet.GetCornerByTwoOther(corners[1], corners[2], Triplets);
          }
          else if (corners.Count == 2)
          {
            if (angles[1] < 90 && angles[1] > 0)
            {
              corners[0] = HexagonCorner.LeftBottom;
              corners[1] = HexagonCorner.RightTop;
            }
            else if (angles[1] < 0 && angles[1] > -90)
            {
              corners[0] = HexagonCorner.LeftTop;
              corners[1] = HexagonCorner.RightBottom;
            }
            else if (angles[1] < -90 && angles[1] > -180)
            {
              corners[0] = HexagonCorner.RightTop;
              corners[1] = HexagonCorner.LeftBottom;
            }
            else if (angles[1] < 180 && angles[1] > 90)
            {
              corners[0] = HexagonCorner.RightBottom;
              corners[1] = HexagonCorner.LeftTop;
            }
            else if (angles[1] == 90)
            {
              switch (Cluster.Macro)
              {
                case "Cluster_32_macro": // Tharka's Cascade
                  corners[0] = HexagonCorner.RightBottom;
                  corners[1] = HexagonCorner.LeftTop;
                  break;
              }
            }
            else if (angles[1] == 0)
            {
              switch (Cluster.Macro)
              {
                case "Cluster_15_macro": // Ianamus Zura
                case "Cluster_19_macro": // Hewa's Twin I & II
                  corners[0] = HexagonCorner.LeftBottom;
                  corners[1] = HexagonCorner.RightTop;
                  break;
                case "Cluster_42_macro": // Hewa's Twin III & IV
                  corners[0] = HexagonCorner.LeftTop;
                  corners[1] = HexagonCorner.RightBottom;
                  break;
              }
            }
            else if (angles[1] == -90)
            {
              switch (Cluster.Macro)
              {
                case "Cluster_25_macro": // Faulty Logic
                case "Cluster_112_macro": // Savage Spur
                  corners[0] = HexagonCorner.RightTop;
                  corners[1] = HexagonCorner.LeftBottom;
                  break;
                case "Cluster_104_macro":
                  corners[0] = HexagonCorner.LeftTop; // Earth
                  corners[1] = HexagonCorner.RightBottom; // Moon
                  break;
              }
            }
            else if (angles[1] == -180) { }
          }
          Log.Debug($"Final Corners({corners.Count}): {string.Join(", ", corners)}");
          foreach ((HexagonCorner, HexagonCorner) pair in HorizontalPairs)
          {
            Log.Debug($"Checking pair: {pair.Item1} - {pair.Item2}");
            if (corners.Contains(pair.Item1) || corners.Contains(pair.Item2))
            {
              int index = corners.Contains(pair.Item1) ? corners.IndexOf(pair.Item1) : corners.IndexOf(pair.Item2);
              double x = 0;
              double y = 0;
              switch (corners[index])
              {
                case HexagonCorner.RightCenter:
                  x = _x + 0.5;
                  y = _y + 0.25;
                  break;
                case HexagonCorner.RightBottom:
                  x = _x + 0.375;
                  y = _y + 0.5;
                  break;
                case HexagonCorner.LeftBottom:
                  x = _x + 0.125;
                  y = _y + 0.5;
                  break;
                case HexagonCorner.LeftCenter:
                  x = _x;
                  y = _y + 0.25;
                  break;
                case HexagonCorner.LeftTop:
                  x = _x + 0.125;
                  y = _y;
                  break;
                case HexagonCorner.RightTop:
                  x = _x + 0.375;
                  y = _y;
                  break;
              }
              Log.Debug($"Sector({index}) {Cluster.Sectors[index].Name}: Corner: {corners[index]}, Position: X = {x}, Y = {y}");
              GalaxyMapSector clusterMapSector = map.CreateMapSector(
                x,
                y,
                this,
                Canvas,
                Cluster,
                Cluster.Sectors[index],
                HexagonWidth,
                HexagonHeight,
                ScaleFactor,
                true
              );
              double internalSizeKm = clusterMapSector.Create(map);
              if (internalSizeKm > maxInternalSizeKm)
              {
                maxInternalSizeKm = internalSizeKm;
              }
              Sectors.Add(clusterMapSector);
            }
            else
            {
              Log.Debug($"Pair {pair.Item1} - {pair.Item2} not found in corners");
            }
          }
        }
      }
      return maxInternalSizeKm; // Return the maximum internal size of the sectors in the cluster
    }

    public virtual void ReAssign(GalaxyMapViewer map, Cluster? cluster)
    {
      Cluster = cluster;
      // Safeguard: ensure geometry matches current map zoom/layout even when we don't recreate visuals
      HexagonWidth = map.HexagonWidth;
      HexagonHeight = map.HexagonHeight;
      ScaleFactor = map.ScaleFactor;
      if (Hexagon != null)
      {
        Hexagon.ToolTip =
          Cluster != null
            ? ToolTipCreator(Cluster, null, null, MapPosition)
            : ToolTipCreator(null, null, new Position(OriginalX, 0, OriginalZ), MapPosition);
        if (Cluster == null)
        {
          Hexagon.StrokeDashArray = [2, 2]; // Dash pattern: 2 units dash, 2 units gap
          Hexagon.Visibility = map.ShowEmptyClusterPlaces.IsChecked ? Visibility.Visible : Visibility.Hidden;
        }
        else
        {
          Hexagon.StrokeDashArray = null;
          Hexagon.Visibility = Visibility.Visible;
        }
        Hexagon.Stroke = Cluster != null ? (Cluster.Source == "New" ? Brushes.DarkGreen : Brushes.Black) : Brushes.DarkGray;
        Hexagon.Tag = Cluster != null ? Cluster.Name : "Empty Map Cell";
        Hexagon.DataContext = Cluster == null ? this : Cluster;

        // Refresh polygon points and position immediately to reflect current geometry
        UpdatePoints();
        Hexagon.Points = Points;
        if (Canvas != null)
        {
          Canvas.SetLeft(Hexagon, X);
          Canvas.SetTop(Hexagon, Y);
        }
      }
    }

    public virtual void Clear(GalaxyMapViewer map, Canvas canvas)
    {
      if (Hexagon != null)
      {
        canvas.Children.Remove(Hexagon);
        Hexagon = null;
      }

      foreach (GalaxyMapSector sector in Sectors)
      {
        sector.Clear(map, canvas);
      }
    }

    protected Grid ToolTipCreator(Cluster? cluster, Sector? sector = null, Position? position = null, MapPosition? mapPosition = null)
    {
      Grid toolTipGrid = new()
      {
        MinWidth = 200,
        MinHeight = 100,
        Background = Brushes.LightGray,
      };
      if (sector != null)
      {
        if (sector.Color != null)
        {
          SolidColorBrush brush = new(
            Color.FromArgb((byte)(sector.Color.Alpha), (byte)sector.Color.Red, (byte)sector.Color.Green, (byte)sector.Color.Blue)
          );
          toolTipGrid.Background = brush;
        }
      }
      toolTipGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(100) });
      toolTipGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
      bool isSector = sector != null;
      foreach (string toolTipItem in ToolTipItems)
      {
        if (isSector && sector == null)
        {
          if (toolTipItem != "Cluster")
          {
            continue;
          }
        }
        string labelStr = string.Empty;
        string textStr = string.Empty;
        bool alignRight = false;
        bool centered = false;
        Brush foreground = Brushes.Black;
        if (!isSector && cluster != null && cluster.Source == "New")
        {
          foreground = Brushes.DarkGreen;
        }
        switch (toolTipItem)
        {
          case "Cluster":
            isSector = false;
            break;
          case "Name":
            if (sector != null && isSector)
            {
              labelStr = "Sector:";
              textStr = sector.Name;
              centered = true;
            }
            else if (cluster != null)
            {
              labelStr = "Cluster:";
              textStr = cluster.Name;
              centered = true;
            }
            else
            {
              labelStr = "Empty Cluster Cell";
            }
            break;
          case "Source":
            if (sector != null && isSector)
            {
              labelStr = "Source:";
              textStr = $"{sector.SourceName} ({sector.Source})";
            }
            else if (cluster != null)
            {
              labelStr = "Source:";
              textStr = $"{cluster.SourceName} ({cluster.Source})";
            }
            break;
          case "Coordinates":
            labelStr = "Coordinates:";
            break;
          case "X":
          case "Y":
          case "Z":
            labelStr = $"{toolTipItem}:";
            if (sector != null && isSector)
            {
              position = sector.Position;
            }
            else if (cluster != null)
            {
              position = cluster.Position;
            }
            else
              position ??= new Position();
            textStr = toolTipItem switch
            {
              "X" => position.X.ToString("N0"),
              "Y" => position.Y.ToString("N0"),
              "Z" => position.Z.ToString("N0"),
              _ => string.Empty,
            };
            alignRight = true;
            break;
          case "Column":
            if (!isSector && mapPosition != null)
            {
              labelStr = "Column:";
              textStr = mapPosition.Column.ToString();
              alignRight = true;
            }
            break;
          case "Row":
            if (!isSector && mapPosition != null)
            {
              labelStr = "Row:";
              textStr = mapPosition.Row.ToString();
              alignRight = true;
            }
            break;
          case "Macro":
            if (sector != null && isSector)
            {
              labelStr = "Macro:";
              textStr = sector.Macro;
            }
            else if (cluster != null)
            {
              labelStr = "Macro:";
              textStr = cluster.Macro;
            }
            break;
          case "Owner":
            if (sector != null && isSector)
            {
              labelStr = "Owner:";
              textStr = sector.DominantOwnerFaction == null ? "Contested" : sector.DominantOwnerFaction.Name;
            }
            break;
          case "Sunlight":
            if (sector != null && isSector)
            {
              labelStr = "Sunlight:";
              textStr = $"{sector.Sunlight:F2}";
            }
            break;
          case "Economy":
            if (sector != null && isSector)
            {
              labelStr = "Economy:";
              textStr = $"{sector.Economy:F2}";
            }
            break;
          case "Security":
            if (sector != null && isSector)
            {
              labelStr = "Security:";
              textStr = $"{sector.Security:F2}";
            }
            break;
          default:
            break;
        }
        if (!string.IsNullOrEmpty(labelStr) || !string.IsNullOrEmpty(textStr))
        {
          toolTipGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
          int row = toolTipGrid.RowDefinitions.Count - 1;
          StackPanel? stackPanel = null;
          if (centered)
          {
            stackPanel = new()
            {
              Orientation = Orientation.Horizontal,
              HorizontalAlignment = HorizontalAlignment.Center,
              VerticalAlignment = VerticalAlignment.Center,
            };
          }
          if (!string.IsNullOrEmpty(labelStr))
          {
            TextBlock label = new()
            {
              Text = labelStr,
              FontWeight = FontWeights.DemiBold,
              Background = Brushes.Transparent,
              Foreground = Brushes.Black,
              VerticalAlignment = VerticalAlignment.Center,
              HorizontalAlignment = HorizontalAlignment.Stretch,
              TextAlignment = TextAlignment.Left,
              Margin = new Thickness(5, 0, 5, 0),
              FontSize = 10,
            };
            if (centered)
            {
              stackPanel?.Children.Add(label);
            }
            else
            {
              Grid.SetRow(label, row);
              Grid.SetColumn(label, 0);
              if (string.IsNullOrEmpty(textStr))
              {
                Grid.SetColumnSpan(label, 2);
                label.TextAlignment = TextAlignment.Center;
              }
              toolTipGrid.Children.Add(label);
            }
          }
          if (!string.IsNullOrEmpty(textStr))
          {
            TextBlock text = new()
            {
              Text = textStr,
              Background = Brushes.Transparent,
              Foreground = foreground,
              VerticalAlignment = VerticalAlignment.Center,
              HorizontalAlignment = HorizontalAlignment.Stretch,
              TextAlignment = alignRight ? TextAlignment.Right : TextAlignment.Left,
              Margin = new Thickness(5, 0, 5, 0),
              FontSize = 10,
            };
            if (centered)
            {
              stackPanel?.Children.Add(text);
              Grid.SetRow(stackPanel, row);
              Grid.SetColumn(stackPanel, 0);
              Grid.SetColumnSpan(stackPanel, 2);
              toolTipGrid.Children.Add(stackPanel);
            }
            else
            {
              Grid.SetRow(text, row);
              Grid.SetColumn(text, 1);
              toolTipGrid.Children.Add(text);
            }
          }
        }
      }
      return toolTipGrid;
    }

    public virtual void Select(bool isSelected)
    {
      if (Hexagon != null)
      {
        Hexagon.StrokeThickness = isSelected ? 2 : 1;
      }
    }

    public virtual void Update(GalaxyMapViewer map)
    {
      if (Canvas == null)
      {
        return;
      }
      if (Hexagon != null)
      {
        HexagonWidth = map.HexagonWidth;
        HexagonHeight = map.HexagonHeight;
        ScaleFactor = map.ScaleFactor;
        UpdatePoints();
        Hexagon.Points = Points;
        // Position the Hexagon on the Canvas
        Canvas.SetLeft(Hexagon, X);
        Canvas.SetTop(Hexagon, Y);
        Hexagon.Visibility = map.IsVisibleBySource(Source) ? Visibility.Visible : Visibility.Hidden;
      }
      foreach (GalaxyMapSector sector in Sectors)
      {
        sector.Update(map);
      }
    }

    protected void UpdatePoints()
    {
      Points.Clear();
      Points.Add(new Point(Width * 0.25, y: 0));
      Points.Add(new Point(Width * 0.75, y: 0));
      Points.Add(new Point(Width, Height * 0.5));
      Points.Add(new Point(Width * 0.75, Height));
      Points.Add(new Point(Width * 0.25, Height));
      Points.Add(new Point(x: 0, Height * 0.5));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
