using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Pon.Tests
{
  public class AIAdvancedTests
  {
    [Test]
    public void TestSingleMove()
    {
      // Create a simple grid where only 1 move is required to combo
      // Grid should be empty after that
      CreateGridToEmptyTest("0 0 0 0\n0 0 0 0\n1 1 0 1");
    }

    [Test]
    public void TestSingleMoveFall()
    {
      CreateGridToEmptyTest("0 0 0 0\n0 1 0 0\n1 1 0 0");
    }

    [Test]
    public void TestSingleMoveFall2()
    {
      CreateGridToEmptyTest("0 0 0 0\n0 1 0 0\n0 1 0 1");
    }

    [Test]
    public void TestDoubleMove()
    {
      var solver = SolveTestGrid("0 0 0 0\n1 0 0 0\n2 1 0 1", 2);
      Assert.AreEqual(2, solver.Moves.Count);
      Assert.AreEqual(1, solver.Moves.Last().comboCount);
    }

    [Test]
    public void TestDoubleMove2()
    {
      var solver = SolveTestGrid("2 1 1\n" +
                                 "1 2 1\n" +
                                 "3 3 2", 2);
      Assert.AreEqual(2, solver.Moves.Count);
      Assert.AreEqual(1, solver.Moves.Last().comboCount);
    }


    [Test]
    public void TestMinLines()
    {
      var solver = SolveTestGrid("0 0 0 0\n0 0 0 0\n1 0 0 0\n2 1 0 1", 1, 3);
      Assert.IsTrue(solver.SpeedUp);
      Assert.AreEqual(0, solver.Moves.Count);
    }

    [Test]
    public void TestTripleMove()
    {
      var solver = SolveTestGrid("0 1 0 0 0\n2 3 0 0 0\n1 2 0 0 1", 3);
      Assert.AreEqual(3, solver.Moves.Count);
      Assert.AreEqual(1, solver.Moves.Last().comboCount);
    }

    [Test]
    public void TestFirstColumn()
    {
      var gs = "1 0 0\n" +
               "2 0 0\n" +
               "2 0 0\n" +
               "1 0 1";


      var grid = CreateGridTest(gs, 1, 0);

      for (int x = 0; x < grid.width; x++)
      {
        for (int y = 0; y < grid.height; y++)
        {
          Assert.AreNotEqual(grid.Get(x, y).ToInt(), 1, "Cell [" + x + "," + y + "] is 1.");
        }
      }
    }

    [Test]
    public void TestConsistency()
    {
      var gs = "1 0 0\n" +
               "2 0 3\n" +
               "2 0 3\n" +
               "3 0 2\n" +
               "3 0 2\n" +
               "1 0 1";

      var grid = GridTests.CreateGrid(gs, 0);
      Assert.NotNull(grid);
      grid.Print();

      var solver = new AISolver(grid,
        new AISettings() {maxDepth = 1, pickNotTheBestProbability = 0f, minLinesBeforeSpeed = 0});
      solver.Work();

      Assert.AreNotEqual(0, solver.Moves.Count, "Solver found no moves");

      foreach (var m in solver.Moves)
      {
        Debug.Log("Move " + m);

        grid.Move(grid.Get(m.x, m.y), m.direction);

        for (var i = 0f; i < 10f; i += 0.05f)
        {
          grid.Update(0f, 0.05f);
        }

        var g1 = Grid.ToString(grid.ToIntArray());
        var g2 = Grid.ToString(m.gridAfterMove);

        if (g1.StartsWith("0 0 0 \n")) g1 = g1.Replace("0 0 0 \n", "");
        if (g1.EndsWith("0 0 0 \n")) g1 = g1.Replace("0 0 0 \n", "");
        if (g2.StartsWith("0 0 0 \n")) g2 = g2.Replace("0 0 0 \n", "");
        if (g2.EndsWith("0 0 0 \n")) g1 = g1.Replace("0 0 0 \n", "");

        Debug.Log("COMPARE\n" + g1 + "VS\n" + g2);

        Assert.AreEqual(g1, g2, "Grid after moves are different!");
      }
    }

    [Test]
    public void TestFirstColumn2()
    {
      var gs = "1 0 0\n" +
               "2 0 3\n" +
               "2 0 3\n" +
               "3 0 2\n" +
               "3 0 2\n" +
               "1 0 1";

      var grid = CreateGridTest(gs, 1, 0);

      for (int x = 0; x < grid.width; x++)
      {
        for (int y = 0; y < grid.height; y++)
        {
          Assert.AreNotEqual(grid.Get(x, y).ToInt(), 1, "Cell [" + x + "," + y + "] is 1.");
        }
      }
    }

    private AISolver SolveTestGrid(string gridString, int depth, int minLines = 0)
    {
      var grid = GridTests.CreateGrid(gridString, 0);
      Assert.NotNull(grid);

      var solver = new AISolver(grid,
        new AISettings() {maxDepth = depth, pickNotTheBestProbability = 0f, minLinesBeforeSpeed = minLines});
      solver.Work();
      return solver;
    }

    private Grid CreateGridTest(string gridString, int depth, int minLines)
    {
      var grid = GridTests.CreateGrid(gridString, 0);
      Assert.NotNull(grid);
      grid.Print();

      var solver = new AISolver(grid,
        new AISettings() {maxDepth = depth, pickNotTheBestProbability = 0f, minLinesBeforeSpeed = minLines});
      solver.Work();

      Assert.AreNotEqual(0, solver.Moves.Count, "Solver found no moves");

      foreach (var m in solver.Moves)
      {
        grid.Move(grid.Get(m.x, m.y), m.direction);
      }

      for (var i = 0f; i < 10f; i += 0.05f)
      {
        grid.Update(0f, 0.05f);
      }

      grid.Print();

      return grid;
    }

    private void CreateGridToEmptyTest(string gridString, int depth = 1, int minLines = 0)
    {
      var grid = CreateGridTest(gridString, depth, minLines);

      for (int x = 0; x < grid.width; x++)
      {
        for (int y = 0; y < grid.height; y++)
        {
          Assert.True(grid.Get(x, y).IsEmpty, "Cell [" + x + "," + y + "] not empty.");
        }
      }
    }
  }
}