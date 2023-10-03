using AdRewards;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class myIAPManager : MonoBehaviour {
  public GetAdRewards adRewards;
  // Use this for initialization

  public void myPurchaseSucceed ()
  {

    if (PurchaseProcessingResult.Complete == 0)
    {
      Debug.Log("Google Analytics: Purchase Recorded.");
      adRewards.PurchaseNoAds = true;
      GoogleAnalyticsHelper.AnalyticsStorePurchase();
      Destroy(GameObject.Find("UIFakeStoreWindow"));
      MyDebug("Purchase Succeeded.");
    }
  }

  public void myPurchaseFail()
  {
    MyDebug("Purchase Failed.");
  }

  public void myListenerSucceed()
  {
    MyDebug("Listener Succeeded.");
  }

  public void myListenerFail()
  {
    MyDebug("Listener Failed.");
  }

  private void MyDebug(string debug)
  {
    Debug.Log("\r\n" + debug);
    
  }
}
