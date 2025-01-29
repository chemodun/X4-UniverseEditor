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
        private static readonly double HexagonSizesRelation = Math.Sqrt(3)/2;    // Height of the hexagon in pixels (Width * sqrt(3)/2)
        private static readonly double HexagonHeightDefault = HexagonWidthDefault * HexagonSizesRelation;    // Height of the hexagon in pixels (Width * sqrt(3)/2)
        private double _hexagonWidth = HexagonWidthDefault;      // Width of the hexagon in pixels
        public double HexagonWidth {
            get => _hexagonWidth;
            set {
                _hexagonWidth = value;
                _hexagonHeight = value * HexagonSizesRelation;
                OnPropertyChanged(nameof(HexagonWidth));
                OnPropertyChanged(nameof(HexagonHeight));
                DrawClusterMap();
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

        private const double ColumnWidth = 15000000;  // 15,000,000 units for horizontal (X) axis
        private const double RowHeight = 17320000;    // 17,320,000 units for vertical (Z) axis

        private double ScaleFactor = 0.001;   // Scaling factor to convert units to pixels
        private readonly Dictionary<string, Dictionary<string, string>> mapDict = [];
        private readonly List<double> AxisZ = [];
        private readonly List<int> AxisX = [];

        // Reference to the main window's size (assumed to be passed or accessible)
        private readonly MainWindow MainWindowReference;

        public ClusterMapWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            DataContext = this;
            Owner = mainWindow;
            MainWindowReference = mainWindow;

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
            DrawClusterMap();
        }

        /// <summary>
        /// Prepares a map of clusters organized into rows and columns based on their positions.
        /// </summary>
        /// <returns>True if the map was prepared successfully; otherwise, false.</returns>
        public bool PrepareClusterMap()
        {
            if (MainWindowReference.Galaxy == null)
            {
                Log.Error("Galaxy data is not loaded.");
                return false;
            }

            // Determine the rowId and columnId for each cluster and populate the dictionary
            foreach (var cluster in MainWindowReference.Galaxy.Clusters)
            {
                string columnIdStr = $"{(int)Math.Floor(cluster.Position.X / ColumnWidth)}";
                string rowIdStr = $"{cluster.Position.Z / RowHeight:F1}";

                if (!mapDict.TryGetValue(rowIdStr, out Dictionary<string, string>? value))
                {
                    value = [];
                    mapDict[rowIdStr] = value;
                }

                if (cluster.Sectors.Count > 1)
                {
                    value[columnIdStr] = cluster.Name;
                }
                else
                {
                    value[columnIdStr] = $"{cluster.Sectors[0].Name} ({cluster.Name})";
                }

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

        /// <summary>
        /// Draws the cluster map using hexagons on the Canvas.
        /// </summary>
        private void DrawClusterMap()
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

            // Calculate canvas size with scaling
            double canvasWidth = (AxisX.Count * 0.75 + 0.25) * HexagonWidth * ScaleFactor;
            double canvasHeight = (AxisZ.Count + 1) * 0.5 * HexagonHeight * ScaleFactor;

            // Set Canvas size
            ClusterCanvas.Width = canvasWidth;
            ClusterCanvas.Height = canvasHeight;

            ClusterCanvas.Children.Clear();

            foreach (var Z in AxisZ)
            {
                Dictionary<string, string> row = mapDict[Z.ToString("F1")];
                foreach (var X in AxisX)
                {
                    if (!row.ContainsKey(X.ToString()) )
                    {
                        continue;
                    }
                    string clusterName = row[X.ToString()];
                    // Calculate pixel positions
                    double x = (X - minCol) * HexagonWidth * 3 / 4  * ScaleFactor;
                    double y = (maxRow - Z) * HexagonHeight * ScaleFactor;

                    // Create hexagon with centered text
                    Grid hexGrid = CreateHexagon(x, y, HexagonWidth * ScaleFactor, HexagonHeight * ScaleFactor, clusterName);

                    // Add hexagon grid to Canvas
                    ClusterCanvas.Children.Add(hexGrid);
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
            DrawClusterMap();
        }

        /// <summary>
        /// Creates a hexagon Polygon with centered TextBlock wrapped in a Grid.
        /// </summary>
        /// <param name="x">Top-left X position on Canvas.</param>
        /// <param name="y">Top-left Y position on Canvas.</param>
        /// <param name="width">Width of the hexagon.</param>
        /// <param name="height">Height of the hexagon.</param>
        /// <param name="text">Text to display at the center.</param>
        /// <returns>Grid containing the hexagon and the TextBlock.</returns>
        private static Grid CreateHexagon(double x, double y, double width, double height, string text)
        {
            // Create the hexagon polygon
            Polygon hex = new()
            {
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                Fill = Brushes.LightGray,
                Tag = text
            };

            // Define the points of the hexagon relative to (0,0)
            PointCollection points =
            [
                new Point(width * 0.25, 0),
                new Point(width * 0.75, 0),
                new Point(width, height * 0.5),
                new Point(width * 0.75, height),
                new Point(width * 0.25, height),
                new Point(0, height * 0.5)
            ];

            hex.Points = points;

            // Create TextBlock
            TextBlock tb = new()
            {
                Text = text,
                Foreground = Brushes.Black,
                FontSize = Math.Min(height * 0.2, 8), // Initial proportional font size
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };

            // Create Grid to contain Hexagon and TextBlock
            Grid grid = new()
            {
                Width = width,
                Height = height
            };

            grid.Children.Add(hex);
            grid.Children.Add(tb);

            // Position the Grid on the Canvas
            Canvas.SetLeft(grid, x);
            Canvas.SetTop(grid, y);

            // Add click event to hexagon
            hex.MouseLeftButtonUp += (s, e) =>
            {
                MessageBox.Show($"Cluster: {text}", "Cluster Information", MessageBoxButton.OK, MessageBoxImage.Information);
            };

            return grid;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}