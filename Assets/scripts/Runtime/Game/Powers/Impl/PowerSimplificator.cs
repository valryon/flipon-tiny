using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pon.Powers
{
  public class PowerSimplificator : Power
  {
    public override float ChargeMultiplicator => 1.3f;

    public override PowerType PowerType => PowerType.Simplificator;

    protected override IEnumerator UsePowerRoutine(GridScript gridScript, Grid grid)
    {
      gridScript.PowerCharge = 0f;

      // Keep only N colors
      Dictionary<BlockDefinition, int> colors = new Dictionary<BlockDefinition, int>();

      for (int x = 0; x < grid.width; x++)
      {
        for (int y = 0; y < grid.height; y++)
        {
          var b = grid.Get(x, y);
          if (b != null && b.IsEmpty == false && b.IsBeingRemoved == false && b.Definition.isGarbage == false)
          {
            if (colors.ContainsKey(b.Definition) == false)
            {
              colors.Add(b.Definition, 0);
            }

            colors[b.Definition]++;
          }
        }
      }

      int colorsToKeep = Mathf.Max(1, colors.Count - 1);
      var colorsKept = colors.OrderBy(c => c.Value)
        .Select(c => c.Key)
        .Take(colorsToKeep).ToList();

      if (colorsKept.Count > 1)
      {
        gridScript.FreezeCombos = true;

        // Change all blocks that are not those colors
        for (int x = 0; x < grid.width; x++)
        {
          for (int y = 0; y < grid.height; y++)
          {
            var b = grid.Get(x, y);
            if (b != null && b.IsEmpty == false && b.IsBeingRemoved == false && b.Definition.isGarbage == false)
            {
              if (colorsKept.Contains(b.Definition)) continue;

              var left = grid.Get(x - 1, y)?.Definition;
              var right = grid.Get(x + 1, y)?.Definition;
              var top = grid.Get(x, y + 1)?.Definition;
              var bot = grid.Get(x, y - 1)?.Definition;
              var newColor = colorsKept.Where(c => c != b.Definition
                                                   && c != left && c != right && c != top && c != bot)
                .PickRandom();

              if (newColor == null) newColor = colorsKept.PickRandom();
              if (newColor != null)
              {
                b.ConvertTo(0, 0, newColor);
                yield return new WaitForSeconds(0.005f);
                b.Chainable = false;
              }
            }
          }
        }

        gridScript.FreezeCombos = false;
        yield return new WaitForSeconds(0.1f);
      }

      RaisePowerUsed(new PowerUseParams()
      {
        gridScript = gridScript,
        player = gridScript.player,
        defs = colorsKept.ToArray(),
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
      // // Wait for effect
      // yield return new WaitForSeconds(0.75f);
      //
      // List<BlockScript> nonEmptyBlocks = new List<BlockScript>();
      // for (int x = 0; x < grid.width; x++)
      // {
      //   for (int y = 0; y < grid.height; y++)
      //   {
      //     var bs = gridScript.Get(x, y);
      //     if (bs != null)
      //     {
      //       var b = bs.block;
      //       if (b != null && b.IsEmpty == false && b.IsBeingRemoved == false)
      //       {
      //         nonEmptyBlocks.Add(bs);
      //       }
      //     }
      //   }
      // }
      //
      // // Mess a % of the grid
      // int blocksToMess = Mathf.FloorToInt(nonEmptyBlocks.Count / 3.5f);
      // nonEmptyBlocks = nonEmptyBlocks.OrderBy(b => Random.Range(0f, 1f)).Take(blocksToMess).ToList();
      //
      // foreach (var bs in nonEmptyBlocks)
      // {
      //   bs.block.ConvertTo(0, 0,
      //     BlockDefinitionBank.Instance.GetRandomNormal(1, grid.width, false,
      //       new List<BlockDefinition>() {bs.block.Definition}));
      //
      //   gridScript.PlayEffectAndSetColor("Explosion",
      //     bs.transform.position, new Vector3(0.5f, 0.5f),
      //     Color.black);
      // }

      EndPowerOnOpponent(gridScript, grid);
      yield return null;
    }
  }
}