// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using System;
using System.Collections;
using UnityEngine;

namespace Pon
{
  /// <summary>
  /// Ready to use timers for coroutines
  /// </summary>
  public class Timer
  {
    /// <summary>
    /// Simple timer, no reference, wait and then execute something
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public static IEnumerator Start(float duration, Action callback)
    {
      return Start(duration, false, callback);
    }


    /// <summary>
    /// Simple timer, no reference, wait and then execute something
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="repeat"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public static IEnumerator Start(float duration, bool repeat, Action callback)
    {
      do
      {
        yield return new WaitForSeconds(duration);

        if (callback != null)
          callback();
      } while (repeat);
    }

    public static IEnumerator StartRealtime(float time, System.Action callback)
    {
      float start = Time.realtimeSinceStartup;
      while (Time.realtimeSinceStartup < start + time)
      {
        yield return null;
      }

      if (callback != null) callback();
    }

    public static IEnumerator NextFrame(Action callback)
    {
      yield return new WaitForEndOfFrame();

      if (callback != null)
        callback();
    }
  }
}