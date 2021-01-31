// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using System;
using UnityEngine;

namespace Pon
{
  public static class TextureEx
  {
    public static Texture2D Blank(int width, int height)
    {
      return Blank(width, height, Color.white * 1f);
    }

    public static Texture2D Blank(int width, int height, Color c)
    {
      var t = new Texture2D(width, height);
      t.filterMode = FilterMode.Point;

      for (int x = 0; x < t.width; x++)
      {
        for (int y = 0; y < t.height; y++)
        {
          t.SetPixel(x, y, c);
        }
      }

      t.Apply();

      return t;
    }

    public static Texture2D GradientHorizontal(int width, int height, Color color1, Color color2)
    {
      var t = new Texture2D(width, height);
      t.filterMode = FilterMode.Bilinear;

      for (int x = 0; x < t.width; x++)
      {
        float p = (x / (float) t.width);

        for (int y = 0; y < t.height; y++)
        {
          t.SetPixel(x, y, Color.Lerp(color1, color2, p));
        }
      }

      t.Apply();

      return t;
    }

    public static Sprite ToSprite(this Texture2D t, float pixelsPerUnit)
    {
      return ToSprite(t, new Vector2(0.5f, 0.5f), pixelsPerUnit);
    }

    public static Sprite ToSprite(this Texture2D t, Vector2 pivot, float pixelsPerUnit)
    {
      if (t == null)
      {
        return null;
      }

      return Sprite.Create(t, new Rect(0, 0, t.width, t.height), pivot, pixelsPerUnit);
    }
  }
}