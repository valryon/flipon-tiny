using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Firebase.Analytics;

/// Google analytics class v 1.0
/// (c) by Almar Joling
/// E-mail: unitydev@persistentrealities.com
/// Website: http://www.persistentrealities.com

/// Performs a google analytics request with given  parameters
/// Important: account id and domain id are required.
/// Make sure that your site is verified, so that tracking is enabled! (it needs a file to be placed somewhere in a directory online)

/// Example usage:
/// One time initialize:
/// Use a different hostname if you like.
/// GoogleAnalyticsHelper.Instance.Settings("UA-yourid", "127.0.0.1");

/// Then, for an "event":
///  GoogleAnalyticsHelper.Instance.LogEvent("levelname", "pickups", "pickupname", "optionallabel", optionalvalue);

/// For a "page" (like loading a level?)
/// GoogleAnalyticsHelper.Instance.LogPage("Mainscreen");      

public sealed class GoogleAnalyticsHelper
{
    private string accountid;
    private string domain;

    private static readonly GoogleAnalyticsHelper instance = new GoogleAnalyticsHelper();

    public static GoogleAnalyticsHelper Instance
    {
        get
        {
            return instance;
        }
    }

    /// Init class with given site id and domain name
    public void Settings(string accountid, string domain)
    {
        this.domain = domain;
        this.accountid = accountid;
    }


    public void LogPage(string page)
    {
        this.LogEvent(page, "", "", "", 0);
    }

    /// Perform a log call, if only page is specified, a page visit will be tracked
    /// With category, action, and optionally opt_label and opt_value, there will be an event added instead
    /// Note that the statistics can take up to 24h before showing up at your Google Analytics account!
    public void LogEvent(string page, string category, string action, string opt_label, int opt_value)
    {

        if (this.domain.Length == 0)
        {
            Debug.Log("GoogleAnalytics settings not set!");
            return;
        }

        long utCookie = Random.Range(10000000, 99999999);
        long utRandom = Random.Range(1000000000, 2000000000);
        long utToday = GetEpochTime();
        string encoded_equals = "%3D";
        string encoded_separator = "%7C";

        string _utma = utCookie + "." + utRandom + "." + utToday + "." + utToday + "." + utToday + ".21.10" + UnityWebRequest.EscapeURL(";") + UnityWebRequest.EscapeURL("+");
        string cookieUTMZstr = "utmcsr" + encoded_equals + "(direct)" + encoded_separator + "utmccn" + encoded_equals + "(direct)" + encoded_separator + "utmcmd" + encoded_equals + "(none)" + UnityWebRequest.EscapeURL(";");

        string _utmz = utCookie + "." + utToday + "2.2.2." + cookieUTMZstr;

        /// If no page was given, use levelname:
        if (page.Length == 0)
        {
            page = SceneManager.GetActiveScene().name;

		}

        Dictionary<string, string> requestParams = new Dictionary<string, string>();

        requestParams.Add("utmwv", "1");
        requestParams.Add("utmn", utRandom.ToString());
        requestParams.Add("utmhn", UnityWebRequest.EscapeURL(domain));
        //requestParams.Add("utmcs", "ISO-8859-1");
        requestParams.Add("utmsr", Screen.currentResolution.width.ToString() + "x" + Screen.currentResolution.height.ToString());

        //requestParams.Add("utmsc", "24-bit");

        requestParams.Add("utmul", "nl");
        //requestParams.Add("utmje", "0");     
        //requestParams.Add("utmfl", "11.1 r102");     
        requestParams.Add("utmdt", UnityWebRequest.EscapeURL(page));
        //requestParams.Add("utmhid", utRandom.ToString());            
        requestParams.Add("utmr", "-");
        requestParams.Add("utmp", page);
        requestParams.Add("utmac", this.accountid);
        requestParams.Add("utmcc", "__utma" + encoded_equals + _utma + "__utmz" + encoded_equals + _utmz);


        /// Add event if available:
        if (category.Length > 0  && action.Length > 0)
        {
            string eventparams = "5(" + category + "*" + action;

            if (opt_label.Length > 0)
            {
                eventparams += "*" + opt_label + ")(" + opt_value.ToString() + ")";

            }
            else
            {

                eventparams += ")";
            }

            requestParams.Add("utme", eventparams);
            requestParams.Add("utmt", "event");
        }


        /// Create query string:
        ArrayList pageURI = new ArrayList();
        foreach (KeyValuePair<string, string> kvp in requestParams)
        {
            pageURI.Add(kvp.Key + "=" + kvp.Value);
        }


        string url = "https://utmobilegamesfall2023-default-rtdb.firebaseio.com";

        /// Log url:
        Debug.Log("[Google URL]" + url);
        Debug.Log(GetRequest(url));


    }
    IEnumerator GetRequest(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = url.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                    break;
            }
        }
    }

    private long GetEpochTime()
    {
        System.DateTime dtCurTime = System.DateTime.Now;
        System.DateTime dtEpochStartTime = System.Convert.ToDateTime("1/1/1970 0:00:00 AM");
        System.TimeSpan ts = dtCurTime.Subtract(dtEpochStartTime);

        long epochtime;
        epochtime = ((((((ts.Days * 24) + ts.Hours) * 60) + ts.Minutes) * 60) + ts.Seconds);

        return epochtime;
    }

    // GOOGLE ANALYTICS METHODS: LOG EVENTS
    public static void AnalyticsLevelStart(string levelName)
    {
        Debug.Log("Logging a level start event for " + levelName);
        FirebaseAnalytics.LogEvent(
            FirebaseAnalytics.EventLevelStart,
            new Parameter(FirebaseAnalytics.ParameterLevelName, levelName)
            );
    }

    public static void AnalyticsLevelEnd(string levelName)
    {
        Debug.Log("Logging a level end event for " + levelName);
        FirebaseAnalytics.LogEvent(
            FirebaseAnalytics.EventLevelEnd,
            new Parameter(FirebaseAnalytics.ParameterLevelName, levelName)
            );
    }

    public static void AnalyticsLevelUp()
    {
        // Log an event with multiple parameters.
        Debug.Log("Logging a level up event.");
        FirebaseAnalytics.LogEvent(
          FirebaseAnalytics.EventLevelUp,
          new Parameter(FirebaseAnalytics.ParameterLevel, 5),
          new Parameter(FirebaseAnalytics.ParameterCharacter, "mrspoon")
        );
    }

    public static void AnalyticsStorePurchase()
    {
        // Log an event with multiple parameters.
        Debug.Log("Logging a Purchase event.");
        FirebaseAnalytics.LogEvent(
          FirebaseAnalytics.EventPurchase,
          new Parameter(FirebaseAnalytics.ParameterCurrency, "US Dollar"),
          new Parameter(FirebaseAnalytics.ParameterItemName, "noads"),
          new Parameter("Purchase Completed", 1));

        Debug.Log("Purchase Recorded");
    }

    public static void AnalyticsAdImpression()
    {
        Debug.Log("Logging an Ad Impression event");
        FirebaseAnalytics.LogEvent(
            FirebaseAnalytics.EventAdImpression,
            new Parameter(FirebaseAnalytics.ParameterAdPlatform, "Google")
            );
    }
}