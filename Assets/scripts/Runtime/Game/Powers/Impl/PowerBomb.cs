using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pon.Powers
{
  public class PowerBomb : Power
  {
    public override PowerType PowerType => PowerType.Bomb;

    protected override IEnumerator UsePowerRoutine(GridScript gridScript, Grid grid)
    {
      const int LINES_TO_REMOVE = 3;

      gridScript.PowerCharge = 0f;
      List<BlockDefinition> destroyedBlocks = new List<BlockDefinition>();

      // Remove n lines
      for (int n = 0; n < LINES_TO_REMOVE; n++)
      {
        for (int x = 0; x < gridScript.TheGrid.width; x++)
        {
          var b = gridScript.TheGrid.Get(x, n);
          if (b != null && b.IsDestructable)
          {
            destroyedBlocks.Add(b.Definition);
            b.EmptyWithAnimation(0, 1);
          }

          yield return new WaitForSeconds(0.015f);
        }

        yield return new WaitForSeconds(0.03f);
      }

      RaisePowerUsed(new PowerUseParams()
      {
        gridScript = gridScript,
        player = gridScript.player,
        defs = destroyedBlocks.ToArray(),
      });

      EndPower(gridScript);
    }

    public override bool CanUseOnOpponent(GridScript gridScript, Grid grid)
    {
      return false;
    }

    protected override IEnumerator UsePowerOnOpponent(GridScript gridScript, Grid grid, PowerUseParams param)
    {
      // yield return SpawnBlock(gridScript, grid, param.defs.Length, param.defs, 0.05f);
      yield return null;
      EndPowerOnOpponent(gridScript, grid);
    }
  }
}