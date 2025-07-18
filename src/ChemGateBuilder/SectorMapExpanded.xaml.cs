using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Utilities.Logging;
using X4Map;

namespace ChemGateBuilder
{
  public partial class SectorMapExpandedWindow : Window, INotifyPropertyChanged
  {
    // Rename to avoid conflict
    public SectorMap _sectorMapExpanded = new();

    public SectorMap SectorMapExpanded
    {
      get => _sectorMapExpanded;
      set
      {
        // _sectorMapExpanded.PropertyChanged -= ChildPropertyChanged;
        _sectorMapExpanded = value;
        OnPropertyChanged(nameof(SectorMapExpanded));
      }
    }
    private ObjectCoordinates _newGateCoordinates = new();
    public ObjectCoordinates NewGateCoordinates
    {
      get => _newGateCoordinates;
      set
      {
        _newGateCoordinates = value;
        OnPropertyChanged(nameof(NewGateCoordinates));
      }
    }

    private ObjectRotation _newGateRotation = new();
    public ObjectRotation NewGateRotation
    {
      get => _newGateRotation;
      set
      {
        _newGateRotation = value;
        OnPropertyChanged(nameof(NewGateRotation));
      }
    }

    public double MapColorsOpacity { get; set; } = 0.5;

    public SectorMapExpandedWindow(Window owner, string title, SectorMap sectorMap, double mapColorsOpacity)
    {
      Owner = owner;
      Title = $"Sector Map: {title}";
      var minSize = Math.Min(Owner.ActualWidth, Owner.ActualHeight) * 0.9;
      Width = minSize;
      Height = minSize;
      MapColorsOpacity = mapColorsOpacity;
      InitializeComponent();
      DataContext = this;
      _sectorMapExpanded.Connect(SectorMapExpandedCanvas, SectorHexagon);
      _sectorMapExpanded.From(sectorMap);
      SectorMapItem? newItem = _sectorMapExpanded.GetItem(SectorMap.NewGateId);
      if (newItem != null && newItem.ObjectData != null)
      {
        NewGateCoordinates.X = newItem.ObjectData.X;
        NewGateCoordinates.Y = newItem.ObjectData.Y;
        NewGateCoordinates.Z = newItem.ObjectData.Z;
        NewGateRotation.Pitch = newItem.ObjectData.Angle;
      }
    }

    private void SectorMapExpandedCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      SectorMapExpanded?.OnSizeChanged(e.NewSize.Width, e.NewSize.Height);
    }

    private void SectorMapExpandedItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      SectorMapExpanded.MouseLeftButtonDown(sender, e);
    }

    private void SectorMapExpandedItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
      SectorMapExpanded.MouseRightButtonDown(sender, e);
    }

    private void SectorMapExpandedItem_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
      SectorMapExpanded.MouseRightButtonUp(sender, e);
    }

    private void SectorMapExpandedItem_MouseMove(object sender, MouseEventArgs e)
    {
      SectorMapExpanded.MouseMove(sender, e, NewGateCoordinates, NewGateRotation);
    }

    private void SectorMapExpandedItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      SectorMapExpanded.MouseLeftButtonUp(sender, e);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Escape)
      {
        this.Close();
      }
    }
  }
}
