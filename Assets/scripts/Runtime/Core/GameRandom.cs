// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using UnityEngine;
using System.Collections.Generic;

namespace Pon
{
  /// <summary>
  /// Random for gameplay, for a predictable seeded sequence.
  /// Careful with the calls, make sure they NEVER rely on a player action.
  /// </summary>
  public class GameRandom
  {
    /// <summary>
    /// Debug to see where random calls are done
    /// </summary>
    private bool debug = false;

    /// <summary>
    /// The random.
    /// </summary>
    private System.Random random;

    /// <summary>
    /// The seed.
    /// </summary>
    private int seed;

    public GameRandom(int startSeed)
    {
      // Default initialization
      random = new System.Random();
      Seed = startSeed;
    }

    /// <summary>
    /// Gets or sets the seed.
    /// This also reset the random, even if it's the same seed.
    /// </summary>
    public int Seed
    {
      get => seed;
      set
      {
        seed = value;
        random = new System.Random(seed);

        if (debug) Log.Debug("RANDOM seed = " + seed);
      }
    }

    /// <summary>
    /// Random number between min and max
    /// </summary>
    public int Range(int min, int max)
    {
      int v = random.Next(min, max);

      if (debug) Log.Debug("RANDOM Range(" + min + "," + max + ")=" + v);

      return v;
    }

    /// <summary>
    /// Random float number between min and max
    /// </summary>
    public float Range(float min, float max)
    {
      return Range(random, min, max);
    }

    /// <summary>
    /// Random float number between min and max
    /// </summary>
    public float Range(System.Random r, float min, float max)
    {
      float v = (float) r.NextDouble() * (max - min) + min;

      if (debug) Log.Debug("RANDOM Range(" + min + "," + max + ")=" + v);

      return v;
    }

    /// <summary>
    /// Random Vector2
    /// </summary>
    public Vector2 GetVector2(float xMin, float xMax, float yMin, float yMax)
    {
      return new Vector2(
        Range(xMin, xMax),
        Range(yMin, yMax)
      );
    }

    /// <summary>
    /// Random pick in the list using given probabilities
    /// </summary>
    public T RandomProbability<T>(List<KeyValuePair<float, T>> probabilityTable)
    {
      return RandomProbability<T>(random, probabilityTable);
    }

    /// <summary>
    /// Random pick in the list using given probabilities
    /// </summary>
    /// <returns>The probability.</returns>
    /// <param name="probabilityTable">Probability table.</param>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    public T RandomProbability<T>(System.Random r, List<KeyValuePair<float, T>> probabilityTable)
    {
      float total = 0;

      foreach (var proba in probabilityTable)
      {
        total += proba.Key;
      }

      return RandomEx.MatchingProbability<T>(probabilityTable, Range(r, 0f, total));
    }
  }
}