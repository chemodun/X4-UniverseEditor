using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using Utilities.Logging;
using X4DataLoader;

namespace X4Map
{
  public class GalaxyMapViewer : ScrollViewer, INotifyPropertyChanged
  {
    public readonly Galaxy GalaxyData;
    public FactionColors FactionColors;
    private double _mapColorsOpacity;
    public double MapColorsOpacity
    {
      get => _mapColorsOpacity;
      set
      {
        if (_mapColorsOpacity != value)
        {
          _mapColorsOpacity = value;
          OnPropertyChanged(nameof(MapColorsOpacity));
          UpdateMap();
        }
      }
    }
    private readonly List<GalaxyMapCluster> _clusters = [];
    private readonly Canvas _galaxyCanvas;
    private const double HexagonWidthDefault = 100; // Width of the hexagon in pixels
    public static readonly double HexagonSizesRelation = Math.Sqrt(3) / 2; // Height of the hexagon in pixels (Width * sqrt(3)/2)
    private static readonly double HexagonHeightDefault = HexagonWidthDefault * HexagonSizesRelation; // Height of the hexagon in pixels (Width * sqrt(3)/2)
    private double _hexagonWidth = HexagonWidthDefault; // Width of the hexagon in pixels
    public double HexagonWidth
    {
      get => _hexagonWidth;
      set
      {
        _hexagonWidth = value;
        _hexagonHeight = value * HexagonSizesRelation;
        OnPropertyChanged(nameof(HexagonWidth));
        OnPropertyChanged(nameof(HexagonHeight));
        UpdateMap();
      }
    } // Width of the hexagon in pixel

    private double _hexagonHeight = HexagonWidthDefault * HexagonSizesRelation; // Height of the hexagon in pixels (Width * sqrt(3)/2)
    public double HexagonHeight
    {
      get => _hexagonHeight;
      set { }
    } // Height of the hexagon in pixels (Width * sqrt(3)/2)
    private double _hexagonWidthMinimal = HexagonWidthDefault;
    public double HexagonWidthMinimal
    {
      get => _hexagonWidthMinimal;
      set
      {
        _hexagonWidthMinimal = value;
        OnPropertyChanged(nameof(HexagonWidthMinimal));
      }
    }
    private double _hexagonWidthMaximal = HexagonWidthDefault;
    public double HexagonWidthMaximal
    {
      get => _hexagonWidthMaximal;
      set
      {
        _hexagonWidthMaximal = value;
        OnPropertyChanged(nameof(HexagonWidthMaximal));
      }
    }
    private int _sectorRadius = 400;
    public int SectorRadius
    {
      get => _sectorRadius;
      set
      {
        if (_sectorRadius != value)
        {
          _sectorRadius = value;
          OnPropertyChanged(nameof(SectorRadius));
          UpdateMap();
        }
      }
    }
    private const int ColumnWidth = 15000000; // 15,000,000 meters for horizontal (X) axis
    private const int RowHeight = 17320000 / 2; // 17,320,000 meters for vertical (Z) axis

    public double ScaleFactor = 0.001; // Scaling factor to convert units to pixels
    private double _canvasWidthBase = 0;
    private double _canvasHeightBase = 0;
    private int MinCol = 0;
    private int MaxCol = 0;

    private int MinRow = 0;
    private int MaxRow = 0;
    private double scrollHorizontalOffset = 0;
    private double canvasToScrollWidthDelta = 0;
    private double canvasToScrollHeightDelta = 0;
    private double scrollVerticalOffset = 0;
    private double canvasWidth = 0;

    private readonly List<GalaxyMapCell> MapCells = [];
    public List<SectorMapItem> SectorsItems = [];
    public ObservableCollection<MapOptions> DLCsOptions { get; set; } = [];
    public ObservableCollection<MapOptions> ModsOptions { get; set; } = [];
    public ObservableCollection<MapOptions> DeveloperOptions { get; set; } = [];
    public MapOptions ShowEmptyClusterPlaces = new("Show Empty Cluster Cells", "EmptyCell", false);

    private readonly List<GalaxyConnection> _extraConnections;
    private readonly Dictionary<string, List<ObjectInSector>>? _extraObjects;
    private readonly List<GalaxyMapInterConnection> InterConnections = [];

    public GalaxyMapViewer(
      Galaxy galaxy,
      FactionColors factionColors,
      double mapColorsOpacity,
      int sectorRadius,
      Dictionary<string, List<ObjectInSector>>? extraObjects = null,
      List<GalaxyConnection>? extraConnections = null
    )
    {
      GalaxyData = galaxy;
      FactionColors = factionColors;
      _mapColorsOpacity = mapColorsOpacity;
      _sectorRadius = sectorRadius;
      _extraObjects = extraObjects;
      _extraConnections = extraConnections ?? [];
      _galaxyCanvas = new Canvas { Background = Brushes.LightGray };
      AddChild(_galaxyCanvas);
      Content = this;
      foreach (string dlcId in Galaxy.DLCOrder)
      {
        ExtensionInfo? dlc = GalaxyData.DLCs.Find(dlc => dlc.Id == dlcId);
        if (dlc != null)
        {
          var extension = new MapOptions(dlc.Name, dlc.Id, true);
          extension.PropertyChanged += MapOptions_PropertyChanged;
          DLCsOptions.Add(extension);
        }
      }
      foreach (ExtensionInfo mod in GalaxyData.Mods)
      {
        var extension = new MapOptions(mod.Name, mod.Id, true);
        extension.PropertyChanged += MapOptions_PropertyChanged;
        ModsOptions.Add(extension);
      }
      if (!PrepareGalaxyMap())
      {
        Log.Error("Cluster map is not prepared.");
        return;
      }
      ScrollChanged += GalaxyMapViewer_ScrollChanged;
      SizeChanged += GalaxyMapViewer_SizeChanged;
    }

    private bool PrepareGalaxyMap()
    {
      if (GalaxyData == null)
      {
        Log.Error("Galaxy data is not loaded.");
        return false;
      }

      // Determine the rowId and columnId for each cluster and populate the dictionary
      foreach (var cluster in GalaxyData.Clusters)
      {
        int col = (int)(cluster.Position.X / ColumnWidth);
        int row = (int)(cluster.Position.Z / RowHeight);

        if (col < MinCol)
        {
          MinCol = col;
        }
        if (col > MaxCol)
        {
          MaxCol = col;
        }
        if (row < MinRow)
        {
          MinRow = row;
        }
        if (row > MaxRow)
        {
          MaxRow = row;
        }
        MapCells.Add(new GalaxyMapCell(cluster, col, row));
      }

      _canvasWidthBase = (MaxCol - MinCol + 1) * 0.75 + 0.25;
      _canvasHeightBase = (MaxRow - MinRow) * 0.5 + 1;
      return true;
    }

    private void CreateMap()
    {
      if (MapCells.Count == 0)
      {
        Log.Warn("Cluster map is empty.");
        return;
      }
      if (ActualHeight == 0 || ActualWidth == 0)
      {
        Log.Warn("GalaxyMapViewer size is zero.");
        return;
      }

      Width = _canvasWidthBase * HexagonWidth * ScaleFactor;
      Height = _canvasHeightBase * HexagonHeight * ScaleFactor;

      bool isMinColOdd = Math.Abs(MinCol) % 2 == 1;
      for (int row = MaxRow; row >= MinRow; row--)
      {
        bool isRowOdd = Math.Abs(row) % 2 == 1;
        int startCol = MinCol;
        if (isRowOdd)
        {
          startCol += isMinColOdd ? 0 : 1;
        }
        else
        {
          startCol += isMinColOdd ? 1 : 0;
        }
        for (int col = startCol; col <= MaxCol; col += 2)
        {
          Cluster? cluster = GalaxyMapCell.GetCluster(MapCells, col, row);
          GalaxyMapCluster clusterMapCluster = new(this, 0.75 * (col - MinCol), (MaxRow - row) * 0.5, _galaxyCanvas, cluster);
          clusterMapCluster.Create();
          _clusters.Add(clusterMapCluster);
          if (cluster == null)
          {
            continue;
          }
          if (cluster.Sectors.Count > 1 && cluster.Highways.Count > 0)
          {
            foreach (HighwayClusterLevel highway in cluster.Highways.Cast<HighwayClusterLevel>())
            {
              SectorMapItem? pointEntry = SectorsItems.Find(item => item.Id == highway?.EntryPointPath?.Zone?.Name);
              SectorMapItem? pointExit = SectorsItems.Find(item => item.Id == highway?.ExitPointPath?.Zone?.Name);
              if (pointEntry == null || pointExit == null)
              {
                continue;
              }
              GalaxyMapInterConnection galaxyMapHighway = new(pointEntry, pointExit, false);
              galaxyMapHighway.Create(_galaxyCanvas);
              InterConnections.Add(galaxyMapHighway);
            }
          }
        }
      }
      if (SectorsItems.Count > 0)
      {
        if (GalaxyData?.Connections.Count > 0)
        {
          foreach (GalaxyConnection connection in GalaxyData.Connections)
          {
            if (
              connection.PathDirect == null
              || connection.PathDirect.Gate == null
              || connection.PathDirect.Gate.Name == null
              || connection.PathOpposite == null
              || connection.PathOpposite.Gate == null
              || connection.PathOpposite.Gate.Name == null
            )
            {
              continue;
            }
            SectorMapItem? gateDirect = SectorsItems.Find(item => item.Id == connection.PathDirect.Gate.Name);
            SectorMapItem? gateOpposite = SectorsItems.Find(item => item.Id == connection.PathOpposite.Gate.Name);
            if (gateDirect == null || gateOpposite == null)
            {
              continue;
            }
            GalaxyMapInterConnection galaxyMapGateConnection = new(gateDirect, gateOpposite, true, connection.Source);
            galaxyMapGateConnection.Create(_galaxyCanvas);
            InterConnections.Add(galaxyMapGateConnection);
          }
        }
        foreach (GalaxyConnection connection in _extraConnections)
        {
          if (connection.Name == null)
          {
            continue;
          }
          List<SectorMapItem> extraGatesItems = SectorsItems.FindAll(item => item.Id == connection.Name);
          if (extraGatesItems.Count == 2)
          {
            GalaxyMapInterConnection galaxyMapGateConnection = new(extraGatesItems[0], extraGatesItems[1], true);
            galaxyMapGateConnection.Create(_galaxyCanvas);
            InterConnections.Add(galaxyMapGateConnection);
          }
        }
        List<SectorMapItem> newGatesItems = SectorsItems.FindAll(item => item.Id == SectorMap.NewGateId);
        if (newGatesItems.Count == 2)
        {
          GalaxyMapInterConnection galaxyMapGateConnection = new(newGatesItems[0], newGatesItems[1], true);
          galaxyMapGateConnection.Create(_galaxyCanvas);
          InterConnections.Add(galaxyMapGateConnection);
        }
      }
    }

    private void UpdateMap()
    {
      if (_clusters.Count == 0)
      {
        CreateMap();
      }
      else
      {
        scrollVerticalOffset = VerticalOffset;
        scrollHorizontalOffset = HorizontalOffset;
        canvasWidth = _galaxyCanvas.Width;
        _galaxyCanvas.Width = _canvasWidthBase * HexagonWidth * ScaleFactor;
        _galaxyCanvas.Height = _canvasHeightBase * HexagonHeight * ScaleFactor;
        foreach (GalaxyMapCluster cluster in _clusters)
        {
          cluster.Update();
        }
        foreach (GalaxyMapInterConnection connection in InterConnections)
        {
          connection.SetVisible(
            IsVisibleBySource(connection.SourceId)
              && IsVisibleBySource(connection.DirectSourceId)
              && IsVisibleBySource(connection.OppositeSourceId)
          );
        }
      }
    }

    private void GalaxyMapViewer_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      if (ActualWidth != 0 && ActualHeight != 0)
      {
        double width = ActualWidth;
        double height = ActualHeight;
        double scaleFactorWidth = width / _canvasWidthBase / HexagonWidthDefault;
        double scaleFactorHeight = height / _canvasHeightBase / HexagonHeightDefault;
        ScaleFactor = Math.Min(scaleFactorWidth, scaleFactorHeight);
        if (width * HexagonSizesRelation < height)
        {
          HexagonWidthMaximal = width / ScaleFactor * 2;
        }
        else
        {
          HexagonWidthMaximal = height / HexagonSizesRelation / ScaleFactor * 2;
        }
      }
      UpdateMap();
    }

    private void GalaxyMapViewer_ScrollChanged(object sender, RoutedEventArgs e)
    {
      double viewportWidth = ViewportWidth;
      double viewportHeight = ViewportHeight;
      double currentCanvasWidth = _galaxyCanvas.Width;
      double currentCanvasHeight = _galaxyCanvas.Height;
      if (currentCanvasWidth - viewportWidth != canvasWidth && viewportWidth != 0 && viewportWidth != ActualWidth)
      {
        if (currentCanvasWidth > viewportWidth)
        {
          double newCanvasToScrollWidthDelta = currentCanvasWidth - viewportWidth;
          if (newCanvasToScrollWidthDelta != canvasToScrollWidthDelta)
          {
            Log.Debug($"GalaxyCanvas.Width: {_galaxyCanvas.Width}, canvasWidth: {canvasWidth}");
            Log.Debug($"ActualWidth: {ActualWidth}, ViewportWidth: {viewportWidth}");
            if (scrollHorizontalOffset == 0 || canvasToScrollWidthDelta == 0)
            {
              Log.Debug($"Centering the map: new offsets: {newCanvasToScrollWidthDelta / 2}");
              ScrollToHorizontalOffset(newCanvasToScrollWidthDelta / 2);
            }
            else
            {
              Log.Debug(
                $"Scrolling the map: old offset: {scrollHorizontalOffset}, new offset: {scrollHorizontalOffset * newCanvasToScrollWidthDelta / canvasToScrollWidthDelta}"
              );
              ScrollToHorizontalOffset(scrollHorizontalOffset * newCanvasToScrollWidthDelta / canvasToScrollWidthDelta);
            }
            canvasToScrollWidthDelta = newCanvasToScrollWidthDelta;
          }
        }
        if (currentCanvasHeight > viewportHeight)
        {
          double newCanvasToScrollHeightDelta = currentCanvasHeight - viewportHeight;
          if (newCanvasToScrollHeightDelta != canvasToScrollHeightDelta)
          {
            Log.Debug($"ActualHeight: {ActualHeight}, ViewportHeight: {viewportHeight}");
            if (scrollVerticalOffset == 0 || canvasToScrollHeightDelta == 0)
            {
              Log.Debug($"Centering the map: new offsets: {newCanvasToScrollHeightDelta / 2}");
              ScrollToVerticalOffset(newCanvasToScrollHeightDelta / 2);
            }
            else
            {
              Log.Debug(
                $"Scrolling the map: old offset: {scrollVerticalOffset}, new offset: {scrollVerticalOffset * newCanvasToScrollHeightDelta / canvasToScrollHeightDelta}"
              );
              ScrollToVerticalOffset(scrollVerticalOffset * newCanvasToScrollHeightDelta / canvasToScrollHeightDelta);
            }
            canvasToScrollHeightDelta = newCanvasToScrollHeightDelta;
          }
        }
      }
    }

    private void MapOptions_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(MapOptions.IsChecked))
      {
        UpdateMap();
      }
    }

    public bool IsVisibleBySource(string id)
    {
      if (DLCsOptions.Any(extension => extension.Id == id))
      {
        return DLCsOptions.First(extension => extension.Id == id).IsChecked;
      }
      if (ModsOptions.Any(extension => extension.Id == id))
      {
        return ModsOptions.First(extension => extension.Id == id).IsChecked;
      }
      if (DeveloperOptions.Any(extension => extension.Id == id))
      {
        return DeveloperOptions.First(extension => extension.Id == id).IsChecked;
      }
      return true;
    }

    public double ReverseX(double x)
    {
      return (x / 0.75 + MinCol) * ColumnWidth;
    }

    public double ReverseZ(double z)
    {
      return (MaxRow - z * 2) * RowHeight;
    }

    public List<ObjectInSector>? GetExtraObjects(string sectorId)
    {
      if (_extraObjects == null)
      {
        return null;
      }
      if (_extraObjects.TryGetValue(sectorId, out List<ObjectInSector>? objects))
      {
        return objects;
      }
      return null;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }

  class GalaxyMapCell(Cluster cluster, int col, int row)
  {
    public int X { get; set; } = col;
    public int Z { get; set; } = row;
    public Cluster Cluster { get; set; } = cluster;

    public static Cluster? GetCluster(List<GalaxyMapCell> cells, int col, int row)
    {
      return cells.Find(cell => cell.X == col && cell.Z == row)?.Cluster;
    }
  }

  class GalaxyMapCluster(GalaxyMapViewer map, double x, double y, Canvas canvas, Cluster? cluster) : INotifyPropertyChanged
  {
    protected Cluster? Cluster = cluster;
    protected virtual double Modifier { get; set; } = 1.0;
    private readonly double _x = x;
    protected double X
    {
      get
      {
        if (Map != null)
        {
          return _x * Map.HexagonWidth * Map.ScaleFactor;
        }
        return 0;
      }
    }

    protected double OriginalX
    {
      get
      {
        if (Map != null)
        {
          return Map.ReverseX(_x);
        }
        return 0;
      }
    }

    protected double OriginalZ
    {
      get
      {
        if (Map != null)
        {
          return Map.ReverseZ(_y);
        }
        return 0;
      }
    }

    private static readonly string[] _toolTipItems = new[] { "Name", "Source", "Macro", "Coordinates", "X", "Y", "Z" };
    protected virtual string[] ToolTipItems
    {
      get => _toolTipItems;
    }

    private readonly double _y = y;
    protected double Y
    {
      get
      {
        if (Map != null)
        {
          return _y * Map.HexagonHeight * Map.ScaleFactor;
        }
        return 0;
      }
    }
    protected double Width
    {
      get
      {
        if (Map != null)
        {
          return Map.HexagonWidth * Map.ScaleFactor * Modifier;
        }
        return 0;
      }
    }
    protected double Height
    {
      get
      {
        if (Map != null)
        {
          return Map.HexagonHeight * Map.ScaleFactor * Modifier;
        }
        return 0;
      }
    }
    public string Source => Cluster?.Source ?? "EmptyCell";
    protected GalaxyMapViewer Map = map;
    protected Canvas? Canvas = canvas;
    protected PointCollection Points = [];
    protected Polygon? Hexagon = null;
    private readonly List<GalaxyMapSector> _sectors = [];

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

    public virtual void Create()
    {
      if (Map == null || Canvas == null)
      {
        return;
      }
      if (Cluster != null && Cluster.Sectors.Count == 1)
      {
        Sector sector = Cluster.Sectors[0];
        GalaxyMapSector clusterMapSector = new(Map, _x, _y, Canvas, Cluster, sector);
        clusterMapSector.Create();
        _sectors.Add(clusterMapSector);
      }
      else
      {
        UpdatePoints();
        // Log.Debug($"Creating cluster {Cluster.Name} at ({X}, {Y}) ({x}, {y}) with Points {string.Join(", ", Points.Select(p => $"({p.X}, {p.Y})"))})");
        Hexagon = new()
        {
          Stroke = Cluster != null ? Brushes.Black : Brushes.DarkGray,
          StrokeThickness = 1,
          Fill = Brushes.Transparent,
          Tag = Cluster != null ? Cluster.Name : "Empty Map Cell",
          DataContext = Cluster,
          Points = Points,
          ToolTip = Cluster != null ? ToolTipCreator(Cluster) : ToolTipCreator(null, null, new Position(OriginalX, 0, OriginalZ)),
        };
        if (Cluster == null)
        {
          Hexagon.StrokeDashArray = [2, 2]; // Dash pattern: 2 units dash, 2 units gap
          Hexagon.Visibility = Map.ShowEmptyClusterPlaces.IsChecked ? Visibility.Visible : Visibility.Hidden;
        }
        // Position the Hexagon on the Canvas
        Canvas.SetLeft(Hexagon, X);
        Canvas.SetTop(Hexagon, Y);
        Canvas.Children.Add(Hexagon);
        List<HexagonCorner> corners = [];
        List<double?> angles = [];
        if (Cluster != null)
        {
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
          foreach ((HexagonCorner, HexagonCorner) pair in HorizontalPairs)
          {
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
              Log.Debug($"Sector {Cluster.Sectors[index].Name}: Corner: {corners[index]}, Position: X = {x}, Y = {y}");
              GalaxyMapSector clusterMapSector = new(Map, x, y, Canvas, Cluster, Cluster.Sectors[index], true);
              clusterMapSector.Create();
              _sectors.Add(clusterMapSector);
            }
          }
        }
      }
    }

    protected Grid ToolTipCreator(Cluster? cluster, Sector? sector = null, Position? position = null)
    {
      Grid toolTipGrid = new()
      {
        MinWidth = 200,
        MinHeight = 100,
        Background = Brushes.LightGray,
      };
      if (sector != null && Map != null)
      {
        SolidColorBrush? brush = Map.FactionColors.GetBrush(sector.DominantOwner);
        if (brush != null)
        {
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
        switch (toolTipItem)
        {
          case "Cluster":
            isSector = false;
            break;
          case "Name":
            labelStr =
              sector != null && isSector ? $"Sector: {sector.Name}" : (cluster != null ? $"Cluster: {cluster.Name}" : "Empty Cluster Cell");
            break;
          case "Source":
            if (sector != null && isSector)
            {
              labelStr = "Source:";
              textStr = sector.Source;
            }
            else if (cluster != null)
            {
              labelStr = "Source:";
              textStr = cluster.Source;
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
          if (!string.IsNullOrEmpty(labelStr))
          {
            TextBlock label = new()
            {
              Text = labelStr,
              FontWeight = FontWeights.DemiBold,
              Background = Brushes.Transparent,
              VerticalAlignment = VerticalAlignment.Center,
              HorizontalAlignment = HorizontalAlignment.Stretch,
              TextAlignment = TextAlignment.Left,
              Margin = new Thickness(5, 0, 5, 0),
              FontSize = 10,
            };
            Grid.SetRow(label, row);
            Grid.SetColumn(label, 0);
            if (string.IsNullOrEmpty(textStr))
            {
              Grid.SetColumnSpan(label, 2);
              label.TextAlignment = TextAlignment.Center;
            }
            toolTipGrid.Children.Add(label);
          }
          if (!string.IsNullOrEmpty(textStr))
          {
            TextBlock text = new()
            {
              Text = textStr,
              Background = Brushes.Transparent,
              VerticalAlignment = VerticalAlignment.Center,
              HorizontalAlignment = HorizontalAlignment.Stretch,
              TextAlignment = alignRight ? TextAlignment.Right : TextAlignment.Left,
              Margin = new Thickness(5, 0, 5, 0),
              FontSize = 10,
            };
            Grid.SetRow(text, row);
            Grid.SetColumn(text, 1);
            toolTipGrid.Children.Add(text);
          }
        }
      }
      return toolTipGrid;
    }

    public virtual void Update()
    {
      if (Canvas == null)
      {
        return;
      }
      if (Hexagon != null)
      {
        UpdatePoints();
        Hexagon.Points = Points;
        // Position the Hexagon on the Canvas
        Canvas.SetLeft(Hexagon, X);
        Canvas.SetTop(Hexagon, Y);
        if (Map != null)
        {
          Hexagon.Visibility = Map.IsVisibleBySource(Source) ? Visibility.Visible : Visibility.Hidden;
        }
      }
      foreach (GalaxyMapSector sector in _sectors)
      {
        sector.Update();
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

  class GalaxyMapSector(GalaxyMapViewer map, double x, double y, Canvas canvas, Cluster cluster, Sector sector, bool isHalf = false)
    : GalaxyMapCluster(map, x, y, canvas, cluster)
  {
    protected override double Modifier { get; set; } = isHalf ? 0.5 : 1;
    protected Sector? Sector = sector;
    protected Grid? Grid = null;
    protected TextBlock? TextBlock = null;
    private readonly SectorMap SectorMapHelper = new();
    private readonly double FrontSizeProportion = 0.12;
    private readonly double FrontSizeMax = 22;

    private static readonly string[] _toolTipItems = new[]
    {
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
    };

    protected override string[] ToolTipItems => _toolTipItems.Concat(base.ToolTipItems).ToArray(); /* Implement setter if needed, or leave it empty if not applicable */

    public override void Create()
    {
      if (Cluster == null || Sector == null || Canvas == null || Map == null)
      {
        return;
      }
      UpdatePoints();
      Hexagon = new()
      {
        Stroke = Brushes.Black,
        StrokeThickness = 1,
        Fill = Brushes.LightGray,
        Tag = Sector.Name,
        DataContext = Sector,
        Points = Points,
        ToolTip = ToolTipCreator(Cluster, Sector),
      };
      SolidColorBrush? brush = Map.FactionColors.GetBrush(Sector.DominantOwner);
      if (brush != null)
      {
        Hexagon.Fill = brush;
        Hexagon.Fill.Opacity = Map.MapColorsOpacity;
      }
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
      SectorMapHelper.InternalSizeKm = Map.SectorRadius;
      SectorMapHelper.ItemSizeMinPx = 4;
      SectorMapHelper.SetColors(Map.FactionColors);
      SectorMapHelper.SetSector(Sector, Map.GalaxyData);
      List<ObjectInSector>? extraObjects = Map.GetExtraObjects(Sector.Macro);
      if (extraObjects != null)
      {
        foreach (ObjectInSector modObject in extraObjects)
        {
          SectorMapHelper.AddItem(modObject);
        }
      }
      foreach (SectorMapItem item in SectorMapHelper.Items)
      {
        Map.SectorsItems.Add(item);
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
    }

    public override void Update()
    {
      if (Cluster == null || Sector == null || Canvas == null || Hexagon == null || Grid == null || TextBlock == null)
      {
        return;
      }
      UpdatePoints();
      Hexagon.Points = Points;
      // Update TextBox
      TextBlock.FontSize = SetFontSize();
      Grid.Width = Width;
      Grid.Height = Height;
      // Position the Hexagon on the Canvas
      bool isVisible = Map == null || Map.IsVisibleBySource(Sector.Source);
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

  public class GalaxyMapInterConnection : INotifyPropertyChanged
  {
    private SectorMapItem? _directItem;
    public SectorMapItem? DirectItem
    {
      get => _directItem;
      set
      {
        _directItem = value;
        OnPropertyChanged(nameof(DirectItem));
      }
    }
    private SectorMapItem? _oppositeItem;
    public SectorMapItem? OppositeItem
    {
      get => _oppositeItem;
      set
      {
        _oppositeItem = value;
        OnPropertyChanged(nameof(OppositeItem));
      }
    }

    public string SourceId = "unknown";
    public string DirectSourceId
    {
      get { return _directItem?.Source ?? "unknown"; }
    }
    public string OppositeSourceId
    {
      get { return _oppositeItem?.Source ?? "unknown"; }
    }

    private readonly bool IsGate = true;
    private Line? Line = null;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public GalaxyMapInterConnection(SectorMapItem gateDirect, SectorMapItem gateOpposite, bool isGate = true, string sourceId = "unknown")
    {
      DirectItem = gateDirect;
      OppositeItem = gateOpposite;
      IsGate = isGate;
      SourceId = sourceId;
    }

    public void Create(Canvas canvas)
    {
      if (DirectItem == null || OppositeItem == null)
      {
        return;
      }

      Line line = new() { DataContext = this, StrokeThickness = IsGate ? 2 : 1 };
      if (IsGate)
      {
        if (DirectItem.Status == "active" && OppositeItem.Status == "active")
        {
          line.Stroke = DirectItem.From switch
          {
            "new" => Brushes.Green,
            "mod" => Brushes.DarkOrange,
            _ => Brushes.Gold,
          };
        }
        else
        {
          line.Stroke = Brushes.DarkGray;
        }
      }
      else
      {
        line.Stroke = Brushes.SkyBlue;
      }
      Binding x1Binding = new(path: "CenterX") { Source = DirectItem };
      line.SetBinding(Line.X1Property, x1Binding);
      Binding y1Binding = new(path: "CenterY") { Source = DirectItem };
      line.SetBinding(Line.Y1Property, y1Binding);
      Binding x2Binding = new(path: "CenterX") { Source = OppositeItem };
      line.SetBinding(Line.X2Property, x2Binding);
      Binding y2Binding = new(path: "CenterY") { Source = OppositeItem };
      line.SetBinding(Line.Y2Property, y2Binding);
      // Canvas.SetZIndex(line, -1);
      canvas.Children.Add(line);
      Line = line;
    }

    public void SetVisible(bool isVisible)
    {
      if (Line != null)
      {
        Line.Visibility = isVisible ? Visibility.Visible : Visibility.Hidden;
      }
    }
  }

  public class MapOptions : INotifyPropertyChanged
  {
    private string _name = string.Empty;
    private string _id = string.Empty;
    private bool _isChecked = false;

    public MapOptions(string name, string id, bool isChecked)
    {
      Name = name;
      Id = id;
      IsChecked = isChecked;
    }

    public string Name
    {
      get => _name;
      set
      {
        if (_name != value)
        {
          _name = value;
          OnPropertyChanged(nameof(Name));
        }
      }
    }

    public string Id
    {
      get => _id;
      set
      {
        if (_id != value)
        {
          _id = value;
          OnPropertyChanged(nameof(Id));
        }
      }
    }

    public bool IsChecked
    {
      get => _isChecked;
      set
      {
        if (_isChecked != value)
        {
          _isChecked = value;
          OnPropertyChanged(nameof(IsChecked));
        }
      }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
