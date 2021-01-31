// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using UnityEngine;

namespace Pon
{
  /// <summary>
  /// Usable block in game.
  /// </summary>
  [System.Serializable]
  public class GarbageBlockDefinition : BlockDefinition
  {
    /// <summary>
    /// Block cannot combo.
    /// </summary>
    public override bool isGarbage => true;

    [Header("Garbages")]
    public Sprite leftSprite;

    public Sprite middleSprite;
    public Sprite rightSprite;
  }
}