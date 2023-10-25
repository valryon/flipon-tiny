using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapUIScript : MonoBehaviour
{
	// private Button levelOneButton;
	public int numStartingLines;
	public string currentLevelName;
	public bool wonLastGame;
	GameObject canvas;

	// objectives to change
	public int score;
	public int combos;
	public int fourCombos;
	public int fiveCombos;
	public int LCombos;
	public int timesPowerUsed;
	public int numBlock1Broken;
	public int numBlock2Broken;
	public int numBlock3Broken;
	public int numBlock4Broken;
	public int numBlock5Broken;
	public int numBlock6Broken;

	public static MapUIScript mapInstance;

	// method that loads Title Screen Scene on button click 
	public void BackToMenu()
	{
		SceneManager.LoadSceneAsync("TitleScreen");
		Debug.Log("GO TO MAIN MENU");
	}

	// method that loads the Game Scene on button click and sets a variable for the game settings
	public void PlayLevel(int levelNum)
	{

		if (levelNum == -1) // endless mode
		{
			SceneManager.LoadSceneAsync("Game");
			Debug.Log("ENDLESS MODE");
		}
		else
		{
			mapInstance.currentLevelName = "Level" + levelNum.ToString();
			GameManager.gameManager.SaveLevel("Level" + levelNum.ToString());
			SceneManager.LoadSceneAsync("Game");
			Debug.Log("PLAY GAME");
		}
	}

	public void Awake()
	{

		// get the canvas object so we can reset it to Active when the scene is loaded
		canvas = GameObject.FindWithTag("Canvas");

		if (canvas != null)
		{
			Debug.Log("canvas was found"); // WORKING
			canvas.SetActive(true); // NOT WORKING (going to scene doesn't render the map canvas)

			// note: canvas has to be child of mapManager so that the reference to the button is not destroyed
		}

		// prevents duplicate mapManagers from spawning when running DontDestroyOnLoad()
		if (mapInstance != null)
		{
			// delete itself if it's a duplicate 
			Destroy(gameObject);
			return;
		}
		else
		{
			mapInstance = this;
			// don't destroy the mapManager object so that we can keep info from the level buttons
			DontDestroyOnLoad(gameObject);

		}
		if(mapInstance.currentLevelName == null)
		{
			mapInstance.currentLevelName = GameManager.gameManager.LoadLevel();
		}
	}
}
