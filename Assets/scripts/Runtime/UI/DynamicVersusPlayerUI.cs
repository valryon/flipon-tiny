// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Pon
{
  public sealed class DynamicVersusPlayerUI : MonoBehaviour
  {
    #region Members

    public GameObject root;
    public CanvasGroup hud;
    public TextMeshProUGUI playerName;
    public LevelWidget level;
    public GameObject timePanel;
    public TextMeshProUGUI score;
    public GameObject scorePanel;
    public SpeedWidget speedUpButton;
    public PowerWidget powerCharge;
    public GarbagesWidget garbagesWidget;
    public ObjectiveWidget objective1;
    public ObjectiveWidget objective2;
    public ObjectiveWidget objective3;
    public ObjectiveWidget objective4;

    private bool speedUp;
    private TimeWidget time;

    #endregion

    #region Timeline

    private void Awake()
    {
      scorePanel.SetActive(false);
      level.gameObject.SetActive(false);
      timePanel.SetActive(false);
    }

    void Start()
    {
      var trigger = speedUpButton.GetComponent<EventTrigger>();

      var entryDown = new EventTrigger.Entry();
      entryDown.eventID = EventTriggerType.PointerDown;
      entryDown.callback.AddListener((data) =>
      {
        speedUp = true;
        GameUIScript.GetSpeedButton(Player.player.index).SetSpeedActivated(true);
      });
      trigger.triggers.Add(entryDown);

      var entryUp = new EventTrigger.Entry();
      entryUp.eventID = EventTriggerType.PointerUp;
      entryUp.callback.AddListener((data) =>
      {
        speedUp = false;
        GameUIScript.GetSpeedButton(Player.player.index).SetSpeedActivated(false);
      });
      trigger.triggers.Add(entryUp);
    }

    private void Update()
    {
      if (speedUp)
      {
        Player.grid.SpeedScrollingUp();
      }

      if (Player.player.GameOver)
      {
        if (speedUpButton) speedUpButton.gameObject.SetActive(false);
        if (powerCharge) powerCharge.gameObject.SetActive(false);
      }
    }

    #endregion

    #region Public Methods

    public void SetPlayer(PlayerScript p)
    {
      Player = p;

      playerName.text = Player.player.name;

      speedUpButton.gameObject.SetActive(!p.grid.settings.noScrolling && p.player.type != PlayerType.AI);
      speedUpButton.SetPlayer(p);

      powerCharge.gameObject.SetActive(p.grid.enablePower);
      powerCharge.SetPlayer(p);

      garbagesWidget.SetPlayer(p);

      var rectTransform = root.GetComponent<RectTransform>();
      rectTransform.localRotation = Quaternion.Euler(0f, 0f, p.player.gridAngle);
    }

    public void SetScore(string text)
    {
      scorePanel.SetActive(!string.IsNullOrEmpty(text));
      score.text = text;
    }

    public void SetLevel(string text)
    {
      level.SetValue(text);
    }

    public void SetTimer(float current, float max)
    {
      timePanel.SetActive(true);
      if (time == null) time = timePanel.GetComponentInChildren<TimeWidget>(true);
      time.SetTime(current, max);
    }

    public void SetPowerCharge(float charge, int direction)
    {
      if (powerCharge != null)
      {
        powerCharge.SetCharge(charge, direction);
      }
    }

    #endregion

    #region Properties

    public PlayerScript Player { get; private set; }

    #endregion
  }
}