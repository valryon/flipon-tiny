using System.Collections.Generic;
// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.
using UnityEngine;

namespace Pon
{
  /// <summary>
  /// Extensions for Unity Random
  /// </summary>
  public static class RandomEx
  {
    /// <summary>
    /// Get a random Vector2
    /// </summary>
    /// <param name="xMin">X minimum.</param>
    /// <param name="xMax">X max.</param>
    /// <param name="yMin">Y minimum.</param>
    /// <param name="yMax">Y max.</param>
    public static Vector2 GetVector2(Vector2 min, Vector2 max)
    {
      return GetVector2(min.x, max.x, min.y, max.y);
    }

    /// <summary>
    /// Get a random Vector2
    /// </summary>
    /// <param name="xMin">X minimum.</param>
    /// <param name="xMax">X max.</param>
    /// <param name="yMin">Y minimum.</param>
    /// <param name="yMax">Y max.</param>
    public static Vector2 GetVector2(float xMin, float xMax, float yMin, float yMax)
    {
      return new Vector2(
        Random.Range(xMin, xMax),
        Random.Range(yMin, yMax)
      );
    }

    /// <summary>
    /// Return a random vector2 in a circle coordinates system
    /// </summary>
    /// <param name="center"></param>
    /// <param name="radius"></param>
    /// <returns></returns>
    public static Vector2 GetVector2(Vector2 center, float radius)
    {
      float randomTeta = Random.Range(0f, 360f);

      float x = Mathf.Cos(randomTeta) * radius;
      float y = Mathf.Sin(randomTeta) * radius;

      return new Vector2(Random.Range(center.x, center.x + x), Random.Range(center.y, center.y + y));
    }

    public static Vector2 GetVector2(Rect rect)
    {
      return GetVector2(rect.xMin, rect.xMax, rect.yMin, rect.yMax);
    }

    public static Vector2 GetVector2(Bounds bounds)
    {
      return GetVector2(bounds.min.x, bounds.max.x, bounds.min.y, bounds.max.y);
    }

    /// <summary>
    /// Get a random Vector3
    /// </summary>
    /// <param name="xMin">X minimum.</param>
    /// <param name="xMax">X max.</param>
    /// <param name="yMin">Y minimum.</param>
    /// <param name="yMax">Y max.</param>
    public static Vector3 GetVector3(Vector3 min, Vector3 max)
    {
      return GetVector3(min.x, max.x, min.y, max.y, min.z, max.z);
    }

    /// <summary>
    /// Get a random Vector3
    /// </summary>
    /// <param name="xMin">X minimum.</param>
    /// <param name="xMax">X max.</param>
    /// <param name="yMin">Y minimum.</param>
    /// <param name="yMax">Y max.</param>
    public static Vector3 GetVector3(float xMin, float xMax, float yMin, float yMax, float zMin, float zMax)
    {
      return new Vector3(
        Random.Range(xMin, xMax),
        Random.Range(yMin, yMax),
        Random.Range(zMin, zMax)
      );
    }

    /// <summary>
    /// Get a random color (alpha = 255)
    /// </summary>
    /// <returns>The color.</returns>
    public static Color GetColor()
    {
      return new Color(
        Random.Range(0f, 1f),
        Random.Range(0f, 1f),
        Random.Range(0f, 1f)
      );
    }

    /// <summary>
    /// Get the matching value of a probability table.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <example>
    ///   RandomProbability\<string\>({ 1: 'white', 9: 'black' }, 0)
    ///   # => 'white'
    ///   RandomProbability\<string\>({ 1: 'white', 9: 'black' }, 1)
    ///   # => 'black'
    ///   RandomProbability\<string\>({ 1: 'white', 9: 'black' }, 4)
    ///   # => 'black'
    ///   RandomProbability\<string\>({ 1: 'white', 9: 'black' }, 10)
    ///   # => null
    /// </example>
    /// <param name="protabiblityTable"></param>
    /// <param name="x">random value</param>
    /// <returns></returns>
    internal static T MatchingProbability<T>(List<KeyValuePair<float, T>> probabilityTable, float x)
    {
      float cumulativeWeight = 0;

      foreach (var proba in probabilityTable)
      {
        cumulativeWeight += proba.Key;

        if (x < cumulativeWeight)
        {
          return proba.Value;
        }
      }

      return default(T);
    }

    /// <summary>
    /// Get a random value from the probability table.
    /// The probability table defines a list of "chances" and a value for these chances.
    /// For example, we could have a probability table of 10% = 'white' color,
    /// and 90% = 'black' color. The result is drawn according to these entries.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="probabilityTable">The probability table Hash, ie. { 10: 'white', 90: 'black' }.</param>
    /// <returns></returns>
    public static T RandomProbability<T>(List<KeyValuePair<float, T>> probabilityTable)
    {
      float total = 0;

      foreach (var proba in probabilityTable)
      {
        total += proba.Key;
      }

      return MatchingProbability<T>(probabilityTable, Random.Range(0, total));
    }
  }
}