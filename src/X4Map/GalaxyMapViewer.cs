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
using X4DataLoader.Helpers;
using X4Map.Constants;

namespace X4Map
{
  public class GalaxyMapViewer : ScrollViewer, INotifyPropertyChanged
  {
    protected bool _editorMode = true;
    public Galaxy GalaxyData { get; protected set; } = new();
    public FactionColors FactionColors { get; protected set; } = new();
    protected double _mapColorsOpacity = 0.5;
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
    protected readonly List<GalaxyMapCluster> _clusters = [];
    protected readonly List<GalaxyMapSector> _sectors = [];
    protected Canvas GalaxyCanvas { get; set; } = new();
    protected const double HexagonWidthDefault = 100; // Width of the hexagon in pixels
    protected static readonly double HexagonHeightDefault = HexagonWidthDefault * SectorMap.HexagonSizesRelation; // Height of the hexagon in pixels (Width * sqrt(3)/2)
    protected double _hexagonWidth = HexagonWidthDefault; // Width of the hexagon in pixels
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

    protected double _hexagonHeight = HexagonWidthDefault * SectorMap.HexagonSizesRelation; // Height of the hexagon in pixels (Width * sqrt(3)/2)
    public double HexagonHeight
    {
      get => _hexagonHeight;
      set { }
    } // Height of the hexagon in pixels (Width * sqrt(3)/2)
    protected double _hexagonWidthMinimal = HexagonWidthDefault;
    public double HexagonWidthMinimal
    {
      get => _hexagonWidthMinimal;
      set
      {
        _hexagonWidthMinimal = value;
        OnPropertyChanged(nameof(HexagonWidthMinimal));
      }
    }
    protected double _hexagonWidthMaximal = HexagonWidthDefault;
    public double HexagonWidthMaximal
    {
      get => _hexagonWidthMaximal;
      set
      {
        _hexagonWidthMaximal = value;
        OnPropertyChanged(nameof(HexagonWidthMaximal));
      }
    }
    protected double _sectorRadius = MapConstants.SectorInternalSizeMinKm;
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
    protected double _canvasWidthBase = 0;
    protected double _canvasHeightBase = 0;
    public MapInfo MapInfo { get; set; } = new(0, 0, 0, 0);
    protected double scrollHorizontalOffset = 0;
    protected double canvasToScrollWidthDelta = 0;
    protected double canvasToScrollHeightDelta = 0;
    protected double scrollVerticalOffset = 0;
    public Polygon? HexagonOnMouseRightButtonDown = null;
    protected Polygon? _hexagonOnMouseLeftButtonDown = null;

    protected Polygon? _hexagonSelected = null;
    protected Polygon? _pressedHexagon = null;
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
    protected bool isPanning = false;
    protected Point panStartPoint = new();
    protected double canvasWidth = 0;

    // Deferred zoom fields
    protected readonly ScaleTransform _zoomScale = new(1.0, 1.0);
    protected readonly TranslateTransform _zoomTranslate = new(0.0, 0.0);
    protected double _deferredZoomScale = 1.0;
    protected double _zoomScalePrevious = 1.0;
    protected DispatcherTimer? _zoomCommitTimer;
    protected const double ZoomInStep = 1.1;
    protected const double ZoomOutStep = 1.0 / ZoomInStep;
    protected const double ZoomCommitDelayMs = 180; // debounce delay before committing layout
    protected const double ZoomMinFactor = 0.25; // relative to current HexagonWidth
    protected const double ZoomMaxFactor = 4.0; // relative to current HexagonWidth
    protected readonly List<GalaxyMapCell> MapCells = [];
    public List<SectorMapItem> SectorsItems = [];
    public ObservableCollection<MapOptions> DLCsOptions { get; set; } = [];
    protected Visibility _optionsDLCsVisibilityState = Visibility.Hidden;
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
    protected Visibility _optionsModsVisibilityState = Visibility.Hidden;
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
    protected Dictionary<string, List<ObjectInSector>> _extraObjects = [];
    protected readonly List<GalaxyMapInterConnection> InterConnections = [];
    public event EventHandler<SectorEventArgs>? OnPressedSector;
    public event EventHandler<ClusterEventArgs>? OnPressedCluster;
    public event EventHandler<CellEventArgs>? OnPressedCell;
    public event EventHandler<SectorEventArgs>? OnRightPressedSector;
    public event EventHandler<ClusterEventArgs>? OnRightPressedCluster;
    public event EventHandler<CellEventArgs>? OnRightPressedCell;

    public void Connect(
      Galaxy galaxy,
      Canvas galaxyCanvas,
      double mapColorsOpacity,
      bool editorMode = true,
      Dictionary<string, List<ObjectInSector>>? extraObjects = null
    )
    {
      _editorMode = editorMode;
      GalaxyData = galaxy;
      GalaxyCanvas = galaxyCanvas;
      _mapColorsOpacity = mapColorsOpacity;
      _sectorRadius = MapConstants.SectorInternalSizeMinKm;
      _extraObjects = extraObjects ?? [];
      ShowEmptyClusterPlaces.PropertyChanged += MapOptions_PropertyChanged;
      DeveloperOptions.Add(ShowEmptyClusterPlaces);
      ScrollChanged += GalaxyMapViewer_ScrollChanged;
      SizeChanged += GalaxyMapViewer_SizeChanged;
      GalaxyCanvas.PreviewMouseWheel += GalaxyMapViewer_PreviewMouseWheel;
      GalaxyCanvas.PreviewMouseLeftButtonDown += GalaxyMapViewer_MouseLeftButtonDown;
      GalaxyCanvas.PreviewMouseRightButtonDown += GalaxyMapViewer_MouseRightButtonDown;
      GalaxyCanvas.MouseMove += GalaxyMapViewer_MouseMove;
      GalaxyCanvas.MouseLeftButtonUp += GalaxyMapViewer_MouseLeftButtonUp;

      // Setup fast, GPU-accelerated deferred zoom transform
      var tg = new TransformGroup();
      tg.Children.Add(_zoomScale);
      tg.Children.Add(_zoomTranslate);
      GalaxyCanvas.RenderTransform = tg;
      GalaxyCanvas.RenderTransformOrigin = new Point(0, 0);
      _zoomCommitTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(ZoomCommitDelayMs), IsEnabled = false };
      _zoomCommitTimer.Tick += (_, _) => CommitDeferredZoom();
    }

    public virtual GalaxyMapCluster CreateMapCluster(
      double x,
      double y,
      MapPosition mapPosition,
      Canvas canvas,
      Cluster? cluster,
      Position? position,
      double hexagonWidth,
      double hexagonHeight,
      double scaleFactor
    )
    {
      var newCluster = new GalaxyMapCluster(x, y, mapPosition, canvas, cluster, position, hexagonWidth, hexagonHeight, scaleFactor);
      _clusters.Add(newCluster);
      return newCluster;
    }

    public virtual void GalaxyMapClusterReassign(GalaxyMapCluster galaxyCluster, Cluster? cluster)
    {
      if (galaxyCluster == null)
      {
        return;
      }
      if (!_clusters.Contains(galaxyCluster))
      {
        _clusters.Add(galaxyCluster);
      }
      galaxyCluster.ReAssign(this, cluster);
    }

    public virtual GalaxyMapSector CreateMapSector(
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
    )
    {
      var newSector = new GalaxyMapSector(x, y, owner, canvas, cluster, sector, hexagonWidth, hexagonHeight, scaleFactor, isHalf);
      _sectors.Add(newSector);
      return newSector;
    }

    public void RefreshGalaxyData()
    {
      if (_clusters.Count > 0)
      {
        foreach (GalaxyMapSector sector in _sectors)
        {
          sector.Clear(this, GalaxyCanvas);
        }
        _sectors.Clear();
        foreach (GalaxyMapCluster cluster in _clusters)
        {
          cluster.Clear(this, GalaxyCanvas);
        }
        _clusters.Clear();
        foreach (GalaxyMapInterConnection connection in InterConnections)
        {
          connection.Remove(GalaxyCanvas);
        }
        InterConnections.Clear();
        foreach (SectorMapItem sector in SectorsItems)
        {
          sector.Remove(GalaxyCanvas);
        }
        MapInfo = new(0, 0, 0, 0);
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
      foreach (ExtensionInfo dlc in GalaxyData.DLCs)
      {
        var extension = new MapOptions(dlc.Name, $"v.{dlc.Version}", dlc.Id, true);
        extension.PropertyChanged += MapOptions_PropertyChanged;
        DLCsOptions.Add(extension);
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

    protected void PrepareGalaxyMap()
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

    bool IsNotZoomed()
    {
      // Avoid division by zero
      if (ExtentWidth == 0 || ExtentHeight == 0)
        return true;

      // Calculate scale factors
      double scaleX = ExtentWidth / ViewportWidth;
      double scaleY = ExtentHeight / ViewportHeight;

      // If both are ~1, there's no zoom
      return Math.Abs(scaleX - 1.0) < 0.0001 || Math.Abs(scaleY - 1.0) < 0.0001;
    }

    public Task ExportToPng(Canvas sourceCanvas, string filePath)
    {
      ArgumentNullException.ThrowIfNull(sourceCanvas);

      if (!Dispatcher.CheckAccess())
      {
        // Marshal to UI thread and return the operation task
        return Dispatcher
          .InvokeAsync(
            () =>
            {
              ExportToPngOnUI(sourceCanvas, filePath);
            },
            DispatcherPriority.Render
          )
          .Task;
      }

      // Already on UI thread
      ExportToPngOnUI(sourceCanvas, filePath);
      UpdateMap();
      return Task.CompletedTask;
    }

    protected static void ExportToPngOnUI(Canvas sourceCanvas, string filePath)
    {
      // Ensure layout is up-to-date
      sourceCanvas.Measure(new Size(sourceCanvas.ActualWidth, sourceCanvas.ActualHeight));
      sourceCanvas.Arrange(new Rect(new Size(sourceCanvas.ActualWidth, sourceCanvas.ActualHeight)));

      // Ensure layout is up-to-date on UI thread
      sourceCanvas.UpdateLayout();
      if (sourceCanvas.ActualWidth == 0 || sourceCanvas.ActualHeight == 0)
        throw new InvalidOperationException("Canvas has zero width or height.");

      // Render the source canvas directly to a bitmap on the UI thread
      int pixelWidth = (int)Math.Ceiling(sourceCanvas.ActualWidth);
      int pixelHeight = (int)Math.Ceiling(sourceCanvas.ActualHeight);
      var rtb = new System.Windows.Media.Imaging.RenderTargetBitmap(
        pixelWidth,
        pixelHeight,
        96,
        96,
        System.Windows.Media.PixelFormats.Pbgra32
      );
      rtb.Render(sourceCanvas);

      // Encode as PNG and save
      var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
      encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(rtb));
      using var fs = System.IO.File.Open(filePath, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None);
      encoder.Save(fs);
      fs.Close();
    }

    protected virtual void CreateMap()
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
          GalaxyMapCluster clusterMapCluster = CreateMapCluster(
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
              GalaxyMapInterConnectionHighWay galaxyMapHighway = new(highway, pointEntry, pointExit, false);
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
            GalaxyMapSector? sectorDirect = _sectors.Find(sc => sc.Macro == connection.PathDirect.Sector?.Macro);
            if (sectorDirect == null)
            {
              continue;
            }
            GalaxyMapSector? sectorOpposite = _sectors.Find(sc => sc.Macro == connection.PathOpposite.Sector?.Macro);
            if (sectorOpposite == null)
            {
              continue;
            }
            SectorMapItem? gateDirect = sectorDirect.Items.Find(item => item.Id == connection.PathDirect.Gate.Name);
            SectorMapItem? gateOpposite = sectorOpposite.Items.Find(item => item.Id == connection.PathOpposite.Gate.Name);
            if (gateDirect == null || gateOpposite == null)
            {
              continue;
            }
            GalaxyMapInterConnection galaxyMapGateConnection = new(connection, gateDirect, gateOpposite, true, connection.Source);
            galaxyMapGateConnection.Create(GalaxyCanvas);
            InterConnections.Add(galaxyMapGateConnection);
          }
        }
      }
      SectorRadius = maxSectorRadius;
    }

    public void RemoveCluster(GalaxyMapCluster cluster)
    {
      if (cluster == null)
      {
        return;
      }
      cluster.Clear(this, GalaxyCanvas);
      _clusters.Remove(cluster);
      foreach (GalaxyMapSector sector in cluster.Sectors)
      {
        _sectors.Remove(sector);
      }
      cluster.Sectors.Clear();
    }

    public void RemoveSector(GalaxyMapSector sector)
    {
      if (sector == null)
      {
        return;
      }
      sector.Clear(this, GalaxyCanvas);
      _sectors.Remove(sector);
      GalaxyMapCluster? ownerCluster = _clusters.Find(cl => cl.Sectors.Contains(sector));
      if (ownerCluster != null)
      {
        ownerCluster.Sectors.Remove(sector);
        if (ownerCluster.Sectors.Count == 0)
        {
          RemoveCluster(ownerCluster);
        }
      }
    }

    protected void RefreshConnectionsForCluster(GalaxyMapCluster updatedCluster)
    {
      if (
        SectorsItems.Count > 0
        && updatedCluster != null
        && updatedCluster.Cluster != null
        && InterConnections != null
        && InterConnections.Count > 0
      )
      {
        if (updatedCluster.Cluster.Sectors.Count > 1 && updatedCluster.Cluster.Highways.Count > 0)
        {
          foreach (HighwayClusterLevel highway in updatedCluster.Cluster.Highways.Cast<HighwayClusterLevel>())
          {
            SectorMapItem? pointEntry = SectorsItems.Find(item => item.Id == highway?.EntryPointPath?.Zone?.Name);
            SectorMapItem? pointExit = SectorsItems.Find(item => item.Id == highway?.ExitPointPath?.Zone?.Name);
            if (pointEntry == null || pointExit == null)
            {
              continue;
            }

            GalaxyMapInterConnectionHighWay? galaxyMapHighway =
              InterConnections.Find(ic => ic is GalaxyMapInterConnectionHighWay gw && gw.Highway == highway)
              as GalaxyMapInterConnectionHighWay;
            if (galaxyMapHighway != null)
            {
              galaxyMapHighway.Remove(GalaxyCanvas);
              galaxyMapHighway.Create(GalaxyCanvas, pointEntry, pointExit);
            }
          }
        }
        if (GalaxyData?.Connections.Count > 0)
        {
          foreach (
            var connection in GalaxyData.Connections.Where(connection =>
              (
                connection != null
                && connection.PathDirect?.Cluster != null
                && StringHelper.EqualsIgnoreCase(connection.PathDirect.Cluster.Macro, updatedCluster.Cluster.Macro)
              )
              || (
                connection != null
                && connection.PathOpposite?.Cluster != null
                && StringHelper.EqualsIgnoreCase(connection.PathOpposite.Cluster.Macro, updatedCluster.Cluster.Macro)
              )
            )
          )
          {
            if (
              connection == null
              || connection.PathDirect == null
              || connection.PathDirect.Cluster == null
              || connection.PathDirect.Gate == null
              || connection.PathDirect.Gate.Name == null
              || connection.PathOpposite == null
              || connection.PathOpposite.Cluster == null
              || connection.PathOpposite.Gate == null
              || connection.PathOpposite.Gate.Name == null
            )
            {
              continue;
            }
            SectorMapItem? gateDirect = null;
            SectorMapItem? gateOpposite = null;
            if (StringHelper.EqualsIgnoreCase(connection.PathDirect.Cluster.Macro, updatedCluster.Cluster.Macro))
            {
              GalaxyMapSector? sectorDirect = _sectors.Find(sc => sc.Macro == connection.PathDirect.Sector?.Macro);
              if (sectorDirect == null)
              {
                continue;
              }
              gateDirect = sectorDirect.Items.Find(item => item.Id == connection.PathDirect.Gate.Name);
              if (gateDirect == null)
              {
                continue;
              }
            }

            if (StringHelper.EqualsIgnoreCase(connection.PathOpposite.Cluster.Macro, updatedCluster.Cluster.Macro))
            {
              GalaxyMapSector? sectorOpposite = _sectors.Find(sc => sc.Macro == connection.PathOpposite.Sector?.Macro);
              if (sectorOpposite == null)
              {
                continue;
              }
              gateOpposite = sectorOpposite.Items.Find(item => item.Id == connection.PathOpposite.Gate.Name);
              if (gateOpposite == null)
              {
                continue;
              }
            }
            GalaxyMapInterConnection? interConnection = InterConnections.Find(ic => ic.Connection == connection);
            if (interConnection != null)
            {
              interConnection.Remove(GalaxyCanvas);
              interConnection.Create(GalaxyCanvas, gateDirect, gateOpposite);
            }
          }
        }
      }
    }

    protected void ScaleFactorUpdate()
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

    public virtual void UpdateMap()
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

    public GalaxyMapCluster? GetClusterByMacro(string macro)
    {
      return _clusters.Find(cluster => StringHelper.EqualsIgnoreCase(cluster.Macro, macro));
    }

    public GalaxyMapSector? GetSectorByMacro(string macro)
    {
      return _sectors.Find(sector => StringHelper.EqualsIgnoreCase(sector.Macro, macro));
    }

    public GalaxyMapCluster? GetClusterBySectorMacro(string sectorMacro)
    {
      GalaxyMapSector? sector = GetSectorByMacro(sectorMacro);
      if (sector != null)
      {
        return _clusters.Find(cluster => cluster.Sectors.Contains(sector));
      }
      return null;
    }

    protected void GalaxyMapViewer_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      if (ActualWidth != 0 && ActualHeight != 0 && _canvasWidthBase != 0 && _canvasHeightBase != 0)
      {
        ScaleFactorUpdate();
        UpdateMap();
      }
    }

    protected void GalaxyMapViewer_ScrollChanged(object sender, RoutedEventArgs e)
    {
      double viewportWidth = ViewportWidth;
      double viewportHeight = ViewportHeight;
      double currentCanvasWidth = GalaxyCanvas.Width;
      double currentCanvasHeight = GalaxyCanvas.Height;

      // Skip if viewport is invalid or unchanged
      if (viewportWidth == 0 || viewportWidth == ActualWidth || currentCanvasWidth - viewportWidth == canvasWidth)
        return;

      AdjustOffset(
        isHorizontal: true,
        currentCanvasSize: currentCanvasWidth,
        viewportSize: viewportWidth,
        ref canvasToScrollWidthDelta,
        scrollOffset: scrollHorizontalOffset,
        scrollAction: ScrollToHorizontalOffset
      );

      AdjustOffset(
        isHorizontal: false,
        currentCanvasSize: currentCanvasHeight,
        viewportSize: viewportHeight,
        ref canvasToScrollHeightDelta,
        scrollOffset: scrollVerticalOffset,
        scrollAction: ScrollToVerticalOffset
      );
    }

    protected void AdjustOffset(
      bool isHorizontal,
      double currentCanvasSize,
      double viewportSize,
      ref double previousDelta,
      double scrollOffset,
      Action<double> scrollAction
    )
    {
      if (currentCanvasSize <= viewportSize)
        return;

      double newDelta = currentCanvasSize - viewportSize;
      if (Math.Abs(newDelta - previousDelta) < double.Epsilon)
        return;

      string axis = isHorizontal ? "Horizontal" : "Vertical";
      Log.Debug($"{axis} Actual: {currentCanvasSize}, Viewport: {viewportSize}");

      if (_zoomScalePrevious == 1.0)
      {
        // Auto-center when zooming out to 100%
        double newOffset = (currentCanvasSize - viewportSize) / 2;
        Log.Debug($"Auto-centering {axis.ToLower()} offset to {newOffset}");
        scrollAction(newOffset);
      }
      else if (previousDelta > 0)
      {
        double newOffset = scrollOffset * (newDelta / previousDelta);

        Log.Debug($"{axis} offset scale: {scrollOffset} -> {newOffset}");
        scrollAction(newOffset);
      }

      previousDelta = newDelta;
    }

    protected void GalaxyMapViewer_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
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

    protected void GalaxyMapViewer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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
    protected void GalaxyMapViewer_MouseMove(object sender, MouseEventArgs e)
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
    protected void GalaxyMapViewer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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
        // Smooth, deferred zoom: apply fast GPU transform now, commit layout later
        double prevScale = _deferredZoomScale;
        double factor = e.Delta > 0 ? ZoomInStep : ZoomOutStep;
        double targetScale = prevScale * factor;

        // Clamp the effective hexagon width to limits
        double effectiveWidth = HexagonWidth * targetScale;
        double clampedWidth = Math.Max(HexagonWidthMinimal * ZoomMinFactor, Math.Min(effectiveWidth, HexagonWidthMaximal * ZoomMaxFactor));
        targetScale = clampedWidth / HexagonWidth;

        _deferredZoomScale = targetScale;

        // Apply zoom transform around viewport center
        Point anchor = GetViewportCenterInCanvas();
        _zoomScale.ScaleX = _deferredZoomScale;
        _zoomScale.ScaleY = _deferredZoomScale;
        _zoomTranslate.X = (1 - _deferredZoomScale) * anchor.X;
        _zoomTranslate.Y = (1 - _deferredZoomScale) * anchor.Y;

        // Restart debounce to commit
        _zoomCommitTimer?.Stop();
        _zoomCommitTimer?.Start();
        e.Handled = true;
      }
    }

    Point GetViewportCenterInCanvas()
    {
      // 1. Get scroll offsets and viewport size
      double offsetX = HorizontalOffset;
      double offsetY = VerticalOffset;
      double viewportCenterX = offsetX + ViewportWidth / 2;
      double viewportCenterY = offsetY + ViewportHeight / 2;

      // 2. Inverse transform from visual space to canvas space
      // If you're using RenderTransform (e.g. ScaleTransform), apply its inverse
      var transform = GalaxyCanvas.RenderTransform as Transform;
      if (transform != null && transform.Value.HasInverse)
      {
        var inverse = transform.Value;
        inverse.Invert();
        return inverse.Transform(new Point(viewportCenterX, viewportCenterY));
      }

      // 3. If no transform, return raw center in canvas coordinates
      return new Point(viewportCenterX, viewportCenterY);
    }

    protected void CommitDeferredZoom()
    {
      _zoomCommitTimer?.Stop();
      _zoomScalePrevious = IsNotZoomed() ? 1.0 : ExtentWidth / ViewportWidth;
      if (Math.Abs(_deferredZoomScale - 1.0) < 0.0001)
      {
        return;
      }
      // Compute target HexagonWidth within bounds and reset transform
      double targetWidth = Math.Max(HexagonWidthMinimal, Math.Min(HexagonWidth * _deferredZoomScale, HexagonWidthMaximal));

      _zoomScale.ScaleX = 1.0;
      _zoomScale.ScaleY = 1.0;
      _zoomTranslate.X = 0.0;
      _zoomTranslate.Y = 0.0;
      _deferredZoomScale = 1.0;

      // Setting HexagonWidth will trigger UpdateMap and resize the layout-backed canvas
      HexagonWidth = targetWidth;
    }

    protected void MapOptions_PropertyChanged(object? sender, PropertyChangedEventArgs e)
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

  public class GalaxyMapCell(Cluster cluster, int col, int row, bool isProcessed = false)
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
      return [.. cells.Where(cell => !cell.IsProcessed).Select(cell => cell.Cluster)];
    }
  }

  public class MapOptions : INotifyPropertyChanged
  {
    protected string _name = string.Empty;
    protected string _version = string.Empty;
    protected string _id = string.Empty;
    protected bool _isChecked = false;

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
