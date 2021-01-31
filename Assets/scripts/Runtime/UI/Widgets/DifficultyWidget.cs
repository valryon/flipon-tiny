using TMPro;
using UnityEngine;

namespace Pon
{
  public class DifficultyWidget : MonoBehaviour
  {
    public TextMeshProUGUI levelText;

    public void SetValue(string text)
    {
      gameObject.SetActive(true);

      levelText.text = text;
    }
  }
}