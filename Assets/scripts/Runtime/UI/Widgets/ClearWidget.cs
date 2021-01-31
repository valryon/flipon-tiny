using DG.Tweening;
using Pon;
using UnityEngine;

namespace Runtime.UI.Game.Widgets
{
  public class ClearWidget : MonoBehaviour
  {
    [Header("Bindings")]
    public SpriteRenderer line;

    public Canvas canvas;
    public GameObject clear;
    public GameObject arrow;

    private GridScript grid;
    private Vector3 basePosition, baseLocalPosition;

    private void Awake()
    {
      grid = GetComponentInParent<GridScript>();
      baseLocalPosition = canvas.transform.localPosition;
      basePosition = canvas.transform.position;

      arrow.transform.DOLocalMoveY(arrow.transform.localPosition.y - 0.07f, 0.35f)
        .SetEase(Ease.OutCubic)
        .SetLoops(-1, LoopType.Yoyo);
    }

    private void OnDestroy()
    {
      arrow.transform.DOKill();
    }

    private void Update()
    {
      if (transform.localPosition.y < -0.5f)
      {
        arrow.SetActive(true);
        var t = canvas.transform;
        t.position = new Vector3(basePosition.x, 0.8f, basePosition.z);
        t.localPosition = new Vector3(baseLocalPosition.x, t.localPosition.y, t.localPosition.z);
        ;
      }
      else
      {
        arrow.SetActive(false);
        canvas.transform.localPosition = baseLocalPosition;
      }
    }
  }
}