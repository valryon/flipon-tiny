using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapUIScript : MonoBehaviour
{  
    private Button levelOneButton;
    public int numStartingLines;
    GameObject canvas;

    private static MapUIScript mapInstance;

    // Start is called before the first frame update
    void Start()
    {
        // levelOneButton = FindObjectOfType<Button>();
        // GameObject gameSettings = FindObjectOfType<GameSettings>()

        

        // passes argument to PlayLevel() when level one button is clicked
        
        
    }

    // method that loads Title Screen Scene on button click 
    public void BackToMenu()
    {
        SceneManager.LoadSceneAsync("TitleScreen");
    }

    // method that loads the Game Scene on button click and sets a variable for the game settings
    public void PlayLevel(int startingLines)
    {
        numStartingLines = startingLines;
        SceneManager.LoadSceneAsync("Game");
    }

    public void Awake()
    {
        // get the number of map managers (this)
        /*
        int numManagers = FindObjectsOfType().Length;
        if (numManagers > 1)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
        */
        levelOneButton = GameObject.Find("PlayLevel1Button").GetComponent<Button>();
        levelOneButton.onClick.AddListener(() => PlayLevel(1));

        // don't destroy the mapManager object so that we can keep info from the level buttons
        DontDestroyOnLoad(gameObject);

        // get the canvas object so we can reset it to Active when the scene is loaded
        canvas = GameObject.FindWithTag("Canvas"); 

        if (canvas != null)
        {
            Debug.Log("canvas was found"); // WORKING
            canvas.SetActive(true); // NOT WORKING (going to scene doesn't render the map canvas)

            // note: canvas has to be child of mapManager so that the reference to the button is not destroyed
        }

        
        // prevents duplicate mapManagers from spawning when running DontDestroyOnLoad()
        if (mapInstance == null)
        {
            mapInstance = this;
        }
        else
        {
            Destroy(gameObject); // deletes itself if it's a duplicate
        }
        
    }
}
