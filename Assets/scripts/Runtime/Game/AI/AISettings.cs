using System;
using UnityEngine;

namespace Pon
{
  [Serializable]
  public class AISettings : ICloneable
  {
    [Header("AI Settings")]
    [Range(1, 5)]
    public int maxDepth = 2;

    [Min(0f)]
    public float initialCooldown = 2f;

    [Range(0f, 1f)]
    public float pickNotTheBestProbability = 0.25f;

    /// <summary>
    /// Time between two moves in a serie leading to a combo
    /// </summary>
    [Min(0.05f)]
    public float timeBetweenMoves = 0.5f;

    /// <summary>
    /// Time between two combos
    /// </summary>
    [Min(0f)]
    public float timeThinkMin = 0.75f, timeThinkMax = 1.5f;

    [Range(0f, 1f)]
    public float speedUpWhenNoComboProbability = 0.75f;

    [Range(0, 10)]
    public int minLinesBeforeSpeed = 3;

    [Range(0, 10)]
    public int maxDoingNothingMoments = 3;

    public int[] movesLimits = {30, 10, 5, 5, 2};

    public WeightValues weights = new WeightValues();

    public object Clone()
    {
      return MemberwiseClone();
    }
  }
}