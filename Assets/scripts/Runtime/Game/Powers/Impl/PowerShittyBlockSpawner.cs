using System.Collections;
using System.Linq;

namespace Pon.Powers
{
  public class PowerShittyBlockSpawner : Power
  {
    public override float ChargeMultiplicator => 1.15f;

    public override PowerType PowerType => PowerType.ShittyBlockSpawner;

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
      // Pick your poison
      var shittyBlock = BlockDefinitionBank.Instance.GetNormalBlocks()
        .OrderByDescending(b => b.minLevel)
        .First();

      // Add to the grid
      yield return SpawnBlock(gridScript, grid, 4, new[] {shittyBlock}, 0.2f);
      yield return null;
      EndPowerOnOpponent(gridScript, grid);
    }
  }
}