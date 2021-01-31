// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Pon
{
  public class PowerWidget : MonoBehaviour
  {
    [Header("Bindings")]
    public Button button;

    public Image filler;
    public Image completed;
    public Image icon;

    private float previousCharge;
    private float currentCharge;

    private PlayerScript player;

    private void Awake()
    {
      filler.fillAmount = 0;

      button.onClick.AddListener(PowerButtonUsed);
    }

    private void OnDestroy()
    {
      transform.DOKill();
    }

    public void SetPlayer(PlayerScript p)
    {
      player = p;
      icon.sprite = PowersData.Instance.GetIcon(p.player.power);
    }

    private void PowerButtonUsed()
    {
      if (player.player.type != PlayerType.AI)
      {
        player.grid.UsePower();
      }
    }

    public void SetCharge(float charge, int direction)
    {
      previousCharge = currentCharge;
      currentCharge = charge;

      if (gameObject.activeInHierarchy == false) return;

      if (charge > 0 || direction < 0)
      {
        if (direction < 0)
        {
          filler.fillAmount = 1;
        }

        filler.DOKill();
        filler.DOFillAmount(charge, 0.75f).SetEase(Ease.OutQuart);
      }

      // Full?
      var wasCharged = previousCharge >= 1f;
      var fullyCharged = currentCharge >= 1f;
      if (fullyCharged)
      {
        filler.fillAmount = 0;
        filler.DOKill();
        filler.gameObject.SetActive(false);
        completed.gameObject.SetActive(true);

        if (wasCharged == false)
        {
          transform.DOScale(1.33f, 1f)
            .SetLoops(-1, LoopType.Yoyo);
        }
      }
      else
      {
        filler.gameObject.SetActive(true);
        completed.gameObject.SetActive(false);
        transform.DOKill();
        transform.DOScale(1f, 0f);

        const float MIN = 0.1f;
        const float MAX = 1f;
        filler.DOKill();
        filler.DOFillAmount(Mathf.Lerp(MIN, MAX, currentCharge), 0.25f).SetEase(Ease.OutCubic);

        transform.DOPunchScale(Vector3.one * 0.42f * currentCharge, 0.42f, 0);
      }
    }
  }
}