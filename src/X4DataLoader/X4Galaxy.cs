namespace X4DataLoader
{
    public class Class1
    {
        public void LoadData()
        {
            var coreFolderPath = "path/to/core/folder";
            var relativePaths = new Dictionary<string, string>
            {
                { "translation", "t/0001-l044.xml" },
                { "mapDefaults", "libraries/mapdefaults.xml" },
                { "galaxy", "maps/xu_ep2_universe/galaxy.xml" },
                { "clusters", "maps/xu_ep2_universe/clusters.xml" },
                { "sectors", "maps/xu_ep2_universe/sectors.xml" },
                { "zones", "maps/xu_ep2_universe/zones.xml" },
                { "sechighways", "maps/xu_ep2_universe/sechighways.xml" },
                { "zonehighways", "maps/xu_ep2_universe/zonehighways.xml" }
            };

            var galaxy = DataLoader.LoadAllData(coreFolderPath, relativePaths);

            // Print the loaded data
            foreach (var cluster in galaxy.Clusters)
            {
                Console.WriteLine($"Cluster: {cluster.Name}, Description: {cluster.Description}");
                foreach (var sector in cluster.Sectors)
                {
                    Console.WriteLine($"  Sector: {sector.Name}, Description: {sector.Description}");
                }
            }
        }
    }
}
