// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using UnityEngine;

namespace Pon
{
  public static class Vector2Ex
  {
    public static string GetDetail(this Vector2 self)
    {
      return "(" + self.x + ", " + self.y + ")";
    }
  }
}