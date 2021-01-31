using System.Collections;
using UnityEngine;

namespace Pon.Powers
{
  public class PowerGarbageBreaker : Power
  {
    public override float ChargeMultiplicator => 0.75f;
    public override PowerType PowerType => PowerType.GarbageBreaker;

    public override bool CanUsePower(GridScript gridScript, Grid grid)
    {
      for (int y = 0; y < gridScript.settings.height; y++)
      {
        for (int x = 0; x < gridScript.settings.width; x++)
        {
          var b = gridScript.Get(x, y);
          if (b != null && b.block.IsEmpty == false
                        && b.block.Definition.isGarbage
                        && b.block.IsConverting == false)
          {
            return true;
          }
        }
      }

      return false;
    }

    protected override IEnumerator UsePowerRoutine(GridScript gridScript, Grid grid)
    {
      gridScript.PowerCharge = 0f;

      // Search & destroy garbages
      for (int y = 0; y < gridScript.settings.height; y++)
      {
        for (int x = 0; x < gridScript.settings.width; x++)
        {
          var b = gridScript.Get(x, y);
          if (b != null && b.block.IsEmpty == false
                        && b.block.Definition.isGarbage
                        && b.block.IsConverting == false)
          {
            gridScript.TheGrid.BreakGarbage(b.block);
          }

          yield return new WaitForSeconds(0.015f);
        }
      }

      RaisePowerUsed(new PowerUseParams()
      {
        gridScript = gridScript,
        player = gridScript.player,
        // fx = fx
      });

      EndPower(gridScript);
    }

    public override bool CanUseOnOpponent(GridScript gridScript, Grid grid)
    {
      return false;
    }

    protected override IEnumerator UsePowerOnOpponent(GridScript gridScript, Grid grid, PowerUseParams param)
    {
      EndPowerOnOpponent(gridScript, grid);
      yield return null;
    }
  }
}