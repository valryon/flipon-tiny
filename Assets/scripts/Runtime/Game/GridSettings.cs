// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using UnityEngine;

namespace Pon
{
  #region Grid & Gameplay settings

  [System.Serializable]
  public class GridSettings
  {
    public const int DEFAULT_WIDTH = 6;
    public const int DEFAULT_HEIGHT = 10;

    [Header("Size")]
    public int width = DEFAULT_WIDTH;

    public int height = DEFAULT_HEIGHT;

    [Range(0, 2)]
    public int previewLines = 2;

    [Header("Start")]
    public int startLines = 3;

    public int startLevel = 1;
    public int limitLevel = -1;

    [Header("Scroll")]
    public float speedUpDuration = 0.2f;

    public bool noScrolling = false;
    public bool noLevelUp = false;

    public bool hideOnPause;

    public GridSettings(int level = 1)
    {
      startLevel = Mathf.Max(1, level);
    }

    public GridSettings Clone()
    {
      return (GridSettings) this.MemberwiseClone();
    }
  }

  #endregion

  public struct ComboData
  {
    public int blockCount;
    public int multiplier;
    public bool isChain;
    public Vector3 comboLocation;
    public BlockDefinition definition;

    public ComboData(int b, int multiplier, bool isChain, Vector3 p, BlockDefinition d)
    {
      blockCount = b;
      this.multiplier = multiplier;
      this.isChain = isChain;
      comboLocation = p;
      definition = d;
    }
  }
}