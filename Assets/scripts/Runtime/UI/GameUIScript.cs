// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using DG.Tweening;
using Random = UnityEngine.Random;

namespace Pon
{
  public class GameUIScript : MonoBehaviour
  {
    private static GameUIScript Instance;

    #region Members

    [Header("Prefabs: panels")]
    public DynamicVersusUIScript activeUI;

    [Header("Prefabs: widgets")]
    public ComboTextWidget scorePrefab;

    public ComboTextWidget chainPrefab;

    #endregion

    #region Initialization

    public static void Init(GameSettings settings)
    {
      if (Instance == null)
      {
        Log.Error("Missing GameUIScript instance...");
        return;
      }

      Instance.activeUI.gameObject.SetActive(true);
    }

    #endregion


    #region Timeline

    void Awake()
    {
      Instance = this;
    }

    #endregion

    #region Public Methods

    public static void SetPlayersCount(int totalPlayersCount, int aiPlayerCount)
    {
      if (Instance != null)
      {
        Instance.activeUI.SetPlayersCount(totalPlayersCount, aiPlayerCount);
      }
    }

    public static void SetScore(int player, long points)
    {
      Instance.activeUI.SetScore(player, points.ToString("N0"));
    }

    public static void SetSpeed(int player, int l)
    {
      Instance.activeUI.SetLevel(player, l.ToString());
    }

    public static void SetPowerCharge(int player, float charge, int direction)
    {
      if (Instance != null)
      {
        Instance.activeUI.SetPowerCharge(player, Mathf.Clamp(charge, 0f, 1f), direction);
      }
    }

    public static void SetTime(float t, float max = -1)
    {
      Instance.activeUI.SetTimer(t, max);
    }

    public static void SetAlpha(float alpha, bool anim)
    {
      Instance.activeUI.SetAlpha(alpha, anim);
    }

    /// <summary>
    /// Create an objective in one of the 3 slots
    /// </summary>
    public static void SetObjective(int player, int index, Objective obj)
    {
      if (Instance != null)
      {
        if (obj != null && obj.IsMultiObjectives)
        {
          // Split!
          var data = obj.GetSubObjectives();
          var subObjectives = data.Select(d => new Objective() {stats = d, StartStats = obj.StartStats}).ToList();

          int i = 1;
          foreach (var o in subObjectives)
          {
            if (o.GetObjectiveType() != ObjectiveStatType.TimeLimit)
            {
              Instance.activeUI.SetObjective(player, i, o);
              i++;
            }
          }
        }
        else
        {
          Instance.activeUI.SetObjective(player, index, obj);
        }
      }
    }

    /// <summary>
    /// Update an objective 
    /// </summary>
    public static void UpdateObjective(PlayerScript player, int index, ObjectiveStats stats)
    {
      if (Instance != null)
      {
        Instance.activeUI.UpdateObjective(player, index, stats);
      }
    }

    public static Canvas SetPlayer(PlayerScript p, int playersCount)
    {
      if (Instance != null)
      {
        return Instance.activeUI.SetPlayer(p, playersCount);
      }

      return null;
    }

    public static GenericUIScript GetUI()
    {
      if (Instance != null)
      {
        return Instance.activeUI;
      }

      return null;
    }

    public static ObjectiveWidget[] GetWidgets(int player)
    {
      if (Instance != null)
      {
        return new[]
        {
          Instance.activeUI.GetWidget(player, 1),
          Instance.activeUI.GetWidget(player, 2),
          Instance.activeUI.GetWidget(player, 3),
          Instance.activeUI.GetWidget(player, 4)
        };
      }

      return new ObjectiveWidget[0];
    }

    public static void DisplayComboWidget(GridScript grid, int combo, int blocksCount, long score, BlockDefinition def,
      Vector2 loc, Vector2[] positions)
    {
      if (Instance == null) return;

      var go = Instantiate(Instance.scorePrefab.gameObject, grid.ui.transform, true);
      go.transform.position = loc + RandomEx.GetVector2(-0.5f, 0.5f, -0.25f, 0.25f);
      go.transform.localScale = Vector3.one;

      var comboWidget = go.AddOrGetComponent<ComboTextWidget>();

      comboWidget.SetScore(score, def.color);

      ObjectiveWidget targetWidget = null;
      var widgets = GetWidgets(grid.player.index);
      widgets = widgets.Where(w => w != null).ToArray();

      // Combo
      foreach (var w in widgets)
      {
        if (w.Objective != null && w.Objective.GetObjectiveType() == ObjectiveStatType.TotalCombos)
        {
          AnimateObjectiveUpdate(grid, w, loc);
        }
      }

      // Score
      foreach (var w in widgets)
      {
        if (w.Objective != null && w.Objective.GetObjectiveType() == ObjectiveStatType.Score)
        {
          targetWidget = w;
        }
      }

      // 4 combo
      if (blocksCount == 4)
      {
        foreach (var w in widgets)
        {
          if (w.Objective != null && w.Objective.GetObjectiveType() == ObjectiveStatType.Total4Combos)
          {
            AnimateObjectiveUpdate(grid, w, loc);
          }
        }
      }
      // 5 combo
      else if (blocksCount == 5)
      {
        foreach (var w in widgets)
        {
          if (w.Objective != null && w.Objective.GetObjectiveType() == ObjectiveStatType.Total5Combos)
          {
            AnimateObjectiveUpdate(grid, w, loc);
          }
        }
      }
      // L combos
      else if (blocksCount > 5)
      {
        foreach (var w in widgets)
        {
          if (w.Objective != null && w.Objective.GetObjectiveType() == ObjectiveStatType.TotalLCombos)
          {
            AnimateObjectiveUpdate(grid, w, loc);
          }
        }
      }


      if (comboWidget != null)
      {
        AnimateComboScoreWidget(comboWidget, targetWidget, combo);
      }
    }

    public static void DisplayChainWidget(GridScript grid, int chainCount, long score, Color color, Vector3 position)
    {
      if (Instance == null) return;
      var prefab = Instance.chainPrefab;

      if (prefab == null)
      {
        Log.Error("Missing Chain widget prefab");
        return;
      }

      var go = Instantiate(prefab.gameObject, grid.ui.transform, true);
      go.transform.position = position + RandomEx.GetVector3(-0.5f, 0.5f, -0.25f, 0.25f, 0, 0);
      go.transform.localScale = Vector3.one;

      var comboWidget = go.AddOrGetComponent<ComboTextWidget>();
      comboWidget.SetMultiplier(chainCount, color);

      ObjectiveWidget targetWidget = null;
      var widgets = GetWidgets(grid.player.index);
      widgets = widgets.Where(w => w != null).ToArray();

      // Chains
      foreach (var w in widgets)
      {
        if (w.Objective != null && (w.Objective.GetObjectiveType() == ObjectiveStatType.HighestChain
                                    || w.Objective.GetObjectiveType() == ObjectiveStatType.TotalChains))
        {
          AnimateObjectiveUpdate(grid, w, position);
          targetWidget = w;
        }
      }

      AnimateComboScoreWidget(comboWidget, targetWidget, chainCount);
    }

    public static GameObject CreateUIEffect(GameObject fxPrefab, Vector3 position)
    {
      var fx = Instantiate(fxPrefab, position, Quaternion.identity);
      fx.gameObject.layer = LayerMask.NameToLayer("Player UI");
      fx.GetComponentInChildren<Renderer>().sortingLayerName = "UI";
      fx.GetComponentInChildren<Renderer>().sortingOrder = -5;

      return fx;
    }

    private static void AnimateComboScoreWidget(ComboTextWidget widget, ObjectiveWidget targetWidget, int combo)
    {
      var seq = DOTween.Sequence();

      // Move slightly up
      seq.Append(widget.transform.DOMoveY(widget.transform.position.y + 0.3f, 0.75f)
        .SetEase(targetWidget != null ? Ease.InQuart : Ease.InOutQuart));

      // Grow & bounce animation
      widget.transform.localScale = Vector3.zero;
      seq.Join(widget.transform.DOScale(1f, 0.42f).SetEase(Ease.OutBack));

      // Fade
      if (targetWidget == null)
      {
        // Keep displayed a bit first
        seq.AppendInterval(0.21f);

        seq.Append(widget.text.DOFade(0f, 0.65f).SetEase(Ease.OutQuart));
        if (widget.text2 != null) seq.Join(widget.text2.DOFade(0f, 0.65f).SetEase(Ease.OutQuart));
        seq.OnComplete(() => { Destroy(widget.gameObject); });
      }
      else
      {
        // Go to widget 
        var destination = targetWidget.Text.transform.position;

        seq.Append(widget.transform.DOMove(destination, 0.63f).SetEase(Ease.InCubic));


        var seq2 = DOTween.Sequence();
        seq2.Append(widget.text.DOColor(Color.white, 0.225f));
        if (widget.text2 != null) seq2.Join(widget.text2.DOColor(Color.white, 0.225f));
        seq2.Join(widget.transform.DOScale(Vector3.one * 0.5f, 0.225f));

        seq.Join(seq2);

        seq.AppendCallback(() =>
        {
          widget.transform.DOScale(Vector3.zero, 0.15f)
            .OnComplete(() => { Destroy(widget.gameObject); });
          targetWidget.Highlight();
        });
      }
    }

    public static Image CreateObjectiveIcon(GridScript grid, ObjectiveWidget obj, Vector3 loc)
    {
      if (obj == null) return null;

      var go = new GameObject("Star");
      go.transform.parent = grid.ui.transform;
      go.transform.position = loc;
      go.transform.localScale = Vector3.one;

      var image = go.AddComponent<Image>();
      image.sprite = obj.Icon.sprite;

      return image;
    }

    private static void AnimateObjectiveUpdate(GridScript grid, ObjectiveWidget obj, Vector3 loc)
    {
      if (obj.IsCompleted) return;

      var image = CreateObjectiveIcon(grid, obj, loc);

      image.transform.localScale = Vector3.zero;

      var seq = DOTween.Sequence();

      // Move slightly up
      seq.Append(image.transform.DOMove(
          new Vector3(image.transform.position.x + Random.Range(-1f, 1f),
            image.transform.position.y + Random.Range(-1f, 1f)), 0.35f)
        .SetEase(Ease.OutQuart));
      seq.Join(image.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack));

      seq.AppendInterval(0.25f);

      // Create a random path to the icon
      var destination = obj.Icon.transform.position;
      List<Vector3> path = new List<Vector3>();
      int count = Random.Range(1, 3);
      for (int i = 1; i < count; i++)
      {
        var point = Vector3.Lerp(loc, destination, i / (float) count);
        point += RandomEx.GetVector3(-0.15f, 0.15f, -0.15f, 0.15f, 0, 0);
        path.Add(point);
      }

      path.Add(destination);

      seq.Append(image.transform.DOPath(path.ToArray(), 0.85f, PathType.CatmullRom).SetEase(Ease.InCubic));
      seq.AppendCallback(obj.Highlight);
      seq.Append(image.transform.DOScale(Vector3.zero, 0.15f));
      seq.AppendCallback(() => { Destroy(image.gameObject); });
    }

    #endregion

    #region Properties

    public static SpeedWidget GetSpeedButton(int player)
    {
      return Instance.activeUI.GetSpeedUpButton(player);
    }

    public static PowerWidget GetPowerBar(int player)
    {
      return Instance.activeUI.GetPowerBar(player);
    }

    public static void SetMultiplier(int player, int multiplier)
    {
      Instance.activeUI.SetMultiplier(player, multiplier);
    }

    #endregion
  }
}