using System.ComponentModel;
using System.Windows.Media;

namespace X4Map
{
  public class ObjectInSector : INotifyPropertyChanged
  {
    private string? _Info;
    private int _x;
    private int _y;
    private int _z;
    private string? _type;
    private string? _id;
    private bool _active;
    private Color? _color = null;

    public string? Info
    {
      get => _Info;
      set
      {
        if (_Info != value)
        {
          _Info = value;
          OnPropertyChanged(nameof(Info));
        }
      }
    }

    public int X
    {
      get => _x;
      set
      {
        if (_x != value)
        {
          _x = value;
          OnPropertyChanged(nameof(X));
        }
      }
    }

    public int Y
    {
      get => _y;
      set
      {
        if (_y != value)
        {
          _y = value;
          OnPropertyChanged(nameof(Y));
        }
      }
    }

    public int Z
    {
      get => _z;
      set
      {
        if (_z != value)
        {
          _z = value;
          OnPropertyChanged(nameof(Z));
        }
      }
    }

    public string? Type
    {
      get => _type;
      set
      {
        if (_type != value)
        {
          _type = value;
          OnPropertyChanged(nameof(Type));
        }
      }
    }

    public string? Id
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
    public string From = "";
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

    public Color? Color
    {
      get => _color;
      set
      {
        if (_color != value)
        {
          _color = value;
          OnPropertyChanged(nameof(Color));
        }
      }
    }

    public Dictionary<string, string> Attributes = [];
    public event PropertyChangedEventHandler? PropertyChanged;

    public double GetMaxCoordinate(double maxCoordinate)
    {
      double distance = Math.Round(Math.Sqrt(X * X + Z * Z) / SectorMap.HexagonSizesRelation);
      return Math.Max(maxCoordinate, distance);
    }

    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
