using Utilities.Logging;

namespace X4DataLoader
{
  public class X4Galaxy
  {
    public static Galaxy LoadData(string coreFolderPath, List<GameFilesStructureItem> gameFilesStructure, bool loadMods = false)
    {
      Log.Debug($"Starting to load galaxy data from {coreFolderPath}");

      Galaxy galaxy = new();
      DataLoader.LoadData(galaxy, coreFolderPath, gameFilesStructure, loadMods);

      Log.Debug("Galaxy data loaded successfully");

      return galaxy;
    }
  }
}
