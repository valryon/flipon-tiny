using System;
using UnityEngine;
using GoogleMobileAds.Api;
using System.Collections.Generic;

public class Banner : MonoBehaviour
{
  private BannerView _bannerView;

  void Start()
  {
    // Set your test devices.
    // https://developers.google.com/admob/unity/test-ads
    RequestConfiguration requestConfiguration = new RequestConfiguration
    {
      TestDeviceIds = new List<string>
      {
        AdRequest.TestDeviceSimulator,
        // Add your test device IDs (replace with your own device IDs).
        #if UNITY_IPHONE
        "96e23e80653bb28980d3f40beb58915c"
        #elif UNITY_ANDROID
        "75EF8D155528C04DACBBA6F36F433035"
        #endif
      }
    };
    MobileAds.SetRequestConfiguration(requestConfiguration);

    // Initialize the Google Mobile Ads SDK.
    MobileAds.Initialize((InitializationStatus status) =>
    {
      RequestBanner();
    });
  }

  private void RequestBanner()
  {
    // These ad units are configured to always serve test ads.
#if UNITY_EDITOR
    string adUnitId = "unused";
#elif UNITY_ANDROID
      string adUnitId = "ca-app-pub-4133264752903581~5788723812";
#elif UNITY_IPHONE
      string adUnitId = "ca-app-pub-4133264752903581/3796985290";
#else
      string adUnitId = "unexpected_platform";
#endif

    // Clean up banner ad before creating a new one.
    if (_bannerView != null)
    {
      _bannerView.Destroy();
    }

    AdSize adaptiveSize =
        AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);

    _bannerView = new BannerView(adUnitId, adaptiveSize, AdPosition.Bottom);
    ListenToAdEvents();
    // Register for ad events.
    _bannerView.OnBannerAdLoaded += OnBannerAdLoaded;
    _bannerView.OnBannerAdLoadFailed += OnBannerAdLoadFailed;

    AdRequest adRequest = new AdRequest();

    // Load a banner ad.
    _bannerView.LoadAd(adRequest);
  }

  #region Banner callback handlers
  private void ListenToAdEvents()
  {
    // Raised when an ad is loaded into the banner view.
    _bannerView.OnBannerAdLoaded += () =>
    {
      Debug.Log($"Banner view loaded an ad with response : {_bannerView.GetResponseInfo()}");
      Debug.Log($"Ad Height: { _bannerView.GetHeightInPixels()}, width: {_bannerView.GetWidthInPixels()}");
    };
    // Raised when an ad fails to load into the banner view.
    _bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
    {
      Debug.LogError("Banner view failed to load an ad with error : "
        + error);
    };
    // Raised when the ad is estimated to have earned money.
    _bannerView.OnAdPaid += (AdValue adValue) =>
    {
      Debug.Log(String.Format("Banner view paid {0} {1}.",
        adValue.Value,
        adValue.CurrencyCode));
    };
    // Raised when an impression is recorded for an ad.
    _bannerView.OnAdImpressionRecorded += () =>
    {
      Debug.Log("Banner view recorded an impression.");
    };
    // Raised when a click is recorded for an ad.
    _bannerView.OnAdClicked += () =>
    {
      Debug.Log("Banner view was clicked.");
    };
    // Raised when an ad opened full screen content.
    _bannerView.OnAdFullScreenContentOpened += () =>
    {
      Debug.Log("Banner view full screen content opened.");
    };
    // Raised when the ad closed full screen content.
    _bannerView.OnAdFullScreenContentClosed += () =>
    {
      Debug.Log("Banner view full screen content closed.");
    };
  }
  private void OnBannerAdLoaded()
  {
    Debug.Log("Banner view loaded an ad with response : " + _bannerView.GetResponseInfo());
    Debug.Log($"Ad Height: {_bannerView.GetHeightInPixels()}, width: {_bannerView.GetWidthInPixels()}");
  }


  private void OnBannerAdLoadFailed(LoadAdError error)
  {
    Debug.LogError("Banner view failed to load an ad with error : "
        + error);
  }

  #endregion
}