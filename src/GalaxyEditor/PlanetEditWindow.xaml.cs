using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GalaxyEditor
{
  public partial class PlanetEditWindow : Window, INotifyPropertyChanged
  {
    private UnifyItemPlanet _item;
    private readonly GalaxyReferencesHolder _galaxyReferences;

    public UnifyItemPlanet Item
    {
      get => _item;
      set
      {
        _item = value;
        OnPropertyChanged(nameof(Item));
      }
    }
    public ObservableCollection<CatalogItemWithTextReference> ClassOptions { get; }
    public ObservableCollection<CatalogItemWithTextReference> GeologyOptions { get; }
    public ObservableCollection<CatalogItemWithTextReference> AtmosphereOptions { get; }
    public ObservableCollection<CatalogItemWithTextReference> PopulationOptions { get; }
    public ObservableCollection<CatalogItemWithTextReference> SettlementsOptions { get; }

    public PlanetEditWindow(BitmapImage icon, UnifyItemPlanet item, GalaxyReferencesHolder galaxyReferences)
    {
      InitializeComponent();
      DataContext = this;
      Icon = icon;
      _item = item;
      _galaxyReferences = galaxyReferences;
      ClassOptions = new ObservableCollection<CatalogItemWithTextReference>(_galaxyReferences.PlanetClasses);
      GeologyOptions = new ObservableCollection<CatalogItemWithTextReference>(_galaxyReferences.PlanetGeology);
      AtmosphereOptions = new ObservableCollection<CatalogItemWithTextReference>(_galaxyReferences.PlanetAtmosphere);
      PopulationOptions = new ObservableCollection<CatalogItemWithTextReference>(_galaxyReferences.PlanetPopulation);
      SettlementsOptions = new ObservableCollection<CatalogItemWithTextReference>(_galaxyReferences.PlanetSettlements);
    }

    private void ButtonSave_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = true;
      Close();
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = false;
      Close();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
