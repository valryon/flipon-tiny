using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Pon
{
  public class LevelWidget : MonoBehaviour
  {
    public TextMeshProUGUI levelText;

    private bool firstValue = true;
    private Vector3 baseScale;

    private void Awake()
    {
      baseScale = levelText.transform.localScale;
    }

    public void SetValue(string text)
    {
      gameObject.SetActive(true);

      var bump = string.IsNullOrEmpty(levelText.text) == false && levelText.text != text;

      levelText.text = text;

      if (bump && !firstValue)
      {
        levelText.transform.DOKill();
        levelText.transform.localScale = baseScale;
        levelText.transform.DOPunchScale(Vector3.one * 1.15f, 0.75f, 1).SetDelay(0.15f);

        var color = levelText.color;
        levelText.color = Color.Lerp(color, Color.white, 0.5f);
        levelText.DOColor(color, 0.5f);
      }

      firstValue = false;
    }
  }
}