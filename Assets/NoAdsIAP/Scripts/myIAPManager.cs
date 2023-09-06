using AdRewards;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;
public class myIAPManager : MonoBehaviour {
    private static Text myText;
    public GetAdRewards adRewards;
    // Use this for initialization
    void Start () {
        myText = GameObject.Find("MyText").GetComponent<Text>();
    }
	
    public void myPurchaseSucceed ()
    {
        if (PurchaseProcessingResult.Complete == 0)
        {
            adRewards.PurchaseNoAds = true;
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
        Debug.Log(debug);
        myText.text += "\r\n" + debug;
        
    }
}
