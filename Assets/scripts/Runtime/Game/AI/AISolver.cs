// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

namespace Pon
{
  public class AISolver
  {
    private Grid grid;
    private Queue<AIMove> movesToPlay = new Queue<AIMove>();

    private AISettings settings;
    private AIHeuristic heuristic;

    private int nothingToDoCount;

    public AISolver(Grid g, AISettings s)
    {
      grid = g;
      settings = s;
      heuristic = new AIHeuristic()
      {
        Values = s.weights
      };
    }

    public void Work()
    {
#if UNITY_EDITOR
      Profiler.BeginSample("AI Solver");
#endif

      // Reset
      PlayMove = false;
      SpeedUp = false;
      movesToPlay.Clear();

      // Make sure the AI has enough lines to work with
      int maxY = grid.GetNonEmptyTopLineY();
      if (maxY <= settings.minLinesBeforeSpeed && settings.minLinesBeforeSpeed > 0)
      {
        SpeedUp = true;
      }
      else
      {
        // We're looking for the best possible path in a tree decision
        // We're doing it on "n" levels. 
        // Each level increase chances to get the best move but it's More and MORE expensive!!!!

        // Get a minimalist version of the grid
        var g = grid.ToIntArray();

        // Get all moves from the current grid
        var moves = GetWeightedMoves(0, 0, g, 0, grid.width, null); // Level 0

        // Then explore 
        for (int depth = 1; depth <= settings.maxDepth - 1; depth++)
        {
          var newTopMoves = new List<AIMove>();

          // Use the moves
          for (int i = 0; i < moves.Count; i++)
          {
            var move = moves[i];
            var children = GetWeightedMoves(depth, i, move.gridAfterMove, move.x - 1, move.x + 1, move);

            // Add the results
            if (children.Count > 0)
            {
              newTopMoves.AddRange(children);
            }
            else
            {
              newTopMoves.Add(move);
            }
          }

          // Rince and repeat. Keep only the children.
          moves = newTopMoves;
        }

        // Now explore the tree. We have all the children, pick the best
        moves = moves
          .OrderByDescending(m => m.weight)
          .ThenBy(m => m.depthLevel)
          //.Take(4)
          .ToList();

        PlayMove = false;

        if (moves.Count > 0)
        {
          // Pick the best?
          int index = 0;
          float r = Random.Range(0f, 1f);
          if (moves.Count > 1 &&
              settings.pickNotTheBestProbability > 0 && r < settings.pickNotTheBestProbability)
          {
            index = (int) Random.Range(r * moves.Count, settings.pickNotTheBestProbability * moves.Count);
          }

          var lastMove = moves[index];
          if (lastMove.comboCount > 0)
          {
            PrepareMoveToPlay(lastMove);
          }
          else
          {
            nothingToDoCount++;

            if (nothingToDoCount >= settings.maxDoingNothingMoments)
            {
              PrepareMoveToPlay(lastMove);
            }
            else
            {
              SpeedUp = (settings.speedUpWhenNoComboProbability > 0 &&
                         Random.Range(0f, 1f) < settings.speedUpWhenNoComboProbability);
            }
          }
        }
        else
        {
          PlayMove = false;
        }
      }

#if UNITY_EDITOR
      Profiler.EndSample();
#endif
    }


    private void PrepareMoveToPlay(AIMove move)
    {
      nothingToDoCount = 0;
      PlayMove = true;
      var path = GetPathFromChildren(move);
      foreach (var m in path)
      {
        movesToPlay.Enqueue(m);
      }
    }

    private List<AIMove> GetPathFromChildren(AIMove child)
    {
      List<AIMove> path = new List<AIMove>() {child};
      AIMove current = child.parent;
      while (current != null)
      {
        path.Insert(0, current);
        current = current.parent;
      }

      return path;
    }


    /// <summary>
    /// Compute a list of possible moves from a given grid
    /// </summary>
    /// <param name="level">Depth</param>
    /// <param name="n">Branch index</param>
    /// <param name="g"></param>
    /// <param name="minX">Left bound</param>
    /// <param name="maxX">Right bound</param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public List<AIMove> GetWeightedMoves(int level, int n, sbyte[,] g, int minX, int maxX, AIMove parent)
    {
      int width = g.GetUpperBound(0) + 1;
      int height = g.GetUpperBound(1) + 1;

      var moves = GetMoves(level, g, minX, maxX, parent);
      if (moves.Count > 0)
      {
        foreach (var m in moves)
        {
          m.gridAfterMove = GetGridWithMove(g, m);

          var weightData = heuristic.WeightMove(m, width, height, level);

          m.weight = weightData.total + (parent?.weight ?? 0);
          m.depthLevel = level;
          m.comboCount = weightData.combosCount + (parent?.comboCount ?? 0);

          m.gridAfterMove = RemoveCombos(m.gridAfterMove, weightData.blocksInCombo);
        }
      }

      return moves;
    }

    /// <summary>
    /// Get all possible moves from the simplified grid
    /// </summary>
    public List<AIMove> GetMoves(int level, sbyte[,] g, int minX, int maxX, AIMove parent)
    {
      // Explore the grid!
      int width = g.GetUpperBound(0) + 1;
      int height = g.GetUpperBound(1) + 1;

      // Find the moves
      var moves = new List<AIMove>();
      int currentCount = 0;
      int limit = settings.movesLimits.Length > level ? settings.movesLimits[level] : int.MaxValue;

      // Look left only on the first step. Then right only.
      int start = minX + 1;
      for (int x = start;
        x < maxX;
        x++)
      {
        // Be sure we're in the grid
        if (x < 0 || x >= width) continue;

        for (int y = 0; y < height; y++)
        {
          int block = g[x, y];
          if (block >= 0 && block < 99) // Not empty & not garbage
          {
            // Check left
            if (x > 0 && x == start)
            {
              if (block != 0 || g[x - 1, y] != 0)
              {
                if (parent == null || (parent.x != (x - 1) && parent.y != y && parent.direction != -1))
                {
                  moves.Add(new AIMove(x, y, -1, parent));
                  currentCount++;
                }
              }
            }

            // Check right
            if (x < width - 1)
            {
              if (block != 0 || g[x + 1, y] != 0)
              {
                if (parent == null || (parent.x != (x + 1) && parent.y != y && parent.direction != 1))
                {
                  moves.Add(new AIMove(x, y, 1, parent));
                  currentCount++;
                }
              }
            }
          }

          if (level > 0 && currentCount > limit) return moves;
        }
      }

      // First depth = randomize please
      if (level == 0 && limit > 0)
      {
        return moves.OrderBy(m => Random.Range(0f, 1f)).Take(limit).ToList();
      }

      return moves;
    }

    /// <summary>
    /// Apply the move to the simplified grid and returns the new grid (with uncleared combos)
    /// </summary>
    public sbyte[,] GetGridWithMove(sbyte[,] baseGrid, AIMove move)
    {
      // Copy array
      sbyte[,] gridCopy = (sbyte[,]) baseGrid.Clone();
      int x1 = move.x;
      int x2 = move.x + move.direction;
      int yBase = move.y;

      // Swap
      sbyte a = baseGrid[x1, yBase];
      sbyte b = baseGrid[x2, yBase];
      gridCopy[x1, yBase] = b;
      gridCopy[x2, yBase] = a;

      // Do the fall checks from bot to top
      int pxStart = x1 < x2 ? x1 : x2;
      int pxMax = x1 > x2 ? x1 : x2;

      gridCopy = ForceFall(gridCopy, pxStart, pxMax);

      return gridCopy;
    }

    private sbyte[,] RemoveCombos(sbyte[,] source, Vector2[] combos)
    {
      sbyte[,] gridCopy = (sbyte[,]) source.Clone();
      if (combos == null || combos.Length == 0)
      {
        return gridCopy;
      }

      foreach (var p in combos)
      {
        gridCopy[(int) p.x, (int) p.y] = 0;
      }

      int xStart = (int) combos[0].x;
      int xEnd = (int) combos[combos.Length - 1].x;

      gridCopy = ForceFall(gridCopy, xStart, xEnd);

      return gridCopy;
    }

    private sbyte[,] ForceFall(sbyte[,] source, int pxStart, int pxMax)
    {
      int width = source.GetUpperBound(0) + 1;
      int height = source.GetUpperBound(1) + 1;

      // Do the fall checks from bot to top
      for (int px = Mathf.Max(0, pxStart);
        px <= Mathf.Min(width - 1, pxMax);
        px++)
      {
        for (int py = 0; py < height; py++)
        {
          var block = source[px, py];

          // Ignore garbage
          if (block > 0 && block < 99)
          {
            for (int yFall = py - 1; yFall >= 0; yFall--)
            {
              if (source[px, yFall] == 0)
              {
                source[px, yFall] = block;
                source[px, yFall + 1] = 0;
              }
            }
          }
        }
      }

      return source;
    }

    #region Properties

    public Queue<AIMove> Moves => movesToPlay;

    public bool PlayMove { get; protected set; }

    public bool SpeedUp { get; protected set; }

    #endregion
  }
}