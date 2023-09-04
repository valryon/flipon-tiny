using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoogleAnalyticsLoader : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
        {
            GoogleAnalyticsHelper.Instance.Settings("utmobilegamesfall2023", "127.0.0.1"); // your id
            GoogleAnalyticsHelper.Instance.LogEvent("MyGame", Application.platform.ToString(), "MyAction", "MySubAction", 0);
        }
    }
}
