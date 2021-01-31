// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pon
{
  public class SpeedWidget : MonoBehaviour
  {
    [Header("Bindings")]
    public Button button;

    public Image frozen;
    public TextMeshProUGUI multiplier;

    private PlayerScript player;
    private bool isUsed;

    private int previousMultiplier;

    private void Awake()
    {
      SetMultiplier(0);
    }

    public void SetPlayer(PlayerScript p)
    {
      player = p;
    }

    private void Update()
    {
      frozen.fillAmount = 0;
      if (player != null && player.grid.IsStarted && player.grid.IsGameOver == false)
      {
        frozen.fillAmount = player.grid.FrozenCooldownPercent;
      }
    }

    public void SetMultiplier(int m)
    {
      if (multiplier == null) return;
      if (m <= 1)
      {
        multiplier.text = $"x{Mathf.Max(1, m)}";
        multiplier.transform.DOKill();
        multiplier.transform.DOScale(0, m == 0 && previousMultiplier <= 1 ? 0 : 0.25f).SetEase(Ease.InBack)
          .OnComplete(() => { multiplier.text = string.Empty; });
      }
      else
      {
        multiplier.text = $"x{m}";
        multiplier.transform.DOKill();

        if (m == 2)
        {
          multiplier.transform.DOScale(1, 0.25f).SetEase(Ease.OutBack);
        }
        else
        {
          multiplier.transform.localScale = Vector3.one;
          multiplier.transform.DOPunchScale(Vector3.one * 1.1f, 0.25f, 0, 0);
        }
      }

      previousMultiplier = m;
    }

    public void SetSpeedActivated(bool activated)
    {
      if (activated == isUsed) return;
      isUsed = activated;

      transform.DOKill();
      if (activated)
      {
        transform.DOScale(0.75f, 0.2f).SetEase(Ease.OutCubic);
      }
      else
      {
        transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack);
      }
    }
  }
}