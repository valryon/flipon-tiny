using System;
using UnityEngine;
using GoogleMobileAds;
using GoogleMobileAds.Api;

public class Banner : MonoBehaviour
{
	public AdOrientation orientation = AdOrientation.HORIZONTAL;
	public AdHorizontalPosition horizontalPosition = AdHorizontalPosition.CENTER_HORIZONTAL;
	public AdVerticalPosition verticalPosition = AdVerticalPosition.BOTTOM;
	private BannerView bannerView;
	public void Start()
	{
		MobileAds.Initialize(InitializationStatus => { });
		this.RequestBanner();

	}
	private void RequestBanner()
	{
#if UNITY_ANDROID
		string adUnitid = "ca-app-pub-3940256099942544/6300978111*;
#elif UNITY_IPHONE
		string adUnitId = "ca-app-pub-3940256099942544/2934735716*;
#else
		string adUnitId = "unexpected platform";
#endif
		AdSize adSize = new AdSize(Screen.width, (int)(Screen.width / 6.4f));

		// Create a 320x50 banner at the top of the screen.
		this.bannerView = new BannerView(adUnitId, adSize, AdPosition.Top);

		//create an empty ad request
		AdRequest request = new AdRequest.Builder().Build();
		//Load the banner with the requeat.
		this.bannerView.LoadAd(request);
	}

}