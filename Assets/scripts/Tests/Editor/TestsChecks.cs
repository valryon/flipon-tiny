using UnityEngine;
using NUnit.Framework;
using System.Linq;

namespace Pon.Tests
{
  // ReSharper disable once InconsistentNaming
  public class _TestsChecks
  {
    [Test]
    public void TestTestMapCreation3()
    {
      var gridString = "1 2 3 x\n4 5 1 3";
      var grid = GridTests.CreateGrid(gridString, 0);
      Assert.AreEqual(gridString,
        Grid.ToString(grid.ToIntArray(), true));
    }

    [Test]
    public void TestTestMapCreation2()
    {
      var grid = GridTests.CreateGrid("0 0 1 0\n1 1 0 0", 0);
      Assert.AreEqual(4, grid.width);
      Assert.AreEqual(2, grid.height);

      grid = GridTests.CreateGrid("1 0\n1 1\n0 0", 0);
      Assert.AreEqual(2, grid.width);
      Assert.AreEqual(3, grid.height);
    }

    [Test]
    public void TestTestMapCreation()
    {
      var grid = GridTests.CreateGrid("0 0 1 2\n1 2 2 3\n3 1 2 3", 0);
      Assert.NotNull(grid);

      Assert.AreEqual(4, grid.width);
      Assert.AreEqual(3, grid.height);

      Assert.AreEqual(12, grid.Map.Count);
      Assert.AreEqual(10, grid.Map.Values.Count(b => b.IsEmpty == false));

      // Check first line to be sure Y coordinates are from bot to top.
      Assert.True(grid.Get(0, 2).IsEmpty);
      Assert.True(grid.Get(1, 2).IsEmpty);
      Assert.False(grid.Get(2, 2).IsEmpty);
      Assert.False(grid.Get(3, 2).IsEmpty);

      // Check x,y and position
      Assert.AreEqual(1, grid.Get(1, 2).x);
      Assert.AreEqual(2, grid.Get(1, 2).y);
      Assert.AreEqual(new Vector2(1, 2), grid.Get(1, 2).position);
    }
  }
}