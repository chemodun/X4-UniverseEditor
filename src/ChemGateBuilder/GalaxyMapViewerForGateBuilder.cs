using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Utilities.Logging;
using X4DataLoader;
using X4Map;

namespace ChemGateBuilder
{
  public class GalaxyMapViewerForGateBuilder : GalaxyMapViewer
  {
    private List<string> _extraConnectionsNames = [];

    public void Connect(
      Galaxy galaxy,
      Canvas galaxyCanvas,
      double mapColorsOpacity,
      Dictionary<string, List<ObjectInSector>>? extraObjects = null,
      List<string>? extraConnectionsNames = null
    )
    {
      base.Connect(galaxy, galaxyCanvas, mapColorsOpacity, extraObjects);
      _extraConnectionsNames = extraConnectionsNames ?? [];
    }

    protected override void CreateMap()
    {
      base.CreateMap();
      foreach (string connectionName in _extraConnectionsNames)
      {
        if (connectionName == null)
        {
          continue;
        }
        List<SectorMapItem> extraGatesItems = SectorsItems.FindAll(item => item.Id == connectionName);
        if (extraGatesItems.Count == 2)
        {
          GalaxyMapInterConnection galaxyMapGateConnection = new(null, extraGatesItems[0], extraGatesItems[1], true);
          galaxyMapGateConnection.Create(GalaxyCanvas);
          InterConnections.Add(galaxyMapGateConnection);
        }
      }
      List<SectorMapItem> newGatesItems = SectorsItems.FindAll(item => item.Id == SectorMap.NewGateId);
      if (newGatesItems.Count == 2)
      {
        GalaxyMapInterConnection galaxyMapGateConnection = new(null, newGatesItems[0], newGatesItems[1], true);
        galaxyMapGateConnection.Create(GalaxyCanvas);
        InterConnections.Add(galaxyMapGateConnection);
      }
    }
  }
}
