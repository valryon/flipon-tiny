// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using DG.Tweening;
using UnityEngine;

namespace Pon
{
  /// <summary>
  /// 2+ players UI.
  /// This is just a facade and a generator for the players UI.
  /// It has no canvas itself (otherwise it will block inputs from player canvases!)
  /// </summary>
  public class DynamicVersusUIScript : GenericUIScript
  {
    #region Members

    [Header("Bindings")]
    public DynamicVersusPlayerUI playerUI;

    private DynamicVersusPlayerUI[] playerInterfaces;

    #endregion

    #region Timeline

    #endregion

    #region Generic UI

    public override void SetPlayersCount(int totalPlayersCount, int aiPlayerCount)
    {
      playerInterfaces = new DynamicVersusPlayerUI[totalPlayersCount];

      ConfigureRect(totalPlayersCount, aiPlayerCount);

      for (int i = 0; i < totalPlayersCount; i++)
      {
        playerInterfaces[i] = CreatePlayerUI();
      }
    }

    public override Canvas SetPlayer(PlayerScript player, int playersCount)
    {
      int i = player.player.index;

      var ui = playerInterfaces[i];
      ui.name = ui.name.Replace("(Clone)", (" " + player.player.name));
      ui.SetPlayer(player);

      var canvas = ui.GetComponent<Canvas>();
      SetUIForPlayer(player, canvas);
      return canvas;
    }

    private void ConfigureRect(int totalPlayersCount, int iaPlayersCount)
    {
      playerZones = new PlayerZone[totalPlayersCount];

      // 2P
      // =================================================
      if (totalPlayersCount == 2)
      {
        playerZones[0] = new PlayerZone()
        {
          index = 0,
          rect = new Rect(0, 0f, 0.5f, 1f),
          angle = 0f
        };
        playerZones[1] = new PlayerZone()
        {
          index = 1,
          rect = new Rect(0.5f, 0f, 0.5f, 1f),
          angle = 0f
        };
      }

      // 3P
      // =================================================
      else if (totalPlayersCount == 3)
      {
        playerZones[0] = new PlayerZone()
        {
          index = 0,
          rect = new Rect(0f, 0.5f, 0.5f, 0.5f),
          angle = 0f
        };
        playerZones[1] = new PlayerZone()
        {
          index = 1,
          rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f),
          angle = 0f
        };
        playerZones[2] = new PlayerZone()
        {
          index = 2,
          rect = new Rect(0.25f, 0f, 0.5f, 0.5f),
          angle = 0f
        };
      }

      // 4P
      // =================================================
      else if (totalPlayersCount == 4)
      {
        playerZones[0] = new PlayerZone()
        {
          index = 0,
          rect = new Rect(0f, 0.5f, 0.5f, 0.5f),
          angle = 0f
        };
        playerZones[1] = new PlayerZone()
        {
          index = 1,
          rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f),
          angle = 0f
        };
        playerZones[2] = new PlayerZone()
        {
          index = 2,
          rect = new Rect(0f, 0f, 0.5f, 0.5f),
          angle = 0f
        };
        playerZones[3] = new PlayerZone()
        {
          index = 3,
          rect = new Rect(0.5f, 0f, 0.5f, 0.5f),
          angle = 0f
        };
      }
      // nP
      // =================================================
      else
      {
        int col = Mathf.CeilToInt(totalPlayersCount / 2f);
        float width = 1f / Mathf.Ceil(totalPlayersCount / 2f);

        for (int i = 0; i < col; i++)
        {
          playerZones[i] = new PlayerZone()
          {
            index = i,
            rect = new Rect(i * width, 0f, width, 0.5f),
            angle = 0f
          };
        }

        for (int i = col; i < totalPlayersCount; i++)
        {
          playerZones[i] = new PlayerZone()
          {
            index = i,
            rect = new Rect((i - col) * width, 0.5f, width, 0.5f),
            angle = 0f
          };
        }
      }
    }

    private DynamicVersusPlayerUI CreatePlayerUI()
    {
      playerUI.gameObject.SetActive(false);

      var ui = Instantiate(playerUI.gameObject, transform, true);
      ui.transform.localScale = Vector3.one;
      ui.SetActive(true);

      var pUI = ui.GetComponent<DynamicVersusPlayerUI>();

      return pUI;
    }


    public override void SetScore(int player, string t)
    {
      if (playerInterfaces != null && playerInterfaces.Length > player && playerInterfaces[player] != null)
      {
        playerInterfaces[player].SetScore(t);
      }
    }

    public override void SetLevel(int player, string t)
    {
      if (playerInterfaces != null && playerInterfaces.Length > player && playerInterfaces[player] != null)
      {
        playerInterfaces[player].SetLevel(t);
      }
    }

    public override void SetTimer(float current, float max)
    {
      foreach (var p in playerInterfaces)
      {
        p.SetTimer(current, max);
      }
    }

    public override ObjectiveWidget GetWidget(int player, int index)
    {
      if (playerInterfaces != null && playerInterfaces.Length > player && playerInterfaces[player] != null)
      {
        if (playerInterfaces[player].objective1 != null && index == 1)
        {
          return playerInterfaces[player].objective1;
        }

        if (playerInterfaces[player].objective2 != null && index == 2)
        {
          return playerInterfaces[player].objective2;
        }

        if (playerInterfaces[player].objective3 != null && index == 3)
        {
          return playerInterfaces[player].objective3;
        }

        if (playerInterfaces[player].objective4 != null && index == 4)
        {
          return playerInterfaces[player].objective4;
        }
      }

      Log.Error("UI: Unknow slot/missing objective widget for index " + index + " player " + player);
      return null;
    }

    public override void SetPowerCharge(int player, float charge, int direction)
    {
      if (playerInterfaces != null && playerInterfaces.Length > player && playerInterfaces[player] != null)
      {
        playerInterfaces[player].SetPowerCharge(charge, direction);
      }
    }

    public override void SetAlpha(float alpha, bool anim)
    {
      foreach (var pi in playerInterfaces)
      {
        pi.hud.DOFade(alpha, anim ? 0.25f : 0f).SetEase(Ease.OutQuart);
      }
    }

    #endregion

    public override SpeedWidget GetSpeedUpButton(int player)
    {
      return playerInterfaces[player].speedUpButton;
    }

    public override PowerWidget GetPowerBar(int player)
    {
      return playerInterfaces[player].powerCharge;
    }
  }
}