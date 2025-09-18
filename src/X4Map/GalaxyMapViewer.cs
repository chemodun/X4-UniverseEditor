using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.RightsManagement;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Utilities.Logging;
using X4DataLoader;
using X4Map.Constants;

namespace X4Map
{
  public class GalaxyMapViewer : ScrollViewer, INotifyPropertyChanged
  {
    private bool _editorMode = true;
    public Galaxy GalaxyData { get; private set; } = new();
    public FactionColors FactionColors { get; private set; } = new();
    private double _mapColorsOpacity = 0.5;
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
    private readonly List<GalaxyMapSector> _sectors = [];
    private Canvas GalaxyCanvas { get; set; } = new();
    private const double HexagonWidthDefault = 100; // Width of the hexagon in pixels
    private static readonly double HexagonHeightDefault = HexagonWidthDefault * SectorMap.HexagonSizesRelation; // Height of the hexagon in pixels (Width * sqrt(3)/2)
    private double _hexagonWidth = HexagonWidthDefault; // Width of the hexagon in pixels
    public double HexagonWidth
    {
      get => _hexagonWidth;
      set
      {
        _hexagonWidth = value;
        _hexagonHeight = value * SectorMap.HexagonSizesRelation;
        OnPropertyChanged(nameof(HexagonWidth));
        OnPropertyChanged(nameof(HexagonHeight));
        UpdateMap();
      }
    } // Width of the hexagon in pixel

    private double _hexagonHeight = HexagonWidthDefault * SectorMap.HexagonSizesRelation; // Height of the hexagon in pixels (Width * sqrt(3)/2)
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
    private double _sectorRadius = MapConstants.SectorInternalSizeMinKm;
    public double SectorRadius
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
    public static readonly int ColumnWidth = 15000000; // 15,000,000 meters for horizontal (X) axis
    public static readonly int RowHeight = 17320000 / 2; // 17,320,000 meters for vertical (Z) axis

    public double ScaleFactor = 0.001; // Scaling factor to convert units to pixels
    private double _canvasWidthBase = 0;
    private double _canvasHeightBase = 0;
    public MapInfo MapInfo { get; set; } = new(0, 0, 0, 0);
    private double scrollHorizontalOffset = 0;
    private double canvasToScrollWidthDelta = 0;
    private double canvasToScrollHeightDelta = 0;
    private double scrollVerticalOffset = 0;
    public Polygon? HexagonOnMouseRightButtonDown = null;
    private Polygon? _hexagonOnMouseLeftButtonDown = null;

    private Polygon? _hexagonSelected = null;
    private Polygon? _pressedHexagon = null;
    public Polygon? PressedHexagon
    {
      get => _pressedHexagon;
      set
      {
        _pressedHexagon = value;
        OnPropertyChanged(nameof(PressedHexagon));
        if (value != null)
        {
          if (value.DataContext is Sector sector && sector != null)
          {
            GalaxyMapSector? selectedMapSector = _sectors.Find(mapSector => mapSector.Macro == sector.Macro);
            if (selectedMapSector != null && selectedMapSector == SelectedMapSector)
            {
              SelectedMapSector = null;
            }
            else
            {
              SelectedMapSector = selectedMapSector;
            }
            if (SelectedMapSector != null)
            {
              GalaxyMapCluster? selectedMapCluster = _clusters.Find(mapCluster =>
                mapCluster.Sectors.Any(mapSector => mapSector.Macro == sector.Macro)
              );
              SelectedMapCluster = selectedMapCluster;
            }
            else
            {
              SelectedMapCluster = null;
            }
            OnPressedSector?.Invoke(this, new SectorEventArgs(sector));
          }
          else if (value.DataContext is Cluster cluster && cluster != null)
          {
            GalaxyMapCluster? selectedMapCluster = _clusters.Find(mapCluster => mapCluster.Macro == cluster.Macro);
            if (selectedMapCluster != null && selectedMapCluster == SelectedMapCluster)
            {
              SelectedMapCluster = null;
            }
            else
            {
              SelectedMapCluster = selectedMapCluster;
            }
            SelectedMapSector = null;
            OnPressedCluster?.Invoke(this, new ClusterEventArgs(cluster));
          }
          else if (value.DataContext is GalaxyMapCluster galaxyMapCluster && galaxyMapCluster != null)
          {
            SelectedMapCluster = galaxyMapCluster == SelectedMapCluster ? null : galaxyMapCluster;
            SelectedMapSector = null;
            OnPressedCell?.Invoke(this, new CellEventArgs(galaxyMapCluster));
          }
        }
      }
    }

    public GalaxyMapCluster? SelectedMapCluster { get; set; } = null;
    public GalaxyMapSector? SelectedMapSector { get; set; } = null;

    // Fields to track panning state
    private bool isPanning = false;
    private Point panStartPoint = new();
    private double canvasWidth = 0;
    private readonly List<GalaxyMapCell> MapCells = [];
    public List<SectorMapItem> SectorsItems = [];
    public ObservableCollection<MapOptions> DLCsOptions { get; set; } = [];
    private Visibility _optionsDLCsVisibilityState = Visibility.Hidden;
    public Visibility OptionsDLCsVisibilityState
    {
      get => _optionsDLCsVisibilityState;
      set
      {
        _optionsDLCsVisibilityState = value;
        OnPropertyChanged(nameof(OptionsDLCsVisibilityState));
      }
    }
    public ObservableCollection<MapOptions> ModsOptions { get; set; } = [];
    private Visibility _optionsModsVisibilityState = Visibility.Hidden;
    public Visibility OptionsModsVisibilityState
    {
      get => _optionsModsVisibilityState;
      set
      {
        _optionsModsVisibilityState = value;
        OnPropertyChanged(nameof(OptionsModsVisibilityState));
      }
    }
    public ObservableCollection<MapOptions> DeveloperOptions { get; set; } = [];
    public MapOptions ShowEmptyClusterPlaces = new("Show Empty Cluster Cells", "", "EmptyCell", false);

    private List<string> _extraConnectionsNames = [];
    private Dictionary<string, List<ObjectInSector>> _extraObjects = [];
    private readonly List<GalaxyMapInterConnection> InterConnections = [];
    public event EventHandler<SectorEventArgs>? OnPressedSector;
    public event EventHandler<ClusterEventArgs>? OnPressedCluster;
    public event EventHandler<CellEventArgs>? OnPressedCell;
    public event EventHandler<SectorEventArgs>? OnRightPressedSector;
    public event EventHandler<ClusterEventArgs>? OnRightPressedCluster;
    public event EventHandler<CellEventArgs>? OnRightPressedCell;

    public Canvas? ExportCanvas{ get; set; }

    public void Connect(
      Galaxy galaxy,
      Canvas galaxyCanvas,
      double mapColorsOpacity,
      bool editorMode = true,
      Dictionary<string, List<ObjectInSector>>? extraObjects = null,
      List<string>? extraConnectionsNames = null
    )
    {
      _editorMode = editorMode;
      GalaxyData = galaxy;
      GalaxyCanvas = galaxyCanvas;
      _mapColorsOpacity = mapColorsOpacity;
      _sectorRadius = MapConstants.SectorInternalSizeMinKm;
      _extraObjects = extraObjects ?? [];
      _extraConnectionsNames = extraConnectionsNames ?? [];
      ShowEmptyClusterPlaces.PropertyChanged += MapOptions_PropertyChanged;
      DeveloperOptions.Add(ShowEmptyClusterPlaces);
      ScrollChanged += GalaxyMapViewer_ScrollChanged;
      SizeChanged += GalaxyMapViewer_SizeChanged;
      GalaxyCanvas.PreviewMouseWheel += GalaxyMapViewer_PreviewMouseWheel;
      GalaxyCanvas.PreviewMouseLeftButtonDown += GalaxyMapViewer_MouseLeftButtonDown;
      GalaxyCanvas.PreviewMouseRightButtonDown += GalaxyMapViewer_MouseRightButtonDown;
      GalaxyCanvas.MouseMove += GalaxyMapViewer_MouseMove;
      GalaxyCanvas.MouseLeftButtonUp += GalaxyMapViewer_MouseLeftButtonUp;
    }

    public void RefreshGalaxyData()
    {
      if (_clusters.Count > 0)
      {
        foreach (GalaxyMapCluster cluster in _clusters)
        {
          cluster.Remove(GalaxyCanvas);
        }
        foreach (GalaxyMapInterConnection connection in InterConnections)
        {
          connection.Remove(GalaxyCanvas);
        }
        foreach (SectorMapItem sector in SectorsItems)
        {
          sector.Remove(GalaxyCanvas);
        }
        MapInfo = new(0, 0, 0, 0);
        _clusters.Clear();
        InterConnections.Clear();
        SectorsItems.Clear();
        MapCells.Clear();
        DLCsOptions.Clear();
        ModsOptions.Clear();
      }
      if (GalaxyData.Clusters.Count == 0)
      {
        Log.Error("Galaxy data is empty.");
        return;
      }
      foreach (string dlcId in Galaxy.DLCOrder)
      {
        ExtensionInfo? dlc = GalaxyData.DLCs.Find(dlc => dlc.Id == dlcId);
        if (dlc != null)
        {
          var extension = new MapOptions(dlc.Name, $"v.{dlc.Version}", dlc.Id, true);
          extension.PropertyChanged += MapOptions_PropertyChanged;
          DLCsOptions.Add(extension);
        }
      }
      OptionsDLCsVisibilityState = DLCsOptions.Count > 0 ? Visibility.Visible : Visibility.Hidden;
      foreach (ExtensionInfo mod in GalaxyData.Mods)
      {
        var extension = new MapOptions(mod.Name, $"v.{mod.Version}", mod.Id, true);
        extension.PropertyChanged += MapOptions_PropertyChanged;
        ModsOptions.Add(extension);
      }
      OptionsModsVisibilityState = ModsOptions.Count > 0 ? Visibility.Visible : Visibility.Hidden;
      PrepareGalaxyMap();
      CreateMap();
    }

    private void PrepareGalaxyMap()
    {
      // Determine the rowId and columnId for each cluster and populate the dictionary
      foreach (var cluster in GalaxyData.Clusters)
      {
        int col = (int)(cluster.Position.X / ColumnWidth);
        int row = (int)(cluster.Position.Z / RowHeight);

        if (col < MapInfo.ColumnMin)
        {
          MapInfo.ColumnMin = col;
        }
        if (col > MapInfo.ColumnMax)
        {
          MapInfo.ColumnMax = col;
        }
        if (row < MapInfo.RowMin)
        {
          MapInfo.RowMin = row;
        }
        if (row > MapInfo.RowMax)
        {
          MapInfo.RowMax = row;
        }
        MapCells.Add(new GalaxyMapCell(cluster, col, row));
      }

      _canvasWidthBase = (MapInfo.ColumnMax - MapInfo.ColumnMin + 1) * 0.75 + 0.25;
      _canvasHeightBase = (MapInfo.RowMax - MapInfo.RowMin) * 0.5 + 1;
    }

    public async Task ExportToPng(Canvas sourceCanvas, string filePath)
    {
      if (sourceCanvas == null)
        throw new ArgumentNullException(nameof(sourceCanvas));

      Dispatcher.Invoke(sourceCanvas.UpdateLayout, DispatcherPriority.Render);

      if (sourceCanvas.ActualWidth == 0 || sourceCanvas.ActualHeight == 0)
        throw new InvalidOperationException("Canvas has zero width or height.");

      // Create a new canvas and copy properties
      // Need to run this on the UI thread and have it return back the resulting canvas

      Dispatcher.Invoke(new Action(() =>
      {
        ExportCanvas = new Canvas
        {
          Width = sourceCanvas.ActualWidth,
          Height = sourceCanvas.ActualHeight,
          Background = Brushes.Transparent
        };
      }), DispatcherPriority.Normal);

      if (ExportCanvas == null)
        throw new InvalidOperationException("Failed to create export canvas.");

      var elements = Dispatcher.Invoke(() => sourceCanvas.Children, DispatcherPriority.Background);
      foreach (UIElement child in elements)
      {
        try
        {
          var xaml = System.Windows.Markup.XamlWriter.Save(child);
          var deepCopy = System.Windows.Markup.XamlReader.Parse(xaml) as UIElement;
          if (deepCopy == null)
            continue;
          ExportCanvas.Children.Add(deepCopy);
        }
        catch (Exception ex)
        {
          Debug.WriteLine($"Failed to copy element: {ex.Message}");
        }
      }

      var WidthAndHeight = Dispatcher.Invoke(() =>
      {
        ExportCanvas.Measure(new Size(ExportCanvas.Width, ExportCanvas.Height));
        ExportCanvas.Arrange(new Rect(0, 0, ExportCanvas.Width, ExportCanvas.Height));
        ExportCanvas.UpdateLayout();
        return (ExportCanvas.Width, ExportCanvas.Height);
       }, DispatcherPriority.Background);


      // Render to bitmap
      var rtb = new System.Windows.Media.Imaging.RenderTargetBitmap(
        (int) WidthAndHeight.Width, (int) WidthAndHeight.Height, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
      rtb.Render(ExportCanvas);

      // Encode as PNG
      var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
      encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(rtb));
      using var fs = System.IO.File.OpenWrite(filePath);
      encoder.Save(fs);
    }

    private void CreateMap()
    {
      double maxSectorRadius = SectorRadius;
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
      ScaleFactorUpdate();
      GalaxyCanvas.Width = _canvasWidthBase * HexagonWidth * ScaleFactor;
      GalaxyCanvas.Height = _canvasHeightBase * HexagonHeight * ScaleFactor;

      bool isMinColOdd = Math.Abs(MapInfo.ColumnMin) % 2 == 1;
      for (int row = MapInfo.RowMax; row >= MapInfo.RowMin; row--)
      {
        bool isRowOdd = Math.Abs(row) % 2 == 1;
        int startCol = MapInfo.ColumnMin;
        if (isRowOdd)
        {
          startCol += isMinColOdd ? 0 : 1;
        }
        else
        {
          startCol += isMinColOdd ? 1 : 0;
        }
        for (int col = startCol; col <= MapInfo.ColumnMax; col += 2)
        {
          MapPosition mapPosition = new(col, row);
          Cluster? cluster = GalaxyMapCell.GetCluster(MapCells, mapPosition);
          Position position = cluster != null ? cluster.Position : new Position(col * ColumnWidth, 0, row * RowHeight);
          GalaxyMapCluster clusterMapCluster = new(
            0.75 * (col - MapInfo.ColumnMin),
            (MapInfo.RowMax - row) * 0.5,
            mapPosition,
            GalaxyCanvas,
            cluster,
            position,
            HexagonWidth,
            HexagonHeight,
            ScaleFactor
          );
          double sectorRadius = clusterMapCluster.Create(this);
          if (sectorRadius > maxSectorRadius)
          {
            maxSectorRadius = sectorRadius;
          }
          _clusters.Add(clusterMapCluster);
          if (cluster != null)
          {
            _sectors.AddRange(clusterMapCluster.Sectors);
          }
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
              galaxyMapHighway.Create(GalaxyCanvas);
              InterConnections.Add(galaxyMapHighway);
            }
          }
        }
      }
      List<Cluster> unprocessedClusters = GalaxyMapCell.GetUnprocessedClusters(MapCells);
      if (unprocessedClusters.Count > 0)
      {
        Log.Warn($"There are {unprocessedClusters.Count} unprocessed clusters in the map.");
        foreach (Cluster cluster in unprocessedClusters)
        {
          // Process each unprocessed cluster
          Log.Warn($"Unprocessed cluster: {cluster.Name} at position {cluster.Position}");
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
            galaxyMapGateConnection.Create(GalaxyCanvas);
            InterConnections.Add(galaxyMapGateConnection);
          }
        }
        foreach (string connectionName in _extraConnectionsNames)
        {
          if (connectionName == null)
          {
            continue;
          }
          List<SectorMapItem> extraGatesItems = SectorsItems.FindAll(item => item.Id == connectionName);
          if (extraGatesItems.Count == 2)
          {
            GalaxyMapInterConnection galaxyMapGateConnection = new(extraGatesItems[0], extraGatesItems[1], true);
            galaxyMapGateConnection.Create(GalaxyCanvas);
            InterConnections.Add(galaxyMapGateConnection);
          }
        }
        List<SectorMapItem> newGatesItems = SectorsItems.FindAll(item => item.Id == SectorMap.NewGateId);
        if (newGatesItems.Count == 2)
        {
          GalaxyMapInterConnection galaxyMapGateConnection = new(newGatesItems[0], newGatesItems[1], true);
          galaxyMapGateConnection.Create(GalaxyCanvas);
          InterConnections.Add(galaxyMapGateConnection);
        }
      }
      SectorRadius = maxSectorRadius;

    }

    private void ScaleFactorUpdate()
    {
      double width = ActualWidth;
      double height = ActualHeight;
      double scaleFactorWidth = width / _canvasWidthBase / HexagonWidthDefault;
      double scaleFactorHeight = height / _canvasHeightBase / HexagonHeightDefault;
      ScaleFactor = Math.Min(scaleFactorWidth, scaleFactorHeight);
      if (width * SectorMap.HexagonSizesRelation < height)
      {
        HexagonWidthMaximal = width / ScaleFactor * 2;
      }
      else
      {
        HexagonWidthMaximal = height / SectorMap.HexagonSizesRelation / ScaleFactor * 2;
      }
    }

    public void UpdateMap()
    {
      if (GalaxyData.Clusters.Count == 0)
      {
        Log.Warn("Cluster map is empty.");
        return;
      }
      if (_clusters.Count == 0)
      {
        CreateMap();
      }
      else
      {
        scrollVerticalOffset = VerticalOffset;
        scrollHorizontalOffset = HorizontalOffset;
        canvasWidth = GalaxyCanvas.Width;
        GalaxyCanvas.Width = _canvasWidthBase * HexagonWidth * ScaleFactor;
        GalaxyCanvas.Height = _canvasHeightBase * HexagonHeight * ScaleFactor;
        foreach (GalaxyMapCluster cluster in _clusters)
        {
          cluster.Update(this);
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
      if (ActualWidth != 0 && ActualHeight != 0 && _canvasWidthBase != 0 && _canvasHeightBase != 0)
      {
        ScaleFactorUpdate();
        UpdateMap();
      }
    }

    private void GalaxyMapViewer_ScrollChanged(object sender, RoutedEventArgs e)
    {
      double viewportWidth = ViewportWidth;
      double viewportHeight = ViewportHeight;
      double currentCanvasWidth = GalaxyCanvas.Width;
      double currentCanvasHeight = GalaxyCanvas.Height;
      if (currentCanvasWidth - viewportWidth != canvasWidth && viewportWidth != 0 && viewportWidth != ActualWidth)
      {
        if (currentCanvasWidth > viewportWidth)
        {
          double newCanvasToScrollWidthDelta = currentCanvasWidth - viewportWidth;
          if (newCanvasToScrollWidthDelta != canvasToScrollWidthDelta)
          {
            Log.Debug($"GalaxyCanvas.Width: {GalaxyCanvas.Width}, canvasWidth: {canvasWidth}");
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

    private void GalaxyMapViewer_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
      var clickedElement = e.OriginalSource as DependencyObject;
      if (clickedElement is TextBlock textBlock && textBlock.DataContext is Sector _sectorFromTextBlock)
      {
        if (textBlock.Parent is Polygon polygon)
        {
          HexagonOnMouseRightButtonDown = polygon;
        }
        else if (textBlock.Parent is Grid grid)
        {
          HexagonOnMouseRightButtonDown = grid.Children[0] as Polygon;
        }
      }
      else if (clickedElement is Polygon polygon && polygon.DataContext is Sector _sectorFromPolygon)
      {
        HexagonOnMouseRightButtonDown = polygon;
      }
      else if (clickedElement is Grid grid && grid.DataContext is Sector _sectorFromGrid)
      {
        HexagonOnMouseRightButtonDown = grid.Children[0] as Polygon;
      }
      else if (clickedElement is Polygon clusterPolygon && clusterPolygon.DataContext is Cluster _clusterFromPolygon)
      {
        GalaxyMapCluster? galaxyCluster = _clusters.Find(cluster => cluster.Macro == _clusterFromPolygon.Macro);
        if (galaxyCluster != null)
        {
          HexagonOnMouseRightButtonDown = galaxyCluster.Hexagon;
        }
      }
      else if (clickedElement is Polygon cellPolygon && cellPolygon.DataContext is GalaxyMapCluster _galaxyClusterFromPolygon)
      {
        HexagonOnMouseRightButtonDown = _galaxyClusterFromPolygon.Hexagon;
      }
      if (HexagonOnMouseRightButtonDown != null)
      {
        Log.Debug($"Right button down on {HexagonOnMouseRightButtonDown.Tag}");
        if (_editorMode)
        {
          if (_hexagonSelected != null && _hexagonSelected != HexagonOnMouseRightButtonDown)
          {
            _hexagonSelected.StrokeThickness = 1;
          }
          _hexagonSelected = HexagonOnMouseRightButtonDown;
          if (_hexagonSelected != null)
          {
            _hexagonSelected.StrokeThickness = 3;
          }
        }
        if (HexagonOnMouseRightButtonDown.DataContext is Sector sector && sector != null)
        {
          SelectedMapSector = _sectors.Find(mapSector => mapSector.Macro == sector.Macro);
          if (SelectedMapSector != null)
          {
            SelectedMapCluster = _clusters.Find(mapCluster => mapCluster.Sectors.Any(mapSector => mapSector.Macro == sector.Macro));
          }
          else
          {
            SelectedMapCluster = null;
          }
          OnRightPressedSector?.Invoke(this, new SectorEventArgs(sector));
        }
        else if (HexagonOnMouseRightButtonDown.DataContext is Cluster cluster && cluster != null)
        {
          SelectedMapCluster = _clusters.Find(mapCluster => mapCluster.Macro == cluster.Macro);
          SelectedMapSector = null;
          OnRightPressedCluster?.Invoke(this, new ClusterEventArgs(cluster));
        }
        else if (HexagonOnMouseRightButtonDown.DataContext is GalaxyMapCluster galaxyMapCluster && galaxyMapCluster != null)
        {
          SelectedMapCluster = galaxyMapCluster;
          SelectedMapSector = null;
          OnRightPressedCell?.Invoke(this, new CellEventArgs(galaxyMapCluster));
        }
      }
    }

    private void GalaxyMapViewer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      var clickedElement = e.OriginalSource as DependencyObject;
      if (clickedElement is TextBlock textBlock && textBlock.DataContext is Sector _sectorFromTextBlock)
      {
        if (textBlock.Parent is Polygon polygon)
        {
          _hexagonOnMouseLeftButtonDown = polygon;
        }
        else if (textBlock.Parent is Grid grid)
        {
          _hexagonOnMouseLeftButtonDown = grid.Children[0] as Polygon;
        }
      }
      else if (clickedElement is Polygon polygon && polygon.DataContext is Sector _sectorFromPolygon)
      {
        _hexagonOnMouseLeftButtonDown = polygon;
      }
      else if (clickedElement is Grid grid && grid.DataContext is Sector _sectorFromGrid)
      {
        _hexagonOnMouseLeftButtonDown = grid.Children[0] as Polygon;
      }
      else if (clickedElement is Polygon clusterPolygon && clusterPolygon.DataContext is Cluster _clusterFromPolygon)
      {
        GalaxyMapCluster? galaxyCluster = _clusters.Find(cluster => cluster.Macro == _clusterFromPolygon.Macro);
        if (galaxyCluster != null)
        {
          _hexagonOnMouseLeftButtonDown = galaxyCluster.Hexagon;
        }
      }
      else if (clickedElement is Polygon cellPolygon && cellPolygon.DataContext is GalaxyMapCluster _galaxyClusterFromPolygon)
      {
        _hexagonOnMouseLeftButtonDown = _galaxyClusterFromPolygon.Hexagon;
      }
      isPanning = true;
      panStartPoint = e.GetPosition(this);
      scrollHorizontalOffset = HorizontalOffset;
      scrollVerticalOffset = VerticalOffset;
      GalaxyCanvas.CaptureMouse();
    }

    // Mouse Move - Perform Panning
    private void GalaxyMapViewer_MouseMove(object sender, MouseEventArgs e)
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
        _hexagonOnMouseLeftButtonDown = null;
        // Adjust the ScrollViewer's offsets
        ScrollToHorizontalOffset(scrollHorizontalOffset - deltaX);
        ScrollToVerticalOffset(scrollVerticalOffset - deltaY);
        // Change cursor to SizeAll
        this.Cursor = Cursors.SizeAll;
      }
    }

    // Mouse Left Button Up - End Panning
    private void GalaxyMapViewer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      if (isPanning)
      {
        isPanning = false;
        GalaxyCanvas.ReleaseMouseCapture();
        this.Cursor = Cursors.Arrow;
      }
      if (_editorMode)
      {
        if (_hexagonSelected != null)
        {
          _hexagonSelected.StrokeThickness = 1;
          _hexagonSelected = _hexagonSelected == _hexagonOnMouseLeftButtonDown ? null : _hexagonOnMouseLeftButtonDown;
        }
        else
        {
          _hexagonSelected = _hexagonOnMouseLeftButtonDown;
        }
        if (_hexagonSelected != null)
        {
          _hexagonSelected.StrokeThickness = 3;
        }
      }
      PressedHexagon = _hexagonOnMouseLeftButtonDown;
      _hexagonOnMouseLeftButtonDown = null;
    }

    public void GalaxyMapViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
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

    public List<ObjectInSector> GetExtraObjects(string sectorId)
    {
      if (_extraObjects.TryGetValue(sectorId, out List<ObjectInSector>? objects))
      {
        return objects;
      }
      return [];
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }

  public class SectorEventArgs(Sector? selectedSector) : EventArgs
  {
    public Sector? PressedSector { get; } = selectedSector;
  }

  public class ClusterEventArgs(Cluster? selectedCluster) : EventArgs
  {
    public Cluster? PressedCluster { get; } = selectedCluster;
  }

  public class CellEventArgs(GalaxyMapCluster? selectedCell) : EventArgs
  {
    public GalaxyMapCluster? PressedCell { get; } = selectedCell;
  }

  public class MapInfo(int minCol, int maxCol, int minRow, int maxRow)
  {
    public int ColumnMin { get; set; } = minCol;
    public int ColumnMax { get; set; } = maxCol;
    public int RowMin { get; set; } = minRow;
    public int RowMax { get; set; } = maxRow;
  }

  public class MapPosition(int column, int row)
  {
    public int Column { get; set; } = column;
    public int Row { get; set; } = row;
  }

  class GalaxyMapCell(Cluster cluster, int col, int row, bool isProcessed = false)
  {
    public MapPosition Position { get; set; } = new(col, row);
    public Cluster Cluster { get; set; } = cluster;
    public bool IsProcessed { get; set; } = isProcessed;

    public static Cluster? GetCluster(List<GalaxyMapCell> cells, MapPosition position)
    {
      GalaxyMapCell? foundCell = cells.Find(cell => cell.Position.Column == position.Column && cell.Position.Row == position.Row);
      if (foundCell != null)
      {
        foundCell.IsProcessed = true;
        return foundCell.Cluster;
      }
      return null;
    }

    public static List<Cluster> GetUnprocessedClusters(List<GalaxyMapCell> cells)
    {
      return cells.Where(cell => !cell.IsProcessed).Select(cell => cell.Cluster).ToList();
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

    public void Remove(Canvas canvas)
    {
      if (Line != null)
      {
        canvas.Children.Remove(Line);
      }
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
    private string _version = string.Empty;
    private string _id = string.Empty;
    private bool _isChecked = false;

    public MapOptions(string name, string version, string id, bool isChecked)
    {
      Name = name;
      Version = version;
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

    public string Version
    {
      get => _version;
      set
      {
        if (_version != value)
        {
          _version = value;
          OnPropertyChanged(nameof(Version));
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
}
