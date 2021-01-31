using UnityEngine;
using NUnit.Framework;
using System.Collections.Generic;

namespace Pon.Tests
{
  public class GridTests
  {
    public static Grid CreateGrid(string g, int previewLines)
    {
      // Load bank
      var f = new BlockFactoryMock();

      Dictionary<Vector2, BlockDefinition> blocks = new Dictionary<Vector2, BlockDefinition>();

      // Parse lines
      var x = 0;
      var width = 0;

      if (g.EndsWith("\n"))
      {
        g = g.Substring(0, g.Length - 1);
      }

      var lines = g.Split('\n');

      int height = lines.Length;
      int y = height - 1;

      foreach (string line in lines)
      {
        foreach (string b in line.Split(' '))
        {
          BlockDefinition def = null;

          switch (b.ToLower())
          {
            case "0":
              break;
            case "1":
              def = f.GetNormalBlocks()[0];
              break;
            case "2":
              def = f.GetNormalBlocks()[1];
              break;
            case "3":
              def = f.GetNormalBlocks()[2];
              break;
            case "4":
              def = f.GetNormalBlocks()[3];
              break;
            case "5":
              def = f.GetNormalBlocks()[4];
              break;
            case "x":
              def = f.GetGarbages()[0];
              break;
          }

          // Create block
          blocks.Add(new Vector2(x, y), def);

          x++;

          width = Mathf.Max(width, x);
        }

        y--;
        x = 0;
      }

      // Grid
      var grid = new Grid(Vector2.zero, width, height, previewLines, f, blocks);
      grid.Start(0);

      return grid;
    }

    [Test]
    public void TestSwap()
    {
      var grid = CreateGrid("1 2 0 0", 0);

      // Authorized move.
      var block1 = grid.Get(new Vector2(0, 0));
      var block2 = grid.Get(new Vector2(1, 0));

      grid.Move(new Vector2(0, 0), 1);

      var block1Moved = grid.Get(new Vector2(0, 0));
      var block2Moved = grid.Get(new Vector2(1, 0));

      Assert.AreEqual(1, block1.x);
      Assert.AreEqual(0, block1.y);
      Assert.AreEqual(0, block2.x);
      Assert.AreEqual(0, block2.y);

      Assert.AreEqual(block1, block2Moved);
      Assert.AreEqual(block2, block1Moved);

      // -- Update movements
      grid.Update(0f, 1f);

      // Swap with void.
      grid.Move(new Vector2(0, 0), -1);
      block1 = grid.Get(new Vector2(0, 0));
      Assert.AreEqual(0, block1.x);
      Assert.AreEqual(0, block1.y);

      // -- Update movements
      grid.Update(0f, 1f);

      // Swap with empty.
      block2 = grid.Get(new Vector2(1, 0));
      Assert.False(block2.IsEmpty);

      grid.Move(block2, 1);
      Assert.AreEqual(2, block2.x);
      Assert.AreEqual(0, block2.y);
      Assert.True(grid.Get(1, 0).IsEmpty);

      // -- Update movements
      grid.Update(0f, 1f);

      // Swap empty with void. Make sure there is no exception and no data loss.
      grid.Move(new Vector2(3, 0), 1);
      Assert.NotNull(grid.Get(3, 0));
    }

    [Test]
    public void TestScroll()
    {
      var grid = CreateGrid("1 0 0 0", 0);

      var b = grid.Get(0, 0);
      Assert.False(b.IsEmpty);

      grid.Update(0.42f, 0.016f);

      Assert.False(b.IsEmpty);
      Assert.AreEqual(0.42f, b.position.y);

      grid.Update(0.6f, 0.016f);

      Assert.AreEqual(b.y, 1);
    }

    [Test]
    public void TestScrollNewLines()
    {
      var grid = CreateGrid("0 0 0\n3 x 1\n2 x 2\n1 x 3\n", 1);

      var below = grid.Get(0, -1);
      Assert.NotNull(below);
      Assert.False(below.IsActive);

      grid.Update(1f, 0.016f);

      below = grid.Get(0, -1);
      Assert.NotNull(below);
      Assert.False(below.IsActive);
      grid.Update(1f, 0.016f);

      grid.Update(1f, 0.016f);

      // Ensure loop is working
      Assert.NotNull(grid.Get(0, 0));
      Assert.NotNull(grid.Get(1, 0));
      Assert.NotNull(grid.Get(2, 0));
      Assert.False(grid.Get(0, 0).IsEmpty);
      Assert.False(grid.Get(1, 0).IsEmpty);
      Assert.False(grid.Get(2, 0).IsEmpty);
    }

    [Test]
    public void TestComboVerifier()
    {
      // Vertical
      var grid = CreateGrid("2 0 0 0\n2 0 0 0\n2 0 0 0", 0);
      grid.Update(0f, 1f);

      // Check for empty
      Assert.True(grid.Get(0, 0).IsEmpty);
      Assert.True(grid.Get(0, 1).IsEmpty);
      Assert.True(grid.Get(0, 2).IsEmpty);

      // -------------------------------------------------------

      // Horizontal
      grid = CreateGrid("1 1 1 1", 0);
      grid.Update(0f, 1f);

      // Check for empty
      Assert.True(grid.Get(0, 0).IsEmpty);
      Assert.True(grid.Get(1, 0).IsEmpty);
      Assert.True(grid.Get(2, 0).IsEmpty);
      Assert.True(grid.Get(3, 0).IsEmpty);

      // -------------------------------------------------------

      // Garbage = no combo
      grid = CreateGrid("x x x 0 0", 0);
      grid.Update(0f, 1f);

      // Check for empty
      Assert.False(grid.Get(0, 0).IsEmpty);
      Assert.True(grid.Get(0, 0).Definition.isGarbage);
      Assert.False(grid.Get(1, 0).IsEmpty);
      Assert.True(grid.Get(1, 0).Definition.isGarbage);
      Assert.False(grid.Get(2, 0).IsEmpty);
      Assert.True(grid.Get(2, 0).Definition.isGarbage);
    }

    [Test]
    public void TestFall0()
    {
      var grid = CreateGrid("1\n0\n0", 0);

      var falling = grid.Get(0, 2);
      Assert.NotNull(falling);
      Assert.AreEqual(0, falling.x);
      Assert.AreEqual(2, falling.y);
      Assert.AreEqual(2f, falling.position.y);

      grid.Update(0f, 0.42f);

      Assert.AreEqual(0, falling.x);
      Assert.AreEqual(1, falling.y);
      Assert.AreEqual(1.58f, falling.position.y);

      grid.Update(0f, 1f);

      Assert.AreEqual(0, falling.x);
      Assert.AreEqual(0, falling.y);
      Assert.LessOrEqual(falling.position.y - 0.58f, 0.1f);

      Assert.NotNull(grid.Get(0, 1));
      Assert.True(grid.Get(0, 1).IsEmpty);

      grid.Update(0f, 1f);

      Assert.AreEqual(0, falling.x);
      Assert.AreEqual(0, falling.y);
      Assert.AreEqual(0f, falling.position.y);
    }

    [Test]
    public void TestFall1()
    {
      var grid = CreateGrid("1 0 0 0\n2 0 0 0", 0);

      var falling = grid.Get(0, 1);

      // Move block to trigger fall.
      grid.Move(new Vector2(0, 0), 1);

      // Fall is computed on the move. Animation is completely separated.
      Assert.False(grid.Get(0, 1).IsEmpty);
      Assert.True(grid.Get(0, 0).IsEmpty);
      Assert.False(grid.Get(1, 0).IsEmpty);

      // -- Update movements. We need the horizontal move to finish.
      grid.Update(0f, 0.9f);

      Assert.AreEqual(0, falling.x);
      Assert.AreEqual(1, falling.y);
      Assert.AreEqual(MovementType.None, falling.Movement);
      Assert.AreEqual(new Vector2(0f, 1f), falling.position);

      grid.Update(0f, 0.1f);

      // Block falling
      Assert.AreEqual(0, falling.x);
      Assert.AreEqual(0, falling.y);
      Assert.AreEqual(MovementType.Fall, falling.Movement);
      Assert.AreEqual(new Vector2(0f, 0.9f), falling.position);

      // -- Update movements
      grid.Update(0f, 0.45f);

      // There is acceleration in the fall so the block will fall a bit more than 0.45f
      Assert.LessOrEqual(falling.position.y, 0.45f);
      Assert.GreaterOrEqual(falling.position.y, 0.44f);

      grid.Update(0f, 0.55f);

      // Block should be stable now.
      Assert.AreEqual(0, falling.x);
      Assert.AreEqual(0, falling.y);
      Assert.AreEqual(MovementType.None, falling.Movement);
    }

    [Test]
    public void TestFall2()
    {
      var grid = CreateGrid("1 0 0 0\n1 0 0 0\n2 0 0 0\n2 0 0 0", 0);

      var falling1 = grid.Get(0, 3);
      var falling2 = grid.Get(0, 2);
      Assert.NotNull(falling1);
      Assert.NotNull(falling2);
      Assert.False(falling1.IsEmpty);
      Assert.False(falling2.IsEmpty);

      // Move block to trigger fall.
      grid.Move(new Vector2(0, 1), 1);
      grid.Move(new Vector2(0, 0), 1);

      // -- Update movements. 
      grid.Update(0f, 0.8f);

      // Nothing should have moved until swap is complete
      Assert.AreEqual(MovementType.None, falling1.Movement);
      Assert.AreEqual(new Vector2(0f, 3f), falling1.position);
      Assert.AreEqual(MovementType.None, falling2.Movement);
      Assert.AreEqual(new Vector2(0f, 2f), falling2.position);

      // -- Update fall.
      grid.Update(0f, 0.3f);

      Assert.AreEqual(MovementType.Fall, falling1.Movement);
      Assert.AreEqual(new Vector2(0f, 2.7f), falling1.position);
      Assert.AreEqual(MovementType.Fall, falling2.Movement);
      Assert.AreEqual(new Vector2(0f, 1.7f), falling2.position);

      grid.Update(0f, 0.1f);

      Assert.AreEqual(MovementType.Fall, falling1.Movement);
      Assert.LessOrEqual(falling1.position.y - 2.6f, 0.05f); // float + unity = poop
      Assert.AreEqual(MovementType.Fall, falling2.Movement);
      Assert.LessOrEqual(falling2.position.y - 1.6f, 0.05f); // float + unity = poop

      grid.Update(0f, 2f); // Longer fall (2 blocks height)

      // Block should be stable now.
      Assert.AreEqual(0, falling1.x);
      Assert.AreEqual(1, falling1.y);
      Assert.AreEqual(MovementType.None, falling1.Movement);

      Assert.AreEqual(0, falling2.x);
      Assert.AreEqual(0, falling2.y);
      Assert.AreEqual(MovementType.None, falling2.Movement);
    }

    [Test]
    public void TestFall3()
    {
      var grid = CreateGrid("1 0 0 0\n2 0 0 0\n2 0 0 0\n2 0 0 0", 0);

      var falling = grid.Get(0, 3);

      // -- Let combo.&
      grid.Update(0f, 1f);

      // -- Update fall.
      grid.Update(0f, 0.45f);

      Assert.AreEqual(MovementType.Fall, falling.Movement);
      Assert.AreNotEqual(new Vector2(0f, 4f), falling.position);

      grid.Update(0f, 2.65f); // Longer fall (3 blocks height)

      // Block should be stable now.
      Assert.AreEqual(0, falling.x);
      Assert.AreEqual(0, falling.y);
      Assert.AreEqual(MovementType.None, falling.Movement);
    }


    [Test]
    public void TestFallInterruption1()
    {
      var grid = CreateGrid("1 0\n2 0\n2 0\n2 3\n", 0);

      var top1 = grid.Get(0, 3);
      var interrupter = grid.Get(1, 0);

      grid.Update(0f, 1f);
      grid.Update(0f, 0.25f);

      Assert.AreEqual(0, top1.x);
      Assert.AreEqual(2, top1.y);
      Assert.AreEqual(1, interrupter.x);
      Assert.AreEqual(0, interrupter.y);

      // -- Interrupt fall
      grid.Move(new Vector2(1, 0), -1);

      Assert.AreEqual(0, top1.x);
      Assert.AreEqual(2, top1.y);
      Assert.AreEqual(0, interrupter.x);
      Assert.AreEqual(0, interrupter.y);

      // -- Finish fall
      // (Don't use too large elapsedTime, it creates bugs that doesn't exists in game)
      grid.Update(0f, 1f);
      grid.Update(0f, 1f);

      Assert.AreEqual(1, top1.position.y);
      Assert.AreEqual(0, interrupter.y);
    }

    [Test]
    public void TestLineUp()
    {
      var grid = CreateGrid("1 0\n1 0\n2 0\n2 0\n3 3\n", 0);

      var b = grid.Get(0, 0);

      Assert.AreEqual(0f, b.position.y);

      grid.Update(0.42f, 0.16f);

      Assert.AreEqual(0.42f, b.position.y);

      grid.Update(0.42f, 0.16f);

      Assert.AreEqual(2 * 0.42f, b.position.y);

      grid.Update(0.42f, 0.16f);

      Assert.AreEqual(3 * 0.42f, b.position.y);
    }

    [Test]
    public void TestComboCountLShaped()
    {
      // Vertical
      var grid = CreateGrid("2 2 2 0\n2 1 1 0\n2 1 1 0", 0);

      int comboCountComputed = 0;
      int comboCountProvided = 0;

      grid.OnComboDetected += (blocks, n, i) =>
      {
        comboCountComputed += 1;
        comboCountProvided = i;
      };

      grid.Update(0f, 1f);

      // Check for empty
      Assert.True(grid.Get(0, 0).IsEmpty);
      Assert.True(grid.Get(0, 1).IsEmpty);
      Assert.True(grid.Get(0, 2).IsEmpty);
      Assert.True(grid.Get(1, 2).IsEmpty);
      Assert.True(grid.Get(2, 2).IsEmpty);

      Assert.AreEqual(1, comboCountComputed);
      Assert.AreEqual(1, comboCountProvided);
    }

    [Test]
    public void TestComboCountRegular()
    {
      var grid = CreateGrid("2 2 2 0\n3 3 3 0\n4 1 1 0", 0);

      int comboCountComputed = 0;
      int comboCountProvided = 0;

      grid.OnComboDetected += (blocks, n, i) =>
      {
        comboCountComputed += 1;
        comboCountProvided = i;
      };

      grid.Update(0f, 1f);

      Assert.AreEqual(2, comboCountComputed);
      Assert.AreEqual(2, comboCountProvided);

      // Check for empty
      Assert.True(grid.Get(0, 2).IsEmpty);
      Assert.True(grid.Get(1, 2).IsEmpty);
      Assert.True(grid.Get(2, 2).IsEmpty);
      Assert.True(grid.Get(0, 1).IsEmpty);
      Assert.True(grid.Get(1, 1).IsEmpty);
      Assert.True(grid.Get(2, 1).IsEmpty);
    }

    [Test]
    public void TestAirCombo()
    {
      var grid = CreateGrid("1 1 1 0\n3 3 0 2\n2 2 0 3", 0);

      int comboCountComputed = 0;

      grid.OnComboDetected += (blocks, n, i) => { comboCountComputed += 1; };

      grid.Update(0f, 20f);

      Assert.AreEqual(0, comboCountComputed);

      Assert.True(grid.Get(2, 2).IsEmpty);
      Assert.False(grid.Get(2, 0).IsEmpty);
    }
  }
}