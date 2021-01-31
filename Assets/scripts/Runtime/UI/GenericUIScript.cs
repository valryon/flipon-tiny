// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using System;
using UnityEngine;
using UnityEngine.UI;

namespace Pon
{
  [Serializable]
  public struct PlayerZone
  {
    public int index;
    public float angle;
    public Rect rect;

    public override string ToString()
    {
      return $"{index}:{rect} {angle}°";
    }
  }

  /// <summary>
  /// This is an intermediate layer between the UI and the accessors.
  /// It allows us, in the full game, to have a 1P landscape, 1P portrait and a multiplayer UI that we access in the same way.
  /// </summary>
  public abstract class GenericUIScript : MonoBehaviour
  {
    #region Members

    [Header("Generic settings")]
    public Vector2 referenceResolution = new Vector2(1920, 1080);

    public Vector2 gridRelativePosition = Vector3.zero;
    public Vector3 gridScale = Vector3.one;

    protected PlayerZone[] playerZones;

    private TimeWidget time;

    #endregion

    #region Public methods

    public abstract void SetPlayersCount(int totalPlayersCount, int aiPlayerCount);

    public abstract void SetScore(int player, string text);

    public abstract void SetLevel(int player, string text);

    public abstract void SetTimer(float current, float max);

    public void SetObjectivesDisplay(int player, int objectivesCount)
    {
      var w1 = GetWidget(player, 1);
      var w2 = GetWidget(player, 1);
      var w3 = GetWidget(player, 1);
      var w4 = GetWidget(player, 1);
      if (w1 != null) w1.gameObject.SetActive(objectivesCount > 0);
      if (w2 != null) w2.gameObject.SetActive(objectivesCount > 1);
      if (w3 != null) w3.gameObject.SetActive(objectivesCount > 2);
      if (w4 != null) w4.gameObject.SetActive(objectivesCount > 3);
    }

    public abstract void SetAlpha(float alpha, bool anim);

    public abstract Canvas SetPlayer(PlayerScript player, int playersCount);

    protected void SetUIForPlayer(PlayerScript player, Canvas canvas)
    {
      if (canvas == null) Log.Error("Missing canvas for player UI " + player.name + "!", this);
      else
      {
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.sortingLayerName = "UI";
        canvas.transform.SetParent(player.transform, false);
        canvas.name = "Player UI";
        canvas.worldCamera = player.grid.gridCam;
        canvas.gameObject.layer = LayerMask.NameToLayer("Player UI");

        // Tweak CanvasScaler
        // Reference resolution is 1080p x splitscreen
        var zone = GetPlayerZone(player.player);
        var scaler = canvas.GetComponent<CanvasScaler>();

        scaler.referenceResolution = new Vector2(referenceResolution.x * (1f / zone.rect.width),
          referenceResolution.y * (1f / zone.rect.height));
      }
    }

    public abstract ObjectiveWidget GetWidget(int player, int index);

    public void SetObjective(int player, int index, Objective obj)
    {
      var w = GetWidget(player, index);
      if (w != null)
      {
        w.SetObjective(obj);
      }
    }

    public void UpdateObjective(PlayerScript player, int index, ObjectiveStats stats)
    {
      var w = GetWidget(player.player.index, index);
      if (w != null)
      {
        w.UpdateObjective(player, stats);
      }
    }

    public PlayerZone GetPlayerZone(Player player)
    {
      for (int i = 0; i < playerZones.Length; i++)
      {
        if (playerZones[i].index == player.index)
        {
          return playerZones[i];
        }
      }

      return default;
    }

    public abstract void SetPowerCharge(int player, float charge, int direction);

    public abstract SpeedWidget GetSpeedUpButton(int player);

    public abstract PowerWidget GetPowerBar(int player);

    public void SetMultiplier(int player, int multiplier)
    {
      GameUIScript.GetSpeedButton(player).SetMultiplier(multiplier);
    }

    #endregion
  }
}