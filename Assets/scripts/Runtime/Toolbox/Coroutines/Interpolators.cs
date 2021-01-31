// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using System;
using System.Collections;
using UnityEngine;

namespace Pon
{
  // TODO: Remove and replace entirely by DOTween
  /// <summary>
  /// Coroutine helpers for interpolators
  /// </summary>
  public class Interpolators
  {
    public readonly static AnimationCurve LinearCurve;
    public readonly static AnimationCurve EaseOutCurve;
    public readonly static AnimationCurve EaseInCurve;
    public readonly static AnimationCurve EaseInOutCurve;
    public readonly static AnimationCurve BumpCurve;

    static Interpolators()
    {
      // See: http://www.alsacreations.com/tuto/lire/876-transitions-css3-transition-timing-function.html
      LinearCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

      EaseOutCurve = new AnimationCurve();
      EaseOutCurve.AddKey(new Keyframe(0f, 0f)
      {
        inTangent = 2,
        outTangent = 2,
      });
      EaseOutCurve.AddKey(new Keyframe(1f, 1f)
      {
        inTangent = 0,
        outTangent = 0,
      });

      EaseInCurve = new AnimationCurve();
      EaseInCurve.AddKey(new Keyframe(0f, 0f)
      {
        inTangent = 0,
        outTangent = 0,
      });
      EaseInCurve.AddKey(new Keyframe(1f, 1f)
      {
        inTangent = 2,
        outTangent = 2,
      });


      BumpCurve = new AnimationCurve();
      BumpCurve.AddKey(new Keyframe(0f, 0f)
      {
        inTangent = 0,
        outTangent = 0,
      });
      BumpCurve.AddKey(new Keyframe(0.7f, 1.2f)
      {
        inTangent = 0,
        outTangent = 0,
      });
      BumpCurve.AddKey(new Keyframe(1f, 1f)
      {
        inTangent = 0,
        outTangent = 0,
      });

      EaseInOutCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    }

    /// <summary>
    /// Linear interpolation
    /// </summary>
    /// <remarks>Formula: min + (max - min) * t</remarks>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="duration"></param>
    /// <param name="step"></param>
    /// <param name="completed"></param>
    /// <returns></returns>
    public static IEnumerator Linear(float from, float to, float duration, Action<float> step, Action completed)
    {
      return InterpolateFloat(false, LinearCurve, from, to, duration, step, completed);
    }

    /// <summary>
    /// Linear interpolation independant from Time.timeScale
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="duration"></param>
    /// <param name="step"></param>
    /// <param name="completed"></param>
    /// <returns></returns>
    public static IEnumerator LinearRealtime(float from, float to, float duration, System.Action<float> step,
      System.Action completed)
    {
      return InterpolateFloat(true, LinearCurve, from, to, duration, step, completed);
    }

    /// <summary>
    /// Linear interpolation
    /// </summary>
    /// <remarks>Formula: min + (max - min) * t</remarks>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="duration"></param>
    /// <param name="step"></param>
    /// <param name="completed"></param>
    /// <returns></returns>
    public static IEnumerator Linear(Vector3 from, Vector3 to, float duration, Action<Vector3> step, Action completed)
    {
      return InterpolateVector(false, LinearCurve, from, to, duration, step, completed);
    }

    /// <summary>
    /// Interpolation with curves
    /// </summary>
    /// <param name="curve"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="duration"></param>
    /// <param name="step"></param>
    /// <param name="completed"></param>
    /// <returns></returns>
    public static IEnumerator Curve(AnimationCurve curve, float from, float to, float duration,
      System.Action<float> step, System.Action completed)
    {
      return InterpolateFloat(false, curve, from, to, duration, step, completed);
    }

    /// <summary>
    /// Interpolation with curves
    /// </summary>
    /// <param name="curve"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="duration"></param>
    /// <param name="step"></param>
    /// <param name="completed"></param>
    /// <returns></returns>
    public static IEnumerator Curve(AnimationCurve curve, Vector3 from, Vector3 to, float duration,
      System.Action<Vector3> step, System.Action completed)
    {
      return InterpolateVector(false, curve, from, to, duration, step, completed);
    }

    /// <summary>
    /// Interpolation with curves independant from Time.timeScale
    /// </summary>
    /// <param name="curve"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="duration"></param>
    /// <param name="step"></param>
    /// <param name="completed"></param>
    /// <returns></returns>
    public static IEnumerator CurveRealtime(AnimationCurve curve, Vector3 from, Vector3 to, float duration,
      System.Action<Vector3> step, System.Action completed)
    {
      return InterpolateVector(true, curve, from, to, duration, step, completed);
    }

    /// <summary>
    /// Interpolation with curves independant from Time.timeScale
    /// </summary>
    /// <param name="curve"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="duration"></param>
    /// <param name="step"></param>
    /// <param name="completed"></param>
    /// <returns></returns>
    public static IEnumerator CurveRealtime(AnimationCurve curve, float from, float to, float duration,
      System.Action<float> step, System.Action completed)
    {
      return InterpolateFloat(true, curve, from, to, duration, step, completed);
    }

    /// <summary>
    /// Interpolator implementation
    /// </summary>
    /// <param name="realtime"></param>
    /// <param name="curve"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="duration"></param>
    /// <param name="step"></param>
    /// <param name="completed"></param>
    /// <returns></returns>
    public static IEnumerator InterpolateFloat(bool realtime, AnimationCurve curve, float from, float to,
      float duration, System.Action<float> step, System.Action completed)
    {
      float t = 0;

      float lastTime = Time.realtimeSinceStartup;

      while (t < 1)
      {
        yield return null;

        float timeDela = Time.deltaTime;
        if (realtime)
        {
          timeDela = (Time.realtimeSinceStartup - lastTime);
        }

        t += (timeDela / duration);

        float stepValue = Mathf.Lerp(from, to, curve.Evaluate(t));

        if (step != null)
        {
          step(stepValue);
        }

        lastTime = Time.realtimeSinceStartup;
      }

      if (completed != null)
      {
        completed();
      }
    }

    /// <summary>
    /// Interpolator implementation
    /// </summary>
    /// <param name="realtime"></param>
    /// <param name="curve"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="duration"></param>
    /// <param name="step"></param>
    /// <param name="completed"></param>
    /// <returns></returns>
    public static IEnumerator InterpolateVector(bool realtime, AnimationCurve curve, Vector3 from, Vector3 to,
      float duration, System.Action<Vector3> step, System.Action completed)
    {
      float t = 0;

      float lastTime = Time.realtimeSinceStartup;

      while (t < 1)
      {
        yield return null;

        float timeDela = Time.deltaTime;
        if (realtime)
        {
          timeDela = (Time.realtimeSinceStartup - lastTime);
        }

        t += (timeDela / duration);

        Vector3 stepValue = Vector3.Lerp(from, to, curve.Evaluate(t));

        if (step != null)
        {
          step(stepValue);
        }

        lastTime = Time.realtimeSinceStartup;
      }

      if (completed != null)
      {
        completed();
      }
    }
  }
}