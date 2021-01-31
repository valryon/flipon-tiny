// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using UnityEngine;

namespace Pon
{
  /// <summary>
  /// The game initializer.
  /// </summary>
  public class PonInitializerScript : MonoBehaviour
  {
    private static PonInitializerScript Instance;

    #region Timeline

    static PonInitializerScript()
    {
      UnityLog.Init();
    }

    void Awake()
    {
      if (Instance != null)
      {
        Destroy(gameObject);
        return;
      }

      Instance = this;

      SetupQualitySettings();

      DontDestroyOnLoad(gameObject);
    }

    #endregion

    #region Private methods

    private void SetupQualitySettings()
    {
      Application.targetFrameRate = 60;
      QualitySettings.vSyncCount = 1;
    }
  }

  #endregion
}