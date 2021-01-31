using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pon
{
  [Serializable]
  public struct PowerData
  {
    public PowerType type;
    public string name;
    public string debugName;
    public Sprite icon;
    public GameObject activationFX;
    public GameObject activationOpponentFX;
  }

  public class PowersData : MonoBehaviour
  {
    public static PowersData Instance;

    [Header("Powers")]
    public List<PowerData> PowerData;

    private void Awake()
    {
      Instance = this;
    }

    public PowerData Get(PowerType power)
    {
      if (power == PowerType.None) return default;
      return PowerData.First(p => p.type == power);
    }

    public Sprite GetIcon(PowerType power)
    {
      return Get(power).icon;
    }

    private void Update()
    {
      foreach (PowerType power in Enum.GetValues(typeof(PowerType)))
      {
        if (power == PowerType.None) continue;
        ;
        if (PowerData.Any(p => p.type == power) == false)
        {
          PowerData.Add(new PowerData()
          {
            type = power,
            name = power.ToString(),
            debugName = power.ToString()
          });
        }
      }
    }
  }
}