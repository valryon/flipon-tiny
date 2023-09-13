#if UNITY_ANDROID
using System;
using UnityEditor;
using UnityEditor.Callbacks;

using GoogleMobileAds.Editor;

public static class AndroidBuildPostProcessor
{

  [PostProcessBuild]
  public static void OnPostProcessBuild(BuildTarget buildTarget, string path)
  {
    if (GoogleMobileAdsSettings.LoadInstance().GoogleMobileAdsAndroidAppId.Length == 0)
    {
      NotifyBuildFailure(
        "Android Google Mobile Ads app ID is empty. Please enter a valid app ID to run ads properly.");
    }
  }

  private static void NotifyBuildFailure(string message)
  {
    string prefix = "[GoogleMobileAds] ";

    bool openSettings = EditorUtility.DisplayDialog(
      "Google Mobile Ads", "Error: " + message, "Open Settings", "Close");
    if (openSettings)
    {
      GoogleMobileAdsSettingsEditor.OpenInspector();
    }
#if UNITY_2017_1_OR_NEWER
    throw new BuildPlayerWindow.BuildMethodException(prefix + message);
#else
    throw new OperationCanceledException(prefix + message);
#endif
  }
}

#endif
