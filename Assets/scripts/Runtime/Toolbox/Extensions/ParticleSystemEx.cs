using UnityEngine;

namespace Pon
{
  public static class ParticleSystemEx
  {
    public static void SetGradientBySpeed(this ParticleSystem self, Color a, Color b)
    {
      var module = self.colorBySpeed;
      var gradient = new ParticleSystem.MinMaxGradient(a, b);
      module.color = gradient;
    }

    public static void SetGradientByLifetime(this ParticleSystem self, Color a, Color b)
    {
      var module = self.colorOverLifetime;
      var gradient = new ParticleSystem.MinMaxGradient(a, b);
      module.color = gradient;
    }

    public static void SetSpriteLayer(this ParticleSystem self, string name, int order)
    {
      self.GetComponentInChildren<Renderer>().sortingLayerName = name;
      self.GetComponentInChildren<Renderer>().sortingOrder = order;
    }
  }
}