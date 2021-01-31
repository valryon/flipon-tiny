// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pon
{
  /// <summary>
  /// Access Blocks from everywhere.
  /// </summary>
  public class BlockDefinitionBank : MonoBehaviour, IBlockFactory
  {
    public static BlockDefinitionBank Instance;
    private static sbyte LastId = 1;

    public BlockDefinition[] definitions;
    public Sprite unknowBlockSprite;

    #region Timeline

    protected void Awake()
    {
      if (Instance != null) return;

      Instance = this;

      foreach (var b in definitions)
      {
        b.id = LastId;
        LastId++;
      }
    }

    #endregion

    #region IBlockFactory

    public List<BlockDefinition> GetNormalBlocks(int level = 99, int width = 0)
    {
      return definitions.Where(b =>
        b.isGarbage == false &&
        b.minLevel <= level &&
        (width <= 0 || (b.minWidth <= width))
      ).Select(b => b).ToList();
    }

    public List<BlockDefinition> GetGarbages(int level = 99)
    {
      return definitions.Where(b => b.isGarbage && b.minLevel <= level).ToList();
    }

    public BlockDefinition GetRandomNormal(int level, int width, GameRandom gameRandom = null,
      List<BlockDefinition> excludedDef = null)
    {
      var n = GetNormalBlocks(level, width);

      if (excludedDef != null && excludedDef.Count > 0)
      {
        n.RemoveAll(b => excludedDef.Contains(b));
      }

      if (n.Count > 0)
      {
        if (gameRandom != null)
        {
          return n[gameRandom.Range(0, n.Count)];
        }
        else
        {
          return n[Random.Range(0, n.Count)];
        }
      }

      return null;
    }

    public BlockDefinition GetRandomGarbage(int level = 99)
    {
      var s = GetGarbages(level);

      if (s.Count > 0)
      {
        return s[Random.Range(0, s.Count)];
      }

      return null;
    }

    public BlockDefinition GetBlock(string n)
    {
      return definitions.FirstOrDefault(d => d.name.ToLower() == n.ToLower());
    }

    #endregion
  }
}