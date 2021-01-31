// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using DG.Tweening;
using UnityEngine;

namespace Pon
{
  /// <summary>
  /// Cursor script.
  /// </summary>
  public class CursorScript : MonoBehaviour
  {
    #region Members

    [Header("Cursor")]
    public InputType inputType;

    [Header("Move data")]
    public int movesCount;

    public bool stickToTarget = true;

    [HideInInspector]
    public GridScript grid;

    protected bool showCursor;

    #endregion

    #region Timeline

    protected virtual void OnEnable()
    {
      transform.localScale = Vector3.zero;
    }

    private void OnDestroy()
    {
      transform.DOKill();
    }

    public virtual void Update()
    {
      UpdatePosition();
    }

    void LateUpdate()
    {
      UpdatePosition();
    }

    private void UpdatePosition()
    {
      if (Target != null)
      {
        var scale = Target.transform.localScale;
        if (stickToTarget)
        {
          transform.position = Target.transform.position;
        }
        else
        {
          transform.position = grid.transform.position +
                               new Vector3(Target.block.x * grid.transform.localScale.x,
                                 (Target.block.y + grid.TheGrid.ScrollSinceLastDing) * grid.transform.localScale.y);
          scale = Vector3.one;
        }

        transform.localPosition += new Vector3(0.5f * scale.x, 0.5f * scale.y); // Center
        transform.localScale = scale;
      }
    }

    #endregion

    #region Public methods

    public virtual void SetActive(bool activation, bool animate = true)
    {
      const float SHOW_DURATION = 0.075f;
      const float HIDE_DURATION = 0.05f;

      // Animate
      if (showCursor != activation)
      {
        if (activation)
        {
          showCursor = true;
          gameObject.SetActive(true);

          transform.DOScale(Vector3.one, animate ? SHOW_DURATION : 0f).SetEase(Ease.OutCubic);
        }
        else
        {
          showCursor = false;
          transform.DOScale(Vector3.zero, animate ? HIDE_DURATION : 0f).SetEase(Ease.InCubic)
            .OnComplete(() =>
            {
              if (gameObject) gameObject.SetActive(false);
            });
          if (animate == false) gameObject.SetActive(false);
        }
      }
    }

    public void RegisterMove()
    {
      movesCount++;

      if (movesCount > 1)
      {
        Target.block.movingSpeed =
          new Vector2(BlockScript.defaultMovingSpeed.x * 1.5f, BlockScript.defaultMovingSpeed.y);
      }
    }

    public void Clean()
    {
      movesCount = 0;

      if (Target != null)
      {
        Target.block.movingSpeed = BlockScript.defaultMovingSpeed;
      }

      Target = null;
    }

    #endregion

    #region Properties

    protected BlockScript target;

    public virtual BlockScript Target
    {
      get => target;
      set
      {
        if (target != value)
        {
          // Auto select/unselect
          if (target != null)
          {
            target.Unselect();
            target = null;
          }

          target = value;

          if (target != null)
          {
            target.Select();
          }
        }
      }
    }

    #endregion
  }
}