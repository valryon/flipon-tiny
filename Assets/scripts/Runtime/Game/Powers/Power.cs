using System;
using System.Collections;
using DG.Tweening;
using Pon.Powers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Pon
{
  public class PowerUseParams
  {
    public GridScript gridScript;
    public Player player;
    public BlockDefinition[] defs;
  }

  public enum PowerType
  {
    None,
    Bomb,
    TimeFreeze,
    Simplificator,
    GarbageBreaker,
    UNUSED,
    LineHider,
    ShittyBlockSpawner
  }

  public abstract class Power
  {
    public virtual float ChargeMultiplicator => 1f;
    public event Action<Power, PowerUseParams> OnPowerUsed;

    public abstract PowerType PowerType { get; }

    public virtual string Sound => "power_" + PowerType.ToString().ToLower();

    public PowerData Data => PowersData.Instance.Get(PowerType);

    // Factory
    public static Power Create(PowerType type)
    {
      switch (type)
      {
        case PowerType.Bomb:
          return new PowerBomb();
        case PowerType.Simplificator:
          return new PowerSimplificator();
        case PowerType.GarbageBreaker:
          return new PowerGarbageBreaker();
        case PowerType.ShittyBlockSpawner:
          return new PowerShittyBlockSpawner();
        case PowerType.LineHider:
          return new PowerLineHider();
        case PowerType.TimeFreeze:
          return new PowerTimeFreeze();
        case PowerType.None:
          return null;
      }

      Log.Error("Unknown power " + type);
      return null;
    }

    public virtual bool CanUsePower(GridScript gridScript, Grid grid)
    {
      // At least one block found
      for (int y = 0; y < gridScript.settings.height; y++)
      {
        var b = gridScript.Get(0, y);
        if (b != null && b.block.IsEmpty == false && b.block.IsBeingRemoved == false)
        {
          return true;
        }
      }

      return false;
    }

    public virtual bool CanUseOnOpponent(GridScript gridScript, Grid grid)
    {
      return true;
    }

    public void UsePower(GridScript gridScript, Grid grid)
    {
      Log.Info(gridScript.player.name + ": Power START " + gridScript.player.power);

      gridScript.FreezePowerGain = true;

      Loom.RunCoroutine(UsePowerRoutine(gridScript, grid));
    }

    protected abstract IEnumerator UsePowerRoutine(GridScript gridScript, Grid grid);

    public void UsePowerOnOpponent(PowerUseParams param, GridScript gridScript, Grid grid)
    {
      if (gridScript.player.GameOver) return;

      Log.Info(gridScript.player.name + ": Power side effect START " + this);

      var midGrid = new Vector3(grid.width / 2f, grid.height / 2f);
    }

    protected abstract IEnumerator UsePowerOnOpponent(GridScript gridScript, Grid grid, PowerUseParams param);


    protected void RaisePowerUsed(PowerUseParams param)
    {
      OnPowerUsed?.Invoke(this, param);
    }

    protected void EndPower(GridScript grid)
    {
      Log.Info(grid.player.name + ": Power END " + this);

      // Delay 
      Loom.RunCoroutine(Timer.Start(3f, () => { grid.FreezePowerGain = false; }));
    }

    protected void EndPowerOnOpponent(GridScript gridScript, Grid grid)
    {
      Log.Info(gridScript.player.name + ": Power side effect END " + this);
    }

    #region Tools

    protected IEnumerator SpawnBlock(GridScript gridScript, Grid grid, int count, BlockDefinition[] defs, float delay)
    {
      int i = 0;
      int countLeft = count;

      for (int y = grid.height - 3; y < grid.height; y++)
      {
        for (int x = Random.Range(0, 2); x < grid.width; x += 2)
        {
          if (countLeft > 0)
          {
            var bs = gridScript.Get(x, y);
            if (bs != null)
            {
              var b = bs.block;
              if (b.IsEmpty && b.IsBeingRemoved == false)
              {
                var defToUse = defs[i % defs.Length];

                b.SetDefinition(defToUse, b.IsActive);
                b.SetFallPosition(4);
                b.FallMomentum = 0f;

                i++;
                countLeft--;
                yield return new WaitForSeconds(delay);
              }
            }
          }
        }
      }
    }

    #endregion
  }
}