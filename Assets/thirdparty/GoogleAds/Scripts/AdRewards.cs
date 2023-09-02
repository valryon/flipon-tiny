using GoogleMobileAds.Api;
using Microsoft.SqlServer.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdRewards : MonoBehaviour
{
	public enum AdOrientation { HORIZONTAL, VERTICAL };

	public void Start()
	{
		// Initialize the Google Mobile Ads SDK.
		MobileAds.Initialize((InitializationStatus initStatus) =>
		{
			// This callback is called once the MobileAds SDK is initialized.
		});
		this.LoadRewardedAd();
	}
	// These ad units are configured to always serve test ads.
#if UNITY_ANDROID
  private string _adUnitId = "ca-app-pub-3940256099942544/5224354917";
#elif UNITY_IPHONE
  private string _adUnitId = "ca-app-pub-3940256099942544/1712485313";
#else
	private string _adUnitId = "unused";
#endif

	private RewardedAd rewardedAd;

	/// <summary>
	/// Loads the rewarded ad.
	/// </summary>
	public void LoadRewardedAd()
	{
		// Clean up the old ad before loading a new one.
		if (rewardedAd != null)
		{
			rewardedAd.Destroy();
			rewardedAd = null;
		}

		Debug.Log("Loading the rewarded ad.");

		// create our request used to load the ad.
		var adRequest = new AdRequest();
		adRequest.Keywords.Add("unity-admob-sample");

		// send the request to load the ad.
		RewardedAd.Load(_adUnitId, adRequest,
			(RewardedAd ad, LoadAdError error) =>
			{
				// if error is not null, the load request failed.
				if (error != null || ad == null)
				{
					Debug.LogError("Rewarded ad failed to load an ad " +
								   "with error : " + error);
					return;
				}

				Debug.Log("Rewarded ad loaded with response : "
						  + ad.GetResponseInfo());

				rewardedAd = ad;
				ShowRewardedAd();
	});
    }
public void ShowRewardedAd()
{
	const string rewardMsg =
		"Rewarded ad rewarded the user. Type: {0}, amount: {1}.";

	if (rewardedAd != null && rewardedAd.CanShowAd())
	{

		RegisterEventHandlers(rewardedAd);
		rewardedAd.Show((Reward reward) =>
		{
				// TODO: Reward the user.
				Debug.Log(String.Format(rewardMsg, reward.Type, reward.Amount));
				// rewardedAd.OnAdRewarded += HandleRewardBasedVideoRewarded;
			});
	}
}
public void HandleRewardBasedVideoRewarded(object sender, Reward args)
{
	string type = args.Type;
	double amount = args.Amount;
	//Reawrd User here
	print("User rewarded with: " + amount.ToString() + " " + type);
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
		rewardedAd.Destroy();
		Debug.Log("Rewarded ad was clicked.");
	};
	// Raised when an ad opened full screen content.
	ad.OnAdFullScreenContentOpened += () =>
	{
		Debug.Log("Rewarded ad full screen content opened.");
	};
	// Raised when the ad closed full screen content.
	ad.OnAdFullScreenContentClosed += () =>
	{
		Debug.Log("Rewarded ad full screen content closed.");
			// Reload the ad so that we can show another as soon as possible.
			LoadRewardedAd();
	};
	// Raised when the ad failed to open full screen content.
	ad.OnAdFullScreenContentFailed += (AdError error) =>
	{
		Debug.LogError("Rewarded ad failed to open full screen content " +
					   "with error : " + error);
			// Reload the ad so that we can show another as soon as possible.
			LoadRewardedAd();
	};
} 
}
