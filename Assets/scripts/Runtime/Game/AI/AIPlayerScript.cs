// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Pon
{
  public class AIPlayerScript : PlayerScript
  {
    #region Members

    public DifficultySettings difficultySettings;

    private bool showCursor;
    private bool playMove;
    private float speedUp;
    private Queue<AIMove> movesToPlay = new Queue<AIMove>();
    private float moveCooldown;
    private float initialCooldown = 2f;
    private float powerReadyCooldown;

    private AISolver solver;
    private AISettings aiSettings;

    #endregion

    #region Timeline

    protected override void Start()
    {
      base.Start();

      var game = PonGameScript.instance;
      aiSettings = player.GetAISettings(); // Get it from settings!

      var weights = aiSettings.weights;

      // Add weights on objectives if we have some
      if (game != null && game.Objectives != null)
      {
        var objs = game.Objectives.GetSubObjectives();
        if (objs.Count > 0)
        {
          if (objs.Any(o => o.totalCombos > 0))
          {
            weights.combo3 += weights.objective;
          }

          if (objs.Any(o => o.total4Combos > 0))
          {
            weights.combo4 += weights.objective;
          }

          if (objs.Any(o => o.total5Combos > 0))
          {
            weights.combo5 += weights.objective;
          }

          if (objs.Any(o => o.totalLCombos > 0))
          {
            weights.comboL += weights.objective;
          }

          if (objs.Any(o => o.timeReached > 0))
          {
            weights.danger += weights.objective;
          }
        }
      }

      aiSettings.weights = weights;

      initialCooldown = aiSettings.initialCooldown;
    }

    protected override void Update()
    {
      base.Update();
      if (player.GameOver || grid.IsGameOver || grid.IsStarted == false) return;

      if (grid.PowerCharge >= 1f && grid.TheGrid.HighestBlock.y > 3 && Random.Range(0f, 1f) > 0.85f)
      {
        grid.UsePower();
        return;
      }

      if (initialCooldown > 0)
      {
        initialCooldown -= Time.deltaTime;
        return;
      }

      if (grid == null || !grid.IsStarted || grid.IsGameOver || grid.IsPaused)
      {
        return;
      }

      if (speedUp > 0)
      {
        speedUp -= Time.deltaTime;
        grid.SpeedScrollingUp();
        return;
      }

      grid.StopSpeedScrollingUp();

      // Don't play too fast
      moveCooldown -= Time.deltaTime;
      if (moveCooldown > 0)
      {
        if (moveCooldown > aiSettings.timeBetweenMoves)
        {
          // showCursor = false;
        }

        return;
      }

      // Do we have moves in memory?
      if (movesToPlay.Count > 0 && playMove)
      {
        moveCooldown = aiSettings.timeBetweenMoves;

        var m = movesToPlay.Dequeue();
        PlayMove(m);

        if (movesToPlay.Count == 0)
        {
          moveCooldown = Random.Range(aiSettings.timeThinkMin, aiSettings.timeThinkMax);
          playMove = false;
        }
      }
      else
      {
        if (solver == null)
        {
          solver = new AISolver(grid.TheGrid, aiSettings);
        }

        solver.Work();

        movesToPlay = solver.Moves;
        playMove = solver.PlayMove;
        var shouldSpeedUp = solver.SpeedUp;

        // Debug.Log("AI - solver worked. playMove=" + playMove + " moves=" + movesToPlay.Count + " speedUp=" + shouldSpeedUp);

        // Pick best move to play
        if (shouldSpeedUp)
        {
          speedUp = 0.55f;
          moveCooldown = Random.Range(aiSettings.timeThinkMin / 2f, aiSettings.timeThinkMax / 2f);
        }
        else if (playMove == false)
        {
          moveCooldown = Random.Range(aiSettings.timeThinkMin, aiSettings.timeThinkMax);
        }
      }
    }

    #endregion

    #region Private methods

    protected override void UpdateCursors()
    {
      if (player.GameOver) return;

      foreach (var c in cursors)
      {
        c.Value.SetActive(showCursor);

        if (showCursor == false)
        {
          c.Value.Target = null;
        }
      }
    }

    protected virtual void PlayMove(AIMove m, float cursorDelay = 0.25f)
    {
      StartCoroutine(PlayMoveRoutine(m, cursorDelay));
    }

    protected virtual IEnumerator PlayMoveRoutine(AIMove m, float cursorDelay)
    {
      var b = grid.Get(m.x, m.y);

      showCursor = true;
      CreateCursor(0, b, true);

      yield return new WaitForSeconds(cursorDelay);

      if (grid.CanMove(b.block.x, b.block.y))
      {
        grid.Move(b.block, m.direction);
      }
      else
      {
        // Can't move?
        // Maybe it's due to optimisation...
        // Try to move the neighbor
        var neighbor = grid.Get(m.x + m.direction, m.y);
        if (neighbor != null)
        {
          grid.Move(neighbor.block, -1 * m.direction);
        }
      }
    }

    #endregion
  }
}