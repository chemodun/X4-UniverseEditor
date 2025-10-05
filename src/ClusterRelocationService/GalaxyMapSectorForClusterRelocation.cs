using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Utilities.Logging;
using X4DataLoader;
using X4DataLoader.Helpers;
using X4Map;

namespace ClusterRelocationService
{
  public class GalaxyMapSectorForClusterRelocation : GalaxyMapSector
  {
    private System.Windows.Media.Brush? _originalStroke = null;
    private bool _isMoved = false;
    public bool IsRelocated
    {
      get => _isMoved;
      set
      {
        if (_isMoved == value)
        {
          return;
        }
        _isMoved = value;
        UpdateStatus();
        OnPropertyChanged(nameof(IsRelocated));
      }
    }

    private bool _isCurrent = false;
    public bool IsCurrent
    {
      get => _isCurrent;
      set
      {
        if (_isCurrent == value)
        {
          return;
        }
        _isCurrent = value;
        UpdateStatus();
        OnPropertyChanged(nameof(IsCurrent));
      }
    }

    private bool _isMarkedForRelocation = false;
    public bool IsMarkedForRelocation
    {
      get => _isMarkedForRelocation;
      set
      {
        if (_isMarkedForRelocation == value)
        {
          return;
        }
        _isMarkedForRelocation = value;
        UpdateStatus();
        OnPropertyChanged(nameof(IsMarkedForRelocation));
      }
    }

    private bool _isOverlapped = false;
    public bool IsOverlapped
    {
      get => _isOverlapped;
      set
      {
        if (_isOverlapped == value)
        {
          return;
        }
        _isOverlapped = value;
        UpdateStatus();
        OnPropertyChanged(nameof(IsOverlapped));
      }
    }

    public GalaxyMapSectorForClusterRelocation(
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
      : base(x, y, owner, canvas, cluster, sector, hexagonWidth, hexagonHeight, scaleFactor, isHalf) { }

    public override double Create(GalaxyMapViewer map)
    {
      double result = base.Create(map);
      UpdateStatus();
      return result;
    }

    private void SetMark(System.Windows.Media.Brush? brush)
    {
      if (Hexagon == null)
      {
        return;
      }
      if (brush != null)
      {
        if (_originalStroke == null)
        {
          _originalStroke = Hexagon.Stroke;
        }
        Hexagon.Stroke = brush;
        Hexagon.StrokeThickness = 3;
      }
      else
      {
        if (_originalStroke != null)
        {
          Hexagon.Stroke = _originalStroke;
          Hexagon.StrokeThickness = 1;
          _originalStroke = null;
        }
      }
    }

    private void UpdateStatus()
    {
      if (Canvas == null)
      {
        return;
      }
      if (Hexagon == null)
      {
        return;
      }
      if (IsMarkedForRelocation)
      {
        SetMark(GalaxyMapViewerForClusterRelocation.BrushOnRelocation);
      }
      else if (IsCurrent)
      {
        SetMark(GalaxyMapViewerForClusterRelocation.BrushCurrent);
      }
      else if (IsRelocated)
      {
        SetMark(GalaxyMapViewerForClusterRelocation.BrushRelocated);
      }
      else if (IsOverlapped)
      {
        SetMark(GalaxyMapViewerForClusterRelocation.BrushIfOverlapped);
      }
      else
      {
        SetMark(null);
      }
    }
  }
}
