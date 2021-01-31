using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Pon
{
  public class GarbagesWidget : MonoBehaviour
  {
    public GarbageWidgetElement prefab;
    public RectTransform lastPosition;
    public LayoutGroup group;

    private PlayerScript player;
    private List<GarbageWidgetElement> blocks = new List<GarbageWidgetElement>();

    public void SetPlayer(PlayerScript p)
    {
      player = p;

      // Position widget to the right of the grid
      var magicNumber = 2f;
      if (PonGameScript.instance != null && PonGameScript.instance.Settings.players.Length > 1)
      {
        magicNumber = 1.5f;
      }

      transform.position = new Vector3((p.grid.settings.width + 1) / magicNumber,
        transform.position.y, transform.position.z);

      var lastPositionGo = new GameObject("Last");
      lastPosition = lastPositionGo.AddComponent<RectTransform>();
      lastPosition.SetParent(@group.transform, false);

      player.grid.OnGarbageStored += OnGarbageStored;
      player.grid.OnGarbageAdded += OnGarbageAdded;
    }

    private void OnDestroy()
    {
      if (player != null && player.grid != null)
      {
        player.grid.OnGarbageStored -= OnGarbageStored;
        player.grid.OnGarbageAdded -= OnGarbageAdded;
      }
    }

    private void OnGarbageStored(GridScript.GarbageStored obj)
    {
      RefreshBlockList();
    }

    private void OnGarbageAdded(GridScript.GarbageStored g)
    {
      var b = blocks.FirstOrDefault(el => el.GarbageStored.Equals(g));
      if (b == null) return;

      blocks.Remove(b);

      b.fill.fillAmount = 1f;

      var rt = ((RectTransform) b.transform);
      rt.DOKill();

      rt.SetParent(rt.parent.parent, true);
      var seq = DOTween.Sequence();

      //seq.Join(rt.DOAnchorPosX(-75, 0.15f).SetEase(Ease.OutCubic));
      seq.Join(rt.DOScale(0.33f, 0.25f).SetEase(Ease.InBack));
      seq.Append(rt.DOAnchorPosY(rt.anchoredPosition.y + 500, 0.35f)
        .SetEase(Ease.OutCubic));
      seq.OnComplete(() =>
      {
        if (b) Destroy(b.gameObject);
      });
    }

    private void Update()
    {
      if (player == null) return;
      if (player.player.GameOver)
      {
        Clear();
        return;
      }

      foreach (var v in blocks)
      {
        if (v == null) continue;

        // if (player.grid.IsRainingGarbages) v.fill.fillAmount = 1f;
        // else
        // {
        v.fill.fillAmount = 1f - player.grid.GarbageCooldownPercent;
        // }
      }
    }

    private void Clear()
    {
      foreach (var b in blocks)
      {
        b.transform.DOKill();
        Destroy(b.gameObject);
      }

      blocks.Clear();
    }

    private void RefreshBlockList()
    {
      Clear();
      foreach (var g in player.grid.Garbages)
      {
        if (g.width == 0) continue;

        var b = Instantiate(prefab, @group.transform, false);
        b.GarbageStored = g;
        blocks.Add(b);

        b.text.text = g.width.ToString();
        b.fill.fillAmount = 0f;

        b.transform.localScale = Vector3.zero;
        b.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack);
      }

      lastPosition.SetAsLastSibling();
    }
  }
}