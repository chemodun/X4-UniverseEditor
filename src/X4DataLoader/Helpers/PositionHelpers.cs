using System.Globalization;

namespace X4DataLoader.Helpers
{
  public static class PositionHelper
  {
    public static bool IsSamePosition(Position pos1, Position pos2)
    {
      return pos1.X == pos2.X && pos1.Y == pos2.Y && pos1.Z == pos2.Z;
    }
  }
}
