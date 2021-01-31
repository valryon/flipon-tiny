// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using System;
using UnityEngine;

namespace Pon
{
  public enum PlayerType
  {
    Local,
    AI,
  }

  [System.Serializable]
  public class Player
  {
    [Header("Player")]
    public int index;

    public PlayerType type;
    public string name;
    public bool allowGridAngle;
    public PowerType power;
    public Sprite background;

    [HideInInspector]
    public Rect gridViewport = new Rect(0, 0, 1, 1);

    [HideInInspector]
    public float gridAngle = 0;

    [Header("AI")]
    public DifficultySettings difficultySettings;

    public long Score { get; set; }
    public bool GameOver { get; set; }

    public AISettings GetAISettings()
    {
      return difficultySettings.aiSettings;
    }

    public override string ToString()
    {
      return $"({index}){name} ({type})";
    }
  }
}