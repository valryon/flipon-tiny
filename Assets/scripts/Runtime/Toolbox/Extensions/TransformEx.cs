// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pon
{
  public static class TransformEx
  {
    /// <summary>
    /// Is the transform in the screen space?
    /// </summary>
    /// <param name="transform"></param>
    /// <returns></returns>
    public static bool IsOnScreen(this Transform transform)
    {
      // On camera?
      Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position);

      if (screenPosition.x > 0 && screenPosition.x < Screen.width)
      {
        if (screenPosition.y > 0 && screenPosition.y < Screen.height)
        {
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Look for a component in parent
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="transform"></param>
    /// <returns></returns>
    public static T GetComponentInParent<T>(this Transform transform)
      where T : UnityEngine.Component
    {
      Transform parent = transform.parent;

      while (parent != null)
      {
        T component = parent.GetComponent<T>();

        if (component != null)
        {
          return component;
        }

        parent = parent.parent;
      }

      return default(T);
    }

    /// <summary>
    /// Transform's children as a list
    /// </summary>
    /// <returns></returns>
    public static List<Transform> GetChildren(this Transform t)
    {
      List<Transform> children = new List<Transform>();

      for (int i = 0; i < t.childCount; i++)
      {
        children.Add(t.GetChild(i));
      }

      return children;
    }
  }
}