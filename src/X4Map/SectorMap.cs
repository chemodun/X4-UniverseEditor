using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using Utilities.Logging;
using X4DataLoader;
using X4Map.Constants;

namespace X4Map
{
  public class SectorMap : INotifyPropertyChanged
  {
    private double _visualX;
    private double _visualY;
    protected double _visualSizePx = 200; // Default size
    protected double _internalSizeMinKm = MapConstants.SectorInternalSizeMinKm;
    protected double _internalSizeKm = MapConstants.SectorInternalSizeMinKm;
    protected double _internalSizeMaxKm = MapConstants.SectorInternalSizeMinKm * 5;
    private string? _selectedItemId = "";
    private string _ownerColor = OwnerColorInitial;
    public static readonly string OwnerColorInitial = "#F0F0F0";
    private readonly List<string> StationsToDisplay = ["equipmentdock", "tradestation", "tradingstation", "shipyard", "wharf"];
    public bool MapMode = false;
    public double VisualX
    {
      get => _visualX;
      set
      {
        _visualX = value;
        OnPropertyChanged();
      }
    }
    public double VisualY
    {
      get => _visualY;
      set
      {
        _visualY = value;
        OnPropertyChanged();
      }
    }
    public double VisualSizePx
    {
      get => _visualSizePx;
      set
      {
        _visualSizePx = value;
        OnPropertyChanged();
      }
    }
    public double InternalSizeMinKm
    {
      get => _internalSizeMinKm;
      set
      {
        _internalSizeMinKm = value;
        OnPropertyChanged();
      }
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
    public double InternalSizeMaxKm
    {
      get => _internalSizeMaxKm;
      set
      {
        _internalSizeMaxKm = value;
        OnPropertyChanged();
      }
    }
    public string? SelectedItemId
    {
      get => _selectedItemId;
      set
      {
        _selectedItemId = value;
        OnPropertyChanged();
      }
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
    public static readonly string NewGateId = "NewGate";
    public static readonly double HexagonSizesRelation = Math.Sqrt(3) / 2; // Height of the hexagon in pixels (Width * sqrt(3)/2)

    public double ItemSizeMinPx = ItemSizeMinDefaultPx;

    public ObservableCollection<SectorMapItem> Items { get; set; } = [];

    public bool IsDragging = false;
    public SectorMapItem? SelectedItem = null;
    public System.Windows.Point MouseOffset;
    public Canvas? MapCanvas;
    public Polygon? MapHexagon;
    private Sector? Sector;
    public event PropertyChangedEventHandler? PropertyChanged;

    public string Source
    {
      get
      {
        if (Sector == null)
          return "unknown";
        return Sector.Source;
      }
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public void Connect(Canvas canvas, Polygon hexagon, bool mapMode = false)
    {
      MapCanvas = canvas;
      MapHexagon = hexagon;
      MapMode = mapMode;
    }

    public void UnsetSector()
    {
      Sector = null;
      ClearItems();
      OwnerColor = SectorMap.OwnerColorInitial;
    }

    public void ClearItems()
    {
      Items.Clear();
    }

    public void From(SectorMap sectorMap)
    {
      InternalSizeKm = sectorMap.InternalSizeKm;
      Sector = sectorMap.Sector;
      OwnerColor = sectorMap.OwnerColor;
      ClearItems();
      foreach (SectorMapItem item in sectorMap.Items)
      {
        if (item != null && item.ObjectData != null)
        {
          AddItem(item.ObjectData);
        }
      }
      SelectedItemId = sectorMap.SelectedItemId;
      SetInternalSize(sectorMap.InternalSizeKm);
    }

    public List<ObjectInSector> SetSector(Sector? sector, Galaxy galaxy)
    {
      Sector = sector;
      ClearItems();
      string sectorOwner = sector?.DominantOwner ?? "";
      X4Color x4Color = sector?.Color ?? galaxy.Colors.Find(color => color.Id == "grey_128") ?? new X4Color();
      string color = $"#{x4Color.Red:X2}{x4Color.Green:X2}{x4Color.Blue:X2}";
      if (!String.IsNullOrEmpty(color))
      {
        OwnerColor = color;
      }
      List<ObjectInSector> sectorObjects = [];
      if (Sector != null && Sector.Zones != null && Sector.Zones.Count != 0)
      {
        foreach (var zone in Sector.Zones)
        {
          if (zone.Connections == null || zone.Connections.Count == 0)
            continue;
          foreach (var connection in zone.Connections.Values)
          {
            if (connection is GateConnection gateConnection)
            {
              bool active = gateConnection.IsActive;
              string? sectorTo = active ? galaxy.GetOppositeSectorForGateConnection(gateConnection)?.Name : "";
              Position zoneCoordinates = zone.Position;
              if (zoneCoordinates == null)
                continue;
              Position? gateCoordinates = gateConnection.Position;
              if (gateCoordinates == null)
                continue;
              ObjectInSector newObject = new()
              {
                Active = active && !string.IsNullOrEmpty(sectorTo),
                Info = sectorTo ?? "",
                X = (int)((zoneCoordinates.X + gateCoordinates.X) / 1000),
                Y = (int)((zoneCoordinates.Y + gateCoordinates.Y) / 1000),
                Z = (int)((zoneCoordinates.Z + gateCoordinates.Z) / 1000),
                Type = "gate",
                From = galaxy.Extensions.FirstOrDefault(e => e.Id == zone.Source)?.Name ?? "Vanilla",
                Id = gateConnection.Name,
              };
              sectorObjects.Add(newObject);
              AddItem(newObject);
            }
          }
        }
        foreach (HighwayPoint highwayPoint in Sector.HighwayPoints)
        {
          if (highwayPoint.Position == null)
            continue;
          if (highwayPoint.HighwayLevel != HighwayLevel.Cluster)
            continue;
          ObjectInSector newObject = new()
          {
            Active = true,
            Info = highwayPoint.SectorConnected?.Name ?? "",
            X = (int)(highwayPoint.Position.X / 1000),
            Y = (int)(highwayPoint.Position.Y / 1000),
            Z = (int)(highwayPoint.Position.Z / 1000),
            Type = "highway",
            From = galaxy.Extensions.FirstOrDefault(e => e.Id == highwayPoint.Source)?.Name ?? "Vanilla",
            Id = highwayPoint.Name,
          };
          newObject.Attributes.Add("PointType", highwayPoint.Type == HighwayPointType.EntryPoint ? "entry" : "exit");
          sectorObjects.Add(newObject);
          AddItem(newObject);
        }
        List<Station> stations = Sector.GetStationsByTagsOrTypes(StationsToDisplay);
        foreach (Station station in stations)
        {
          if (station.Position == null)
            continue;
          x4Color = station.Color ?? galaxy.Colors.Find(color => color.Id == "grey_64") ?? new X4Color();
          ObjectInSector newObject = new()
          {
            Active = true,
            Info = station.Name,
            X = (int)(station.Position.X / 1000) + (station.Zone?.Position != null ? (int)(station.Zone.Position.X / 1000) : 0),
            Y = (int)(station.Position.Y / 1000) + (station.Zone?.Position != null ? (int)(station.Zone.Position.Y / 1000) : 0),
            Z = (int)(station.Position.Z / 1000) + (station.Zone?.Position != null ? (int)(station.Zone.Position.Z / 1000) : 0),
            Type = "station",
            From = galaxy.Extensions.FirstOrDefault(e => e.Id == station.Source)?.Name ?? "Vanilla",
            Id = station.Id,
            Color = Color.FromArgb((byte)x4Color.Alpha, (byte)x4Color.Red, (byte)x4Color.Green, (byte)x4Color.Blue),
          };
          if (station.Zone == null && newObject.X == 0 && newObject.Y == 0 && newObject.Z == 0)
          {
            MakeRandomCoordinates(newObject);
          }
          string stationType = station.Tags.Count == 0 ? station.Type : station.Tags[0];
          if (stationType == "tradingstation")
          {
            stationType = "tradestation";
          }
          newObject.Attributes.Add("StationType", stationType);
          newObject.Attributes.Add("StationOwner", station.OwnerName);
          sectorObjects.Add(newObject);
          AddItem(newObject);
        }
      }
      return sectorObjects;
    }

    private static void MakeRandomCoordinates(ObjectInSector objectData)
    {
      Random random = new();
      objectData.X = random.Next(-30, 30);
      objectData.Y = 0;
      objectData.Z = random.Next(-30, 30);
    }

    public void AddItem(ObjectInSector objectData)
    {
      SectorMapItem item = new()
      {
        SectorMap = this,
        ObjectData = objectData,
        IsNew = objectData.Id == SectorMap.NewGateId,
      };
      Items.Add(item);
      double newInternalSize = objectData.GetMaxCoordinate(InternalSizeKm);
      if (newInternalSize > InternalSizeKm)
      {
        SetInternalSize(newInternalSize);
      }
      else
      {
        item.Update();
      }
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
      SectorMapItem? item = Items.FirstOrDefault(i => i.ObjectData?.Id == ItemId);
      if (item != null)
      {
        item.Update();
        return true;
      }
      return false;
    }

    public void UpdateItem(ObjectInSector? objectData)
    {
      SectorMapItem? item = Items.FirstOrDefault(i => i.ObjectData?.Id == objectData?.Id);
      if (item != null)
      {
        if (objectData != null)
        {
          item.ObjectData = objectData;
          item.Update();
        }
        else
        {
          Items.Remove(item);
        }
      }
      else
      {
        if (objectData != null)
          AddItem(objectData);
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

    public void SetInternalSize(double sizeKm)
    {
      InternalSizeKm = sizeKm;
      InternalSizeMinKm = Math.Max(MapConstants.SectorInternalSizeMinKm, sizeKm / 10);
      InternalSizeMaxKm = Math.Min(MapConstants.SectorInternalSizeMaxKm, sizeKm * 5);
      UpdateItems();
    }

    public SectorMapItem? GetItem(string id)
    {
      return Items.FirstOrDefault(i => i.ObjectData != null && i.ObjectData.Id == id);
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
        Log.Debug(
          $"[MouseLeftButtonDown] Selected Item: {SelectedItem?.ObjectData?.Id}, IsDragging: {IsDragging}, MouseOffset: {MouseOffset}"
        );
      }
    }

    public void MouseMove(object sender, MouseEventArgs e, ObjectCoordinates coordinates)
    {
      if (SelectedItem != null && IsDragging)
      {
        double halfSize = SelectedItem.ItemSizePx / 2;
        SectorMapItem selectedItem = SelectedItem;
        Log.Debug(
          $"[MouseMove] Selected Item: {SelectedItem?.ObjectData?.Id}, IsDragging: {IsDragging}, MouseOffset: {MouseOffset}, sender: {sender}, isImage: {sender is Image}"
        );
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

    public ObjectInSector? MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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
            return item.ObjectData;
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
    private ObjectInSector? _objectData;
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
    public ObjectInSector? ObjectData
    {
      get => _objectData;
      set
      {
        _objectData = value;
        UpdatePosition();
      }
    }
    public string Type
    {
      get
      {
        if (_objectData == null || _objectData?.Type == null)
          return "empty";
        return _objectData.Type;
      }
    } // e.g., "empty", "gate", "highway", etc.
    public string From
    {
      get
      {
        if (_objectData == null || _objectData?.From == null)
          return "unknown";
        return _objectData.From;
      }
    } // e.g., "new", "mod", "map", etc.
    public string Status
    {
      get
      {
        if (_objectData == null || _objectData?.Active == null)
          return "unknown";
        return _objectData.Active ? "active" : "inactive";
      }
    } // e.g., "active", "inactive", "unknown"
    public string? Id
    {
      get
      {
        if (_objectData == null)
          return "unknown";
        return _objectData.Id;
      }
    }
    public string ToolTip
    {
      get => _toolTip;
      set
      {
        _toolTip = value;
        OnPropertyChanged();
      }
    }
    public double ItemSizePx
    {
      get => _itemSizePx;
      set
      {
        _itemSizePx = value;
        OnPropertyChanged();
        OnPropertyChanged(nameof(CenterX));
        OnPropertyChanged(nameof(CenterY));
      }
    }
    public double X
    {
      get => _x;
      set
      {
        _x = value;
        OnPropertyChanged();
        OnPropertyChanged(nameof(CenterX));
      }
    }

    public double Y
    {
      get => _y;
      set
      {
        _y = value;
        OnPropertyChanged();
        OnPropertyChanged(nameof(CenterY));
      }
    }

    public bool IsNew
    {
      get => _isNew;
      set
      {
        _isNew = value;
        OnPropertyChanged();
      }
    }

    // Computed Properties for Center Coordinates
    public double CenterX
    {
      get => X + ItemSizePx / 2;
    }
    public double CenterY
    {
      get => Y + ItemSizePx / 2;
    }
    public Image? Image { get; private set; } = null;
    private Dictionary<string, string> Attributes
    {
      get => ObjectData?.Attributes ?? [];
    }

    public string Source
    {
      get => SectorMap?.Source ?? "unknown";
    }

    public void ConnectImage(Image image)
    {
      Image = image;
    }

    public void SetVisible(bool visible)
    {
      if (Image != null)
      {
        Image.Visibility = visible ? Visibility.Visible : Visibility.Hidden;
      }
    }

    public void Remove(Canvas canvas)
    {
      if (Image != null)
      {
        canvas.Children.Remove(Image);
      }
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
        else
        {
          ItemSizePx = 1.5 * ItemSizePx;
        }
      }
    }

    private void UpdatePosition()
    {
      if (SectorMap == null || ObjectData == null)
        return;
      X = SectorMap.VisualX + (ObjectData.X * SectorMap.VisualSizePx / SectorMap.InternalSizeKm + SectorMap.VisualSizePx - ItemSizePx) / 2;
      Y =
        SectorMap.VisualY
        - SectorMap.VisualSizePx * 0.067
        + (-ObjectData.Z * SectorMap.VisualSizePx / SectorMap.InternalSizeKm + SectorMap.VisualSizePx - ItemSizePx) / 2;
      OnPropertyChanged(nameof(X));
      OnPropertyChanged(nameof(Y));
      UpdateToolTip();
    }

    public void UpdateInternalCoordinates(ObjectCoordinates coordinates)
    {
      if (SectorMap == null)
        return;
      coordinates.X = (int)(
        ((X - SectorMap.VisualX) * 2 + ItemSizePx - SectorMap.VisualSizePx) * SectorMap.InternalSizeKm / SectorMap.VisualSizePx
      );
      coordinates.Z = (int)(
        (SectorMap.VisualSizePx + (SectorMap.VisualY - SectorMap.VisualSizePx * 0.067 - Y) * 2 - ItemSizePx)
        * SectorMap.InternalSizeKm
        / SectorMap.VisualSizePx
      );
      if (_objectData == null)
        return;
      _objectData.X = coordinates.X;
      _objectData.Z = coordinates.Z;
      UpdateToolTip();
    }

    // Colors based on gate type and status

    private void UpdateToolTip()
    {
      if (_objectData == null)
        ToolTip = "No data";
      string result = $"{char.ToUpper(Type[0])}{Type[1..]}";
      if (Type == "gate" || Type == "highway")
        result += $": {Status}\n";
      if (Type == "gate")
      {
        result += $"To: {_objectData?.Info ?? ""}\n";
      }
      else if (Type == "highway")
      {
        if (Attributes.TryGetValue("PointType", out string? pointType))
        {
          string fromTo = pointType == "entry" ? "to" : "from";
          result += $"{char.ToUpper(pointType[0])}{pointType[1..]} point {fromTo} {_objectData?.Info ?? ""}\n";
        }
      }
      else if (Type == "station")
      {
        result = $"{_objectData?.Info ?? ""}\nOwner: {_objectData?.Attributes["StationOwner"] ?? ""}\n";
      }
      result += $"X: {_objectData?.X ?? 0, 4}, Y: {_objectData?.Y ?? 0, 4}, Z: {_objectData?.Z ?? 0, 4}";
      result += $"\nSource: {From}";
      ToolTip = result;
    }

    public BitmapSource ObjectImage
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
        BitmapSource image = new BitmapImage(new Uri(imagePath));
        if (Type == "station" && ObjectData?.Color != null)
        {
          WriteableBitmap writeableBitmap = new(image);
          if (ObjectData.Color.HasValue)
          {
            ReplaceColor(writeableBitmap, Colors.Black, ObjectData.Color.Value);
            image = writeableBitmap;
          }
        }
        return image;
      }
    }

    private static void ReplaceColor(WriteableBitmap bitmap, Color targetColor, Color replacementColor)
    {
      int width = bitmap.PixelWidth;
      int height = bitmap.PixelHeight;
      int stride = width * ((bitmap.Format.BitsPerPixel + 7) / 8);
      byte[] pixelData = new byte[height * stride];
      bitmap.CopyPixels(pixelData, stride, 0);

      for (int y = 0; y < height; y++)
      {
        for (int x = 0; x < width; x++)
        {
          int index = y * stride + 4 * x;
          byte b = pixelData[index];
          byte g = pixelData[index + 1];
          byte r = pixelData[index + 2];
          byte a = pixelData[index + 3];

          if (r == targetColor.R && g == targetColor.G && b == targetColor.B && a == targetColor.A)
          {
            pixelData[index] = replacementColor.B;
            pixelData[index + 1] = replacementColor.G;
            pixelData[index + 2] = replacementColor.R;
            pixelData[index + 3] = replacementColor.A;
          }
        }
      }

      bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, stride, 0);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
  }
}
