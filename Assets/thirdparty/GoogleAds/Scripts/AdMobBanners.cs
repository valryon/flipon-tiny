using UnityEngine;
using System.Collections;


public class AdMobBanners : MonoBehaviour
{
#if UNITY_ANDROID
	string adUnitid = "ca-app-pub-4133264752903581~5788723812";
#elif UNITY_IPHONE
	string adUnitId = "ca-app-pub-4133264752903581/3796985290";
#else
	string adUnitId = "unexpected platform";
#endif
	public string publisherID = "pub-4133264752903581";
	public bool isTesting = true;
	public bool guessSelfDeviceId = true;
	public AdSizes size = AdSizes.SMART_BANNER;
	public AdOrientation orientation = AdOrientation.HORIZONTAL;
	public AdHorizontalPosition horizontalPosition = AdHorizontalPosition.CENTER_HORIZONTAL;
	public AdVerticalPosition verticalPosition = AdVerticalPosition.BOTTOM;
	public float refreshInterval = 30;
	public bool loadOnStart = true;
	public bool setTargetOnStart = false;
	public bool loadOnReconfigure = true;

	private bool visible = true;

	void Start()
	{

		this.Initialize();

		if (this.loadOnStart)
		{

			this.Load();
		}

		if (this.setTargetOnStart)
		{

			this.SetTarget();
		}

		this.StartCoroutine(this.Refresh());
	}

#if UNITY_ANDROID && !UNITY_EDITOR

	private AndroidJavaObject plugin;

	private void Initialize(){

		AndroidJavaClass pluginClass = new AndroidJavaClass("com.guillermonkey.unity.admob.AdMobPlugin");

		this.plugin	= pluginClass.CallStatic<AndroidJavaObject>(
			"getInstance",
			this.publisherId,
			this.isTesting,
			this.testDeviceIds,
			this.guessSelfDeviceId,
			(int)this.size,
			(int)this.orientation,
			(int)this.horizontalPosition,
			(int)this.verticalPosition
		);
	}

	public void Reconfigure(){

		this.plugin.Call(
			"reconfigure",
			this.publisherId,
			this.isTesting,
			this.testDeviceIds,
			this.guessSelfDeviceId,
			(int)this.size,
			(int)this.orientation,
			(int)this.horizontalPosition,
			(int)this.verticalPosition
		);

		if(this.loadOnReconfigure){

			this.Load();
		}
	}

	public void Load(){

		this.plugin.Call("load");

		this.Show();
	}

	public void Show(){

		this.plugin.Call("show");

		this.visible = true;
	}

	public void Hide(){

		this.plugin.Call("hide");

		this.visible = false;
	}

	public string GetLastError(){

		return( this.plugin.Call<string>("getLastError") );
	}

	public int GetReceived(){

		return( this.plugin.Call<int>("getReceived") );
	}

	void OnDestroy(){

		this.Hide();
	}

#else

	private int received;

	private void Initialize()
	{

		if (!this.isTesting)
		{

			Debug.LogWarning("AdMobPlugin is NOT in test mode. Please make sure you do not request invalid impressions while testing your application.");
		}
	}

	public void Reconfigure()
	{

		print("AdMobPlugin.Reconfigure()");
	}
	
	public void SetTarget()
	{

		print("AdMobPlugin.SetTarget()");
	}

	public void Load()
	{

		print("AdMobPlugin.Load()");

		this.received++;
	}

	public void Show()
	{

		print("AdMobPlugin.Show()");

		this.visible = true;
	}

	public void Hide()
	{

		print("AdMobPlugin.Hide()");

		this.visible = false;
	}

	public string GetLastError()
	{

		//print("AdMobPlugin.GetLastError()");

		return (null);
	}

	public int GetReceived()
	{

		//print("AdMobPlugin.GetReceived()");

		return (this.received);
	}

#endif

	public bool IsVisible()
	{

		return (this.visible);
	}

	private IEnumerator Refresh()
	{

		while (true)
		{

			if (this.refreshInterval > 0)
			{

				yield return new WaitForSeconds(this.refreshInterval < 30 ? 30 : this.refreshInterval);

				this.Load();
			}
		}
	}
}

/*
 * helper classes and enums
 */

public enum AdSizes { BANNER, IAB_MRECT, IAB_BANNER, IAB_LEADERBOARD, SMART_BANNER };
public enum AdOrientation { HORIZONTAL, VERTICAL };
public enum AdHorizontalPosition { CENTER_HORIZONTAL, LEFT, RIGHT };
public enum AdVerticalPosition { CENTER_VERTICAL, TOP, BOTTOM };