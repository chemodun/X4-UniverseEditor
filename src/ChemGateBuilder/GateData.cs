using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using SharedWindows;
using X4DataLoader;
using X4Map;
using X4Map.Constants;

namespace ChemGateBuilder
{
  public class GatesConnectionData : INotifyPropertyChanged
  {
    private SectorsListItem? _sectorDirect;
    private SectorsListItem? _sectorDirectDefault;
    private ObjectInSector? _sectorDirectSelectedObject = new();
    private SectorMap _sectorDirectMap = new();
    private ObservableCollection<ObjectInSector> _sectorDirectObjects = [];
    private GateData _gateDirect;
    private SectorsListItem? _sectorOpposite;
    private SectorsListItem? _sectorOppositeDefault;
    private ObjectInSector? _sectorOppositeSelectedObject = new();
    private SectorMap _sectorOppositeMap = new();
    private ObservableCollection<ObjectInSector> _SectorOppositeObjects = [];
    private GateData _gateOpposite;
    private bool _isChanged = false;
    private bool _isReadyToSave = false;

    private bool IsDataChanged =>
      _sectorDirect != _sectorDirectDefault
      || _sectorOpposite != _sectorOppositeDefault
      || _gateDirect.IsChanged
      || _gateOpposite.IsChanged;
    private bool IsDataReadyToSave =>
      CheckGateDistance() && CheckGateDistance(false) && _sectorDirect != null && _sectorOpposite != null && IsDataChanged;

    public SectorsListItem? SectorDirect
    {
      get => _sectorDirect;
      set
      {
        if (_sectorDirect != value)
        {
          if (_sectorDirect == null)
          {
            FillPositionByRandomValues();
          }
          _sectorDirect = value;
          OnPropertyChanged(nameof(SectorDirect));
        }
      }
    }
    public List<string> SectorDirectExistingObjectsMacros = [];
    public ObjectInSector? SectorDirectSelectedObject
    {
      get => _sectorDirectSelectedObject;
      set
      {
        if (_sectorDirectSelectedObject != value)
        {
          _sectorDirectSelectedObject = value;
          if (_sectorDirectMap != null)
          {
            if (value != null)
            {
              _sectorDirectMap.SelectItem(value?.Id);
            }
            else
            {
              _sectorDirectMap.SelectItem("");
            }
          }
          OnPropertyChanged(nameof(SectorDirectSelectedObject));
        }
      }
    }
    public SectorMap SectorDirectMap
    {
      get => _sectorDirectMap;
      set
      {
        if (_sectorDirectMap != value)
        {
          _sectorDirectMap = value;
          OnPropertyChanged(nameof(SectorDirectMap));
        }
      }
    }

    public ObservableCollection<ObjectInSector> SectorDirectObjects
    {
      get => _sectorDirectObjects;
      set
      {
        if (_sectorDirectObjects != value)
        {
          _sectorDirectObjects = value;
          OnPropertyChanged(nameof(SectorDirectObjects));
        }
      }
    }
    public GateData GateDirect
    {
      get => _gateDirect;
      set
      {
        if (_gateDirect != value)
        {
          if (_gateDirect != null)
          {
            _gateDirect.PropertyChanged -= ChildPropertyChanged;
          }
          _gateDirect = value;
          OnPropertyChanged(nameof(GateDirect));
          if (_gateDirect != null)
          {
            _gateDirect.PropertyChanged += ChildPropertyChanged;
          }
        }
      }
    }

    public SectorsListItem? SectorOpposite
    {
      get => _sectorOpposite;
      set
      {
        if (_sectorOpposite != value)
        {
          if (_sectorOpposite == null)
          {
            FillPositionByRandomValues(false);
          }
          _sectorOpposite = value;
          OnPropertyChanged(nameof(SectorOpposite));
        }
      }
    }
    public List<string> SectorOppositeExistingObjectsMacros = [];
    public ObjectInSector? SectorOppositeSelectedObject
    {
      get => _sectorOppositeSelectedObject;
      set
      {
        if (_sectorOppositeSelectedObject != value)
        {
          _sectorOppositeSelectedObject = value;
          if (_sectorOppositeMap != null)
          {
            if (value != null)
            {
              _sectorOppositeMap.SelectItem(value.Id);
            }
            else
            {
              _sectorOppositeMap.SelectItem("");
            }
          }
          OnPropertyChanged(nameof(SectorOppositeSelectedObject));
        }
      }
    }
    public SectorMap SectorOppositeMap
    {
      get => _sectorOppositeMap;
      set
      {
        if (_sectorOppositeMap != value)
        {
          _sectorOppositeMap = value;
          OnPropertyChanged(nameof(SectorOppositeMap));
        }
      }
    }

    public ObservableCollection<ObjectInSector> SectorOppositeObjects
    {
      get => _SectorOppositeObjects;
      set
      {
        if (_SectorOppositeObjects != value)
        {
          _SectorOppositeObjects = value;
          OnPropertyChanged(nameof(SectorOppositeObjects));
        }
      }
    }

    public GateData GateOpposite
    {
      get => _gateOpposite;
      set
      {
        if (_gateOpposite != value)
        {
          if (_gateOpposite != null)
          {
            _gateOpposite.PropertyChanged -= ChildPropertyChanged;
          }
          _gateOpposite = value;
          OnPropertyChanged(nameof(GateOpposite));
          if (_gateOpposite != null)
          {
            _gateOpposite.PropertyChanged += ChildPropertyChanged;
          }
        }
      }
    }

    public bool IsChanged
    {
      get => _isChanged;
      set
      {
        _isChanged = value;
        OnPropertyChanged(nameof(IsChanged));
        if (Application.Current.MainWindow is MainWindow mainWindow)
        {
          mainWindow.ChangingGalaxyConnectionIsPossible = !value;
        }
      }
    }
    public bool IsReadyToSave
    {
      get => _isReadyToSave;
      set
      {
        _isReadyToSave = value;
        OnPropertyChanged(nameof(IsReadyToSave));
      }
    }

    public GatesConnectionData(bool gateActiveDefault, string gateMacroDefault)
    {
      // Subscribe to child PropertyChanged events
      _gateDirect = new GateData(gateActiveDefault, gateMacroDefault);
      _gateOpposite = new GateData(gateActiveDefault, gateMacroDefault);
      Reset();
      _gateOpposite.PropertyChanged += ChildPropertyChanged;
      _gateDirect.PropertyChanged += ChildPropertyChanged;
    }

    public void SetMapsCanvasAndHexagons(Canvas canvasDirect, Polygon hexagonDirect, Canvas canvasOpposite, Polygon hexagonOpposite)
    {
      SectorDirectMap.Connect(canvasDirect, hexagonDirect);
      SectorOppositeMap.Connect(canvasOpposite, hexagonOpposite);
    }

    public void Reset()
    {
      SectorDirect = _sectorDirectDefault;
      SectorOpposite = _sectorOppositeDefault;
      _gateDirect.Reset();
      _gateOpposite.Reset();
      IsChanged = IsDataChanged;
      IsReadyToSave = IsDataReadyToSave;
      OnPropertyChanged(nameof(SectorDirect));
      OnPropertyChanged(nameof(SectorOpposite));
      OnPropertyChanged(nameof(GateDirect));
      OnPropertyChanged(nameof(GateOpposite));
      UpdateCurrentGateOnMap(nameof(GateDirect));
      UpdateCurrentGateOnMap(nameof(GateOpposite));
    }

    public void SetGateStatusDefaults(bool gateActiveDefault)
    {
      _gateDirect.SetDefaults(gateActiveDefault, "");
      _gateOpposite.SetDefaults(gateActiveDefault, "");
      OnPropertyChanged("");
    }

    public void ResetToInitial(bool gateActiveDefault, string gateMacroDefault)
    {
      _gateDirect = new GateData(gateActiveDefault, gateMacroDefault);
      _gateOpposite = new GateData(gateActiveDefault, gateMacroDefault);
      _sectorDirectDefault = null;
      _sectorOppositeDefault = null;
      Reset();
      _gateOpposite.PropertyChanged += ChildPropertyChanged;
      _gateDirect.PropertyChanged += ChildPropertyChanged;
    }

    public void SetDefaultsFromReference(GalaxyConnectionData reference, ObservableCollection<SectorsListItem> AllSectors)
    {
      if (reference == null)
        return;
      GalaxyConnection connection = reference.Connection;
      string sectorDirectMacro = connection.PathDirect?.Sector?.Macro ?? "";
      _sectorDirectDefault = AllSectors.FirstOrDefault(s => s.Macro == sectorDirectMacro);
      string sectorOppositeMacro = connection.PathOpposite?.Sector?.Macro ?? "";
      _sectorOppositeDefault = AllSectors.FirstOrDefault(s => s.Macro == sectorOppositeMacro);
      _gateDirect.SetDefaults(
        reference.GateDirectActive,
        connection.PathDirect?.Gate?.GateMacro ?? "",
        new ObjectCoordinates(reference.GateDirectX, reference.GateDirectY, reference.GateDirectZ),
        reference.DirectPosition,
        reference.DirectRotation
      );
      _gateOpposite.SetDefaults(
        reference.GateOppositeActive,
        connection.PathOpposite?.Gate?.GateMacro ?? "",
        new ObjectCoordinates(reference.GateOppositeX, reference.GateOppositeY, reference.GateOppositeZ),
        reference.OppositePosition,
        reference.OppositeRotation
      );
    }

    private void ChildPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
      // Propagate the change
      if (sender == GateDirect)
      {
        OnPropertyChanged(nameof(GateDirect));
      }
      else if (sender == GateOpposite)
      {
        OnPropertyChanged(nameof(GateOpposite));
      }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObjectInSector? UpdateCurrentGateOnMap(string propertyName, SectorMap? alternateSectorMap = null, bool returnOnly = false)
    {
      GateData gateCurrent = propertyName == nameof(GateDirect) ? GateDirect : GateOpposite;
      if (gateCurrent == null)
        return null;
      SectorsListItem? sectorTo = propertyName == nameof(GateDirect) ? SectorOpposite : SectorDirect;
      SectorMap sectorMap = propertyName == nameof(GateDirect) ? SectorDirectMap : SectorOppositeMap;
      if (alternateSectorMap != null)
      {
        sectorMap = alternateSectorMap;
      }
      if (sectorMap == null || sectorMap.IsDragging)
        return null;
      ObjectInSector newObject = new()
      {
        Active = gateCurrent.Active && sectorTo != null,
        Info = sectorTo != null ? sectorTo.Name : "",
        X = gateCurrent.Coordinates.X,
        Y = gateCurrent.Coordinates.Y,
        Z = gateCurrent.Coordinates.Z,
        Angle = gateCurrent.Rotation.Pitch,
        Type = "gate",
        From = "new",
        Id = SectorMap.NewGateId,
      };
      if (!returnOnly)
      {
        sectorMap.UpdateItem(newObject);
        return null;
      }
      return newObject;
    }

    public void UpdateCurrentGateCoordinates(int X, int Y, int Z, string propertyName)
    {
      GateData gateCurrent = propertyName == nameof(GateDirect) ? GateDirect : GateOpposite;
      if (gateCurrent == null)
        return;
      gateCurrent.Coordinates = new ObjectCoordinates(X, Y, Z);
    }

    private bool CheckGateDistance(bool isDirect = true)
    {
      if (Application.Current.MainWindow is MainWindow mainWindow && mainWindow != null)
      {
        if (mainWindow.GatesMinimalDistanceBetween == 0 || mainWindow.GatesConnectionCurrent == null)
        {
          return true;
        }
        string? sectorMacro = isDirect ? SectorDirect?.Macro : SectorOpposite?.Macro;
        if (sectorMacro == null)
        {
          return true;
        }
        string? sectorName = isDirect ? SectorDirect?.Name : SectorOpposite?.Name;
        string message = $"The new gate in {sectorName} is too close to another object in the sector";
        ObjectCoordinates coordinates = isDirect ? GateDirect.Coordinates : GateOpposite.Coordinates;
        ObservableCollection<ObjectInSector> sectorObjects = isDirect ? SectorDirectObjects : SectorOppositeObjects;
        foreach (var sectorConnection in sectorObjects)
        {
          if (sectorConnection == null)
          {
            continue;
          }
          ObjectCoordinates coordinates2 = new(sectorConnection.X, sectorConnection.Y, sectorConnection.Z);
          double distance = CalculateDistance(coordinates, coordinates2);
          if (distance < mainWindow.GatesMinimalDistanceBetween)
          {
            string recommendation = AxisToChangeToMeetDistanceWithRecommendedValue(coordinates, coordinates2);
            mainWindow.StatusBar.SetStatusMessage($"{message}. {recommendation}", StatusMessageType.Warning);
            return false;
          }
        }
        foreach (var connection in mainWindow.ChemGateKeeperMod.GalaxyConnections)
        {
          if (
            connection == null
            || connection.Connection == null
            || connection.Connection.PathDirect == null
            || connection.Connection.PathOpposite == null
            || connection.Connection.PathDirect.Sector == null
            || connection.Connection.PathOpposite.Sector == null
            || connection.Connection.PathDirect.Sector.Macro == null
            || connection.Connection.PathOpposite.Sector.Macro == null
          )
          {
            continue;
          }
          if (connection == mainWindow.CurrentGalaxyConnection)
          {
            continue;
          }
          foreach (
            var modSectorMacro in new string[]
            {
              connection.Connection.PathDirect.Sector.Macro,
              connection.Connection.PathOpposite.Sector.Macro,
            }
          )
          {
            if (modSectorMacro != sectorMacro)
            {
              continue;
            }
            ObjectCoordinates coordinates2 =
              modSectorMacro == connection.Connection.PathDirect.Sector.Macro
                ? new ObjectCoordinates(connection.GateDirectX, connection.GateDirectY, connection.GateDirectZ)
                : new ObjectCoordinates(connection.GateOppositeX, connection.GateOppositeY, connection.GateOppositeZ);
            double distance = CalculateDistance(coordinates, coordinates2);
            if (distance < mainWindow.GatesMinimalDistanceBetween)
            {
              string recommendation = AxisToChangeToMeetDistanceWithRecommendedValue(coordinates, coordinates2);
              mainWindow.StatusBar.SetStatusMessage($"{message}. {recommendation}", StatusMessageType.Warning);
              return false;
            }
          }
        }
        return true;
      }
      return false;
    }

    private static double CalculateDistance(ObjectCoordinates coordinates1, ObjectCoordinates coordinates2)
    {
      return Math.Sqrt(
        Math.Pow(coordinates1.X - coordinates2.X, 2)
          + Math.Pow(coordinates1.Y - coordinates2.Y, 2)
          + Math.Pow(coordinates1.Z - coordinates2.Z, 2)
      );
    }

    private static string AxisToChangeToMeetDistanceWithRecommendedValue(ObjectCoordinates coordinates1, ObjectCoordinates coordinates2)
    {
      double distance = CalculateDistance(coordinates1, coordinates2);
      double recommendedDistance = Application.Current.MainWindow is MainWindow mainWindow ? mainWindow.GatesMinimalDistanceBetween : 0;
      if (distance < recommendedDistance)
      {
        double xDiff = coordinates1.X - coordinates2.X;
        double zDiff = coordinates1.Z - coordinates2.Z;

        // Calculate the required increase to meet the recommended distance
        double requiredIncrease = recommendedDistance - distance;

        // Determine which axis has the smallest difference
        if (Math.Abs(xDiff) <= Math.Abs(zDiff))
        {
          double newX = coordinates2.X + Math.Sign(xDiff) * (Math.Abs(xDiff) + requiredIncrease);
          return $"Please set X to at least {newX:0}";
        }
        else
        {
          double newZ = coordinates2.Z + Math.Sign(zDiff) * (Math.Abs(zDiff) + requiredIncrease);
          return $"Please set Z to at least {newZ:0}";
        }
      }
      return "";
    }

    public void UpdateDataFlags()
    {
      if (_isChanged != IsDataChanged)
        IsChanged = IsDataChanged;
      if (_isReadyToSave != IsDataReadyToSave)
        IsReadyToSave = IsDataReadyToSave;
    }

    protected void OnPropertyChanged(string propertyName)
    {
      UpdateDataFlags();
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      if (propertyName == nameof(SectorDirect) || propertyName == nameof(SectorOpposite))
      {
        bool isDirect = propertyName == nameof(SectorDirect);
        SectorsListItem? sectorCurrent = isDirect ? SectorDirect : SectorOpposite;
        var sectorObjects = isDirect ? SectorDirectObjects : SectorOppositeObjects;
        sectorObjects.Clear();
        SectorMap sectorMap = isDirect ? SectorDirectMap : SectorOppositeMap;
        if (sectorCurrent?.Macro == null)
        {
          sectorMap.UnsetSector();
        }
        else if (Application.Current.MainWindow is MainWindow mainWindow && mainWindow.Galaxy != null)
        {
          Galaxy galaxy = mainWindow.Galaxy;
          Sector? sector = sectorCurrent != null ? galaxy.GetSectorByMacro(sectorCurrent.Macro) : null;
          List<ObjectInSector> objectsInSectorList = sectorMap.SetSector(sector, galaxy);
          if (sectorCurrent != null)
          {
            foreach (var connection in objectsInSectorList)
            {
              sectorObjects.Add(connection);
            }
            var sectorsViewSource = isDirect ? mainWindow.SectorsOppositeViewSource : mainWindow.SectorsDirectViewSource;
            if (sectorsViewSource != null)
            {
              if (sector != null)
              {
                List<string> sectorMacros = [.. galaxy.GetOppositeSectorsFromConnections(sector).Select(s => s.Macro)];
                if (isDirect)
                {
                  SectorDirectExistingObjectsMacros = sectorMacros;
                }
                else
                {
                  SectorOppositeExistingObjectsMacros = sectorMacros;
                }
              }
              sectorsViewSource.View.Refresh();
            }
            foreach (ObjectInSector modObject in mainWindow.GetObjectsInSectorFromMod(sectorCurrent.Macro))
            {
              sectorMap.AddItem(modObject);
            }
          }
        }
        UpdateCurrentGateOnMap(isDirect ? nameof(GateOpposite) : nameof(GateDirect));
        UpdateCurrentGateOnMap(isDirect ? nameof(GateDirect) : nameof(GateOpposite));
      }
      else if (propertyName == nameof(GateDirect) || propertyName == nameof(GateOpposite))
      {
        UpdateCurrentGateOnMap(propertyName);
      }
    }

    private void FillPositionByRandomValues(bool isDirect = true)
    {
      var gate = isDirect ? GateDirect : GateOpposite;
      if (gate == null)
        return;

      var position = gate.Position;
      if (position.X == 0 && position.Y == 0 && position.Z == 0)
      {
        var random = new Random();
        int[] possibleValues = [-250, -200, -150, -100, -50, 0, 50, 100, 150, 200, 250];

        do
        {
          position.X = possibleValues[random.Next(possibleValues.Length)];
          position.Y = possibleValues[random.Next(possibleValues.Length)];
          position.Z = possibleValues[random.Next(possibleValues.Length)];
        } while (position.X == 0 && position.Y == 0 && position.Z == 0);

        OnPropertyChanged(isDirect ? nameof(GateDirect) : nameof(GateOpposite));
      }
    }
  }

  public class SectorsListItem
  {
    public string? Name { get; set; }
    public string? Source { get; set; }
    public string? Macro { get; set; }
    public bool Selectable { get; set; }
  }

  public class GateData : INotifyPropertyChanged
  {
    private ObjectCoordinates _coordinates = new();
    private ObjectCoordinates _position = new();
    private ObjectRotation _rotation = new();
    private bool _active;
    private string _gateMacro;

    private bool _activeDefault = true;
    private string _gateMacroDefault = "";

    public ObjectCoordinates Coordinates
    {
      get => _coordinates;
      set
      {
        if (_coordinates != null)
        {
          // Unsubscribe from previous Coordinates PropertyChanged
          _coordinates.PropertyChanged -= ChildPropertyChanged;
        }

        _coordinates = value;
        OnPropertyChanged(nameof(Coordinates));

        if (_coordinates != null)
        {
          // Subscribe to new Coordinates PropertyChanged
          _coordinates.PropertyChanged += ChildPropertyChanged;
        }
      }
    }

    public ObjectCoordinates Position
    {
      get => _position;
      set
      {
        if (_position != value)
        {
          _position = value;
          OnPropertyChanged(nameof(Position));
        }
      }
    }

    public ObjectRotation Rotation
    {
      get => _rotation;
      set
      {
        if (_rotation != value)
        {
          _rotation = value;
          OnPropertyChanged(nameof(Rotation));
        }
      }
    }

    public bool Active
    {
      get => _active;
      set
      {
        if (_active != value)
        {
          _active = value;
          OnPropertyChanged(nameof(Active));
        }
      }
    }
    public string GateMacro
    {
      get => _gateMacro;
      set
      {
        if (_gateMacro != value)
        {
          _gateMacro = value;
          OnPropertyChanged(nameof(GateMacro));
        }
      }
    }
    public bool IsChanged =>
      _active != _activeDefault || _gateMacro != _gateMacroDefault || _coordinates.IsChanged || _position.IsChanged || _rotation.IsChanged;

    public GateData(bool activeDefault, string gateMacroDefault)
    {
      SetDefaults(activeDefault, gateMacroDefault);
      _active = activeDefault;
      _gateMacro = gateMacroDefault;
      // Subscribe to initial child PropertyChanged events
      if (_coordinates != null)
        _coordinates.PropertyChanged += ChildPropertyChanged;

      if (_position != null)
        _position.PropertyChanged += ChildPropertyChanged;

      if (_rotation != null)
        _rotation.PropertyChanged += ChildPropertyChanged;
    }

    public void Reset()
    {
      _active = _activeDefault;
      _gateMacro = _gateMacroDefault;
      OnPropertyChanged(nameof(Active));
      OnPropertyChanged(nameof(GateMacro));
      Coordinates.Reset();
      Position.Reset();
      Rotation.Reset();
      OnPropertyChanged(nameof(Coordinates));
      OnPropertyChanged(nameof(Position));
      OnPropertyChanged(nameof(Rotation));
    }

    public void SetDefaults(
      bool activeDefault,
      string gateMacroDefault = "",
      ObjectCoordinates? coordinatesDefault = null,
      ObjectCoordinates? positionDefault = null,
      ObjectRotation? rotationDefault = null
    )
    {
      _activeDefault = activeDefault;
      if (!string.IsNullOrEmpty(gateMacroDefault))
        _gateMacroDefault = gateMacroDefault;
      if (coordinatesDefault != null)
        Coordinates.SetDefaults(coordinatesDefault.X, coordinatesDefault.Y, coordinatesDefault.Z);
      if (positionDefault != null)
        Position.SetDefaults(positionDefault.X, positionDefault.Y, positionDefault.Z);
      if (rotationDefault != null)
        Rotation.SetDefaults(rotationDefault.Roll, rotationDefault.Pitch, rotationDefault.Yaw);
    }

    private void ChildPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
      // Determine which child sent the event and handle accordingly
      if (sender == Coordinates)
      {
        // Optionally, you can specify which property changed
        // For simplicity, notify that Coordinates has changed
        OnPropertyChanged(nameof(Coordinates));
      }
      else if (sender == Position)
      {
        OnPropertyChanged(nameof(Position));
      }
      else if (sender == Rotation)
      {
        OnPropertyChanged(nameof(Rotation));
      }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
