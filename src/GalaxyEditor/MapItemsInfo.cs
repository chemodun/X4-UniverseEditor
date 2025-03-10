using System.Collections.ObjectModel;
using System.ComponentModel;
using X4DataLoader;
using X4Map;

namespace GalaxyEditor
{
  public class CellItemInfo(GalaxyMapCluster? cluster)
  {
    private readonly GalaxyMapCluster? _cluster = cluster;
    public string X
    {
      get => _cluster?.OriginalX.ToString("N0") ?? "";
    }
    public string Y
    {
      get => _cluster?.OriginalY.ToString("N0") ?? "";
    }
    public string Z
    {
      get => _cluster?.OriginalZ.ToString("N0") ?? "";
    }
    public string Column
    {
      get => _cluster?.MapPosition.Column.ToString("N0") ?? "";
    }
    public string Row
    {
      get => _cluster?.MapPosition.Row.ToString("N0") ?? "";
    }
  }

  public class ClusterItemInfo(Cluster? cluster)
  {
    private readonly Cluster? _cluster = cluster;
    public string Name
    {
      get => _cluster?.Name ?? "";
    }
    public string Macro
    {
      get => _cluster?.Macro ?? "";
    }
    public string X
    {
      get => _cluster?.Position.X.ToString("N0") ?? "";
    }
    public string Y
    {
      get => _cluster?.Position.Y.ToString("N0") ?? "";
    }
    public string Z
    {
      get => _cluster?.Position.Z.ToString("N0") ?? "";
    }
  }

  public class SectorItemInfo(Sector? sector)
  {
    private readonly Sector? _sector = sector;
    public string Name
    {
      get => _sector?.Name ?? "";
    }
    public string Owner
    {
      get => _sector?.DominantOwnerFaction?.Name ?? "";
    }
    public string Economy
    {
      get => _sector?.Economy.ToString("F2") ?? "";
    }
    public string Security
    {
      get => _sector?.Security.ToString("F2") ?? "";
    }
    public string Sunlight
    {
      get => _sector?.Sunlight.ToString("F2") ?? "";
    }
    public string Macro
    {
      get => _sector?.Macro ?? "";
    }
    public string X
    {
      get => _sector?.Position.X.ToString("N0") ?? "";
    }
    public string Y
    {
      get => _sector?.Position.Y.ToString("N0") ?? "";
    }
    public string Z
    {
      get => _sector?.Position.Z.ToString("N0") ?? "";
    }
  }
}
