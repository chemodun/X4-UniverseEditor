using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Utilities.Logging;
using X4DataLoader;

namespace ChemGateBuilder
{
  public partial class GalaxyMapWindow : Window, INotifyPropertyChanged
  {
    // Constants for hexagon dimensions and scaling
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

    private const double ColumnWidth = 15000000; // 15,000,000 meters for horizontal (X) axis
    private const double RowHeight = 17320000; // 17,320,000 meters for vertical (Z) axis

    public double ScaleFactor = 0.001; // Scaling factor to convert units to pixels
    private double _canvasWidthBase = 0;
    private double _canvasHeightBase = 0;
    private readonly Dictionary<string, Dictionary<string, Cluster>> MapDict = [];
    private readonly List<double> AxisZ = [];
    private readonly List<int> AxisX = [];
    private int MinCol = 0;
    private double MaxRow = 0;
    private readonly CollectionViewSource? SectorsList = null;

    // Reference to the main window's size (assumed to be passed or accessible)
    public readonly MainWindow MainWindowReference;
    public readonly Galaxy? Galaxy;

    private readonly List<GalaxyMapCluster> _clusters = [];

    public Sector? SelectedSector = null;

    // Fields to track panning state
    private bool isPanning = false;
    private Point panStartPoint = new();
    private double scrollHorizontalOffset = 0;
    private double canvasToScrollWidthDelta = 0;
    private double canvasToScrollHeightDelta = 0;
    private double scrollVerticalOffset = 0;
    private double canvasWidth = 0;

    // private readonly Sector? clickedSector = null;
    public List<SectorMapItem> SectorsItems = [];

    public ObservableCollection<ExtensionOnMap> EnabledDLCs { get; set; } = [];
    public ObservableCollection<ExtensionOnMap> EnabledMods { get; set; } = [];

    public GalaxyMapWindow(MainWindow mainWindow, CollectionViewSource sectorsViewSource)
    {
      InitializeComponent();
      DataContext = this;
      Owner = mainWindow;
      SectorsList = sectorsViewSource;
      MainWindowReference = mainWindow;
      Galaxy = MainWindowReference.Galaxy;
      if (Galaxy != null)
      {
        foreach (ExtensionInfo dlc in Galaxy.DLCs)
        {
          var extension = new ExtensionOnMap(dlc.Name, dlc.Id, true);
          extension.PropertyChanged += Extension_PropertyChanged;
          EnabledDLCs.Add(extension);
        }
        foreach (ExtensionInfo mod in Galaxy.Mods)
        {
          var extension = new ExtensionOnMap(mod.Name, mod.Id, true);
          extension.PropertyChanged += Extension_PropertyChanged;
          EnabledMods.Add(extension);
        }
      }

      // Set window size to 90% of the main window
      Width = mainWindow.ActualWidth * 0.9;
      Height = mainWindow.ActualHeight * 0.9;

      if (!PrepareGalaxyMap())
      {
        Log.Error("Cluster map is not prepared.");
        return;
      }
      UpdateMap();
      // Attach mouse event handlers for panning
      GalaxyCanvas.MouseLeftButtonDown += GalaxyCanvas_MouseLeftButtonDown;
      GalaxyCanvas.MouseLeftButtonUp += GalaxyCanvas_MouseLeftButtonUp;
      GalaxyCanvas.MouseMove += GalaxyCanvas_MouseMove;
      // GalaxyCanvas.MouseWheel += GalaxyCanvas_MouseWheel;
    }

    /// <summary>
    /// Prepares a map of clusters organized into rows and columns based on their positions.
    /// </summary>
    /// <returns>True if the map was prepared successfully; otherwise, false.</returns>
    public bool PrepareGalaxyMap()
    {
      if (Galaxy == null)
      {
        Log.Error("Galaxy data is not loaded.");
        return false;
      }

      // Determine the rowId and columnId for each cluster and populate the dictionary
      foreach (var cluster in Galaxy.Clusters)
      {
        string columnIdStr = $"{(int)Math.Floor(cluster.Position.X / ColumnWidth)}";
        string rowIdStr = $"{cluster.Position.Z / RowHeight:F1}";

        if (!MapDict.TryGetValue(rowIdStr, out Dictionary<string, Cluster>? value))
        {
          value = [];
          MapDict[rowIdStr] = value;
        }

        value[columnIdStr] = cluster;

        if (!AxisZ.Contains(double.Parse(rowIdStr)))
        {
          AxisZ.Add(double.Parse(rowIdStr));
        }

        if (!AxisX.Contains(int.Parse(columnIdStr)))
        {
          AxisX.Add(int.Parse(columnIdStr));
        }
      }

      AxisX.Sort();
      AxisZ.Sort();
      AxisZ.Reverse();
      MaxRow = AxisZ.First();
      MinCol = AxisX.First();

      _canvasWidthBase = (AxisX.Last() - MinCol + 1) * 0.75 + 0.25;
      _canvasHeightBase = MaxRow - AxisZ.Last() + 1;
      return true;
    }

    private void CreateMap()
    {
      if (MapDict.Count == 0)
      {
        Log.Warn("Cluster map is empty.");
        return;
      }
      if (GalaxyScrollViewer.ActualHeight == 0 || GalaxyScrollViewer.ActualWidth == 0)
      {
        Log.Warn("GalaxyScrollViewer size is zero.");
        return;
      }

      GalaxyCanvas.Width = _canvasWidthBase * HexagonWidth * ScaleFactor;
      GalaxyCanvas.Height = _canvasHeightBase * HexagonHeight * ScaleFactor;

      GalaxyCanvas.Children.Clear();
      foreach (var Z in AxisZ)
      {
        Dictionary<string, Cluster> row = MapDict[Z.ToString(format: "F1")];
        foreach (var X in AxisX)
        {
          if (!row.ContainsKey(X.ToString()))
          {
            continue;
          }
          Cluster cluster = row[X.ToString()];
          GalaxyMapCluster clusterMapCluster = new(this, 0.75 * (X - MinCol), MaxRow - Z, GalaxyCanvas, cluster);
          clusterMapCluster.Create(this);
          _clusters.Add(clusterMapCluster);
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
              galaxyMapHighway.Create(GalaxyCanvas);
            }
          }
        }
      }
      if (SectorsItems.Count > 0)
      {
        if (Galaxy?.Connections.Count > 0)
        {
          foreach (GalaxyConnection connection in Galaxy.Connections)
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
            GalaxyMapInterConnection galaxyMapGateConnection = new(gateDirect, gateOpposite);
            galaxyMapGateConnection.Create(GalaxyCanvas);
          }
        }
        if (
          MainWindowReference.ChemGateKeeperMod.GalaxyConnections != null
          && MainWindowReference.ChemGateKeeperMod.GalaxyConnections.Count > 0
        )
        {
          foreach (GalaxyConnectionData connection in MainWindowReference.ChemGateKeeperMod.GalaxyConnections)
          {
            if (connection.Connection.Name == null)
            {
              continue;
            }
            List<SectorMapItem> newGatesItems = SectorsItems.FindAll(item => item.Id == connection.Connection.Name);
            if (newGatesItems.Count == 2)
            {
              GalaxyMapInterConnection galaxyMapGateConnection = new(newGatesItems[0], newGatesItems[1], true);
              galaxyMapGateConnection.Create(GalaxyCanvas);
            }
          }
        }
        if (
          MainWindowReference.GatesConnectionCurrent?.SectorDirect != null
          && MainWindowReference.GatesConnectionCurrent.SectorOpposite != null
        )
        {
          List<SectorMapItem> newGatesItems = SectorsItems.FindAll(item => item.Id == SectorMap.NewGateId);
          if (newGatesItems.Count == 2)
          {
            GalaxyMapInterConnection galaxyMapGateConnection = new(newGatesItems[0], newGatesItems[1], true);
            galaxyMapGateConnection.Create(GalaxyCanvas);
          }
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
        scrollVerticalOffset = GalaxyScrollViewer.VerticalOffset;
        scrollHorizontalOffset = GalaxyScrollViewer.HorizontalOffset;
        canvasWidth = GalaxyCanvas.Width;
        GalaxyCanvas.Width = _canvasWidthBase * HexagonWidth * ScaleFactor;
        GalaxyCanvas.Height = _canvasHeightBase * HexagonHeight * ScaleFactor;
        foreach (GalaxyMapCluster cluster in _clusters)
        {
          cluster.Update();
        }
      }
    }

    private void Extension_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(ExtensionOnMap.IsChecked))
      {
        UpdateMap();
      }
    }

    private void GalaxyScrollViewer_ScrollChanged(object sender, RoutedEventArgs e)
    {
      double viewportWidth = GalaxyScrollViewer.ViewportWidth;
      double viewportHeight = GalaxyScrollViewer.ViewportHeight;
      double currentCanvasWidth = GalaxyCanvas.Width;
      double currentCanvasHeight = GalaxyCanvas.Height;
      if (currentCanvasWidth - viewportWidth != canvasWidth && viewportWidth != 0 && viewportWidth != GalaxyScrollViewer.ActualWidth)
      {
        if (currentCanvasWidth > viewportWidth)
        {
          double newCanvasToScrollWidthDelta = currentCanvasWidth - viewportWidth;
          if (newCanvasToScrollWidthDelta != canvasToScrollWidthDelta)
          {
            Log.Debug($"GalaxyCanvas.Width: {GalaxyCanvas.Width}, canvasWidth: {canvasWidth}");
            Log.Debug($"GalaxyScrollViewer.ActualWidth: {GalaxyScrollViewer.ActualWidth}, ViewportWidth: {viewportWidth}");
            if (scrollHorizontalOffset == 0 || canvasToScrollWidthDelta == 0)
            {
              Log.Debug($"Centering the map: new offsets: {newCanvasToScrollWidthDelta / 2}");
              GalaxyScrollViewer.ScrollToHorizontalOffset(newCanvasToScrollWidthDelta / 2);
            }
            else
            {
              Log.Debug(
                $"Scrolling the map: old offset: {scrollHorizontalOffset}, new offset: {scrollHorizontalOffset * newCanvasToScrollWidthDelta / canvasToScrollWidthDelta}"
              );
              GalaxyScrollViewer.ScrollToHorizontalOffset(scrollHorizontalOffset * newCanvasToScrollWidthDelta / canvasToScrollWidthDelta);
            }
            canvasToScrollWidthDelta = newCanvasToScrollWidthDelta;
          }
        }
        if (currentCanvasHeight > viewportHeight)
        {
          double newCanvasToScrollHeightDelta = currentCanvasHeight - viewportHeight;
          if (newCanvasToScrollHeightDelta != canvasToScrollHeightDelta)
          {
            Log.Debug($"GalaxyScrollViewer.ActualHeight: {GalaxyScrollViewer.ActualHeight}, ViewportHeight: {viewportHeight}");
            if (scrollVerticalOffset == 0 || canvasToScrollHeightDelta == 0)
            {
              Log.Debug($"Centering the map: new offsets: {newCanvasToScrollHeightDelta / 2}");
              GalaxyScrollViewer.ScrollToVerticalOffset(newCanvasToScrollHeightDelta / 2);
            }
            else
            {
              Log.Debug(
                $"Scrolling the map: old offset: {scrollVerticalOffset}, new offset: {scrollVerticalOffset * newCanvasToScrollHeightDelta / canvasToScrollHeightDelta}"
              );
              GalaxyScrollViewer.ScrollToVerticalOffset(scrollVerticalOffset * newCanvasToScrollHeightDelta / canvasToScrollHeightDelta);
            }
            canvasToScrollHeightDelta = newCanvasToScrollHeightDelta;
          }
        }
      }
    }

    // Mouse Left Button Down - Start Panning
    private void GalaxyCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      var clickedElement = e.OriginalSource as DependencyObject;
      if (clickedElement is TextBlock textBlock && textBlock.DataContext is Sector _sectorFromTextBlock)
      {
        SelectedSector = _sectorFromTextBlock;
      }
      else if (clickedElement is Polygon polygon && polygon.DataContext is Sector _sectorFromPolygon)
      {
        SelectedSector = _sectorFromPolygon;
      }
      else if (clickedElement is Grid grid && grid.DataContext is Sector _sectorFromGrid)
      {
        SelectedSector = _sectorFromGrid;
      }
      if (SectorsList != null && !String.IsNullOrEmpty(SelectedSector?.Macro))
      {
        if (SectorsList.View is CollectionView collectionView)
        {
          SectorsListItem? sector = collectionView.Cast<SectorsListItem>().FirstOrDefault(sector => sector.Macro == SelectedSector?.Macro);
          if (sector != null && !sector.Selectable)
          {
            SelectedSector = null;
            Log.Debug($"Sector {sector.Name} is not selectable. Skipping.");
          }
        }
      }
      isPanning = true;
      panStartPoint = e.GetPosition(this);
      scrollHorizontalOffset = GalaxyScrollViewer.HorizontalOffset;
      scrollVerticalOffset = GalaxyScrollViewer.VerticalOffset;
      GalaxyCanvas.CaptureMouse();
    }

    // Mouse Move - Perform Panning
    private void GalaxyCanvas_MouseMove(object sender, MouseEventArgs e)
    {
      if (isPanning)
      {
        Point currentPoint = e.GetPosition(this);
        double deltaX = currentPoint.X - panStartPoint.X;
        double deltaY = currentPoint.Y - panStartPoint.Y;
        if (Math.Abs(deltaX) < 0.1 && Math.Abs(deltaY) < 0.1)
        {
          return;
        }
        SelectedSector = null;
        // Adjust the ScrollViewer's offsets
        GalaxyScrollViewer.ScrollToHorizontalOffset(scrollHorizontalOffset - deltaX);
        GalaxyScrollViewer.ScrollToVerticalOffset(scrollVerticalOffset - deltaY);
        // Change cursor to SizeAll
        this.Cursor = Cursors.SizeAll;
      }
    }

    // Mouse Left Button Up - End Panning
    private void GalaxyCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      if (isPanning)
      {
        isPanning = false;
        GalaxyCanvas.ReleaseMouseCapture();
        this.Cursor = Cursors.Arrow;
      }
      if (SelectedSector != null)
      {
        MessageBoxResult result = MessageBox.Show(
          $"Do you want to select Sector: {SelectedSector.Name}",
          "Select Sector",
          MessageBoxButton.YesNo,
          MessageBoxImage.Question
        );
        if (result == MessageBoxResult.Yes)
        {
          Close();
        }
        else
        {
          SelectedSector = null;
        }
      }
    }

    private void Window_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
      if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
      {
        double step = HexagonWidth / HexagonWidthMinimal < 2 ? 10 : HexagonWidthMinimal * 0.5;
        if (e.Delta > 0)
        {
          // Zoom In
          HexagonWidth = Math.Min(HexagonWidth + step, HexagonWidthMaximal);
        }
        else if (e.Delta < 0)
        {
          // Zoom Out
          HexagonWidth = Math.Max(HexagonWidth - step, HexagonWidthMinimal);
        }
        e.Handled = true;
      }
    }

    private void GalaxyMapWindow_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      if (GalaxyScrollViewer.ActualWidth != 0 && GalaxyScrollViewer.ActualHeight != 0)
      {
        double width = GalaxyScrollViewer.ActualWidth;
        double height = GalaxyScrollViewer.ActualHeight;
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

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }

  class GalaxyMapCluster(GalaxyMapWindow map, double x, double y, Canvas canvas, Cluster cluster) : INotifyPropertyChanged
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
    protected GalaxyMapWindow? Map = map;
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

    public virtual void Create(Window window)
    {
      if (Map == null || Cluster == null || Canvas == null)
      {
        return;
      }
      if (Cluster.Sectors.Count == 1)
      {
        Sector sector = Cluster.Sectors[0];
        GalaxyMapSector clusterMapSector = new(Map, _x, _y, Canvas, Cluster, sector);
        clusterMapSector.Create(window);
        _sectors.Add(clusterMapSector);
      }
      else
      {
        UpdatePoints();
        // Log.Debug($"Creating cluster {Cluster.Name} at ({X}, {Y}) ({x}, {y}) with Points {string.Join(", ", Points.Select(p => $"({p.X}, {p.Y})"))})");
        Hexagon = new()
        {
          Stroke = Brushes.Black,
          StrokeThickness = 1,
          Fill = Brushes.Transparent,
          Tag = Cluster.Name,
          DataContext = Cluster,
          Points = Points,
          ToolTip = ToolTipCreator(Cluster),
        };
        // Position the Hexagon on the Canvas
        Canvas.SetLeft(Hexagon, X);
        Canvas.SetTop(Hexagon, Y);
        Canvas.Children.Add(Hexagon);
        List<HexagonCorner> corners = [];
        List<double?> angles = [];
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
            clusterMapSector.Create(window);
            _sectors.Add(clusterMapSector);
          }
        }
      }
    }

    protected Grid ToolTipCreator(Cluster cluster, Sector? sector = null)
    {
      Grid toolTipGrid = new()
      {
        MinWidth = 200,
        MinHeight = 100,
        Background = Brushes.LightGray,
      };
      if (sector != null && Map != null)
      {
        SolidColorBrush? brush = Map.MainWindowReference.FactionColors.GetBrush(sector.DominantOwner);
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
            labelStr = sector != null && isSector ? $"Sector: {sector.Name}" : $"Cluster: {cluster.Name}";
            break;
          case "Source":
            labelStr = "Source:";
            textStr = sector != null && isSector ? sector.Source : cluster.Source;
            break;
          case "Coordinates":
            labelStr = "Coordinates:";
            break;
          case "X":
          case "Y":
          case "Z":
            labelStr = $"{toolTipItem}:";
            textStr = toolTipItem switch
            {
              "X" => sector != null && isSector ? sector.Position.X.ToString("N0") : cluster.Position.X.ToString("N0"),
              "Y" => sector != null && isSector ? sector.Position.Y.ToString("N0") : cluster.Position.Y.ToString("N0"),
              "Z" => sector != null && isSector ? sector.Position.Z.ToString("N0") : cluster.Position.Z.ToString("N0"),
              _ => string.Empty,
            };
            alignRight = true;
            break;
          case "Macro":
            labelStr = "Macro:";
            textStr = sector != null && isSector ? sector.Macro : cluster.Macro;
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
      if (Cluster == null || Canvas == null)
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

  class GalaxyMapSector(GalaxyMapWindow map, double x, double y, Canvas canvas, Cluster cluster, Sector sector, bool isHalf = false)
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

    public override void Create(Window window)
    {
      if (Cluster == null || Sector == null || Canvas == null || Map == null || Map.Galaxy == null)
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
      SolidColorBrush? brush = Map.MainWindowReference.FactionColors.GetBrush(Sector.DominantOwner);
      if (brush != null)
      {
        Hexagon.Fill = brush;
        Hexagon.Fill.Opacity = Map.MainWindowReference.MapColorsOpacity;
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
      SectorMapHelper.InternalSizeKm = Map.MainWindowReference.SectorRadius;
      SectorMapHelper.ItemSizeMinPx = 4;
      SectorMapHelper.SetColors(Map.MainWindowReference.FactionColors);
      SectorMapHelper.SetSector(Sector, Map.Galaxy);
      if (Map.MainWindowReference != null)
      {
        foreach (ObjectInSector modObject in Map.MainWindowReference.GetObjectsInSectorFromMod(Sector.Macro))
        {
          SectorMapHelper.AddItem(modObject);
        }
        if (Map.MainWindowReference.GatesConnectionCurrent != null)
        {
          if (Map.MainWindowReference.GatesConnectionCurrent.SectorDirect?.Macro == Sector.Macro)
          {
            Map.MainWindowReference.GatesConnectionCurrent.UpdateCurrentGateOnMap("GateDirect", SectorMapHelper);
          }
          else if (Map.MainWindowReference.GatesConnectionCurrent.SectorOpposite?.Macro == Sector.Macro)
          {
            Map.MainWindowReference.GatesConnectionCurrent.UpdateCurrentGateOnMap("GateOpposite", SectorMapHelper);
          }
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
      Canvas.SetLeft(Grid, X);
      Canvas.SetTop(Grid, Y);
      SectorMapHelper.VisualX = X;
      SectorMapHelper.VisualY = Y;
      SectorMapHelper.VisualSizePx = Width;
      foreach (SectorMapItem item in SectorMapHelper.Items)
      {
        item.Update();
      }
    }

    private double SetFontSize()
    {
      return Math.Min(Height * FrontSizeProportion, FrontSizeMax);
    }
  }

  public class GalaxyMapInterConnection : INotifyPropertyChanged
  {
    private SectorMapItem? _source;
    public SectorMapItem? Source
    {
      get => _source;
      set
      {
        _source = value;
        OnPropertyChanged(nameof(Source));
      }
    }
    private SectorMapItem? _destination;
    public SectorMapItem? Destination
    {
      get => _destination;
      set
      {
        _destination = value;
        OnPropertyChanged(nameof(Destination));
      }
    }

    private readonly bool IsGate = true;
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public GalaxyMapInterConnection(SectorMapItem gateDirect, SectorMapItem gateOpposite, bool isGate = true)
    {
      Source = gateDirect;
      Destination = gateOpposite;
      IsGate = isGate;
    }

    public void Create(Canvas canvas)
    {
      if (Source == null || Destination == null)
      {
        return;
      }

      Line line = new() { DataContext = this, StrokeThickness = IsGate ? 2 : 1 };
      if (IsGate)
      {
        if (Source.Status == "active" && Destination.Status == "active")
        {
          line.Stroke = Source.From switch
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
      Binding x1Binding = new(path: "CenterX") { Source = Source };
      line.SetBinding(Line.X1Property, x1Binding);
      Binding y1Binding = new(path: "CenterY") { Source = Source };
      line.SetBinding(Line.Y1Property, y1Binding);
      Binding x2Binding = new(path: "CenterX") { Source = Destination };
      line.SetBinding(Line.X2Property, x2Binding);
      Binding y2Binding = new(path: "CenterY") { Source = Destination };
      line.SetBinding(Line.Y2Property, y2Binding);
      // Canvas.SetZIndex(line, -1);
      canvas.Children.Add(line);
    }
  }

  public class ExtensionOnMap : INotifyPropertyChanged
  {
    private string _name = string.Empty;
    private string _id = string.Empty;
    private bool _isChecked = false;

    public ExtensionOnMap(string name, string id, bool isChecked)
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
