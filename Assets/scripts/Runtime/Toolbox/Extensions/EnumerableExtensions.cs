using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pon
{
  public static class EnumerableExtensions
  {
    public static T PickRandom<T>(this IEnumerable<T> collection)
    {
      T[] items = collection.ToArray();

      if (items.Length == 0) return default;
      return items[Random.Range(0, items.Length)];
    }

    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> collection)
    {
      return collection.OrderBy(m => Random.Range(0f, 1f));
    }
  }
}