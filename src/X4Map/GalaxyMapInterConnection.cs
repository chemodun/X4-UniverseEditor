using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using X4DataLoader;

namespace X4Map
{
  public class GalaxyMapInterConnection : INotifyPropertyChanged
  {
    protected SectorMapItem? _directItem;
    public SectorMapItem? DirectItem
    {
      get => _directItem;
      set
      {
        _directItem = value;
        OnPropertyChanged(nameof(DirectItem));
      }
    }
    protected SectorMapItem? _oppositeItem;
    public SectorMapItem? OppositeItem
    {
      get => _oppositeItem;
      set
      {
        _oppositeItem = value;
        OnPropertyChanged(nameof(OppositeItem));
      }
    }

    public GalaxyConnection? Connection { get; init; } = null;

    public string SourceId = "unknown";
    public string DirectSourceId
    {
      get { return _directItem?.Source ?? "unknown"; }
    }
    public string OppositeSourceId
    {
      get { return _oppositeItem?.Source ?? "unknown"; }
    }

    protected readonly bool IsGate = true;
    protected Line? Line = null;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public GalaxyMapInterConnection(
      GalaxyConnection? connection,
      SectorMapItem gateDirect,
      SectorMapItem gateOpposite,
      bool isGate = true,
      string sourceId = "unknown"
    )
    {
      Connection = connection;
      DirectItem = gateDirect;
      OppositeItem = gateOpposite;
      IsGate = isGate;
      SourceId = sourceId;
    }

    public void Create(Canvas canvas, SectorMapItem? gateDirect = null, SectorMapItem? gateOpposite = null)
    {
      if (gateDirect != null)
      {
        DirectItem = gateDirect;
      }
      if (gateOpposite != null)
      {
        OppositeItem = gateOpposite;
      }
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

  public class GalaxyMapInterConnectionHighWay : GalaxyMapInterConnection
  {
    public Highway? Highway { get; init; } = null;

    public GalaxyMapInterConnectionHighWay(
      Highway? highway,
      SectorMapItem gateDirect,
      SectorMapItem gateOpposite,
      bool isGate = false,
      string sourceId = "unknown"
    )
      : base(null!, gateDirect, gateOpposite, isGate, sourceId)
    {
      Highway = highway;
    }
  }
}
