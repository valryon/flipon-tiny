#if UNITY_4_6
// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pon
{
  public static class RectTransformEx
  {
    /// <summary>
    /// Rect as screen positions
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public static Rect GetScreenRect(this RectTransform t)
    {
      Vector3[] corners = new Vector3[4];
      t.GetWorldCorners(corners);

      return ArrayToRect(corners);
    }

    /// <summary>
    /// Rect as world positions
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public static Rect GetWorldRect(this RectTransform t)
    {
      Vector3[] corners = new Vector3[4];
      t.GetWorldCorners(corners);

      for (int i = 0; i < corners.Length; i++)
      {
        corners[i] = Camera.main.ScreenToWorldPoint(corners[i]);
      }

      return ArrayToRect(corners);
    }

    private static Rect ArrayToRect(Vector3[] corners)
    {
      float height = Mathf.Abs(corners[0].y - corners[1].y);
      float width = Mathf.Abs(corners[2].x - corners[0].x);

      return new Rect(corners[0].x, corners[0].y, width, height);
    }
  }
}
#endif