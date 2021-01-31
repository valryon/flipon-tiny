// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Pon
{
  /// <summary>
  /// Single-block behavior.
  /// </summary>
  public class BlockScript : MonoBehaviour
  {
    public static Vector2 defaultMovingSpeed = new Vector2(12f, 12.5f);

    public const int HIGHLIGHT_ORDER = 50;
    private const int SELECT_ORDER = 10;
    private const int DEFAULT_ORDER = 0;

    private const float REMOVING_DURATION = 1f;
    private const float DURATION_BETWEEN_BLOCKS = 0.15f;

    #region Members

    public Block block;
    public Vector2 movingSpeed = defaultMovingSpeed;
    public Vector2 shift;

    public event System.Action<BlockDefinition> OnEmpty;

    private SpriteRenderer spriteRenderer;
    private SpriteRenderer hideRenderer;
    private BoxCollider2D collider2d;

    private float darkness;
    private int dangerHeight;

    private float previousDarkness;

    #endregion

    #region Timeline

    void Awake()
    {
      spriteRenderer = gameObject.AddOrGetComponent<SpriteRenderer>();
      spriteRenderer.sortingLayerName = "Blocks";

      collider2d = gameObject.AddComponent<BoxCollider2D>();
      collider2d.isTrigger = true;
      collider2d.offset = new Vector2(0.5f, 0.5f);
      collider2d.size = Vector2.one;

      var hideGo = new GameObject("Top Sprite");
      hideRenderer = hideGo.AddComponent<SpriteRenderer>();
      hideRenderer.sprite = BlockDefinitionBank.Instance.unknowBlockSprite;
      hideRenderer.sortingLayerName = spriteRenderer.sortingLayerName;
      hideRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
      hideGo.transform.SetParent(transform, false);
      hideGo.gameObject.SetActive(false);

      var grid = FindObjectOfType<GridScript>();
      if (block != null) SetBlock(block, grid.TheGrid.height, !grid.settings.noScrolling);
    }

    void OnDestroy()
    {
      OnEmpty = null;
    }

    void Update()
    {
      UpdateBlock();

      UpdateDisplay();
    }

    void LateUpdate()
    {
      UpdateBlock();
    }

    private void UpdateBlock()
    {
      if (block == null) return;

      transform.localPosition = block.position + shift;
    }

    private void UpdateDisplay()
    {
      if (block == null) return;

      hideRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;

      // Inactive blocks are darker
      if (block.IsActive == false)
      {
        float darkPercent = Mathf.Clamp(transform.position.y / -1.25f, 0f, 1f);
        darkness = Mathf.Lerp(0.4f, 0.7f, darkPercent);
      }
      else
      {
        darkness = 0;
      }

      if (darkness != previousDarkness)
      {
        spriteRenderer.color = Color.Lerp(Color.white, Color.black, darkness);
      }

      previousDarkness = darkness;
    }

    #endregion

    #region Methods

    public void SetBlock(Block b, int height, bool scrollingOn)
    {
      block = b;
      block.movingSpeed = movingSpeed;

      DefinitionChanged(block.Definition);
      ActivationChanged(block.IsActive, false);
      UpdateSlicedSprite();

      block.OnActivationChanged += ActivationChanged;
      block.OnEmpty += Empty;
      block.OnEmptyWithAnimation += AnimationBeforeEmpty;
      block.OnConversionAnimation += AnimationBeforeConversion;
      block.OnNewDefinition += DefinitionChanged;
      block.OnNewNeighbor += UpdateSlicedSprite;

      dangerHeight = height;
    }

    public void SetMaterial()
    {
      spriteRenderer.material = block.Definition.material;
    }

    private void ActivationChanged(bool activated, bool animate)
    {
      if (block.IsEmpty == false)
      {
        if (activated)
        {
          darkness = 0f;
          if (animate == false)
          {
            spriteRenderer.color = Color.white;
          }
          else
          {
            const float DURATION = 0.2f;
            const float SCALE_BONUS = 0.1f;

            // Fade
            var startColor = spriteRenderer.color;
            StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve, 0f, 1f, DURATION,
              (step) => { spriteRenderer.color = Color.Lerp(startColor, Color.white, step); }, null));

            Vector3 previousScale = transform.localScale;
            spriteRenderer.sortingOrder += 10;

            // Grow
            StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve, 0f, 1f, DURATION,
              (step) =>
              {
                transform.localScale = Vector3.one * (1 + (SCALE_BONUS * step));

                Vector3 shiftToCenter = (transform.localScale - previousScale);
                shiftToCenter.x = -shiftToCenter.x / 2f;
                shiftToCenter.y = 0;

                shift = shiftToCenter;
              }, () =>
              {
                // Reverse grow
                StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve, 1f, 0f, DURATION,
                  (step) =>
                  {
                    transform.localScale = Vector3.one * (1 + (SCALE_BONUS * step));

                    Vector3 shiftToCenter = (transform.localScale - previousScale);
                    shiftToCenter.x = -shiftToCenter.x / 2f;
                    shiftToCenter.y = 0;

                    shift = shiftToCenter;
                  }, () =>
                  {
                    // Reset
                    shift = Vector2.zero;
                    spriteRenderer.sortingOrder -= 10;
                  }));
              }));
          }
        }
      }
    }

    private void DefinitionChanged(BlockDefinition d)
    {
      if (block.IsEmpty == false)
      {
        // Set sprite
        spriteRenderer.sprite = block.Definition.sprite;
        spriteRenderer.color = Color.white;
        spriteRenderer.sortingOrder = DEFAULT_ORDER;

        SetMaterial();

        name = d.name;
      }
      else
      {
        name = "empty";

        spriteRenderer.sprite = null;
      }
    }


    private void AnimationBeforeEmpty(int n, int count, System.Action callback)
    {
      StartCoroutine(EmptyAnimation(n, count, callback));
    }

    private IEnumerator EmptyAnimation(int n, int count, System.Action callback)
    {
      spriteRenderer.sortingOrder += 10 + n;
      int remaining = count - n;

      // Easy constants access
      //---------------------------------------------------

      // Timings
      const float BLINK_DURATION = 0.5f;
      const float GROW_DURATION = 0.15f;
      const float WAIT_BEFORE_EXPLOSION_DURATION = 0.2f;
      const float EXPLOSION_DURATION = 0.1f;
      const float EXPLOSION_DURATION_SHIFT = 0.1f;

      // Values
      const float GROW_SCALE_BONUS = 0.105f;

      //---------------------------------------------------

      if (IsHidden)
      {
        Reveal(0, 2.5f);
      }

      // Blink fade
      spriteRenderer.DOFade(0.5f, 0.1f).SetLoops(-1, LoopType.Yoyo);

      yield return new WaitForSeconds(BLINK_DURATION);

      spriteRenderer.DOKill();
      spriteRenderer.DOFade(0.75f, 0f);

      // Grow...
      var previousScale = transform.localScale;
      StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve, 0f, 1f, GROW_DURATION,
        (step) =>
        {
          transform.localScale = Vector3.one * (1 + (GROW_SCALE_BONUS * step));

          Vector3 shitToCenter = (transform.localScale - previousScale);
          shitToCenter.x = -shitToCenter.x / 2f;
          shitToCenter.y = 0;

          shift = shitToCenter;
        }, null));

      yield return new WaitForSeconds(GROW_DURATION + WAIT_BEFORE_EXPLOSION_DURATION);

      // Wait
      var duration = (n * EXPLOSION_DURATION_SHIFT);
      yield return new WaitForSeconds(duration);

      // Block disappear
      spriteRenderer.DOFade(0f, 0.15f).SetEase(Ease.InCubic)
        .SetDelay(0.1f);

      yield return new WaitForSeconds(EXPLOSION_DURATION);

      OnEmpty.Raise(block.Definition);

      // THIS synchronize falls between all blocks.
      yield return new WaitForSeconds(remaining * EXPLOSION_DURATION_SHIFT);

      // Reset
      shift = Vector2.zero;
      transform.localScale = Vector3.one;

      spriteRenderer.sortingOrder -= 10 + n;

      callback.Raise();
    }

    private void AnimationBeforeConversion(int n, int count, BlockDefinition def, System.Action callback)
    {
      StartCoroutine(ConversionAnimation(n, def, callback));
    }

    private IEnumerator ConversionAnimation(int n, BlockDefinition def, System.Action callback)
    {
      float duration = (REMOVING_DURATION + (n * DURATION_BETWEEN_BLOCKS));

      float s = 15f;
      float p = 0f;
      float d = 1f;

      while (duration > 0)
      {
        duration -= Time.deltaTime;

        // Blink
        p += d * s * Time.deltaTime;
        if (p > 1f || p < 0f) d = -d;

        spriteRenderer.color = Color.Lerp(Color.white, Color.white * 0.5f, p);

        yield return new WaitForEndOfFrame();
      }

      spriteRenderer.color = Color.white;

      if (def != null)
      {
        spriteRenderer.sprite = def.sprite;
      }
      else
      {
        spriteRenderer.sprite = null;
      }

      callback.Raise();
    }

    private void Empty()
    {
      spriteRenderer.sprite = null;
    }

    private void UpdateSlicedSprite()
    {
      if (block == null || block.IsEmpty) return;
      if (!(block.Definition is GarbageBlockDefinition garbage)) return;

      if (block.Left != null)
      {
        if (block.Right != null)
        {
          spriteRenderer.sprite = garbage.middleSprite;
        }
        else
        {
          spriteRenderer.sprite = garbage.rightSprite;
        }
      }
      else
      {
        if (block.Right != null)
        {
          spriteRenderer.sprite = garbage.leftSprite;
        }
        else
        {
          spriteRenderer.sprite = garbage.sprite;
        }
      }
    }

    public void Hide(float duration, float speed = 1)
    {
      // Fade to black for few sec
      hideRenderer.DOKill();
      if (IsHidden == false)
      {
        hideRenderer.color = Color.white.SetAlpha(0f);
        hideRenderer.gameObject.SetActive(true);
      }

      hideRenderer.DOFade(1f, 0.5f / speed).SetEase(Ease.OutCirc);

      if (duration > 0)
      {
        Reveal(duration);
      }
    }

    public void Reveal(float delay, float speed = 1)
    {
      hideRenderer.DOFade(0f, 0.5f / speed).SetEase(Ease.InCirc)
        .SetDelay(delay)
        .OnComplete(() => { hideRenderer.gameObject.SetActive(false); });
    }

    #endregion

    #region Selection

    private int selectionState;

    public void Select()
    {
      const float ANIM_DURATION = 0.15f;

      Vector3 scale = Vector3.one;
      Vector3 targetScale = Vector3.one * 1.3f;
      selectionState = 1;

      if (spriteRenderer.sortingOrder == DEFAULT_ORDER)
      {
        spriteRenderer.sortingOrder = SELECT_ORDER;
      }

      shift = Vector3.zero;
      Vector3 oldScale = transform.localScale;

      StartCoroutine(Interpolators.Curve(
        Interpolators.EaseOutCurve,
        0f, 1f,
        ANIM_DURATION,
        (step) =>
        {
          if (selectionState == 1)
          {
            Vector3 newScale = Vector3.Lerp(scale, targetScale, step);

            transform.localScale = newScale;

            Vector2 shiftToCenter = newScale - oldScale;
            shift = -(shiftToCenter / 2f);
          }
        },
        null));
    }

    public void Unselect()
    {
      const float ANIM_DURATION = 0.1f;

      Vector3 scale = transform.localScale;
      Vector3 targetScale = Vector3.one;
      selectionState = -1;


      StartCoroutine(Interpolators.Curve(
        Interpolators.EaseOutCurve,
        0f, 1f,
        ANIM_DURATION,
        (step) =>
        {
          if (selectionState == -1)
          {
            Vector3 newScale = Vector3.Lerp(scale, targetScale, step);
            transform.localScale = newScale;

            Vector3 shiftToCenter = newScale - targetScale;
            shift = -(shiftToCenter / 2f);
          }
        },
        () =>
        {
          if (selectionState == -1)
          {
            shift = Vector3.zero;
            spriteRenderer.sortingOrder = DEFAULT_ORDER;
          }
        }));
    }

    #endregion

    #region Properties

    public BoxCollider2D BoxCollider => collider2d;

    public bool IsHidden => hideRenderer.gameObject.activeInHierarchy;

    public int DangerHeight
    {
      get => dangerHeight;
      set => dangerHeight = value;
    }

    #endregion
  }
}