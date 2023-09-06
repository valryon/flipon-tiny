using GoogleMobileAds.Api;
using Microsoft.SqlServer.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
namespace AdRewards
{
	public class GetAdRewards : MonoBehaviour
	{
		/// <summary>
		/// UI element activated when an ad is ready to show.
		/// </summary>
		public GameObject AdLoadedStatus;
		public GameObject RewardPopup;
		public Boolean PurchaseNoAds;

		// These ad units are configured to always serve test ads.
		// These ad units are configured to always serve test ads.
#if UNITY_ANDROID
		string adUnitId = "ca-app-pub-4133264752903581~5788723812";
#elif UNITY_IPHONE
      string adUnitId = "ca-app-pub-4133264752903581/8344688679";
#else
        private const string _adUnitId = "unused";
#endif
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
		public void SetPurchase()
		{
			PurchaseNoAds = true;
		}
		private RewardedAd _rewardedAd;
		public void Awake()
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
			MobileAds.Initialize((InitializationStatus initStatus) =>
			{
			// This callback is called once the MobileAds SDK is initialized.
		});
		}
		/// <summary>
		/// Loads the ad.
		/// </summary>
		public void LoadAd()
		{

			// Clean up the old ad before loading a new one.
			if (!PurchaseNoAds)
			{

				// Clean up the old ad before loading a new one.
				if (_rewardedAd != null)
				{
					DestroyAd();
				}

				Debug.Log("Loading rewarded ad.");

				// Create our request used to load the ad.
				var adRequest = new AdRequest();

				// Send the request to load the ad.
				RewardedAd.Load(adUnitId, adRequest, (RewardedAd ad, LoadAdError error) =>
				{


				// If the operation failed with a reason.
				if (error != null)
					{
						Debug.LogError("Rewarded ad failed to load an ad with error : " + error);
						return;
					}
				// If the operation failed for unknown reasons.
				// This is an unexpected error, please report this bug if it happens.
				if (ad == null)
					{
						Debug.LogError("Unexpected error: Rewarded load event fired with null ad and null error.");
						return;
					}

				// The operation completed successfully.
				Debug.Log("Rewarded ad loaded with response : " + ad.GetResponseInfo());
					_rewardedAd = ad;
				// Register to ad events to extend functionality.
				RegisterEventHandlers(ad);
				// Inform the UI that the ad is ready.
				AdLoadedStatus?.SetActive(true);
				});
			}
		}

		/// <summary>
		/// Shows the ad.
		/// </summary>
		public void ShowAd()
		{
			if (!PurchaseNoAds)
			{
				if (_rewardedAd != null && _rewardedAd.CanShowAd())
				{
					Debug.Log("Showing rewarded ad.");
					_rewardedAd.Show((Reward reward) =>
					{
						Debug.Log(String.Format("Rewarded ad granted a reward: {0} {1}",
												reward.Amount,
												reward.Type));
						GameObject temp = Instantiate(RewardPopup);
						temp.GetComponentInChildren<TMP_Text>().text = String.Format("Rewarded ad granted a reward: {0} {1}", reward.Amount, reward.Type);
						this.transform.parent.gameObject.SetActive(false);
					});
				}

				else
				{
					Debug.LogError("Rewarded ad is not ready yet.");
				}

				// Inform the UI that the ad is not ready.
				AdLoadedStatus?.SetActive(false);
			}
		}
		/// <summary>
		/// Destroys the ad.
		/// </summary>
		public void DestroyAd()
		{
			if (_rewardedAd != null)
			{
				Debug.Log("Destroying rewarded ad.");
				_rewardedAd.Destroy();
				_rewardedAd = null;
			}

			// Inform the UI that the ad is not ready.
			AdLoadedStatus?.SetActive(false);
		}
		private void RegisterEventHandlers(RewardedAd ad)
		{
			// Raised when the ad is estimated to have earned money.
			ad.OnAdPaid += (AdValue adValue) =>
			{
				Debug.Log(String.Format("Rewarded ad paid {0} {1}.",
					adValue.Value,
					adValue.CurrencyCode));
			};
			// Raised when an impression is recorded for an ad.
			ad.OnAdImpressionRecorded += () =>
			{
				Debug.Log("Rewarded ad recorded an impression.");
			};
			// Raised when a click is recorded for an ad.
			ad.OnAdClicked += () =>
			{
				Debug.Log("Rewarded ad was clicked.");
			};
			// Raised when the ad opened full screen content.
			ad.OnAdFullScreenContentOpened += () =>
			{
				Debug.Log("Rewarded ad full screen content opened.");
			};
			// Raised when the ad closed full screen content.
			ad.OnAdFullScreenContentClosed += () =>
			{
				Debug.Log("Rewarded ad full screen content closed.");
			};
			// Raised when the ad failed to open full screen content.
			ad.OnAdFullScreenContentFailed += (AdError error) =>
			{
				Debug.LogError("Rewarded ad failed to open full screen content with error : "
					+ error);
			};
		}
	}
}