// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Pon
{
  [Serializable]
  public enum ObjectiveStatType
  {
    None,
    Score,
    Time,
    HighestMultiplier,
    TotalCombos,
    Total4Combos,
    Total5Combos,
    Level,
    TimeLimit,
    TotalLCombos,
    HighestChain,
    TotalChains,
    Height,
    NumBlock1Break,
    NumBlock2Break,
    NumBlock3Break,
    NumBlock4Break,
    NumBlock5Break,
    NumBlock6Break
  }

  [Serializable]
  public struct ObjectiveStats
  {
    public static ObjectiveStats @default = new ObjectiveStats();

    public long score;
    public float timeReached;
    public float timeMax;
    public int highestCombo;
    public int totalCombos;
    public int total4Combos;
    public int total5Combos;

    [FormerlySerializedAs("total6Combos")]
    public int totalLCombos;

    public int speedLevel;
    public int highestChain;
    public int totalChains;
    public int digHeight; // ⚠ Not working in the tiny project

    public int numBlock1Broken; // Blue 
    public int numBlock2Broken; // Green?
    public int numBlock3Broken; // Orange / Red
    public int numBlock4Broken; // Pink
    public int numBlock5Broken; // Purple?
    public int numBlock6Broken; // Yellow

    public ObjectiveStats(PlayerScript p1, float timeElapsed)
      : this(p1.player.Score, p1.grid.SpeedLevel, timeElapsed,
        p1.grid.ComboMultiplier, // Use the current multiplier, not the best
        p1.grid.TotalCombos, p1.grid.Total4Combos, p1.grid.Total5Combos, p1.grid.Total6Combos,
        p1.grid.TotalChains, p1.grid.Chains, p1.grid.HighestY, p1.grid.CurrentBlock1Broken,
        p1.grid.CurrentBlock2Broken, p1.grid.CurrentBlock3Broken, p1.grid.CurrentBlock4Broken,
        p1.grid.CurrentBlock5Broken, p1.grid.CurrentBlock6Broken)
    {
    }

    public ObjectiveStats(long currentScore, int level, float currentTime, int currentHighestCombo,
      int currentTotalCombo, int currentTotal4Combos, int currentTotal5Combos, int currentTotalLCombos,
      int currentTotalChain, int currentHighestChain, int highestY, int currentBlock1Broken, int currentBlock2Broken,
      int currentBlock3Broken, int currentBlock4Broken, int currentBlock5Broken, int currentBlock6Broken)
    {
      score = currentScore;
      speedLevel = level;
      timeReached = currentTime;
      timeMax = currentTime;
      highestCombo = currentHighestCombo;
      totalCombos = currentTotalCombo;
      total4Combos = currentTotal4Combos;
      total5Combos = currentTotal5Combos;
      totalLCombos = currentTotalLCombos;
      totalChains = currentTotalChain;
      highestChain = currentHighestChain;
      digHeight = highestY;
      numBlock1Broken = currentBlock1Broken;
      numBlock2Broken = currentBlock2Broken;
      numBlock3Broken = currentBlock3Broken;
      numBlock4Broken = currentBlock4Broken;
      numBlock5Broken = currentBlock5Broken;
      numBlock6Broken = currentBlock6Broken;
    }
  }

  [Serializable]
  public class Objective
  {
    public ObjectiveStats stats;

    private ObjectiveStats startStats;

    public bool Succeed(PlayerScript player, ObjectiveStats current)
    {
      bool match = true;
      if (stats.score > 0)
      {
        match &= (current.score - startStats.score) >= stats.score;
      }

      if (stats.speedLevel > 0)
      {
        match &= current.speedLevel >= stats.speedLevel;
      }

      if (stats.totalCombos > 0)
      {
        match &= (current.totalCombos - startStats.totalCombos) >= stats.totalCombos;
      }

      if (stats.total4Combos > 0)
      {
        match &= (current.total4Combos - startStats.total4Combos) >= stats.total4Combos;
      }

      if (stats.total5Combos > 0)
      {
        match &= (current.total5Combos - startStats.total5Combos) >= stats.total5Combos;
      }

      if (stats.totalLCombos > 0)
      {
        match &= (current.totalLCombos - startStats.totalLCombos) >= stats.totalLCombos;
      }

      if (stats.highestCombo > 0)
      {
        match &= current.highestCombo >= stats.highestCombo;
      }

      if (stats.timeReached > 0)
      {
        match &= (current.timeReached - startStats.timeReached) >= stats.timeReached;
      }

      if (stats.timeMax > 0)
      {
        match &= (current.timeMax - startStats.timeMax) < stats.timeMax;
      }

      if (stats.totalChains > 0)
      {
        match &= (current.totalChains - startStats.totalChains) >= stats.totalChains;
      }

      if (stats.highestChain > 0)
      {
        match &= (current.highestChain - startStats.highestChain) >= stats.highestChain;
      }

      if (stats.digHeight > 0)
      {
        int c = player.grid.HighestY;
        int t = player.grid.targetHeight;
        match &= c < t;
      }

      if (stats.numBlock1Broken > 0)
      {
        match &= (current.numBlock1Broken - startStats.numBlock1Broken) >= stats.numBlock1Broken;
      }

      if (stats.numBlock2Broken > 0)
      {
        match &= (current.numBlock2Broken - startStats.numBlock2Broken) >= stats.numBlock2Broken;
      }

      if (stats.numBlock3Broken > 0)
      {
        match &= (current.numBlock3Broken - startStats.numBlock3Broken) >= stats.numBlock3Broken;
      }

      if (stats.numBlock4Broken > 0)
      {
        match &= (current.numBlock4Broken - startStats.numBlock4Broken) >= stats.numBlock4Broken;
      }

      if (stats.numBlock5Broken > 0)
      {
        match &= (current.numBlock5Broken - startStats.numBlock5Broken) >= stats.numBlock5Broken;
      }

      if (stats.numBlock6Broken > 0)
      {
        match &= (current.numBlock6Broken - startStats.numBlock6Broken) >= stats.numBlock6Broken;
      }

      return match;
    }

    public ObjectiveStatType GetObjectiveType()
    {
      if (stats.score > 0) return (ObjectiveStatType.Score);
      if (stats.speedLevel > 0) return (ObjectiveStatType.Level);
      if (stats.totalCombos > 0) return (ObjectiveStatType.TotalCombos);
      if (stats.total4Combos > 0) return (ObjectiveStatType.Total4Combos);
      if (stats.total5Combos > 0) return (ObjectiveStatType.Total5Combos);
      if (stats.totalLCombos > 0) return (ObjectiveStatType.TotalLCombos);
      if (stats.highestCombo > 0) return (ObjectiveStatType.HighestMultiplier);
      if (stats.timeReached > 0) return (ObjectiveStatType.Time);
      if (stats.timeMax > 0) return (ObjectiveStatType.TimeLimit);
      if (stats.totalChains > 0) return (ObjectiveStatType.TotalChains);
      if (stats.highestChain > 0) return (ObjectiveStatType.HighestChain);
      if (stats.digHeight > 0) return (ObjectiveStatType.Height);
      if (stats.numBlock1Broken > 0) return (ObjectiveStatType.NumBlock1Break);
      if (stats.numBlock2Broken > 0) return (ObjectiveStatType.NumBlock2Break);
      if (stats.numBlock3Broken > 0) return (ObjectiveStatType.NumBlock3Break);
      if (stats.numBlock4Broken > 0) return (ObjectiveStatType.NumBlock4Break);
      if (stats.numBlock5Broken > 0) return (ObjectiveStatType.NumBlock5Break);
      if (stats.numBlock6Broken > 0) return (ObjectiveStatType.NumBlock6Break);

      return ObjectiveStatType.None;
    }

    /// <summary>
    /// Divide a multi objective into multiple single ones.
    /// </summary>
    /// <returns></returns>
    public List<ObjectiveStats> GetSubObjectives()
    {
      var types = new List<ObjectiveStats>();

      if (stats.score > 0) types.Add(new ObjectiveStats() {score = stats.score});
      if (stats.speedLevel > 0) types.Add(new ObjectiveStats() {speedLevel = stats.speedLevel});
      if (stats.totalCombos > 0) types.Add(new ObjectiveStats() {totalCombos = stats.totalCombos});
      if (stats.total4Combos > 0) types.Add(new ObjectiveStats() {total4Combos = stats.total4Combos});
      if (stats.total5Combos > 0) types.Add(new ObjectiveStats() {total5Combos = stats.total5Combos});
      if (stats.totalLCombos > 0) types.Add(new ObjectiveStats() {totalLCombos = stats.totalLCombos});
      if (stats.highestCombo > 0) types.Add(new ObjectiveStats() {highestCombo = stats.highestCombo});
      if (stats.timeReached > 0) types.Add(new ObjectiveStats() {timeReached = stats.timeReached});
      if (stats.timeMax > 0) types.Add(new ObjectiveStats() {timeMax = stats.timeMax});
      if (stats.totalChains > 0) types.Add(new ObjectiveStats() {totalChains = stats.totalChains});
      if (stats.highestChain > 0) types.Add(new ObjectiveStats() {highestChain = stats.highestChain});
      if (stats.digHeight > 0) types.Add(new ObjectiveStats() {digHeight = stats.digHeight});
      if (stats.numBlock1Broken > 0) types.Add(new ObjectiveStats() { numBlock1Broken = stats.numBlock1Broken });
      if (stats.numBlock2Broken > 0) types.Add(new ObjectiveStats() { numBlock2Broken = stats.numBlock2Broken });
      if (stats.numBlock3Broken > 0) types.Add(new ObjectiveStats() { numBlock3Broken = stats.numBlock3Broken });
      if (stats.numBlock4Broken > 0) types.Add(new ObjectiveStats() { numBlock4Broken = stats.numBlock4Broken });
      if (stats.numBlock5Broken > 0) types.Add(new ObjectiveStats() { numBlock5Broken = stats.numBlock5Broken });
      if (stats.numBlock6Broken > 0) types.Add(new ObjectiveStats() { numBlock6Broken = stats.numBlock6Broken });

      return types;
    }

    public Objective Clone()
    {
      return (Objective) MemberwiseClone();
    }

    public bool IsMultiObjectives => GetSubObjectives().Count > 1;

    public ObjectiveStats StartStats
    {
      get => startStats;
      set => startStats = value;
    }
  }
}