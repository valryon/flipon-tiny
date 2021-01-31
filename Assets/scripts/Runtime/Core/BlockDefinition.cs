// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using UnityEngine;

namespace Pon
{
  /// <summary>
  /// Usable block in game.
  /// </summary>
  [CreateAssetMenu(menuName = "Flipon/█ Block Definition", fileName = "Block")]
  public class BlockDefinition : ScriptableObject
  {
    [HideInInspector]
    public sbyte id;

    [Header("Block")]
    public Color color;

    public Sprite sprite;
    public Material material;

    public virtual bool isGarbage => false;

    private bool IsNotGarbage => !isGarbage;

    [Range(0, 8)]
    public int minWidth = 0;

    [Range(0, 99)]
    public int minLevel = 0;

    public override string ToString()
    {
      return name;
    }
  }
}