using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Utilities.Logging;
using X4DataLoader;
using X4DataLoader.Helpers;
using X4Map;

namespace ClusterRelocationService
{
  public class GalaxyMapViewerForClusterRelocation : GalaxyMapViewer
  {
    public static System.Windows.Media.Brush BrushRelocated = System.Windows.Media.Brushes.DarkGreen;
    public static System.Windows.Media.Brush BrushCurrent = SystemColors.HighlightBrush;
    public static System.Windows.Media.Brush BrushOnRelocation = System.Windows.Media.Brushes.DarkRed;
    public static System.Windows.Media.Brush BrushIfCovers = System.Windows.Media.Brushes.OrangeRed;
    private RelocatedCluster? _relocatedClusterCurrent = null;

    public override GalaxyMapCluster CreateMapCluster(
      double x,
      double y,
      MapPosition mapPosition,
      Canvas canvas,
      Cluster? cluster,
      Position? position,
      double hexagonWidth,
      double hexagonHeight,
      double scaleFactor
    )
    {
      var newCluster = new GalaxyMapClusterForClusterRelocation(
        x,
        y,
        mapPosition,
        canvas,
        cluster,
        position,
        hexagonWidth,
        hexagonHeight,
        scaleFactor
      );
      _clusters.Add(newCluster);
      IsCovers(newCluster);
      return newCluster;
    }

    public override GalaxyMapSector CreateMapSector(
      double x,
      double y,
      GalaxyMapCluster owner,
      Canvas canvas,
      Cluster cluster,
      Sector sector,
      double hexagonWidth,
      double hexagonHeight,
      double scaleFactor,
      bool isHalf = false
    )
    {
      var newSector = new GalaxyMapSectorForClusterRelocation(
        x,
        y,
        owner,
        canvas,
        cluster,
        sector,
        hexagonWidth,
        hexagonHeight,
        scaleFactor,
        isHalf
      );
      _sectors.Add(newSector);
      if (cluster.Sectors.Count == 1 && owner is GalaxyMapClusterForClusterRelocation ownerForRelocation)
      {
        if (ownerForRelocation.IsCovers)
        {
          newSector.IsCovers = true;
        }
      }
      ;
      return newSector;
    }

    public void IsCovers(GalaxyMapClusterForClusterRelocation cluster)
    {
      cluster.IsCovers = GetOverlaidClusters(cluster).Count > 0;
    }

    public List<Cluster> GetOverlaidClusters(GalaxyMapClusterForClusterRelocation cluster)
    {
      return GalaxyData
        .Clusters.Where(c =>
          c != null
          && c != null
          && cluster.Cluster != null
          && !StringHelper.EqualsIgnoreCase(c.Macro, cluster.Cluster.Macro)
          && Math.Abs(c.Position.X - cluster.OriginalX) < ColumnWidth
          && Math.Abs(c.Position.Z - cluster.OriginalZ) < RowHeight
        )
        .ToList();
    }

    public void ReCheckCovers()
    {
      foreach (
        var cluster in _clusters
          .Select(c => c as GalaxyMapClusterForClusterRelocation)
          .Where(c => c != null && c.IsCovers)
          .Cast<GalaxyMapClusterForClusterRelocation>()
      )
      {
        IsCovers(cluster);
      }
    }

    public event EventHandler<RelocationMessageEventArgs>? OnRelocationMessage;

    public Task OnUpdateRelocatedClusters(
      ObservableCollection<RelocatedCluster> relocatedClusters,
      System.Collections.Specialized.NotifyCollectionChangedEventArgs e
    )
    {
      switch (e.Action)
      {
        case NotifyCollectionChangedAction.Add:
          // React to added item(s)
          break;

        case NotifyCollectionChangedAction.Remove:
          foreach (RelocatedCluster oldItem in e.OldItems!)
          {
            // React to removed item(s)
            CancelRelocation(oldItem);
          }
          break;

        case NotifyCollectionChangedAction.Replace:
          // Handle replacement
          break;

        case NotifyCollectionChangedAction.Move:
          // Handle move
          break;

        case NotifyCollectionChangedAction.Reset:
          // Handle clear
          foreach (
            var cluster in _clusters
              .Select(c => c as GalaxyMapClusterForClusterRelocation)
              .Where(c => c != null && c.IsRelocated)
              .Cast<GalaxyMapClusterForClusterRelocation>()
          )
          {
            if (
              cluster != null
              && cluster.Cluster != null
              && !relocatedClusters.Any(c =>
                c != null && c.Cluster != null && StringHelper.EqualsIgnoreCase(c.Cluster.Macro, cluster.Cluster.Macro)
              )
            )
            {
              var isMoved = relocatedClusters.Any(rc => StringHelper.EqualsIgnoreCase(rc.Cluster.Macro, cluster.Cluster.Macro));
              if (!isMoved)
              {
                cluster.IsRelocated = false;
              }
            }
          }
          break;
      }
      return Task.CompletedTask;
    }

    public void OnRelocatedClusterCurrent(RelocatedCluster? relocatedClusterCurrent)
    {
      if (relocatedClusterCurrent != _relocatedClusterCurrent)
      {
        if (_relocatedClusterCurrent != null)
        {
          var foundOld = _clusters.Find(c =>
            c != null && c.Cluster != null && StringHelper.EqualsIgnoreCase(c.Cluster.Macro, _relocatedClusterCurrent.Cluster.Macro)
          );
          if (foundOld is GalaxyMapClusterForClusterRelocation galaxyMapClusterOld)
          {
            galaxyMapClusterOld.IsCurrent = false;
          }
          _relocatedClusterCurrent = null;
        }
        if (relocatedClusterCurrent != null)
        {
          var found = _clusters.Find(c =>
            c != null && c.Cluster != null && StringHelper.EqualsIgnoreCase(c.Cluster.Macro, relocatedClusterCurrent.Cluster.Macro)
          );
          if (found is GalaxyMapClusterForClusterRelocation galaxyMapCluster)
          {
            galaxyMapCluster.IsCurrent = true;
          }
        }
        _relocatedClusterCurrent = relocatedClusterCurrent;
      }
    }

    public GalaxyMapClusterForClusterRelocation? GetMapClusterForRelocated(RelocatedCluster cluster)
    {
      return _clusters.Find(c => c != null && c.Cluster != null && StringHelper.EqualsIgnoreCase(c.Cluster.Macro, cluster.Cluster.Macro))
        as GalaxyMapClusterForClusterRelocation;
    }

    public Task CancelRelocation(RelocatedCluster cluster)
    {
      GalaxyMapClusterForClusterRelocation? currentCluster = GetMapClusterForRelocated(cluster);
      if (currentCluster != null && currentCluster.Cluster != null)
      {
        Cluster galaxyCluster = currentCluster.Cluster;
        RemoveCluster(currentCluster);
        GalaxyMapClusterReassign(currentCluster, null);
        currentCluster.IsRelocated = false;
        currentCluster.IsCurrent = false;
        var foundOriginal = _clusters.Find(c => c.OriginalX == cluster.XOriginal && c.OriginalZ == cluster.ZOriginal);
        if (foundOriginal is GalaxyMapClusterForClusterRelocation originalCluster && originalCluster.Cluster == null)
        {
          galaxyCluster.SetPosition(new Position(cluster.XOriginal, originalCluster.OriginalY, cluster.ZOriginal));
          RemoveCluster(originalCluster);
          GalaxyMapClusterReassign(originalCluster, galaxyCluster);
          OnRelocationMessage?.Invoke(
            this,
            new RelocationMessageEventArgs(
              $"{RelocatedCluster.GetClusterName(galaxyCluster)} reset to original location ({cluster.XOriginal}, {cluster.ZOriginal})"
            )
          );
        }
      }
      return Task.CompletedTask;
    }

    public RelocatedCluster? RelocateCluster(
      GalaxyMapClusterForClusterRelocation currentCluster,
      GalaxyMapClusterForClusterRelocation targetCluster,
      RelocatedCluster? currentClusterRelocated = null
    )
    {
      RelocatedCluster? relocatedCluster = currentClusterRelocated;
      if (currentCluster == null || targetCluster == null || currentCluster.Cluster == null)
      {
        return relocatedCluster;
      }
      Cluster galaxyCluster = currentCluster.Cluster;
      RemoveCluster(currentCluster);
      GalaxyMapClusterReassign(currentCluster, null);
      RemoveCluster(targetCluster);
      if (relocatedCluster == null)
      {
        relocatedCluster = new RelocatedCluster(galaxyCluster);
      }
      currentCluster.IsRelocated = false;
      currentCluster.IsCurrent = false;
      relocatedCluster.SetPosition(new Position(targetCluster.OriginalX, targetCluster.OriginalY, targetCluster.OriginalZ));
      GalaxyMapClusterReassign(targetCluster, galaxyCluster);
      targetCluster.IsRelocated = true;

      OnRelocationMessage?.Invoke(
        this,
        new RelocationMessageEventArgs(
          $"{RelocatedCluster.GetClusterName(galaxyCluster)} relocated to ({targetCluster.OriginalX}, {targetCluster.OriginalZ})"
        )
      );
      return relocatedCluster;
    }

    public bool IsRelocatedClusterOriginalLocationOccupied(RelocatedCluster cluster)
    {
      var found = _clusters.Find(c => c.OriginalX == cluster.XOriginal && c.OriginalZ == cluster.ZOriginal);
      if (found is GalaxyMapClusterForClusterRelocation originalCluster)
      {
        return originalCluster.Cluster != null;
      }
      return true;
    }

    public GalaxyMapClusterForClusterRelocation? GetTargetClusterForRelocated(RelocatedCluster cluster)
    {
      var found = _clusters.Find(c => c.OriginalX == cluster.XCurrent && c.OriginalZ == cluster.ZCurrent);
      if (found is GalaxyMapClusterForClusterRelocation targetCluster)
      {
        return targetCluster;
      }
      return null;
    }

    public bool IsRelocatedClusterTargetLocationOccupied(RelocatedCluster cluster)
    {
      GalaxyMapClusterForClusterRelocation? found = GetTargetClusterForRelocated(cluster);
      if (found == null || found.Cluster != null)
      {
        return true;
      }
      return false;
    }

    public GalaxyMapClusterForClusterRelocation GetRandomFreeCluster()
    {
      var emptyClusters = _clusters
        .Select(c => c as GalaxyMapClusterForClusterRelocation)
        .Where(c => c != null && c.Cluster == null)
        .Cast<GalaxyMapClusterForClusterRelocation>()
        .ToList();
      if (emptyClusters.Count > 0)
      {
        var random = new System.Random();
        return emptyClusters[random.Next(emptyClusters.Count)];
      }
      throw new InvalidOperationException("No empty clusters available");
    }

    public async Task MakeFullMess(Galaxy galaxy, ObservableCollection<RelocatedCluster> relocatedClusters)
    {
      foreach (var cluster in galaxy.Clusters)
      {
        var found = _clusters.Find(c => c != null && c.Cluster != null && StringHelper.EqualsIgnoreCase(c.Cluster.Macro, cluster.Macro));
        if (found is GalaxyMapClusterForClusterRelocation currentCluster && currentCluster.Cluster != null)
        {
          var targetCluster = GetRandomFreeCluster();
          if (targetCluster != null)
          {
            var relocatedCluster = RelocateCluster(currentCluster, targetCluster);
            if (relocatedCluster != null)
            {
              relocatedClusters.Add(relocatedCluster);
              await Task.Delay(5);
              await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);
            }
          }
          else
          {
            Log.Warn($"No empty clusters available to relocate cluster {cluster.Macro}");
          }
        }
      }
    }

    public bool ShareSameCell(Position? a, Position? b)
    {
      if (a == null || b == null)
      {
        return false;
      }
      return Math.Abs(a.X - b.X) < ColumnWidth && Math.Abs(a.Z - b.Z) < RowHeight;
    }
  }

  public class RelocationMessageEventArgs(string message) : EventArgs
  {
    public string Message { get; } = message;
  }
}
