// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using UnityEngine;

namespace Pon
{
  [System.Serializable]
  public struct ObjectiveBankItem
  {
    public string name;
    public Sprite sprite;
  }

  public class ObjectiveData : MonoBehaviour
  {
    private static ObjectiveData Instance;

    public ObjectiveBankItem[] icons;


    private void Awake()
    {
      if (Instance == null)
      {
        Instance = this;
      }
    }

    public static Sprite GetIcon(string name)
    {
      if (Instance != null)
      {
        for (int i = 0; i < Instance.icons.Length; i++)
        {
          if (Instance.icons[i].name == name) return Instance.icons[i].sprite;
        }
      }

      Log.Error("Missing Objective icon " + name);
      return null;
    }
  }
}