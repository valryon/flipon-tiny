using System.Collections;
using UnityEngine;

namespace Pon.Powers
{
  public class PowerTimeFreeze : Power
  {
    public override PowerType PowerType => PowerType.TimeFreeze;

    protected override IEnumerator UsePowerRoutine(GridScript gridScript, Grid grid)
    {
      const float DURATION = 5f;

      gridScript.PowerCharge = 0f;

      gridScript.FreezeCombos = true;
      gridScript.GarbageCooldownSpeed = 0.01f;
      gridScript.TheGrid.Freeze(99f);

      RaisePowerUsed(new PowerUseParams()
      {
        gridScript = gridScript,
        player = gridScript.player,
        defs = null,
        // fx = fx
      });

      yield return new WaitForSeconds(DURATION);

      gridScript.FreezeCombos = false;
      gridScript.GarbageCooldownSpeed = 1f;
      gridScript.TheGrid.Warm();

      yield return new WaitForSeconds(0.25f);

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
      // // Wait for effect
      // yield return new WaitForSeconds(0.5f);
      //
      // gridScript.SpeedBonus = 2f;
      // yield return new WaitForSeconds(5f);
      // gridScript.SpeedBonus = 1f;
      // EndPowerOnOpponent(gridScript, grid);
    }
  }
}