// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Pon
{
  public class TimeWidget : MonoBehaviour
  {
    [Header("Bindings")]
    public TextMeshProUGUI timeText;

    private float previous;

    public void SetTime(float current, float max)
    {
      if (current >= 0)
      {
        current = Mathf.Max(0, current);

        var time = TimeSpan.FromSeconds(current);

        var text = $"{time.Minutes:D2}:{time.Seconds:D2}";

        timeText.text = text;

        const float MIN = 30;
        var realMax = max < MIN ? max : MIN;

        if (current < realMax)
        {
          float p = 1f - (current / realMax);
          p = Interpolators.EaseOutCurve.Evaluate(p);

          var warningColor = Color.Lerp(Color.white, new Color(255 / 255f, 25 / 255f, 25 / 255f), p);

          timeText.color = warningColor;

          if ((int) previous != (int) current)
          {
            timeText.transform.DOPunchScale(Vector3.one * (0.05f + (p * 0.225f)), 0.25f, 0, 0);
          }
        }

        previous = current;
      }
      else
      {
        timeText.text = string.Empty;
      }
    }
  }
}