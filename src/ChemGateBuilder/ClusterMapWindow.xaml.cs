using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.ComponentModel;
using Utilities.Logging;
using X4DataLoader;

namespace ChemGateBuilder
{
    public partial class ClusterMapWindow : Window, INotifyPropertyChanged
    {
        // Constants for hexagon dimensions and scaling
        private const double HexagonWidthDefault = 100;      // Width of the hexagon in pixels
        public static readonly double HexagonSizesRelation = Math.Sqrt(3)/2;    // Height of the hexagon in pixels (Width * sqrt(3)/2)
        private static readonly double HexagonHeightDefault = HexagonWidthDefault * HexagonSizesRelation;    // Height of the hexagon in pixels (Width * sqrt(3)/2)
        private double _hexagonWidth = HexagonWidthDefault;      // Width of the hexagon in pixels
        public double HexagonWidth {
            get => _hexagonWidth;
            set {
                _hexagonWidth = value;
                _hexagonHeight = value * HexagonSizesRelation;
                OnPropertyChanged(nameof(HexagonWidth));
                OnPropertyChanged(nameof(HexagonHeight));
                UpdateMap();
            }
        }      // Width of the hexagon in pixel

        private double _hexagonHeight = HexagonWidthDefault * HexagonSizesRelation;    // Height of the hexagon in pixels (Width * sqrt(3)/2)
        public double HexagonHeight {
            get => _hexagonHeight;
            set {}
        }    // Height of the hexagon in pixels (Width * sqrt(3)/2)
        private double _hexagonWidthMinimal = HexagonWidthDefault;
        public double HexagonWidthMinimal {
            get => _hexagonWidthMinimal;
            set {
                _hexagonWidthMinimal = value;
                OnPropertyChanged(nameof(HexagonWidthMinimal));
            }
        }
        private double _hexagonWidthMaximal = HexagonWidthDefault;
        public double HexagonWidthMaximal {
            get => _hexagonWidthMaximal;
            set {
                _hexagonWidthMaximal = value;
                OnPropertyChanged(nameof(HexagonWidthMaximal));
            }
        }

        private const double ColumnWidth = 15000000;  // 15,000,000 meters for horizontal (X) axis
        private const double RowHeight = 17320000;    // 17,320,000 meters for vertical (Z) axis

        public double ScaleFactor = 0.001;   // Scaling factor to convert units to pixels
        private double _canvasWidthBase = 0;
        private double _canvasHeightBase = 0;
        private readonly Dictionary<string, Dictionary<string, Cluster>> mapDict = [];
        private readonly List<double> AxisZ = [];
        private readonly List<int> AxisX = [];

        // Reference to the main window's size (assumed to be passed or accessible)
        private readonly MainWindow MainWindowReference;
        private readonly Galaxy? Galaxy;

        private readonly List<ClusterMapCluster> _clusters = [];

        public Sector? SelectedSector = null;

        public ClusterMapWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            DataContext = this;
            Owner = mainWindow;
            MainWindowReference = mainWindow;
            Galaxy = MainWindowReference.Galaxy;

            // Set window size to 90% of the main window
            Width = mainWindow.ActualWidth * 0.9;
            Height = mainWindow.ActualHeight * 0.9;

            // Center the window relative to the main window
            Left = mainWindow.Left + (mainWindow.Width - Width) / 2;
            Top = mainWindow.Top + (mainWindow.Height - Height) / 2;
            if (!PrepareClusterMap())
            {
                Log.Error("Cluster map is not prepared.");
                return;
            }
            UpdateMap();
        }

        /// <summary>
        /// Prepares a map of clusters organized into rows and columns based on their positions.
        /// </summary>
        /// <returns>True if the map was prepared successfully; otherwise, false.</returns>
        public bool PrepareClusterMap()
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

                if (!mapDict.TryGetValue(rowIdStr, out Dictionary<string, Cluster>? value))
                {
                    value = [];
                    mapDict[rowIdStr] = value;
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
            return true;
        }

        private void CreateMap()
        {
            if (mapDict.Count == 0)
            {
                Log.Warn("Cluster map is empty.");
                return;
            }
            if (ClusterScrollViewer.ActualHeight == 0 || ClusterScrollViewer.ActualWidth == 0)
            {
                Log.Warn("ClusterScrollViewer size is zero.");
                return;
            }
            double maxRow = AxisZ.First();
            int minCol = AxisX.First();

            _canvasWidthBase = AxisX.Count * 0.75 + 0.25;
            _canvasHeightBase = (AxisZ.Count + 1) * 0.5;

            ClusterCanvas.Width = _canvasWidthBase * HexagonWidth * ScaleFactor;
            ClusterCanvas.Height = _canvasHeightBase * HexagonHeight * ScaleFactor;

            ClusterCanvas.Children.Clear();
            foreach (var Z in AxisZ)
            {
                Dictionary<string, Cluster> row = mapDict[Z.ToString("F1")];
                foreach (var X in AxisX)
                {
                    if (!row.ContainsKey(X.ToString()) )
                    {
                        continue;
                    }
                    Cluster cluster = row[X.ToString()];
                    ClusterMapCluster clusterMapCluster = new(this, 0.75 * (X - minCol), maxRow - Z, ClusterCanvas, cluster);
                    clusterMapCluster.Create();
                    _clusters.Add(clusterMapCluster);
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
                ClusterCanvas.Width = _canvasWidthBase * HexagonWidth * ScaleFactor;
                ClusterCanvas.Height = _canvasHeightBase * HexagonHeight * ScaleFactor;
                foreach (ClusterMapCluster cluster in _clusters)
                {
                    cluster.Update();
                }
            }
        }

        private void ClusterMapWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ClusterScrollViewer.ActualWidth != 0 && ClusterScrollViewer.ActualHeight != 0)
            {
                double width = ClusterScrollViewer.ActualWidth;
                double height = ClusterScrollViewer.ActualHeight;
                double scaleFactorWidth = width / (AxisX.Count * 0.75 + 0.25) / HexagonWidthDefault;
                double scaleFactorHeight = height / (AxisZ.Count + 1)  / 0.5 / HexagonHeightDefault;
                ScaleFactor = Math.Min(scaleFactorWidth, scaleFactorHeight);
                if (width * HexagonSizesRelation < height)
                {
                    HexagonWidthMaximal = width / ScaleFactor;
                }
                else
                {
                    HexagonWidthMaximal = height / HexagonSizesRelation / ScaleFactor;
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

    class ClusterMapCluster(ClusterMapWindow map, double x, double y, Canvas canvas, Cluster cluster) : INotifyPropertyChanged
    {
        protected Cluster? Cluster = cluster;

        private readonly double _x = x;
        protected double X {
            get {
                if (Map != null)
                {
                    return _x * Map.HexagonWidth * Map.ScaleFactor;
                }
                return 0;
            }
        }

        private readonly double _y = y;
        protected double Y {
            get {
                if (Map != null)
                {
                    return _y * Map.HexagonHeight * Map.ScaleFactor;
                }
                return 0;
            }
        }
        protected double Width {
            get {
                if (Map != null)
                {
                    return Map.HexagonWidth * Map.ScaleFactor;
                }
                return 0;
            }
        }
        protected double Height {
            get {
                if (Map != null)
                {
                    return  Map.HexagonHeight * Map.ScaleFactor;
                }
                return 0;
            }
        }
        protected ClusterMapWindow? Map = map;
        protected Canvas? Canvas = canvas;
        protected PointCollection Points = [];
        protected Polygon? Hexagon = null;

        private readonly List<ClusterMapSector> _sectors = [];

        public virtual void Create()
        {
            if (Map == null ||Cluster == null || Canvas == null)
            {
                return;
            }
            UpdatePoints();
            Log.Debug($"Creating cluster {Cluster.Name} at ({X}, {Y}) ({x}, {y}) with Points {string.Join(", ", Points.Select(p => $"({p.X}, {p.Y})"))})");
            Hexagon = new()
            {
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                Fill = Brushes.LightGray,
                Tag = Cluster.Name,
                DataContext = Cluster,
                Points = Points
            };
            // Position the Hexagon on the Canvas
            Canvas.SetLeft(Hexagon, X);
            Canvas.SetTop(Hexagon, Y);
            Canvas.Children.Add(Hexagon);
            if (Cluster.Sectors.Count == 1)
            {
                Sector sector = Cluster.Sectors[0];
                ClusterMapSector clusterMapSector = new(Map, _x, _y, Canvas, Cluster, sector);
                clusterMapSector.Create();
                _sectors.Add(clusterMapSector);
            }
        }

        public virtual void Update()
        {
            if (Cluster == null || Canvas == null || Hexagon == null)
            {
                return;
            }
            UpdatePoints();
            Hexagon.Points = Points;
            // Position the Hexagon on the Canvas
            Canvas.SetLeft(Hexagon, X);
            Canvas.SetTop(Hexagon, Y);
            foreach (ClusterMapSector sector in _sectors)
            {
                sector.Update();
            }
        }

        protected void UpdatePoints()
        {
            Points.Clear();
            Points.Add(new Point(Width * 0.25, 0));
            Points.Add(new Point(Width * 0.75, 0));
            Points.Add(new Point(Width, Height * 0.5));
            Points.Add(new Point(Width * 0.75, Height));
            Points.Add(new Point(Width * 0.25, Height));
            Points.Add(new Point(0, Height * 0.5));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    class ClusterMapSector(ClusterMapWindow map, double x, double y, Canvas canvas, Cluster cluster, Sector sector) : ClusterMapCluster(map, x, y, canvas, cluster)
    {
        protected Sector? Sector = sector;
        protected Grid? Grid = null;
        protected TextBox? TextBox = null;

        public override void Create()
        {
            if (Cluster == null || Sector == null || Canvas == null)
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
                Points = Points
            };
            Grid = new()
            {
                Width = Width,
                Height = Height,
                DataContext = Sector
            };

            Grid.Children.Add(Hexagon);
            // Create TextBox
            TextBox = new()
            {
                Text = Sector.Name,
                Foreground = Brushes.Black,
                FontSize = Math.Min(Height * 0.1, 22), // Initial proportional font size
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                TextWrapping = TextWrapping.Wrap,
                BorderThickness = new Thickness(1),
                IsReadOnly = true
            };
            Grid.Children.Add(TextBox);
            // Position the Hexagon on the Canvas
            Canvas.SetLeft(Grid, X);
            Canvas.SetTop(Grid, Y);
            Canvas.Children.Add(Grid);
            Grid.MouseLeftButtonUp += (s, e) =>
            {
                Sector? sector = null;
                if (s is Polygon polygon && polygon.DataContext is Sector _sectorFromPolygon)
                {
                    sector = _sectorFromPolygon;
                }
                else if (s is Grid grid && grid.DataContext is Sector _sectorFromGrid)
                {
                    sector = _sectorFromGrid;
                }
                if (sector != null)
                {
                    MessageBoxResult result = MessageBox.Show($"Do you want to select Sector: {sector.Name}", "Select Sector", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes && Map != null)
                    {
                        Map.SelectedSector = sector;
                        Map.Close();
                    }
                }
            };
        }

        public override void Update()
        {
            if (Cluster == null || Sector == null || Canvas == null || Hexagon == null || Grid == null || TextBox == null)
            {
                return;
            }
            UpdatePoints();
            Hexagon.Points = Points;
            // Update TextBox
            TextBox.FontSize = Math.Min(Height * 0.1, 22);
            Grid.Width = Width;
            Grid.Height = Height;
            // Position the Hexagon on the Canvas
            Canvas.SetLeft(Grid, X);
            Canvas.SetTop(Grid, Y);
        }
    }
}