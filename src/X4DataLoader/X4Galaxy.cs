using Utilities.Logging;

namespace X4DataLoader
{
  public class X4Galaxy
  {
    public static Galaxy LoadData(string coreFolderPath, bool loadMods = false)
    {
      List<GameFilesStructureItem> gameFilesStructure =
      [
        new GameFilesStructureItem(id: "translations", folder: "t", ["0001-l044.xml", "0001.xml"]),
        new GameFilesStructureItem(id: "colors", folder: "libraries", ["colors.xml"]),
        new GameFilesStructureItem(id: "mapDefaults", folder: "libraries", ["mapdefaults.xml"]),
        new GameFilesStructureItem(id: "clusters", folder: "maps/xu_ep2_universe", ["clusters.xml"], MatchingModes.Suffix),
        new GameFilesStructureItem(id: "sectors", folder: "maps/xu_ep2_universe", ["sectors.xml"], MatchingModes.Suffix),
        new GameFilesStructureItem(id: "zones", folder: "maps/xu_ep2_universe", ["zones.xml"], MatchingModes.Suffix),
        new GameFilesStructureItem(id: "races", folder: "libraries", ["races.xml"]),
        new GameFilesStructureItem(id: "factions", folder: "libraries", ["factions.xml"]),
        new GameFilesStructureItem(id: "modules", folder: "libraries", ["modules.xml"]),
        new GameFilesStructureItem(id: "modulegroups", folder: "libraries", ["modulegroups.xml"]),
        new GameFilesStructureItem(id: "constructionplans", folder: "libraries", ["constructionplans.xml"]),
        new GameFilesStructureItem(id: "stationgroups", folder: "libraries", ["stationgroups.xml"]),
        new GameFilesStructureItem(id: "stations", folder: "libraries", ["stations.xml"]),
        new GameFilesStructureItem(id: "god", folder: "libraries", ["god.xml"]),
        new GameFilesStructureItem(id: "sechighways", folder: "maps/xu_ep2_universe", ["sechighways.xml"], MatchingModes.Suffix),
        new GameFilesStructureItem(id: "zonehighways", folder: "maps/xu_ep2_universe", ["zonehighways.xml"], MatchingModes.Suffix),
        new GameFilesStructureItem(id: "galaxy", folder: "maps/xu_ep2_universe", ["galaxy.xml"]),
        new GameFilesStructureItem(id: "patchactions", folder: "libraries", ["patchactions.xml"]),
      ];

      Log.Debug($"Starting to load galaxy data from {coreFolderPath}");

      Galaxy galaxy = new();
      DataLoader.LoadAllData(galaxy, coreFolderPath, gameFilesStructure, loadMods);
      return galaxy;
    }
  }
}
