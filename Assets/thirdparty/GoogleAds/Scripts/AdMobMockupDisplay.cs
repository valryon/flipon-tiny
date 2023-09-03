using UnityEngine;
using System.Collections.Generic;


[ExecuteInEditMode]
[RequireComponent(typeof(AdMobBanners))]
public class AdMobMockupDisplay : MonoBehaviour
{

	public bool executeInEditMode = true;
	public AdMockupStyle style = AdMockupStyle.LIGHT;
	public string[] texts = AdMobMockupDisplay.quotes;
	public Texture[] icons = new Texture[0];
	public Texture[] actions = new Texture[0];
	public Texture darkBackground = null;
	public Texture lightBackground = null;
	public AdMockup currentAd = null;

	private AdMobBanners plugin = null;
	private string lastError = null;
	public ControlButtons rectangles = new ControlButtons();

	private int currentAdId = -1;

	void Start()
	{

		this.plugin = this.GetComponent<AdMobBanners>();

		this.GenerateRandomAd();
	}

	void Update()
	{

		int received = this.plugin.GetReceived();

		if (this.currentAdId != received)
		{

			currentAdId = received;

			this.GenerateRandomAd();
		}
	}

	void OnGUI()
	{

		if (this.plugin.IsVisible() && Application.isEditor && (Application.isPlaying || this.executeInEditMode))
		{

			if (this.currentAd == null)
			{

				this.GenerateRandomAd();
			}

			this.DrawAd();
		}

		bool changed = false;
		bool targeted = false;

		if (GUI.Button(this.rectangles.loadButton, "LOAD"))
		{

			this.plugin.Load();
		}

		if (GUI.Button(this.rectangles.exitButton, "EXIT") || Input.GetKey("escape"))
		{
			print("AdMobPlugin.Exit()");
			QuitGame();

		}

		if (GUI.Button(this.rectangles.showButton, "SHOW"))
		{

			this.plugin.Show();
		}

		if (GUI.Button(this.rectangles.hideButton, "HIDE"))
		{

			this.plugin.Hide();
		}

		if (GUI.Button(this.rectangles.horizontalPositionButton, this.plugin.horizontalPosition.ToString()))
		{

			changed = true;

			this.plugin.horizontalPosition++;

			if (this.plugin.horizontalPosition > AdHorizontalPosition.RIGHT)
			{

				this.plugin.horizontalPosition = AdHorizontalPosition.CENTER_HORIZONTAL;
			}
		}

		if (GUI.Button(this.rectangles.verticalPositionButton, this.plugin.verticalPosition.ToString()))
		{

			changed = true;

			this.plugin.verticalPosition++;

			if (this.plugin.verticalPosition > AdVerticalPosition.BOTTOM)
			{

				this.plugin.verticalPosition = AdVerticalPosition.CENTER_VERTICAL;
			}
		}

		if (GUI.Button(this.rectangles.orientationButton, this.plugin.orientation.ToString()))
		{

			changed = true;

			plugin.orientation++;

			if (plugin.orientation > AdOrientation.VERTICAL)
			{

				plugin.orientation = AdOrientation.HORIZONTAL;
			}
		}

		if (GUI.Button(this.rectangles.sizeButton, plugin.size.ToString()))
		{

			changed = true;

			this.plugin.size++;

			if (this.plugin.size > AdSizes.SMART_BANNER)
			{

				this.plugin.size = AdSizes.BANNER;
			}
		}

		if (changed)
		{

			this.plugin.Reconfigure();
		}

		if (targeted)
		{

			this.plugin.SetTarget();
		}

		string tmp;

		tmp = this.plugin.GetLastError();

		if (tmp != null)
		{

			this.lastError = tmp;
		}

		if (this.lastError != null && this.lastError.Length > 0)
		{

			if (GUI.Button(this.rectangles.lastErrorRectButton, this.lastError))
			{

				this.lastError = null;
			}
		}
		else
		{
			GUI.Label(this.rectangles.receivedLabel, this.plugin.GetReceived() + " ad(s) loaded so far (" + System.DateTime.Now + ")");
		}

	}
	public void QuitGame()
	{
		print("Byeeee");
		// save any game data here
#if UNITY_EDITOR
		// Application.Quit() does not work in the editor so
		// UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
		UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
	}
	private Rect GetAdRect(AdSizes size, AdHorizontalPosition horizontalPosition, AdVerticalPosition verticalPosition)
	{

		float x = 0, y = 0, width = 0, height = 0;

		switch (size)
		{

			case AdSizes.BANNER:
				width = 320;
				height = 50;
				break;

			case AdSizes.IAB_BANNER:
				width = 300;
				height = 250;
				break;

			case AdSizes.IAB_LEADERBOARD:
				width = 486;
				height = 60;
				break;

			case AdSizes.IAB_MRECT:
				width = 728;
				height = 90;
				break;

			case AdSizes.SMART_BANNER:
				width = Screen.width;
				height = width / 4f;
				break;
		}

		if (width > Screen.width)
		{

			width = Screen.width;
		}

		if (height > Screen.height)
		{

			height = Screen.height;
		}

		switch (horizontalPosition)
		{

			case AdHorizontalPosition.CENTER_HORIZONTAL:
				x = (Screen.width / 2) - (width / 2);
				break;

			case AdHorizontalPosition.LEFT:
				x = 0;
				break;

			case AdHorizontalPosition.RIGHT:
				x = Screen.width - width;
				break;
		}

		switch (verticalPosition)
		{

			case AdVerticalPosition.CENTER_VERTICAL:
				y = (Screen.height / 2) - (height / 2);
				break;

			case AdVerticalPosition.TOP:
				y = 0;
				break;

			case AdVerticalPosition.BOTTOM:
				y = Screen.height - height;
				break;
		}

		return (new Rect(x, y, width, height));
	}

	private void DrawAd()
	{

		Rect backgroundRect = this.GetAdRect(this.plugin.size, this.plugin.horizontalPosition, this.plugin.verticalPosition);
		Rect iconRect = new Rect(backgroundRect.x + 4, backgroundRect.y + 4, 38, 38);
		Rect actionRect = new Rect(backgroundRect.x + backgroundRect.width - 34, backgroundRect.y + 4, 30, 30);
		Rect textRect = new Rect(backgroundRect.x + 4 + 38 + 4, backgroundRect.y + 4, backgroundRect.width - 4 - 38 - 4 - 4 - 30 - 4, backgroundRect.height - 8);
		Texture background = this.GetBackground();
		Color textColor = this.GetTextColor();

		if (background != null)
		{

			GUI.DrawTexture(backgroundRect, background);

		}
		else
		{

			GUI.Box(backgroundRect, (this.currentAd.text == null ? "AD MOCK-UP" : null));
		}

		if (this.currentAd.icon != null)
		{

			GUI.DrawTexture(iconRect, this.currentAd.icon);
		}

		if (this.currentAd.action != null)
		{

			GUI.DrawTexture(actionRect, this.currentAd.action);
		}

		if (this.currentAd.text != null)
		{

			GUIStyle textStyle = new GUIStyle();

			textStyle.normal.textColor = Color.black;
			textStyle.fontStyle = FontStyle.Bold;
			textStyle.wordWrap = true;
			textStyle.alignment = TextAnchor.MiddleCenter;
			textStyle.normal.textColor = textColor;

			GUI.Label(textRect, this.currentAd.text, textStyle);
		}
	}

	private T GetRandomElement<T>(T[] array) where T : class
	{

		int index, length;

		length = (array == null ? 0 : array.Length);

		index = Random.Range(0, length);

		return (length == 0 ? null : array[index]);
	}

	private void GenerateRandomAd()
	{

		this.currentAd = new AdMockup
		{
			icon = this.GetRandomElement(this.icons),
			action = this.GetRandomElement(this.actions),
			text = this.GetRandomElement(this.texts)
		};
	}

	private Texture GetBackground()
	{

		return (this.style == AdMockupStyle.DARK ? this.darkBackground : this.lightBackground);
	}

	private Color GetTextColor()
	{

		return (this.style == AdMockupStyle.DARK ? Color.white : Color.red);
	}

	private static string[] quotes = new string[]{
		"I'M Working"
	};
}


/*
 * helper classes and enums
 */
[System.Serializable]
public class ControlButtons
{

	public Rect loadButton = new Rect(32, Screen.height - 32, 64, 48);
	public Rect exitButton = new Rect(104, Screen.height - 32, 64, 48);
	public Rect showButton = new Rect(32, 88, 64, 48);
	public Rect hideButton = new Rect(104, 88, 64, 48);
	public Rect horizontalPositionButton = new Rect(176, 32, 150, 48);
	public Rect verticalPositionButton = new Rect(176, 88, 150, 48);
	public Rect orientationButton = new Rect(176, 144, 150, 48);
	public Rect sizeButton = new Rect(176, 200, 150, 50);
	public Rect lastErrorRectButton = new Rect(32, 256, 384, 50);
	public Rect receivedLabel = new Rect(32, 256, 384, 50);
}
public class AdMockup
{

	public Texture icon;
	public Texture action;
	public string text;
}

public enum AdMockupStyle { DARK, LIGHT };
