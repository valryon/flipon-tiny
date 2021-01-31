using NUnit.Framework;

namespace Pon.Tests
{
  public class GarbageTests
  {
    const float TIME_STEP = 60;

    [Test]
    public void GarbageFall0()
    {
      var grid = GridTests.CreateGrid("0 x x 0\n0 0 0 0\n0 0 0 0\n0 0 0 0\n0 0 0 0", 0);
      grid.Print();

      Assert.True(grid.Get(1, 4).Definition.isGarbage);
      Assert.True(grid.Get(2, 4).Definition.isGarbage);

      for (int i = 0; i < 4 * TIME_STEP; i++)
      {
        grid.Update(0f, 1f / TIME_STEP);
      }

      grid.Print();

      Assert.True(grid.Get(1, 0).Definition.isGarbage);
      Assert.True(grid.Get(2, 0).Definition.isGarbage);
    }

    [Test]
    public void GarbageFall1()
    {
      var grid = GridTests.CreateGrid("x x x x\n0 x 0 0\n0 0 0 0\n0 0 0 0\n0 0 0 0", 0);
      grid.Print();

      Assert.NotNull(grid.Get(0, 4));
      Assert.NotNull(grid.Get(0, 3));
      Assert.NotNull(grid.Get(1, 3));

      Assert.False(grid.Get(0, 4).IsEmpty);
      Assert.True(grid.Get(0, 3).IsEmpty);
      Assert.False(grid.Get(1, 3).IsEmpty);

      for (int i = 0; i < 4 * TIME_STEP; i++)
      {
        grid.Update(0f, 1f / TIME_STEP);
      }

      grid.Print();

      Assert.NotNull(grid.Get(0, 1));
      Assert.NotNull(grid.Get(0, 0));
      Assert.NotNull(grid.Get(1, 0));

      Assert.False(grid.Get(0, 1).IsEmpty);
      Assert.True(grid.Get(0, 0).IsEmpty);
      Assert.False(grid.Get(1, 0).IsEmpty);
    }

    [Test]
    public void GarbageFall2()
    {
      var grid = GridTests.CreateGrid("0 0 0 0\nx x x x\n0 x x 0\n0 x 0 0\n0 x x 0\n0 0 0 0\n0 0 0 0", 0);
      grid.Print();

      for (int i = 0; i < 6 * TIME_STEP; i++)
      {
        grid.Update(0f, 1f / TIME_STEP);
      }

      grid.Print();

      Assert.False(grid.Get(0, 3).IsEmpty);
      Assert.False(grid.Get(1, 3).IsEmpty);
      Assert.False(grid.Get(2, 3).IsEmpty);
      Assert.False(grid.Get(3, 3).IsEmpty);
      Assert.True(grid.Get(0, 2).IsEmpty);
      Assert.False(grid.Get(1, 2).IsEmpty);
      Assert.False(grid.Get(2, 2).IsEmpty);
      Assert.True(grid.Get(0, 2).IsEmpty);
      Assert.True(grid.Get(0, 1).IsEmpty);
      Assert.False(grid.Get(1, 1).IsEmpty);
      Assert.True(grid.Get(2, 1).IsEmpty);
      Assert.True(grid.Get(3, 1).IsEmpty);
      Assert.True(grid.Get(0, 0).IsEmpty);
      Assert.False(grid.Get(1, 0).IsEmpty);
      Assert.False(grid.Get(2, 0).IsEmpty);
      Assert.True(grid.Get(3, 0).IsEmpty);
    }

    [Test]
    public void GarbageFall3()
    {
      var grid = GridTests.CreateGrid("0 0 0 0\nx x x x\n0 1 0 0\n0 x x 0\n0 0 2 0\n0 0 0 0\n0 0 0 0", 0);
      grid.Print();

      for (int i = 0; i < 6 * TIME_STEP; i++)
      {
        grid.Update(0f, 1f / TIME_STEP);
      }

      grid.Print();

      Assert.False(grid.Get(0, 3).IsEmpty);
      Assert.False(grid.Get(1, 3).IsEmpty);
      Assert.True(grid.Get(1, 3).Definition.isGarbage);
      Assert.False(grid.Get(2, 3).IsEmpty);
      Assert.False(grid.Get(3, 3).IsEmpty);

      Assert.True(grid.Get(0, 2).IsEmpty);
      Assert.False(grid.Get(1, 2).IsEmpty);
      Assert.False(grid.Get(1, 2).Definition.isGarbage);
      Assert.True(grid.Get(2, 2).IsEmpty);
      Assert.True(grid.Get(3, 2).IsEmpty);

      Assert.True(grid.Get(0, 1).IsEmpty);
      Assert.False(grid.Get(1, 1).IsEmpty);
      Assert.False(grid.Get(2, 1).IsEmpty);
      Assert.True(grid.Get(2, 1).Definition.isGarbage);
      Assert.True(grid.Get(3, 1).IsEmpty);

      Assert.True(grid.Get(0, 0).IsEmpty);
      Assert.True(grid.Get(1, 0).IsEmpty);
      Assert.False(grid.Get(2, 0).IsEmpty);
      Assert.False(grid.Get(2, 0).Definition.isGarbage);
      Assert.True(grid.Get(3, 0).IsEmpty);
    }

    [Test]
    public void GarbageFall4()
    {
      var grid = GridTests.CreateGrid("0 0 0\nx x 0\n0 5 0\n0 2 0\n0 1 0\n0 1 0\n1 4 0\n1 3 0\n4 4 0", 0);
      grid.Print();

      for (int i = 0; i < 4 * TIME_STEP; i++)
      {
        grid.Update(0f, 1f / TIME_STEP);
      }

      grid.Print();

      System.Action<int> check = (y) =>
      {
        // GridTests.Print(grid.ToIntArray());
        Assert.False(grid.Get(0, y).IsEmpty, "0," + y + " empty");
        Assert.True(grid.Get(0, y).Definition.isGarbage, "0," + y + " not garbage");
        Assert.False(grid.Get(1, y).IsEmpty, "1," + y + " empty");
        Assert.True(grid.Get(1, y).Definition.isGarbage, "1," + y + " not garbage");
        Assert.False(grid.Get(1, y - 1).IsEmpty, "0," + (y - 1) + " empty");
        Assert.False(grid.Get(1, y - 1).Definition.isGarbage, "0," + (y - 1) + " garbage");
      };

      check(7);

      grid.Move(grid.Get(1, 2), 1);

      for (int i = 0; i < 4 * TIME_STEP; i++)
      {
        grid.Update(0f, 1f / TIME_STEP);
      }

      check(5);

      grid.Move(grid.Get(1, 3), 1);

      for (int i = 0; i < 4 * TIME_STEP; i++)
      {
        grid.Update(0f, 1f / TIME_STEP);
      }

      check(4);

      grid.Move(grid.Get(1, 2), 1);

      for (int i = 0; i < 4 * TIME_STEP; i++)
      {
        grid.Update(0f, 1f / TIME_STEP);
      }

      check(2);

      grid.Move(grid.Get(1, 1), 1);

      for (int i = 0; i < 4 * TIME_STEP; i++)
      {
        grid.Update(0f, 1f / TIME_STEP);
      }

      check(1);
    }

    [Test]
    public void GarbageFall5()
    {
      var grid = GridTests.CreateGrid("x x x 0\n0 4 0 0\n1 1 0 1\n2 2 0 2\n3 3 0 3\n1 1 0 1\n2 2 0 2\n3 3 0 3\n1 1 0 1",
        0);
      grid.Print();

      for (int n = 0; n <= 7; n++)
      {
        // GridTests.Print(grid.ToIntArray());
        grid.Move(grid.Get(3, 0), -1);

        for (int i = 0; i < 2 * TIME_STEP; i++)
        {
          grid.Update(0f, 1f / TIME_STEP);
        }
      }

      grid.Print();

      Assert.True(grid.Get(0, 0).IsEmpty);
      Assert.False(grid.Get(1, 0).IsEmpty);
      Assert.False(grid.Get(1, 0).Definition.isGarbage);
      Assert.True(grid.Get(2, 0).IsEmpty);

      Assert.True(grid.Get(0, 1).Definition.isGarbage);
      Assert.True(grid.Get(1, 1).Definition.isGarbage);
      Assert.True(grid.Get(2, 1).Definition.isGarbage);
    }

    [Test]
    public void GarbageFall6()
    {
      var grid = GridTests.CreateGrid("x x x 0\n0 4 0 0\n1 1 0 1\n2 2 0 2\n3 3 0 3\n1 1 0 1\n2 2 0 2\n3 3 0 3\n1 1 0 1",
        0);
      grid.Print();

      for (int n = 0; n <= 7; n++)
      {
        grid.Move(grid.Get(3, n), -1);
      }

      for (int i = 0; i < 4 * TIME_STEP; i++)
      {
        grid.Update(0f, 1f / TIME_STEP);
      }

      grid.Print();

      Assert.True(grid.Get(0, 0).IsEmpty);
      Assert.False(grid.Get(1, 0).IsEmpty);
      Assert.False(grid.Get(1, 0).Definition.isGarbage);
      Assert.True(grid.Get(2, 0).IsEmpty);

      Assert.True(grid.Get(0, 1).Definition.isGarbage);
      Assert.True(grid.Get(1, 1).Definition.isGarbage);
      Assert.True(grid.Get(2, 1).Definition.isGarbage);
    }

    [Test]
    public void GarbageFall7()
    {
      var grid = GridTests.CreateGrid(
        "x x x x 0 0\n0 0 0 0 0 0\n0 0 0 0 0 0\n0 0 1 0 0 0\n0 0 4 0 0 0\n0 0 4 0 0\n1 2 2 1 0 0", 0);
      grid.Print();

      for (int i = 0; i < TIME_STEP; i++)
      {
        grid.Update(1f / TIME_STEP, 1f / TIME_STEP);
        // GridTests.Print(grid.ToIntArray());

        grid.Print();

        // Assert all garbages are ALWAYS aligned
        for (int y = 0; y < grid.height; y++)
        {
          var b = grid.Get(0, y);
          if (b != null && b.IsEmpty == false && b.Definition.isGarbage)
          {
            var debugMessage = "step " + i + " failed: ";

            var right = b.Right;
            while (right != null)
            {
              Assert.AreEqual(b.position.y, right.position.y,
                debugMessage + "POS= " + b.position.y + " != " + right.position.y);
              Assert.AreEqual(b.y, right.y, debugMessage + "Y= " + b.y + " != " + right.y);

              right = right.Right;
            }
          }
        }
      }
    }
  }
}