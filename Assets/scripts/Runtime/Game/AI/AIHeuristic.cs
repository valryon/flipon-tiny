using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Pon
{
  [Serializable]
  public class WeightValues
  {
    public int depth = -20;
    public int combo3 = 100;
    public int combo4 = 300;
    public int combo5 = 800;
    public int comboL = 1000;
    public int garbage = 500;
    public int danger = 1500;
    public int objective = 1000;
  }

  public struct WeightData
  {
    public int total;
    public Vector2[] blocksInCombo;
    public int combosCount;
  }

  public class AIHeuristic
  {
    private int blocksInComboIndex = 0;
    private Vector2[] blocksInCombo = new Vector2[20];
    private int currentComboBlocksIndex = 0;
    private Vector2[] currentComboBlocks = new Vector2[10];

    public WeightValues Values { get; set; } = new WeightValues();

    /// <summary>
    /// Compute a weight on the move 
    /// </summary>
    public virtual WeightData WeightMove(AIMove m, int width, int height, int depth)
    {
      var w = new WeightData();
      blocksInComboIndex = 0;
      currentComboBlocksIndex = 0;

      if (m.gridAfterMove == null)
      {
        Log.Error("AI - Missing grid for move!");
        return w;
      }

      int weight = WeightGeneral(m, depth);

      // Compute combos      
      sbyte currentBlock = 0;
      int currentCombo = 0;

      void CommitCommbo()
      {
        weight += WeightCombo(currentCombo, currentBlock, m.gridAfterMove, width, height);

        w.combosCount++;

        // Transfer current list to general list
        for (int i = 0; i < currentComboBlocksIndex; i++)
        {
          if (blocksInComboIndex < blocksInCombo.Length)
          {
            blocksInCombo[blocksInComboIndex] = currentComboBlocks[i];
            blocksInComboIndex++;
          }
        }
      }

      var highestBlock = Vector2.one * -1;

      // Vertical
      for (int x = 0; x < width; x++)
      {
        for (int y = 0; y < height; y++)
        {
          sbyte block = m.gridAfterMove[x, y];
          if (block == 99) continue;

          // Not in a combo
          if (currentBlock == 0)
          {
            if (block > 0)
            {
              currentBlock = block;
              currentCombo = 1;

              // Same than a list.Add(new Vector) but better when 10 000 elements! 
              currentComboBlocks[currentComboBlocksIndex].x = x;
              currentComboBlocks[currentComboBlocksIndex].y = y;
              currentComboBlocksIndex++;
            }
            else
            {
              currentBlock = 0;
              currentCombo = 0;
              currentComboBlocksIndex = 0;
            }
          }
          else
          {
            // Blocks are falling
            if (block == 0)
            {
              // Skip
              continue;
            }

            // Check for an identical block
            if (currentBlock == block)
            {
              currentCombo++;

              currentComboBlocks[currentComboBlocksIndex].x = x;
              currentComboBlocks[currentComboBlocksIndex].y = y;
              currentComboBlocksIndex++;
            }
            // Combo breaker
            else
            {
              if (currentCombo >= 3) CommitCommbo();

              // Reset
              currentCombo = 1;
              currentBlock = block;
              currentComboBlocks[currentComboBlocksIndex].x = x;
              currentComboBlocks[currentComboBlocksIndex].y = y;
              currentComboBlocksIndex++;
            }
          }

          if (block > 0)
          {
            if (y > highestBlock.y)
            {
              highestBlock.x = x;
              highestBlock.y = y;
            }
          }
        } // y

        if (currentCombo >= 3) CommitCommbo();

        // Reset
        currentBlock = 0;
        currentCombo = 0;
        currentComboBlocksIndex = 0; // list.Clear
      } // x

      // Horizontal
      for (int y = 0; y < height; y++)
      {
        for (int x = 0; x < width; x++)
        {
          sbyte block = m.gridAfterMove[x, y];
          if (block == 99) continue;

          // Not in a combo
          if (currentBlock == 0)
          {
            if (block > 0)
            {
              currentBlock = block;
              currentCombo = 1;
              currentComboBlocks[currentComboBlocksIndex].x = x;
              currentComboBlocks[currentComboBlocksIndex].y = y;
              currentComboBlocksIndex++;
            }
            else
            {
              currentBlock = 0;
              currentCombo = 0;
              currentComboBlocksIndex = 0;
            }
          }
          else
          {
            // Check for an identical block
            if (currentBlock == block)
            {
              currentCombo++;
              currentComboBlocks[currentComboBlocksIndex].x = x;
              currentComboBlocks[currentComboBlocksIndex].y = y;
              currentComboBlocksIndex++;
            }
            // Combo breaker
            else
            {
              if (currentCombo >= 3) CommitCommbo();

              // Reset
              currentCombo = 1;
              currentBlock = block;
              currentComboBlocks[currentComboBlocksIndex].x = x;
              currentComboBlocks[currentComboBlocksIndex].y = y;
              currentComboBlocksIndex++;
            }
          }
        } // x

        if (currentCombo >= 3) CommitCommbo();

        // Reset
        currentBlock = 0;
        currentCombo = 0;
        currentComboBlocksIndex = 0;
      } // y

      // Check if the game over is near
      weight += WeightDanger(m, highestBlock, w);

      w.total = weight;
      w.blocksInCombo = blocksInCombo.Take(blocksInComboIndex).ToArray();

      return w;
    }

    #region Overridables weights

    public virtual int WeightGeneral(AIMove m, int depth)
    {
      // Weight is slightly impacted by the number of moves required to get to the combo
      // SHOULD BE NEGATIVE. GOING DEEPER IS NOT BETTER.

      // We could weight anything else here

      return depth * Values.depth;
    }

    public virtual int WeightDanger(AIMove m, Vector2 highestBlock, WeightData w)
    {
      const float DANGER_HEIGHT = 0.77f;

      // Combos avoid danger.
      // So doing combos is always nice, better than throwing blocks away
      if (w.combosCount > 0)
      {
        return w.combosCount * Values.danger;
      }

      int weight = 0;
      //int width = m.gridAfterMove.GetUpperBound(0) + 1;
      int height = m.gridAfterMove.GetUpperBound(1) + 1;
      int dangerHeightThreshold = (int) (height * DANGER_HEIGHT);
      if (highestBlock.y > dangerHeightThreshold)
      {
        if (highestBlock.y > GetHighest(m.gridAfterMove).y)
        {
          weight += Values.danger;
        }
      }

      return weight;
    }

    public virtual int WeightCombo(int currentCombo, sbyte blockType, sbyte[,] grid, int gridWidth, int gridHeight)
    {
      int weight = 0;
      if (currentCombo == 3) weight += Values.combo3;
      else if (currentCombo == 4) weight += Values.combo4;
      else if (currentCombo == 5) weight += Values.combo5;
      else if (currentCombo > 5) weight += Values.comboL;

      if (weight > 0)
      {
        // Touching a garbage block?
        Vector2[] neighbors =
        {
          new Vector2(-1, 0),
          new Vector2(1, 0),
          new Vector2(0, 1),
          new Vector2(0, -1),
        };

        foreach (var v in neighbors)
        {
          int x = (int) v.x;
          int y = (int) v.y;

          if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
          {
            sbyte n = grid[x, y];
            if (n == 99)
            {
              weight += Values.garbage;
            }
          }
        }
      }

      return weight;
    }

    #endregion

    #region Private methods

    public Vector2 GetHighest(sbyte[,] g)
    {
      int width = g.GetUpperBound(0) + 1;
      int height = g.GetUpperBound(1) + 1;
      var highestBlock = Vector2.zero;

      for (int x = 0; x < width; x++)
      {
        for (int y = 0; y < height; y++)
        {
          sbyte block = g[x, y];

          if (block > 0)
          {
            if (y > highestBlock.y)
            {
              highestBlock = new Vector2(x, y);
            }
          }
        }
      }

      return highestBlock;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsSameCombo(sbyte currentBlock, sbyte block)
    {
      return (currentBlock == block) && block > 0 && block < 99;
    }

    #endregion
  }
}