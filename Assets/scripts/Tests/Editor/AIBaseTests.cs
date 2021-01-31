using System.Linq;
using NUnit.Framework;

namespace Pon.Tests
{
  public class AIBaseTests
  {
    /// <summary>
    /// Test Heuristic.WeightMove
    /// </summary>
    private void TestComboDetection(string gridString, int combosCount)
    {
      var grid = GridTests.CreateGrid(gridString, 0);
      var move = new AIMove()
      {
        gridAfterMove = grid.ToIntArray()
      };
      var h = new AIHeuristic();
      var w = h.WeightMove(move, grid.width, grid.height, 0);

      Assert.AreEqual(combosCount, w.combosCount);
      if (combosCount > 0)
      {
        Assert.AreNotEqual(0, w.total);
      }
    }

    [Test]
    public void TestComboDetection()
    {
      TestComboDetection("1 1 1 0", 1);
      TestComboDetection("2 2 2 0\n1 1 1 0", 2);
      TestComboDetection("1 0 0 0\n1 0 0 0\n1 0 1 0", 1);
      TestComboDetection("1 0 0 0\n1 0 0 0\n1 1 1 0", 2);
      TestComboDetection("1 2 0 0\n1 2 0 0\n1 2 0 0", 2);
      TestComboDetection("1 0 0 0\n1 0 0 0\n2 1 1 0", 0);
      TestComboDetection("1 2 3 0\n1 2 3 0\n1 2 3 0", 3);
    }

    private void TestGetHighestY(string gridString, int expectedY)
    {
      var grid = GridTests.CreateGrid(gridString, 0).ToIntArray();
      var h = new AIHeuristic();
      Assert.AreEqual(expectedY, h.GetHighest(grid).y);
    }

    private void TestApplyMove(string input, AIMove move, string output)
    {
      var grid = GridTests.CreateGrid(input, 0);
      var solver = new AISolver(grid, new AISettings());
      var grid2 = solver.GetGridWithMove(grid.ToIntArray(), move);

      Assert.AreEqual(output.ToLower().Trim(),
        Grid.ToString(grid2, true));
    }

    [Test]
    public void TestApplyMove()
    {
      TestApplyMove("0 2 0 0\n" +
                    "0 1 0 0\n" +
                    "0 1 0 0\n" +
                    "2 1 0 2",
        new AIMove() {x = 1, y = 3, direction = -1},
        "0 0 0 0\n" +
        "0 1 0 0\n" +
        "2 1 0 0\n" +
        "2 1 0 2"
      );

      // We're not supposed to test stupid moves here.
      // Stupid moves are filtered before.
      TestApplyMove("1 1 1 0", new AIMove() {x = 2, y = 0, direction = 1}, "1 1 0 1");
      TestApplyMove("1 1 1 0", new AIMove() {x = 2, y = 0, direction = -1}, "1 1 1 0");
      TestApplyMove("1 0 0 0\n1 0 0 0", new AIMove() {x = 0, y = 0, direction = 1}, "0 0 0 0\n1 1 0 0");


      TestApplyMove("0 2 0 0\n" +
                    "0 1 0 0\n" +
                    "0 1 0 0\n" +
                    "2 1 0 2",
        new AIMove() {x = 1, y = 3, direction = 1},
        "0 0 0 0\n" +
        "0 1 0 0\n" +
        "0 1 0 0\n" +
        "2 1 2 2"
      );
    }

    [Test]
    public void TestInternalFunctions()
    {
      TestGetHighestY("1 1 1 0", 0);
      TestGetHighestY("2 0 2 0\n1 1 1 0", 1);
      TestGetHighestY("2 0 0 0\n1 0 0 0\n1 0 0 0\n2 0 0 0", 3);
      TestGetHighestY("0 0 0 0\n2 0 2 0\n1 1 1 0", 1);

      var h = new AIHeuristic();
      Assert.IsTrue(h.IsSameCombo(1, 1));
      Assert.IsFalse(h.IsSameCombo(0, 0));
      Assert.IsFalse(h.IsSameCombo(1, 2));
      Assert.IsFalse(h.IsSameCombo(0, 1));
      Assert.IsFalse(h.IsSameCombo(99, 99));
    }

    private void TestGetMoves(string gridString, int expectedMoves)
    {
      var grid = GridTests.CreateGrid(gridString, 0);
      var solver = new AISolver(grid, new AISettings() {maxDepth = 1, pickNotTheBestProbability = 0f});
      var moves = solver.GetMoves(0, grid.ToIntArray(), 0, grid.width, null);

      Assert.AreEqual(expectedMoves, moves.Count);
    }

    [Test]
    public void TestGetMoves()
    {
      TestGetMoves("0 0 0 0", 0);
      TestGetMoves("x x x 0", 0);
      TestGetMoves("0 x x 0", 0);
      TestGetMoves("0 x x 1", 0);
      TestGetMoves("1 0 0 0", 1);
      TestGetMoves("1 0 0 0", 1);
      TestGetMoves("0 1 0 0", 2);
      TestGetMoves("0 0 1 0", 2);
      TestGetMoves("0 0 0 1", 1);
      TestGetMoves("1 0 0 1", 2);
      TestGetMoves("0 1 0 1", 3);
      TestGetMoves("1 1 0 1", 3);
      TestGetMoves("1 0 1 1", 3);
      TestGetMoves("1 1 1 1", 3);
      TestGetMoves("2 0 2 0\n1 0 1 1", 6);
      TestGetMoves("2 0 0 0\n1 0 0 0", 2);
      TestGetMoves("2 0 0 2\n1 0 0 1", 4);
    }

    private void TestWeightMoves(string gridString, bool hasWeight)
    {
      var grid = GridTests.CreateGrid(gridString, 0);
      var solver = new AISolver(grid, new AISettings() {maxDepth = 1, pickNotTheBestProbability = 0f});
      var moves = solver.GetWeightedMoves(0, 0, grid.ToIntArray(), 0, grid.width, null);

      if (hasWeight)
      {
        Assert.AreNotEqual(0, moves.Count);
        Assert.True(moves.Any(m => m.weight > 0));
      }
      else
      {
        Assert.False(moves.Any(m => m.weight > 0));
      }
    }

    [Test]
    public void TestWeightMoves()
    {
      TestWeightMoves("1 0 1 1", true);
      TestWeightMoves("0 0 0 0", false);
      TestWeightMoves("0 1 0 0", false);
      TestWeightMoves("0 0 x 2", false);
      TestWeightMoves("2 0 0 0\n1 0 0 0", false);
    }
  }
}