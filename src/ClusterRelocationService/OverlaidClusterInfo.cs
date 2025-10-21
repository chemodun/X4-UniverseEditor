using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Runtime.InteropServices;
using System.Windows;
using X4DataLoader;
using X4Map;

namespace ClusterRelocationService
{
  public class OverlaidClusterInfo
  {
    public string Name { get; }
    public string Macro { get; }
    public double X { get; }
    public double Z { get; }
    public int OverlaidCount { get; }
    public string OverlaidWith { get; }
    public bool IsRelocated { get; }
    public bool IsVisibleOnMap { get; }

    public OverlaidClusterInfo(Cluster cluster, IEnumerable<Cluster> overlappingClusters, bool isRelocated, bool isVisibleOnMap)
    {
      Name = RelocatedCluster.GetClusterName(cluster);
      Macro = cluster?.Macro ?? string.Empty;
      X = cluster?.Position?.X ?? 0;
      Z = cluster?.Position?.Z ?? 0;
      OverlaidCount = overlappingClusters?.Count() ?? 0;
      OverlaidWith =
        overlappingClusters == null
          ? string.Empty
          : string.Join(", ", overlappingClusters.Select(c => $"{RelocatedCluster.GetClusterName(c)} ({c.Macro})"));
      IsRelocated = isRelocated;
      IsVisibleOnMap = isVisibleOnMap;
    }
  }
}
