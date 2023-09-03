using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class myIAPManager : MonoBehaviour {
    private static Text myText;
    public AdRewards adRewards;
    // Use this for initialization
    void Start () {
        myText = GameObject.Find("MyText").GetComponent<Text>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void myPurchaseSucceed ()
    {
        adRewards.PurchaseNoAds = true;
        MyDebug("Purchase Succeeded.");
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
