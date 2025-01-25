using Utilities.Logging;
namespace X4DataLoader
{
    public class X4Galaxy
    {
        public static Galaxy LoadData(string coreFolderPath)
        {
            var relativePaths = new Dictionary<string, (string path, string fileName)>
            {
                { "translation", ("t", "0001-l044.xml") },
                { "mapDefaults", ("libraries", "mapdefaults.xml") },
                { "galaxy", ("maps/xu_ep2_universe", "galaxy.xml") },
                { "clusters", ("maps/xu_ep2_universe", "clusters.xml") },
                { "sectors", ("maps/xu_ep2_universe", "sectors.xml") },
                { "zones", ("maps/xu_ep2_universe", "zones.xml") },
                { "sechighways", ("maps/xu_ep2_universe", "sechighways.xml") },
                { "zonehighways", ("maps/xu_ep2_universe", "zonehighways.xml") }
            };

            Log.Debug($"Starting to load galaxy data from {coreFolderPath}");

            return DataLoader.LoadAllData(coreFolderPath, relativePaths);
        }
    }
}
