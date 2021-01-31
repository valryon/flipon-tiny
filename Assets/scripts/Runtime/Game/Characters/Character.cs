using UnityEngine;

namespace Pon
{
  public abstract class Character : ScriptableObject
  {
    [Header("Settings")]
    public string characterName;

    [Header("Sprites")]
    public Sprite icon;

    [Header("Versus")]
    public bool playableInVersus = true;

    public Background background;
    public DifficultySettings difficultySettings;
  }
}