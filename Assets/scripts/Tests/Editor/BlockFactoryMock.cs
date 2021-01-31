using UnityEngine;
using Pon;
using System.Linq;
using System.Collections.Generic;

// ReSharper disable Unity.IncorrectScriptableObjectInstantiation

public class BlockFactoryMock : IBlockFactory
{
  private List<BlockDefinition> testsDefs = new List<BlockDefinition>()
  {
    CreateDefinition(id: 1, color: Color.red, name: "red"),
    CreateDefinition(id: 2, color: Color.yellow, name: "yellow"),
    CreateDefinition(id: 3, color: Color.blue, name: "blue"),
    CreateDefinition(id: 4, color: Color.green, name: "green"),
    CreateDefinition(id: 5, color: Color.Lerp(Color.red, Color.white, 0.5f), name: "pink"),
    CreateDefinition(id: 99, color: Color.black, name: "garbage", true),
  };

  private static BlockDefinition CreateDefinition(sbyte id, Color color, string name, bool garbage = false)
  {
    var d = garbage
      ? ScriptableObject.CreateInstance<GarbageBlockDefinition>()
      : ScriptableObject.CreateInstance<BlockDefinition>();
    d.id = id;
    d.color = color;
    d.name = name;
    return d;
  }

  public List<BlockDefinition> GetNormalBlocks(int level = 99, int width = 0)
  {
    return testsDefs.Where(b => b.isGarbage == false).ToList();
  }

  public BlockDefinition GetRandomNormal(int level, int width, GameRandom gameRandom = null,
    List<BlockDefinition> excludedDef = null)
  {
    var n = GetNormalBlocks();

    if (excludedDef != null && excludedDef.Count > 0)
    {
      n.RemoveAll(b => excludedDef.Contains(b));
    }

    return n[Random.Range(0, n.Count)];
  }

  public List<BlockDefinition> GetGarbages(int level = 99)
  {
    return testsDefs.Where(b => b.isGarbage).ToList();
  }

  public BlockDefinition GetRandomGarbage(int level = 99)
  {
    var s = GetGarbages();
    return s[Random.Range(0, s.Count)];
  }
}