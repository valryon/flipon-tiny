// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using UnityEngine;
using System.Collections;

namespace Pon
{
  public static class CoroutineUtils
  {
    public static IEnumerator WaitForRealSeconds(float time)
    {
      float start = Time.realtimeSinceStartup;
      while (Time.realtimeSinceStartup < start + time)
      {
        yield return null;
      }
    }
  }
}