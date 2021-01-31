// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using UnityEngine;

namespace Pon
{
  public static class Vector3Ex
  {
    public static Vector3 CurrentMiddleScreen()
    {
      Vector3 middleScreen = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, 0));
      middleScreen.z = 0;

      return middleScreen;
    }

    public static string GetDetail(this Vector3 self)
    {
      return "(" + self.x + ", " + self.y + ", " + self.z + ")";
    }
  }
}