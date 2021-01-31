// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using System.Linq;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Pon
{
  /// <summary>
  /// Create grid/players and tie everything together
  /// </summary>
  public class PonGameScript : MonoBehaviour
  {
    static PonGameScript()
    {
      UnityLog.Init();
    }

    public static PonGameScript instance;

    #region Members

    [Header("Prefabs")]
    public CursorScript[] cursorPrefabs;

    private GameSettings settings;
    private Objective objectives;
    private List<PlayerScript> players = new List<PlayerScript>();

    private bool isPaused, isOver;
    private float timeElapsed;

    private bool firstGridStarted;
    private int maxStressLevel;

    #endregion

    #region Timeline

    private void Awake()
    {
      instance = this;
    }

    private void Start()
    {
      // Threads
      Loom.Initialize();

      GetSettings();

      PrepareUI();

      CreatePlayersAndGrids();

      StartGrids();
    }

    private void OnDestroy()
    {
      foreach (var p in players)
      {
        if (p != null && p.power != null)
        {
          p.power.OnPowerUsed -= OnPowerUsed;
        }
      }
    }

    void Update()
    {
      if (isPaused == false && isOver == false)
      {
        timeElapsed += Time.deltaTime;
      }

      GameUIScript.SetTime(timeElapsed);

      // Update objectives
      if (objectives != null)
      {
        UpdateObjectives();
      }
    }

    private void UpdateObjectives()
    {
      foreach (var p in players)
      {
        if (p.grid.IsPaused) continue;

        // Give new stats, widget will check if it's relevant
        var currentStats = new ObjectiveStats(p, timeElapsed);

        for (int i = 1; i <= 4; i++)
        {
          GameUIScript.UpdateObjective(p, i, currentStats);
        }
      }

      var p1 = players[0];

      // Stop here if not started
      if (p1.grid.IsStarted == false) return;
      //--------------------------------------------------------------------------------------------------------------

      if (isOver == false)
      {
        for (int i = 0; i < players.Count; i++)
        {
          var pWinner = players[i];
          var pStats = new ObjectiveStats(pWinner, timeElapsed);
          if (objectives.Succeed(pWinner, pStats))
          {
            Log.Info("Versus with level ended!");
            GameOverVersus(pWinner);

            break;
          }
        }
      }
    }

    private void GameOverVersus(PlayerScript winner)
    {
      // One player wins
      isOver = true;

      var sequence = DOTween.Sequence();
      sequence.Append(DOTween.To(() => winner.grid.ScaleTime, (v) =>
        {
          foreach (var p in players)
          {
            p.grid.ScaleTime = v;
          }
        }, 0f, 1f)
        .SetEase(Ease.OutCubic));
      sequence.AppendInterval(1f);
      sequence.AppendCallback(() =>
      {
        winner.player.GameOver = false;
        winner.grid.SetVictory();

        foreach (var pLoser in players)
        {
          if (pLoser == winner) continue;
          pLoser.player.GameOver = true;
          pLoser.grid.SetGameOver();
        }
      });
      sequence.AppendInterval(2f);
      sequence.AppendCallback(TriggerGameOver);
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Find game settings (players, grid size, grid speed, etc) or create default ones.
    /// </summary>
    private void GetSettings()
    {
      settings = FindObjectOfType<GameSettings>();
      if (settings == null)
      {
        // Default
        Log.Error("Missing game settings. Please add one to the scene.");
        return;
      }

      if (settings.players.Length == 0)
      {
        Log.Error("No player defined... Nothing is going to happen!");
        return;
      }

      if (settings.players.Length == 1)
      {
        Log.Error("Needs 2 players.");
        return;
      }

      if (settings.enableObjectives)
      {
        objectives = settings.objective.Clone();
        if (objectives == null)
        {
          Log.Error("Missing objectives");
          return;
        }
      }
    }

    private void PrepareUI()
    {
      GameUIScript.Init(settings);
      GameUIScript.SetPlayersCount(settings.players.Length, settings.players.Count(p => p.type == PlayerType.AI));

      var ui = GameUIScript.GetUI();
      for (int i = 0;
        i < settings.players.Length;
        i++)
      {
        var p = settings.players[i];

        var z = ui.GetPlayerZone(p);
        p.gridViewport = z.rect;

        if (p.allowGridAngle)
        {
          p.gridAngle = z.angle;
        }

        GameUIScript.SetScore(i, 0);
        GameUIScript.SetSpeed(i, settings.gridSettings.startLevel);


        if (objectives != null)
        {
          GameUIScript.SetObjective(p.index, 1, objectives);
        }
      }
    }

    /// <summary>
    /// Create players and related grid using game settings.
    /// </summary>
    private void CreatePlayersAndGrids()
    {
      foreach (var basePlayer in settings.players)
      {
        var p = basePlayer;
        var po = new GameObject();

        PlayerScript player;
        if (p.type == PlayerType.Local)
        {
          player = po.AddComponent<PlayerScript>();
        }
        else if (p.type == PlayerType.AI)
        {
          player = po.AddComponent<AIPlayerScript>();
        }
        else
        {
          Log.Error("Unsupported player type " + p.type);
          return;
        }

        po.name = "Player " + p.name;
        po.transform.parent = transform;
        po.transform.position = Vector3.zero;
        player.player = p;
        if (p.power != PowerType.None)
        {
          player.power = Power.Create(p.power);
          player.power.OnPowerUsed += OnPowerUsed;
        }

        player.cursorPrefabs = cursorPrefabs;
        player.cam = GetCamera(player);
        player.grid = CreateGrid(player, player.cam);
        players.Add(player);

        // Init UI with player
        player.grid.ui = GameUIScript.SetPlayer(player, settings.players.Length);
        if (player.power != null)
        {
          GameUIScript.SetPowerCharge(p.index, 0, 0);
          player.grid.PowerCharge += 0.35f; // Start at n% > 0
        }
      }
    }

    /// <summary>
    /// Create a new camera for the player
    /// </summary>
    private Camera GetCamera(PlayerScript p)
    {
      var camGo = new GameObject("Camera P" + p.player.index);
      camGo.transform.position = new Vector3(p.transform.position.x, 0, -10);
      camGo.transform.parent = p.transform;

      var cam = camGo.AddComponent<Camera>();

      cam.clearFlags = CameraClearFlags.SolidColor;
      cam.backgroundColor = Color.black;
      // Render everything *except* layer UI
      cam.cullingMask = ~(1 << LayerMask.NameToLayer("UI"));
      //cam.orthographicSize = 0: // This is not where we set the size. See GridScript.SetViewport.
      cam.orthographic = true;
      cam.depth = -5;

      return cam;
    }

    /// <summary>
    /// Create a grid for a player.
    /// </summary>
    private GridScript CreateGrid(PlayerScript playerScript, Camera cam)
    {
      var gridObj = new GameObject("Grid");
      gridObj.transform.parent = playerScript.transform;
      gridObj.transform.position = new Vector3(playerScript.player.index * 25, 0, 0);

      var grid = gridObj.AddComponent<GridScript>();
      grid.settings = settings.gridSettings.Clone();
      grid.viewportRect = playerScript.player.gridViewport;
      grid.angle = playerScript.player.gridAngle;
      grid.player = playerScript.player;
      grid.playerScript = playerScript;
      grid.gridCam = cam;
      grid.enablePower = (playerScript.player.power != PowerType.None);

      grid.OnGameOver += GameOver;
      grid.OnCombo += Combo;
      grid.OnScoreChanged += OnScoreChanged;
      grid.OnPowerChargeChanged += OnPowerChargeChanged;
      grid.OnLevelChanged += OnLevelChanged;
      grid.OnMultiplierChange += OnMultiplierChange;

      return grid;
    }

    public void StartGrids()
    {
      foreach (var playerScript in players)
      {
        playerScript.grid.IsStarted = true;
      }
    }

    #endregion

    #region Public methods

    public void SetPause(bool paused)
    {
      if (paused == false && isOver) return;

      isPaused = paused;

      foreach (var p in players)
      {
        p.grid.SetPause(paused);
      }
    }

    #endregion

    #region Events

    public void OnPowerUsed(Power power, PowerUseParams param)
    {
      // Dispatch to other players
      foreach (var p in players)
      {
        if (p.player.index != param.player.index && p.player.GameOver == false)
        {
          if (power.CanUseOnOpponent(p.grid, p.grid.TheGrid))
          {
            power.UsePowerOnOpponent(param, p.grid, p.grid.TheGrid);
          }
        }
      }
    }

    private void OnScoreChanged(GridScript g, long score)
    {
      // Update UI
      GameUIScript.SetScore(g.player.index, score);
    }

    private void OnPowerChargeChanged(GridScript g, PowerType power, float charge, int direction)
    {
      // Update UI
      GameUIScript.SetPowerCharge(g.player.index, charge, direction);
    }

    private void OnLevelChanged(GridScript g, int l)
    {
      // Update UI
      GameUIScript.SetSpeed(g.player.index, l);
    }

    private void OnMultiplierChange(GridScript grid, int m)
    {
      // Update UI
      GameUIScript.SetMultiplier(grid.player.index, m);
    }

    private void Combo(GridScript g, ComboData c)
    {
      // Send blocks!
      if (players.Count > 1) GenerateGarbage(g, c);
    }

    private void GenerateGarbage(GridScript g, ComboData c)
    {
      // Every action send garbages, but not always a lot of it.
      int width = 0;
      int count = 1;

      if (c.isChain == false)
      {
        // 3 blocs = 1x1 every 3 combos
        if (c.blockCount == 3 && c.multiplier % 3 == 0)
        {
          width = 1;
          count = 1;
        }
        // 4 blocs = 1x1
        else if (c.blockCount == 4)
        {
          width = 1;
          count = 1;
        }
        // 5 blocks
        else if (c.blockCount == 5)
        {
          width = 3;
          count = 1;
        } // L-shaped block = wow
        else if (c.blockCount > 5)
        {
          width = 5;
          count = 1;
        }
      }
      else
      {
        width = players[0].grid.settings.width;
      }

      // Factors
      if (settings.garbagesType == GarbagesType.None) return;
      if (settings.garbagesType == GarbagesType.Low)
      {
        if (width == 5)
        {
          count = 2;
        }

        width = 1;
      }

      if (width > 0)
      {
        var si = g.player.index;
        var sender = players.First(p => p.player.index == si);
        if (sender.lastPlayerTarget < 0) sender.lastPlayerTarget = sender.player.index;
        sender.lastPlayerTarget++;
        if (sender.lastPlayerTarget >= players.Count) sender.lastPlayerTarget = 0;

        var alivePlayers = players.Where(p => p.player.GameOver == false).ToArray();
        foreach (var p in alivePlayers)
        {
          int pi = p.player.index;
          if (
            // Target the other player
            (alivePlayers.Length <= 2 && pi != si)
            ||
            // Target rotation
            (pi >= sender.lastPlayerTarget
             && pi != si
             && p.player.GameOver == false))
          {
            sender.lastPlayerTarget = pi;
            width = Mathf.Clamp(width, 1, settings.gridSettings.width);

            for (int i = 0; i < count; i++)
            {
              p.grid.AddGarbage(width, c.isChain, sender, c.comboLocation, c.definition.color);
            }

            break;
          }
        }
      }
    }

    private void GameOver(GridScript grid)
    {
      int gameOverPlayers = 0;

      foreach (var p in players)
      {
        if (p.player.index == grid.player.index)
        {
          p.player.GameOver = true;
          p.player.Score = grid.Score;
        }

        if (p.player.GameOver)
        {
          gameOverPlayers++;
        }
      }

      if (gameOverPlayers >= players.Count - 1)
      {
        foreach (var p in players)
        {
          if (p.player.GameOver == false)
          {
            p.grid.SetPause(true);
          }
        }

        DOVirtual.DelayedCall(1f, TriggerGameOver);
      }
    }

    private void TriggerGameOver()
    {
      Log.Warning("Game is ended.");
      SetPause(true);
      isOver = true;
    }

    #endregion

    #region Properties

    public GameSettings Settings => settings;

    public List<PlayerScript> Players => players;

    public Objective Objectives => objectives;

    public bool IsPaused => isPaused;

    public bool IsOver => isOver;

    #endregion
  }
}