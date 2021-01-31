// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using System;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Pon
{
  public class ObjectiveWidget : MonoBehaviour
  {
#pragma warning disable 0649

    [Header("Bindings")]
    [SerializeField]
    private GameObject panelIcon;

    [SerializeField]
    private TextMeshProUGUI textIcon;

    [FormerlySerializedAs("icon")]
    [SerializeField]
    private Image objectiveIcon;

    [SerializeField]
    private Image tickIcon;

    [SerializeField]
    private GameObject panelNoIcon;

    [SerializeField]
    private TextMeshProUGUI textNoIcon;

    [SerializeField]
    private Image tickNoIcon;

#pragma warning restore 0649

    private TextMeshProUGUI text;
    private Image tick;

    private PlayerScript player;
    private Objective objective;
    private ObjectiveStats lastStats;
    private bool completed;

    private string nextValue;

    void Awake()
    {
      panelIcon.SetActive(false);
      panelNoIcon.SetActive(false);
      gameObject.SetActive(false);
    }

    private void Start()
    {
      player = GetComponentInParent<PlayerScript>();
    }

    #region Objectives

    /// <summary>
    /// Display objective and progression
    /// </summary>
    public void SetObjective(Objective obj)
    {
      bool active = (obj != null);
      gameObject.SetActive(active);

      completed = false;
      objective = obj;
      if (active)
      {
        if (obj.IsMultiObjectives)
        {
          Log.Error("Widget cannot display a multiple objective... divide it first!");
          return;
        }

        UpdateDisplay(objective.GetObjectiveType(), objective.stats, new ObjectiveStats(), objective.StartStats, true);
      }
    }

    public void UpdateObjective(PlayerScript player, ObjectiveStats current)
    {
      if (completed) return;
      if (objective == null) return;

      UpdateDisplay(objective.GetObjectiveType(), objective.stats, current, objective.StartStats, IsImmediateUpdate);

      if (objective.Succeed(player, current))
      {
        CompleteObjective(true);
      }
    }

    public void CompleteObjective(bool success)
    {
      if (gameObject.activeInHierarchy)
      {
        completed = true;

        if (success)
        {
          var seq = DOTween.Sequence();
          seq.AppendInterval(1f);
          seq.Append(text.DOFade(0f, 0.5f).SetEase(Ease.OutQuart));

          seq.AppendCallback(() =>
          {
            tick.gameObject.SetActive(true);
            tick.color = tick.color.SetAlpha(0f);
            tick.DOFade(1f, 0.5f).SetEase(Ease.OutQuart);
          });
        }
        else
        {
          text.color = Color.red;

          var seq = DOTween.Sequence();
          seq.AppendInterval(1f);
          seq.Append(text.DOFade(0f, 0.5f).SetEase(Ease.OutQuart));
          seq.AppendInterval(2f);
          seq.AppendCallback(() =>
          {
            panelNoIcon.SetActive(false);
            panelIcon.SetActive(false);
          });
        }
      }
    }

    #endregion

    #region Display

    private void UpdateDisplay(ObjectiveStatType type, ObjectiveStats target, ObjectiveStats current,
      ObjectiveStats start, bool immediate)
    {
      gameObject.SetActive(true);

      // Build text
      string t = string.Empty;
      Sprite s = null;
      if (type == ObjectiveStatType.HighestMultiplier)
      {
        t = "x" + target.highestCombo;

        // Disable icon for now
        // t = (target.highestCombo - current.highestCombo).ToString();
        // s = ObjectiveData.GetIcon("combo_best");

        // if (current.highestCombo == 0 && lastStats.highestCombo > 0)
        // {
        //   Reset();
        //   immediate = true;
        //
        //   // Create a bunch of ejected stars
        //   for (int i = 0; i < lastStats.highestCombo; i++)
        //   {
        //     var image = GameUIScript.CreateObjectiveIcon(player.grid, this, transform.position);
        //     var p = transform.position + RandomEx.GetVector3(-4, 4f, 0f, 3f, 0, 0);
        //     image.transform.DOMove(p, 0.35f)
        //       .SetEase(Ease.OutCubic).OnComplete(() =>
        //       {
        //         image.transform.DOScale(Vector3.zero, 0.15f)
        //           .OnComplete(() => { Destroy(image.gameObject); });
        //       });
        //   }
        // }
      }

      if (type == ObjectiveStatType.Score)
      {
        t = "<b>" + Mathf.Max(0, target.score - (current.score - start.score)) + "</b> PTS";
        s = ObjectiveData.GetIcon("score");
      }

      if (type == ObjectiveStatType.TotalCombos)
      {
        t = Mathf.Max(0, target.totalCombos - (current.totalCombos - start.totalCombos)).ToString();
        s = ObjectiveData.GetIcon("combo_total");
      }

      if (type == ObjectiveStatType.Total4Combos)
      {
        t = Mathf.Max(0, target.total4Combos - (current.total4Combos - start.total4Combos)).ToString();
        s = ObjectiveData.GetIcon("combo_4");
      }

      if (type == ObjectiveStatType.Total5Combos)
      {
        t = Mathf.Max(0, target.total5Combos - (current.total5Combos - start.total5Combos)).ToString();
        s = ObjectiveData.GetIcon("combo_5");
      }

      if (type == ObjectiveStatType.TotalLCombos)
      {
        t = Mathf.Max(0, target.totalLCombos - (current.totalLCombos - start.totalLCombos)).ToString();
        s = ObjectiveData.GetIcon("combo_L");
      }

      if (type == ObjectiveStatType.Time)
      {
        t = target.timeReached + "'";
        // s = ObjectiveData.GetIcon("time"); -> null
      }

      if (type == ObjectiveStatType.TimeLimit)
      {
        t = (int) target.timeMax + "'";
        // s = ObjectiveData.GetIcon("time"); -> null
      }

      if (type == ObjectiveStatType.Level)
      {
        t = target.speedLevel.ToString();
        s = ObjectiveData.GetIcon("level");
      }

      if (type == ObjectiveStatType.TotalChains)
      {
        t = Mathf.Max(0, target.totalChains - (current.totalChains - start.totalChains)).ToString();
        s = ObjectiveData.GetIcon("chain");
      }

      if (type == ObjectiveStatType.HighestChain)
      {
        throw new NotImplementedException("Highest chains not used yet");
      }

      if (type == ObjectiveStatType.Height)
      {
        s = ObjectiveData.GetIcon("height");

        if (player == null)
        {
          player = FindObjectOfType<PlayerScript>();
        }

        if (player != null)
        {
          int ch = player.grid.HighestY;
          int th = player.grid.targetHeight;

          t = (Mathf.Abs(th - ch) + 1).ToString();
        }
        else
        {
          t = target.digHeight.ToString();
        }
      }

      if (completed == false)
      {
        if (immediate)
        {
          if (s != null)
          {
            SetWithIcon(s, t);
          }
          else
          {
            SetWithoutIcon(t);
          }
        }
        else
        {
          nextValue = t;
        }
      }

      lastStats = current;
    }

    private void SetWithIcon(Sprite s, string t)
    {
      if (panelIcon.activeInHierarchy == false)
      {
        panelIcon.SetActive(true);
        panelNoIcon.SetActive(false);
        text = textIcon;
        text.color = text.color.SetAlpha(1f);
        tick = tickIcon;
        objectiveIcon.color = objectiveIcon.color.SetAlpha(1f);

        text.gameObject.SetActive(true);
        objectiveIcon.gameObject.SetActive(true);
        tick.gameObject.SetActive(false);
      }

      text.text = t;
      objectiveIcon.sprite = s;
    }

    private void SetWithoutIcon(string t)
    {
      if (panelNoIcon.activeInHierarchy == false)
      {
        panelIcon.SetActive(false);
        panelNoIcon.SetActive(true);
        text = textNoIcon;
        text.color = text.color.SetAlpha(1f);
        text.gameObject.SetActive(true);
        tick = tickNoIcon;
        tick.gameObject.SetActive(false);
      }

      text.text = t;
      objectiveIcon = null;
    }

    private Sequence sequence;

    public void Highlight()
    {
      if (sequence != null) sequence.Kill();

      if (IsCompleted == false)
      {
        text.text = nextValue;
        text.color = Color.white;
        text.transform.DOKill();
        text.transform.localScale = Vector3.one;
        text.transform.DOPunchScale(Vector3.one * 1.15f, 0.5f, 1);
      }
    }

    public void Reset()
    {
      if (sequence != null) sequence.Kill();

      text.transform.DOKill();
      text.transform.localScale = Vector3.one;

      sequence = DOTween.Sequence();
      sequence.Append(text.DOColor(Color.red, 0.25f).SetEase(Ease.OutCubic));
      sequence.Join(text.transform.DOPunchPosition(new Vector3(25, 0, 0), 0.5f, 25));
      sequence.Append(text.DOColor(Color.white, 0.25f).SetEase(Ease.OutCubic));
    }

    #endregion

    public Objective Objective => objective;

    public TextMeshProUGUI Text => text;
    public Image Icon => objectiveIcon;

    public bool IsCompleted => completed;

    private bool IsImmediateUpdate
    {
      get
      {
        var type = objective.GetObjectiveType();
        switch (type)
        {
          case ObjectiveStatType.Score:
          case ObjectiveStatType.TotalCombos:
          case ObjectiveStatType.Total4Combos:
          case ObjectiveStatType.Total5Combos:
          case ObjectiveStatType.TotalLCombos:
          case ObjectiveStatType.HighestMultiplier:
            return false;
        }

        return true;
      }
    }
  }
}