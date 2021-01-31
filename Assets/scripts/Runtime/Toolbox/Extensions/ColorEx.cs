// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using UnityEngine;

namespace Pon
{
  public static class ColorEx
  {
    public static Color SetAlpha(this Color c, float a)
    {
      return new Color(c.r, c.g, c.b, a);
    }
  }
}