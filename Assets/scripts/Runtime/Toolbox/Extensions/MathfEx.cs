using UnityEngine;

namespace Pon
{
  public static class MathfEx
  {
    public static float LerpWithoutClamp(float a, float b, float t)
    {
      return a + (b - a) * t;
    }

    public static Vector3 LerpWithoutClamp(Vector3 a, Vector3 b, float t)
    {
      return a + (b - a) * t;
    }
  }
}