using UnityEngine;

namespace Pon
{
  [CreateAssetMenu(fileName = "Flipon/😈 Difficulty", menuName = "DifficultySettings", order = 0)]
  public class DifficultySettings : ScriptableObject
  {
    public string nameKey;
    public AISettings aiSettings;
  }
}