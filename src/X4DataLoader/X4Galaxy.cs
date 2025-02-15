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

    public static bool ValidateDataFolder(string folderPath, out string errorMessage)
    {
      string subfolderPath = System.IO.Path.Combine(folderPath, "t");
      string filePath = System.IO.Path.Combine(subfolderPath, "0001-l044.xml");

      if (Directory.Exists(subfolderPath) && File.Exists(filePath) && new FileInfo(filePath).Length > 0)
      {
        errorMessage = string.Empty;
        return true;
      }
      else
      {
        errorMessage = $"Error: Folder does not contain required X4 data ({folderPath})";
        return false;
      }
    }
  }
}
