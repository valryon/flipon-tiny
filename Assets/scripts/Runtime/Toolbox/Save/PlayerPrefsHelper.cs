// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using UnityEngine;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Pon
{
  public static class PlayerPrefsHelper
  {
    /// <summary>
    /// Save a serialized object in player pref
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="keyName"></param>
    /// <param name="obj"></param>
    public static void Save(string keyName, object obj)
    {
      var b = new BinaryFormatter();
      var m = new MemoryStream();

      b.Serialize(m, obj);

      PlayerPrefs.SetString(keyName, System.Convert.ToBase64String(m.GetBuffer()));
    }

    /// <summary>
    /// Load and deserialized an object stored in player prefs
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="keyName"></param>
    /// <returns></returns>
    public static T Load<T>(string keyName)
      where T : new()
    {
      T t = default(T);

      // Load local file
      var data = PlayerPrefs.GetString(keyName);

      if (string.IsNullOrEmpty(data) == false)
      {
        var b = new BinaryFormatter();
        var m = new MemoryStream(System.Convert.FromBase64String(data));

        t = (T) b.Deserialize(m);
      }

      return t;
    }
  }
}