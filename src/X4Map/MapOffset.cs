using System.ComponentModel;
using System.Windows.Media;
using X4DataLoader;

namespace X4Map
{
  public class ObjectCoordinates(int xDefault = 0, int yDefault = 0, int zDefault = 0) : INotifyPropertyChanged
  {
    private int _x = xDefault;
    private int _xDefault = xDefault;
    private int _y = yDefault;
    private int _yDefault = yDefault;
    private int _z = zDefault;
    private int _zDefault = zDefault;

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
    public bool IsChanged => _x != _xDefault || _y != _yDefault || _z != _zDefault;

    public void Reset()
    {
      _x = _xDefault;
      _y = _yDefault;
      _z = _zDefault;
      OnPropertyChanged(nameof(X));
      OnPropertyChanged(nameof(Y));
      OnPropertyChanged(nameof(Z));
    }

    public void SetDefaults(int xDefault = 0, int yDefault = 0, int zDefault = 0)
    {
      _xDefault = xDefault;
      _yDefault = yDefault;
      _zDefault = zDefault;
    }

    public void SetFrom(ObjectCoordinates coordinates)
    {
      _x = coordinates.X;
      _y = coordinates.Y;
      _z = coordinates.Z;
      OnPropertyChanged(nameof(X));
      OnPropertyChanged(nameof(Y));
      OnPropertyChanged(nameof(Z));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }

  public class ObjectRotation(int rollDefault = 0, int pitchDefault = 0, int yawDefault = 0) : INotifyPropertyChanged
  {
    private int _roll = rollDefault;
    private int _rollDefault = rollDefault;
    private int _pitch = pitchDefault;
    private int _pitchDefault = pitchDefault;
    private int _yaw = yawDefault;
    private int _yawDefault = yawDefault;

    public int Roll
    {
      get => _roll;
      set
      {
        if (_roll != value)
        {
          _roll = value;
          OnPropertyChanged(nameof(Roll));
        }
      }
    }

    public int Pitch
    {
      get => _pitch;
      set
      {
        if (_pitch != value)
        {
          _pitch = value;
          OnPropertyChanged(nameof(Pitch));
        }
      }
    }

    public int Yaw
    {
      get => _yaw;
      set
      {
        if (_yaw != value)
        {
          _yaw = value;
          OnPropertyChanged(nameof(Yaw));
        }
      }
    }
    public bool IsChanged => _roll != _rollDefault || _pitch != _pitchDefault || _yaw != _yawDefault;

    public void Reset()
    {
      _roll = _rollDefault;
      _pitch = _pitchDefault;
      _yaw = _yawDefault;
      OnPropertyChanged(nameof(Roll));
      OnPropertyChanged(nameof(Pitch));
      OnPropertyChanged(nameof(Yaw));
    }

    public void SetDefaults(int rollDefault = 0, int pitchDefault = 0, int yawDefault = 0)
    {
      _rollDefault = rollDefault;
      _pitchDefault = pitchDefault;
      _yawDefault = yawDefault;
    }

    public void SetFrom(ObjectRotation rotation)
    {
      _roll = rotation.Roll;
      _pitch = rotation.Pitch;
      _yaw = rotation.Yaw;
      OnPropertyChanged(nameof(Roll));
      OnPropertyChanged(nameof(Pitch));
      OnPropertyChanged(nameof(Yaw));
    }

    public Quaternion ToQuaternion()
    {
      double rollRad = Roll * Math.PI / 180.0;
      double pitchRad = Pitch * Math.PI / 180.0;
      double yawRad = Yaw * Math.PI / 180.0;

      double cy = Math.Cos(yawRad * 0.5);
      double sy = Math.Sin(yawRad * 0.5);
      double cp = Math.Cos(pitchRad * 0.5);
      double sp = Math.Sin(pitchRad * 0.5);
      double cr = Math.Cos(rollRad * 0.5);
      double sr = Math.Sin(rollRad * 0.5);

      double qw = cr * cp * cy + sr * sp * sy;
      double qx = sr * cp * cy - cr * sp * sy;
      double qy = cr * sp * cy + sr * cp * sy;
      double qz = cr * cp * sy - sr * sp * cy;

      return new Quaternion(qx, qy, qz, qw);
    }

    /// <summary>
    /// Converts a Quaternion to a Rotation (Roll, Pitch, Yaw) in degrees.
    /// </summary>
    /// <param name="q">The Quaternion to convert.</param>
    /// <returns>A Rotation instance representing the equivalent Roll, Pitch, and Yaw.</returns>
    public static ObjectRotation FromQuaternion(Quaternion q)
    {
      // Normalize the quaternion to ensure accurate calculations
      double norm = Math.Sqrt(q.QX * q.QX + q.QY * q.QY + q.QZ * q.QZ + q.QW * q.QW);
      double x = q.QX / norm;
      double y = q.QY / norm;
      double z = q.QZ / norm;
      double w = q.QW / norm;

      // Calculate Roll (x-axis rotation)
      double sinr_cosp = 2 * (w * x + y * z);
      double cosr_cosp = 1 - 2 * (x * x + y * y);
      double rollRad = Math.Atan2(sinr_cosp, cosr_cosp);

      // Calculate Pitch (y-axis rotation)
      double sinp = 2 * (w * y - z * x);
      double pitchRad;
      if (Math.Abs(sinp) >= 1)
        pitchRad = Math.CopySign(Math.PI / 2, sinp); // Use 90 degrees if out of range
      else
        pitchRad = Math.Asin(sinp);

      // Calculate Yaw (z-axis rotation)
      double siny_cosp = 2 * (w * z + x * y);
      double cosy_cosp = 1 - 2 * (y * y + z * z);
      double yawRad = Math.Atan2(siny_cosp, cosy_cosp);

      // Convert radians to degrees
      double rollDeg = rollRad * 180.0 / Math.PI;
      double pitchDeg = pitchRad * 180.0 / Math.PI;
      double yawDeg = yawRad * 180.0 / Math.PI;

      return new ObjectRotation
      {
        Roll = (int)Math.Round(rollDeg),
        Pitch = (int)Math.Round(pitchDeg),
        Yaw = (int)Math.Round(yawDeg),
      };
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
