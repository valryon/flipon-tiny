// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pon
{
  public static class GameObjectEx
  {
    /// <summary>
    /// Ensure you always get a valid and existing component
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="gameObject"></param>
    /// <returns></returns>
    public static T AddOrGetComponent<T>(this GameObject gameObject)
      where T : Component
    {
      T component = gameObject.GetComponent<T>();
      if (component == default(T))
      {
        component = gameObject.AddComponent<T>();
      }

      return component;
    }

    /// <summary>
    /// Look for a component in parent
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="transform"></param>
    /// <returns></returns>
    public static T GetComponentInParent<T>(this GameObject gameObject)
      where T : UnityEngine.Component
    {
      return gameObject.transform.GetComponentInParent<T>();
    }
  }
}