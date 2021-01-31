// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using UnityEngine;
using TMPro;

namespace Pon
{
  public enum ComboWidgetMode
  {
    None,
    Score,
    Multiplier
  }

  public class ComboTextWidget : MonoBehaviour
  {
    public TextMeshProUGUI text;
    public TextMeshProUGUI text2;
    public ComboWidgetMode mode;

    public void SetScore(long score, Color color)
    {
      mode = ComboWidgetMode.Score;
      text.text = "+" + score.ToString("N0");
      text.color = color;
      if (text2 != null) text2.color = color;
    }

    public void SetMultiplier(int multiplier, Color color)
    {
      mode = ComboWidgetMode.Multiplier;
      text.text = "x" + multiplier;
      text.color = color;
      if (text2 != null) text2.color = color;
    }
  }
}