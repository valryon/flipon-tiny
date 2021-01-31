// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

namespace Pon
{
  public class AIMove
  {
    // Move data
    public int x;
    public int y;
    public int direction;

    // Tree data
    public int weight;
    public int comboCount;
    public sbyte[,] gridAfterMove;

    public int depthLevel;

    public AIMove parent;

    public AIMove()
    {
    }

    public AIMove(int x, int y, int direction)
    {
      this.x = x;
      this.y = y;
      this.direction = direction;
      weight = 0;
      gridAfterMove = null;

      depthLevel = 0;
    }

    public AIMove(int x, int y, int dir, AIMove parent)
      : this(x, y, dir)
    {
      this.parent = parent;
    }

    public override string ToString()
    {
      return $"(X:{x} Y:{y} dir:{direction}) weight:{weight} depth:{depthLevel} comboCount:{comboCount}";
    }

    public override bool Equals(object obj)
    {
      if (obj is AIMove m)
      {
        return m.x == x
               && m.y == y
               && m.direction == direction
               && m.depthLevel == depthLevel;
      }

      return false;
    }


// ReSharper disable NonReadonlyMemberInGetHashCode
    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = x;
        hashCode = (hashCode * 397) ^ y;
        hashCode = (hashCode * 397) ^ direction;
        hashCode = (hashCode * 397) ^ weight;
        hashCode = (hashCode * 397) ^ comboCount;
        hashCode = (hashCode * 397) ^ (gridAfterMove != null ? gridAfterMove.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ depthLevel;
        hashCode = (hashCode * 397) ^ (parent != null ? parent.GetHashCode() : 0);
        return hashCode;
      }
    }
  }
}