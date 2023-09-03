using GoogleMobileAds.Api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenAdView : MonoBehaviour
{
	private GameObject adRewards;
	private void Awake()
	{
		AdSize adaptiveSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
		BannerView bannerView = new BannerView("unused", adaptiveSize, AdPosition.Bottom);
		adRewards = GameObject.Find("AdRewards");
	}
	public void OpenAd()
    {
		Debug.Log(adRewards);
		if (adRewards != null){
			adRewards.SetActive(true);
		}
        Debug.Log(GameObject.Find("AdRewards"));
        Destroy(this.gameObject);
    }
}
