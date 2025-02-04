using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Input;
using Utilities.Logging;
using System.Windows.Shapes;
using X4DataLoader;
using System.Windows.Media.Media3D;

namespace ChemGateBuilder
{
    public class SectorMap : INotifyPropertyChanged
    {
        private double _minInternalSizeKm = 100;
        private double _maxInternalSizeKm = 999;
        private double _visualX;
        private double _visualY;
        protected double _visualSizePx = 200; // Default size
        protected double _internalSizeKm = 400;
        private string? _selectedItemId = "";
        private string _ownerColor = OwnerColorInitial;
        public static readonly string OwnerColorInitial = "#F0F0F0";
        private List<string> StationsToDisplay = new() { "equipmentdock", "tradestation", "tradingstation", "shipyard", "wharf" };
        private FactionColors FactionColors = new();
        public bool MapMode = false;

        public double MinInternalSizeKm
        {
            get => _minInternalSizeKm;
            set
            {
                if (_minInternalSizeKm != value)
                {
                    _minInternalSizeKm = value;
                    OnPropertyChanged(nameof(MinInternalSizeKm));
                }
            }
        }
        public double MaxInternalSizeKm
        {
            get => _maxInternalSizeKm;
            set
            {
                if (_maxInternalSizeKm != value)
                {
                    _maxInternalSizeKm = value;
                    OnPropertyChanged(nameof(MaxInternalSizeKm));
                }
            }
        }
        public double VisualX
        {
            get => _visualX;
            set { _visualX = value; OnPropertyChanged(); }
        }
        public double VisualY
        {
            get => _visualY;
            set { _visualY = value; OnPropertyChanged(); }
        }
        public double VisualSizePx
        {
            get => _visualSizePx;
            set { _visualSizePx = value; OnPropertyChanged(); }
        }
        public double InternalSizeKm
        {
            get => _internalSizeKm;
            set
            {
                _internalSizeKm = value;
                OnPropertyChanged();
                UpdateItems();
            }
        }
        public string? SelectedItemId
        {
            get => _selectedItemId;
            set { _selectedItemId = value; OnPropertyChanged(); }
        }

        public string OwnerColor
        {
            get => _ownerColor;
            set
            {
                _ownerColor = value;
                OnPropertyChanged();
            }
        }

        public static readonly double ItemSizeMinDefaultPx = 10;
        public static readonly double MinVisualSectorSize = 50;
        public static readonly double MaxVisualSectorSize = 1200;

        public double ItemSizeMinPx = ItemSizeMinDefaultPx;

        public ObservableCollection<SectorMapItem> Items { get; set; } = [];

        public bool IsDragging = false;
        public SectorMapItem? SelectedItem = null;
        public System.Windows.Point MouseOffset;
        public Canvas? MapCanvas;
        public Polygon? MapHexagon;
        private Sector? Sector;
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void SetColors(FactionColors factionColors)
        {
            FactionColors = factionColors;
        }
        public void UpdateSectorColor(string id)
        {
            string color = FactionColors.GetColorString(id);
            if (!String.IsNullOrEmpty(color))
            {
                OwnerColor = color;
            }

        }
        public void Connect(Canvas canvas, Polygon hexagon, bool mapMode = false)
        {
            MapCanvas = canvas;
            MapHexagon = hexagon;
            MapMode = mapMode;
        }
        public void ClearItems()
        {
            Items.Clear();
        }

        public List<SectorConnectionData> SetSector(Sector? sector, Galaxy galaxy)
        {
            Sector = sector;
            ClearItems();
            List<SectorConnectionData> sectorConnections = [];
            if (Sector != null && Sector.Zones != null && Sector.Zones.Count != 0)
            {
                foreach (var zone in Sector.Zones)
                {
                    if (zone.Connections == null || zone.Connections.Count == 0) continue;
                    foreach (var connection in zone.Connections.Values)
                    {
                        if (connection is GateConnection gateConnection)
                        {
                            bool active = gateConnection.IsActive;
                            string? sectorTo = active ? galaxy.GetOppositeSectorForGateConnection(gateConnection)?.Name : "";
                            Position zoneCoordinates = zone.Position;
                            if (zoneCoordinates == null) continue;
                            Position? gateCoordinates = gateConnection.Position;
                            if (gateCoordinates == null) continue;
                            SectorConnectionData newConnection = new()
                            {
                                Active = active && !string.IsNullOrEmpty(sectorTo),
                                ToSector = sectorTo ?? "",
                                X = (int)((zoneCoordinates.X + gateCoordinates.X) / 1000),
                                Y = (int)((zoneCoordinates.Y + gateCoordinates.Y) / 1000),
                                Z = (int)((zoneCoordinates.Z + gateCoordinates.Z) / 1000),
                                Type = "gate",
                                From = "map",
                                Id = gateConnection.Name
                            };
                            sectorConnections.Add(newConnection);
                            AddItem(newConnection);
                        }
                    }
                }
                foreach (HighwayPoint highwayPoint in Sector.HighwayPoints)
                {
                    if (highwayPoint.Position == null) continue;
                    if (highwayPoint.HighwayLevel != HighwayLevel.Cluster) continue;
                    SectorConnectionData newConnection = new()
                    {
                        Active = true,
                        ToSector = highwayPoint.SectorConnected?.Name ?? "",
                        X = (int)(highwayPoint.Position.X / 1000),
                        Y = (int)(highwayPoint.Position.Y / 1000),
                        Z = (int)(highwayPoint.Position.Z / 1000),
                        Type = "highway",
                        From = "map",
                        Id = highwayPoint.Name
                    };
                    newConnection.Attributes.Add("PointType", highwayPoint.Type == HighwayPointType.EntryPoint ? "entry" : "exit");
                    sectorConnections.Add(newConnection);
                    AddItem(newConnection);
                }
                List<Station> stations = Sector.GetStationsByTagsOrTypes(StationsToDisplay);
                foreach (Station station in stations)
                {
                    if (station.Position == null) continue;
                    SectorConnectionData newConnection = new()
                    {
                        Active = true,
                        ToSector = "",
                        X = (int)(station.Position.X / 1000) + (station.Zone?.Position != null ? (int)(station.Zone.Position.X / 1000) : 0),
                        Y = (int)(station.Position.Y / 1000) + (station.Zone?.Position != null ? (int)(station.Zone.Position.Y / 1000) : 0),
                        Z = (int)(station.Position.Z / 1000) + (station.Zone?.Position != null ? (int)(station.Zone.Position.Z / 1000) : 0),
                        Type = "station",
                        From = "map",
                        Id = station.Name
                    };
                    string stationType = station.Tags.Count == 0 ? station.Type : station.Tags[0];
                    if (stationType == "tradingstation")
                    {
                        stationType = "tradestation";
                    }
                    newConnection.Attributes.Add("StationType", stationType);
                    sectorConnections.Add(newConnection);
                    AddItem(newConnection);
                }
            }
            return sectorConnections;
        }

        public void AddItem(SectorConnectionData connectionData)
        {
            SectorMapItem item = new()
            {
                SectorMap = this,
                ConnectionData = connectionData,
                IsNew = connectionData.Id == "New"
            };
            Items.Add(item);
            item.Update();
        }


        public void SelectItem(string? ItemId)
        {
            string? selectedItemId = SelectedItemId;
            SelectedItemId = ItemId;
            if (selectedItemId != ItemId)
            {
                if (selectedItemId != null && selectedItemId != "")
                {
                    RefreshItem(selectedItemId);
                }
                if (ItemId != null && ItemId != "")
                {
                    RefreshItem(ItemId);
                }
            }
        }

        public bool RefreshItem(string ItemId)
        {
            SectorMapItem? item = Items.FirstOrDefault(i => i.ConnectionData?.Id == ItemId);
            if (item != null)
            {
                item.Update();
                return true;
            }
            return false;
        }

        public void UpdateItem(SectorConnectionData? connectionData)
        {

            SectorMapItem? item = Items.FirstOrDefault(i => i.ConnectionData?.Id == connectionData?.Id);
            if (item != null)
            {
                if (connectionData != null)
                {
                    item.ConnectionData = connectionData;
                    item.Update();
                }
                else
                {
                    Items.Remove(item);
                }
            }
            else
            {
                if (connectionData != null) AddItem(connectionData);
            }
        }

        private void UpdateItems()
        {
            foreach (var item in Items.ToArray())
            {
                item.Update();
            }
        }
        public void OnSizeChanged(double newWidth, double newHeight)
        {
            double newSize = Math.Min(newWidth, newHeight / 0.866);
            VisualSizePx = Math.Min(Math.Max(newSize, MinVisualSectorSize), MaxVisualSectorSize);
            if (VisualSizePx < newWidth)
            {
                VisualX = (newWidth - VisualSizePx) / 2;
            }
            else
            {
                VisualX = 0;
            }
            if (VisualSizePx < newHeight / 0.866)
            {
                VisualY = (newHeight / 0.866 - VisualSizePx) / 2;
            }
            else
            {
                VisualY = 0;
            }
            UpdateItems();
        }
        public void SetInternalSize(int sizeKm)
        {
            InternalSizeKm = sizeKm;
            UpdateItems();
        }

        public SectorMapItem? GetItem(string id)
        {
            return Items.FirstOrDefault(i => i.ConnectionData != null && i.ConnectionData.Id == id);
        }

        public void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Image image && image.DataContext is SectorMapItem item && this != null)
            {
                SelectedItem = item;
                if (item.IsNew) // Only allow dragging for "new" gates
                {
                    IsDragging = true;

                    // Get the mouse position relative to the gate
                    MouseOffset = e.GetPosition(image);

                    // Capture the mouse to receive MouseMove events even if the cursor leaves the image
                    image.CaptureMouse();
                }
                Log.Debug($"[MouseLeftButtonDown] Selected Item: {SelectedItem?.ConnectionData?.Id}, IsDragging: {IsDragging}, MouseOffset: {MouseOffset}");
            }
        }

        public void MouseMove(object sender, MouseEventArgs e, Coordinates coordinates)
        {
            if (SelectedItem != null && IsDragging)
            {
                double halfSize = SelectedItem.ItemSizePx / 2;
                SectorMapItem selectedItem = SelectedItem;
                Log.Debug($"[MouseMove] Selected Item: {SelectedItem?.ConnectionData?.Id}, IsDragging: {IsDragging}, MouseOffset: {MouseOffset}, sender: {sender}, isImage: {sender is Image}");
                if (sender is Image && MapCanvas != null && MapHexagon != null)
                {
                    // Get the current mouse position relative to the SectorCanvas
                    Point mousePosition = e.GetPosition(MapCanvas);
                    // Calculate new position by subtracting the offset
                    double newX = mousePosition.X - MouseOffset.X;
                    double newY = mousePosition.Y - MouseOffset.Y;
                    // Account the size of the item
                    Point newPoint = new(newX + halfSize - VisualX, newY + halfSize - VisualY);
                    // Check if the new position is inside the hexagon
                    bool isInside = MapHexagon.RenderedGeometry.FillContains(newPoint);
                    Log.Debug($"[MouseMove] IsInside: {isInside}");
                    if (isInside)
                    {
                        // Update the SectorMapItem's coordinates
                        selectedItem.X = newX;
                        selectedItem.Y = newY;
                        if (coordinates != null)
                        {
                            selectedItem.UpdateInternalCoordinates(coordinates);
                        }
                        Log.Debug($"[MouseMove] New X: {newX}, New Y: {newY}");
                    }
                }
            }
        }

        public SectorConnectionData? MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (SelectedItem != null)
            {
                IsDragging = false;
                SelectedItem = null;

                if (sender is Image image && image.DataContext is SectorMapItem item && item != null)
                {
                    image.ReleaseMouseCapture();
                    if (!item.IsNew)
                    {
                        return item.ConnectionData;
                    }
                }
            }
            return null;
        }
    }

    public class SectorMapItem : INotifyPropertyChanged
    {
        private double _x;
        private double _y;
        private static readonly double _sizeCoefficient = 0.03;
        private double _itemSizePx = SectorMap.ItemSizeMinDefaultPx;
        private bool _isNew;
        private string _toolTip = "";
        private SectorConnectionData? _connectionData;
        private SectorMap? _sectorMap;
        public SectorMap? SectorMap
        {
            get => _sectorMap;
            set
            {
                _sectorMap = value;
                UpdateSize();
                UpdatePosition();
            }
        }
        public SectorConnectionData? ConnectionData
        {
            get => _connectionData;
            set
            {
                _connectionData = value;
                UpdatePosition();
            }
        }
        public string Type
        {
            get
            {
                if (_connectionData == null || _connectionData?.Type == null)
                    return "empty";
                return _connectionData.Type;
            }
        } // e.g., "empty", "gate", "highway", etc.
        public string From
        {
            get
            {
                if (_connectionData == null || _connectionData?.From == null)
                    return "unknown";
                return _connectionData.From;
            }
        } // e.g., "new", "mod", "map", etc.
        public string Status
        {
            get
            {
                if (_connectionData == null || _connectionData?.Active == null)
                    return "unknown";
                return _connectionData.Active ? "active" : "inactive";
            }
        } // e.g., "active", "inactive", "unknown"
        public string? Id
        {
            get
            {
                if (_connectionData == null)
                    return "unknown";
                return _connectionData.Id;
            }
        }
        public string ToolTip
        {
            get => _toolTip;
            set { _toolTip = value; OnPropertyChanged(); }
        }
        public double ItemSizePx
        {
            get => _itemSizePx;
            set { _itemSizePx = value; OnPropertyChanged(); OnPropertyChanged(nameof(CenterX)); OnPropertyChanged(nameof(CenterY)); }
        }
        public double X
        {
            get => _x;
            set { _x = value; OnPropertyChanged(); OnPropertyChanged(nameof(CenterX)); }
        }

        public double Y
        {
            get => _y;
            set { _y = value; OnPropertyChanged(); OnPropertyChanged(nameof(CenterY)); }
        }

        public bool IsNew
        {
            get => _isNew;
            set { _isNew = value; OnPropertyChanged(); }
        }

        // Computed Properties for Center Coordinates
        public double CenterX { get => X + ItemSizePx / 2; }
        public double CenterY { get => Y + ItemSizePx / 2; }

        private Dictionary<string, string> Attributes
        {
            get => ConnectionData?.Attributes ?? [];
        }

        public void Update()
        {
            UpdateSize();
            UpdatePosition();
        }

        private void UpdateSize()
        {
            if (SectorMap == null)
                return;
            double newSizePx = SectorMap.VisualSizePx * _sizeCoefficient;
            ItemSizePx = Math.Max(newSizePx, SectorMap.ItemSizeMinPx);
            if (Type == "highway")
            {
                if (Attributes.TryGetValue("PointType", out string? pointType) && pointType == "exit")
                {
                    ItemSizePx *= 0.7;
                }
            }
            else if (Type == "station")
            {
                if (SectorMap.MapMode)
                {
                    ItemSizePx = Math.Min(Math.Max(SectorMap.VisualSizePx * 0.1, 20), 50);
                }
            }
        }

        private void UpdatePosition()
        {
            if (SectorMap == null || ConnectionData == null)
                return;
            X = SectorMap.VisualX + (ConnectionData.X * SectorMap.VisualSizePx / SectorMap.InternalSizeKm + SectorMap.VisualSizePx - ItemSizePx) / 2;
            Y = SectorMap.VisualY - SectorMap.VisualSizePx * 0.067 + (-ConnectionData.Z * SectorMap.VisualSizePx / SectorMap.InternalSizeKm + SectorMap.VisualSizePx - ItemSizePx) / 2;
            OnPropertyChanged(nameof(X));
            OnPropertyChanged(nameof(Y));
            UpdateToolTip();
        }

        public void UpdateInternalCoordinates(Coordinates coordinates)
        {
            if (SectorMap == null)
                return;
            coordinates.X = (int)(((X - SectorMap.VisualX) * 2 + ItemSizePx - SectorMap.VisualSizePx) * SectorMap.InternalSizeKm / SectorMap.VisualSizePx);
            coordinates.Z = (int)((SectorMap.VisualSizePx + (SectorMap.VisualY - SectorMap.VisualSizePx * 0.067 - Y) * 2 - ItemSizePx) * SectorMap.InternalSizeKm / SectorMap.VisualSizePx);
            if (_connectionData == null)
                return;
            _connectionData.X = coordinates.X;
            _connectionData.Z = coordinates.Z;
            UpdateToolTip();
        }
        // Colors based on gate type and status

        private void UpdateToolTip()
        {
            if (_connectionData == null)
                ToolTip = "No connection data";
            string result = $"{char.ToUpper(Type[0])}{Type[1..]}";
            if (Type == "gate" || Type == "highway")
                result += $": {Status} ({From})\n";
            if (Type == "gate")
            {
                result += $"To: {_connectionData?.ToSector ?? ""}\n";
            }
            else if (Type == "highway")
            {
                if (Attributes.TryGetValue("PointType", out string? pointType))
                {
                    string fromTo = pointType == "entry" ? "to" : "from";
                    result += $"{char.ToUpper(pointType[0])}{pointType[1..]} point {fromTo} {_connectionData?.ToSector ?? ""}\n";
                }
            }
            else if (Type == "station")
            {
                result = $"{Id}\n";
            }
            result += $"X: {_connectionData?.X ?? 0,4}, Y: {_connectionData?.Y ?? 0,4}, Z: {_connectionData?.Z ?? 0,4}";
            ToolTip = result;
        }

        public BitmapImage ObjectImage
        {
            get
            {
                string imagePath = "pack://application:,,,/Assets/mapob_";
                switch (Type)
                {
                    case "empty":
                        imagePath += "jumpgate_default";
                        break;
                    case "gate":
                        imagePath += "jumpgate";
                        imagePath += From switch
                        {
                            "new" => "_new",
                            "mod" => "_mod",
                            _ => "_map",
                        };
                        imagePath += Status switch
                        {
                            "active" => "_active",
                            "inactive" => "_inactive",
                            _ => "_unknown",
                        };
                        break;
                    case "highway":
                        {
                            imagePath += "superhighway";
                            imagePath += From switch
                            {
                                "new" => "_new",
                                "mod" => "_mod",
                                _ => "_map",
                            };
                            imagePath += Status switch
                            {
                                "active" => "_active",
                                "inactive" => "_inactive",
                                _ => "_unknown",
                            };
                            break;
                        }
                    case "station":
                        {
                            imagePath += Attributes.TryGetValue("StationType", out string? stationType) ? $"{stationType}" : "unknown";
                            break;
                        }
                    default:
                        imagePath += "unknown";
                        break;
                }
                imagePath += ".png";
                return new BitmapImage(new Uri(imagePath));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

namespace ChemGateBuilder.Core.Converters
{
    public class HexagonPointsConverter : IValueConverter
    {
        // Converts VisualSizePx (double) to PointCollection for a horizontally oriented hexagon
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double size && size > 0)
            {

                return Converter(size);
            }
            return DependencyProperty.UnsetValue;
        }

        public static PointCollection Converter(double size)
        {
            double radius = size / 2;
            PointCollection points = [];

            for (int i = 0; i < 6; i++)
            {
                double angle_deg = 60 * i; // Start at 0 degrees for flat-top
                double angle_rad = Math.PI / 180 * angle_deg;
                double x = radius + radius * Math.Cos(angle_rad);
                double y = radius + radius * Math.Sin(angle_rad);
                points.Add(new System.Windows.Point(x, y - size * 0.067));
            }
            return points;
        }
        // Not implemented as conversion back is not required
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}