using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoogleAnalyticsLoader : MonoBehaviour
{
	DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;
	protected bool firebaseInitialized = false;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	static void OnBeforeSceneLoad()
	{
		GameObject helper = new GameObject("Google Analytics");
		helper.AddComponent<GoogleAnalyticsLoader>();
		DontDestroyOnLoad(helper);
	}
	// When the app starts, check to make sure that we have
	// the required dependencies to use Firebase, and if not,
	// add them if possible.
	public virtual void Awake()
	{
		FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
		{
			dependencyStatus = task.Result;
			if (dependencyStatus == DependencyStatus.Available)
			{
				InitializeFirebase();
			}
			else
			{
				Debug.LogError(
				  "Could not resolve all Firebase dependencies: " + dependencyStatus);
			}
		});
	}
	// Start is called before the first frame update
	void Start()
	{
		if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
		{
			GoogleAnalyticsHelper.Instance.Settings("utmobilegamesfall2023", "127.0.0.1"); // Our id
			GoogleAnalyticsHelper.Instance.LogEvent("MyGame", Application.platform.ToString(), "", "MySubAction", 0);
		}
	}
	private void Update()
	{
		Firebase.Analytics.FirebaseAnalytics.LogEvent("custom_progress_event", "percent", 0.4f);
	}
	// Handle initialization of the necessary firebase modules:
	void InitializeFirebase()
	{
		Debug.Log("Enabling data collection.");
		FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
		
		FirebaseAnalytics.LogEvent(
		  FirebaseAnalytics.EventSelectContent,
		  new Parameter(
			FirebaseAnalytics.ParameterItemId, "utmobilegamesfall2023"),
		  new Parameter(
			FirebaseAnalytics.ParameterItemName, "name"),
		  new Parameter(
			Firebase.Analytics.FirebaseAnalytics.UserPropertySignUpMethod, "Google"),
		  new Parameter(
			"current scene", SceneManager.GetActiveScene().name)
		);
		Debug.Log("Set user properties.");
		// Set the user's sign up method.
		FirebaseAnalytics.SetUserProperty(
		  FirebaseAnalytics.UserPropertySignUpMethod,
		  "Google");
		// Set the user ID.
		FirebaseAnalytics.SetUserId("utmobilegamesfall2023");
		// Set default session duration values.
		FirebaseAnalytics.SetSessionTimeoutDuration(new TimeSpan(0, 30, 0));
		firebaseInitialized = true;

	}

}
