using System.Collections;
using UnityEngine;

namespace Pon.Powers
{
  public class PowerLineHider : Power
  {
    public override float ChargeMultiplicator => 1.1f;

    public override PowerType PowerType => PowerType.LineHider;

    protected override IEnumerator UsePowerRoutine(GridScript gridScript, Grid grid)
    {
      gridScript.PowerCharge = 0f;

      RaisePowerUsed(new PowerUseParams()
      {
        gridScript = gridScript,
        player = gridScript.player,
        // fx = fx
      });
      yield return null;

      EndPower(gridScript);
    }

    protected override IEnumerator UsePowerOnOpponent(GridScript gridScript, Grid grid, PowerUseParams param)
    {
      // Wow wow wow.
      // It's going to be fun.
      // Take the blocks below
      // And hide them.
      // Easy?
      // But it scrolls. HA
      // Fuck
      // What a shitty power idea.
      for (int x = 0; x < grid.width; x++)
      {
        for (int y = -grid.previewLines; y < 2; y++)
        {
          var bs = gridScript.Get(x, y);
          if (bs != null && bs.block.IsEmpty == false
                         && bs.block.Definition.isGarbage == false)
          {
            bs.Hide(3.5f);
            yield return new WaitForEndOfFrame();
          }
        }
      }

      EndPowerOnOpponent(gridScript, grid);
    }
  }
}