using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Utilities.Logging;
using X4DataLoader;
using X4Map;

namespace ChemGateBuilder
{
  public partial class GalaxyMapWindow : Window, INotifyPropertyChanged
  {
    private readonly CollectionViewSource? SectorsList = null;

    // Reference to the main window's size (assumed to be passed or accessible)
    public readonly MainWindow MainWindowReference;
    public readonly Galaxy? Galaxy;

    public Sector? SelectedSector = null;

    private Visibility _optionsVisibilityState = Visibility.Hidden;
    private string _optionsVisibilitySymbol = "CircleLeft";
    private double _optionsWidth = 10;

    public Visibility OptionsVisibilityState
    {
      get => _optionsVisibilityState;
      set
      {
        _optionsVisibilityState = value;
        if (value == Visibility.Visible)
        {
          OptionsWidth = double.NaN;
          OptionsVisibilitySymbol = "CircleRight";
        }
        else
        {
          OptionsWidth = 10;
          OptionsVisibilitySymbol = "CircleLeft";
        }
        OnPropertyChanged(nameof(OptionsVisibilityState));
      }
    }

    public string OptionsVisibilitySymbol
    {
      get => _optionsVisibilitySymbol;
      set
      {
        _optionsVisibilitySymbol = value;
        OnPropertyChanged(nameof(OptionsVisibilitySymbol));
      }
    }

    public double OptionsWidth
    {
      get => _optionsWidth;
      set
      {
        _optionsWidth = value;
        OnPropertyChanged(nameof(OptionsWidth));
      }
    }

    public GalaxyMapViewer? GalaxyMapViewer { get; set; } = null;

    public GalaxyMapWindow(
      MainWindow mainWindow,
      CollectionViewSource sectorsViewSource,
      Dictionary<string, List<ObjectInSector>> extraObjects,
      List<string> extraConnectionsNames
    )
    {
      InitializeComponent();
      DataContext = this;
      Owner = mainWindow;
      SectorsList = sectorsViewSource;
      MainWindowReference = mainWindow;
      Galaxy = MainWindowReference.Galaxy;
      GalaxyMapViewer = (GalaxyMapViewer)FindName("GalaxyViewer");
      Canvas galaxyCanvas = (Canvas)FindName("GalaxyCanvas");
      GalaxyMapViewer.Connect(
        Galaxy!,
        galaxyCanvas,
        mainWindow.MapColorsOpacity,
        mainWindow.SectorRadius,
        extraObjects,
        extraConnectionsNames
      );
      GalaxyMapViewer.ShowEmptyClusterPlaces.IsChecked = false;
      GalaxyMapViewer.OnSectorSelected += GalaxyMapViewer_SectorSelected;
      GalaxyMapViewer.RefreshGalaxyData();

      // Set window size to 90% of the main window
      Width = mainWindow.ActualWidth * 0.9;
      Height = mainWindow.ActualHeight * 0.9;
    }

    private void GalaxyMapViewer_SectorSelected(object? sender, SectorEventArgs e)
    {
      // Your code to run when the event is raised
      if (e.SelectedSector != null)
      {
        Log.Debug($"Selected sector: {e.SelectedSector.Name}");
        if (SectorsList != null && !String.IsNullOrEmpty(e.SelectedSector.Macro))
        {
          if (SectorsList.View is CollectionView collectionView)
          {
            SectorsListItem? sector = collectionView
              .Cast<SectorsListItem>()
              .FirstOrDefault(sector => sector.Macro == e.SelectedSector.Macro);
            if (sector != null && !sector.Selectable)
            {
              Log.Debug($"Sector {sector.Name} is not selectable. Skipping.");
            }
            else
            {
              SelectedSector = e.SelectedSector;
              Close();
            }
          }
        }
      }
    }

    private void ButtonOptionsVisibility_Click(object sender, RoutedEventArgs e)
    {
      OptionsVisibilityState = OptionsVisibilityState == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
