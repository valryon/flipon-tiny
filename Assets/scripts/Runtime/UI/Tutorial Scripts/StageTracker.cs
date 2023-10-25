using Pon;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageTracker : MonoBehaviour
{
  // All canvases used by this script to create the dialogue box should match the names in the CreateDialogueBox() function
  public static StageTracker stageTracker;
  DialogueController dialogueControl;
  [SerializeField] MultiDialogueData dialogueDataSO;

  [SerializeField] GameObject dialoguePrefab;
  GameObject currDiaBox;

  static float currTutorialStage = 0f;
  static int comboValue = 0;
  static bool wasPowerUsed = false;

  static public float finalTutorialStage = 39.5f; // Change this value to match last case in tutorial switch statement if dialogue and tutorial steps change
  static float startGameTutorialStage = 20.5f; // Change this value to match case where gameplay is started
  static int startGameDialogue = 20;  // Change this value to the index value of the gameplay start dialogue line

  float[] tutorialActionCases = new float[] { startGameTutorialStage, 25f, 28f, 31f, 33f }; // Change this value to each of the starting cases of each game tutorial step, e.g. 3 combo, 4 combo, 5 combo, etc.
  int[] tutorialDialogueIndex = new int[] { startGameDialogue, 23, 25, 27, 28 };
  int passedTutorialAction = 0;

  static GridScript currGrid;
  PonGameScript gameScript;
  Image charImg;

  private string TUTORIAL_FILENAME = "tutorialData.dat";

  private void Awake()
  {
    if (stageTracker == null)
    {
      stageTracker = this.GetComponent<StageTracker>();
    }
    else
    {
      Destroy(this.gameObject);
    }
    DontDestroyOnLoad(this.gameObject);

    dialogueControl = this.GetComponent<DialogueController>();
  }

    private void Update()
  {
    switch (currTutorialStage)
    {
      case 0f:
        // start of tutorial
        if (SceneManager.GetActiveScene().name == "Tutorial_Entry")
        {
          //Display CK 2
          CreateDialogueBox();
          SetActiveDialogueBox(true);
          SetDialogueObjects();
          dialogueControl.StartMultiDialogue(dialogueDataSO);
          currTutorialStage += 0.5f;
        }
        break;
      case 0.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 1f:
        // CK 3
        dialogueControl.DisplayNextSentenceMultiCharacters();
        currTutorialStage += 0.5f;
        break;
      case 1.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 2f:
        // CK 4
        dialogueControl.DisplayNextSentenceMultiCharacters();
        currTutorialStage += 0.5f;
        break;
      case 2.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 3f:
        // CK 5
        dialogueControl.DisplayNextSentenceMultiCharacters();
        currTutorialStage += 0.5f;
        break;
      case 3.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 4f:
        // CK 6
        dialogueControl.DisplayNextSentenceMultiCharacters();
        currTutorialStage += 0.5f;
        break;
      case 4.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 5f:
        // CK 7
        dialogueControl.DisplayNextSentenceMultiCharacters();
        currTutorialStage += 0.5f;
        break;
      case 5.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 6f:
        // CK 8
        dialogueControl.DisplayNextSentenceMultiCharacters();
        currTutorialStage += 0.5f;
        break;
      case 6.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 7f:
        // Dark Clouds
        dialogueControl.DisplayNextSentenceMultiCharacters();
        charImg.enabled = false;
        currTutorialStage += 0.5f;
        break;
      case 7.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 8f:
        // LOS appears
        charImg.enabled = true;
        dialogueControl.DisplayNextSentenceMultiCharacters();
        currTutorialStage += 0.5f;
        break;
      case 8.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 9f:
        // LOS 11
        dialogueControl.DisplayNextSentenceMultiCharacters();
        currTutorialStage += 0.5f;
        break;
      case 9.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 10f:
        // LOS 12
        dialogueControl.DisplayNextSentenceMultiCharacters();
        currTutorialStage += 0.5f;
        break;
      case 10.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 11f:
        // LOS 13
        dialogueControl.DisplayNextSentenceMultiCharacters();
        currTutorialStage += 0.5f;
        break;
      case 11.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 12f:
        // LOS 14
        dialogueControl.DisplayNextSentenceMultiCharacters();
        currTutorialStage += 0.5f;
        break;
      case 12.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 13f:
        // LOS 15
        dialogueControl.DisplayNextSentenceMultiCharacters();
        currTutorialStage += 0.5f;
        break;
      case 13.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 14f:
        // LOS disappear
        dialogueControl.DisplayNextSentenceMultiCharacters();
        charImg.enabled = false;
        currTutorialStage += 0.5f;
        break;
      case 14.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 15f:
        // CK 17
        charImg.enabled = true;
        dialogueControl.DisplayNextSentenceMultiCharacters();
        currTutorialStage += 0.5f;
        break;
      case 15.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 16f:
        // CK 18
        dialogueControl.DisplayNextSentenceMultiCharacters();
        currTutorialStage += 0.5f;
        break;
      case 16.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 17f:
        // CK 19
        dialogueControl.DisplayNextSentenceMultiCharacters();
        currTutorialStage += 0.5f;
        break;
      case 17.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 18f:
        // CK 20
        dialogueControl.DisplayNextSentenceMultiCharacters();
        currTutorialStage += 0.5f;
        break;
      case 18.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 19f:
        // CK 21
        dialogueControl.DisplayNextSentenceMultiCharacters();
        currTutorialStage += 0.5f;
        break;
      case 19.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 20f:
        // load game
        currTutorialStage += 0.5f;
        StartLoad(1);
        break;
      case 20.5f:
        gameScript = PonGameScript.instance;
        if (SceneManager.GetActiveScene().name == "Tutorial_Game" && gameScript.isTutorial == false)
        {
          currTutorialStage += 0.5f;
          CreateDialogueBox();
          SetDialogueObjects();
          SetActiveDialogueBox(false);
          gameScript.isTutorial = true;
          gameScript.SetPause(true);
          if (passedTutorialAction != 0)
          {
            currTutorialStage = tutorialActionCases[passedTutorialAction];
            SetDialogueIndex(tutorialDialogueIndex[passedTutorialAction]);
          }
        }
        break;
      case 21f:
        // game loaded, game start dialogue
        // 3 Combo
        currTutorialStage += 0.5f;
        dialogueControl.DisplayNextSentenceMultiCharacters();
        SetActiveDialogueBox(true);
        break;
      case 21.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 22f:
        // 3 Combo Pt 2
        dialogueControl.DisplayNextSentenceMultiCharacters();
        currTutorialStage += 0.5f;
        break;
      case 22.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 23f:
        //Unpausing and starting first task
        currTutorialStage += 0.5f;
        SetActiveDialogueBox(false);
        gameScript.SetPause(false);
        break;
      case 23.5f:
        // player does task
        //Waiting for player to finish first task
        if(comboValue == 3)
        {
          currTutorialStage += 0.5f;
          comboValue = 0;
          passedTutorialAction = 1;
        }
        break;
      case 24f:
        // 3 Combo Congrats
        currTutorialStage += 0.5f;
        dialogueControl.DisplayNextSentenceMultiCharacters();
        SetActiveDialogueBox(true);
        gameScript.SetPause(true);
        break;
      case 24.5f:
        //Waiting for player to click through dialogue
        CheckTouch();
        break;
      case 25f:
        // 4 Combo
        dialogueControl.DisplayNextSentenceMultiCharacters();
        SetActiveDialogueBox(true);
        currTutorialStage += 0.5f;
        break;
      case 25.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 26f:
        //Unpause game
        //Unpausing and starting second task
        currTutorialStage += 0.5f;
        SetActiveDialogueBox(false);
        gameScript.SetPause(false);
        break;
      case 26.5f:
        //player does task
        //Waiting for player to finish second task
        if (comboValue == 4)
        {
          currTutorialStage += 0.5f;
          comboValue = 0;
          passedTutorialAction = 2;
        }
        break;
      case 27f:
        // 4 Combo Congrats
        currTutorialStage += 0.5f;
        dialogueControl.DisplayNextSentenceMultiCharacters();
        SetActiveDialogueBox(true);
        gameScript.SetPause(true);
        break;
      case 27.5f:
        //Waiting for player to click through dialogue
        CheckTouch();
        break;
      case 28f:
        // 5 Combo
        dialogueControl.DisplayNextSentenceMultiCharacters();
        SetActiveDialogueBox(true);
        currTutorialStage += 0.5f;
        break;
      case 28.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 29f:
        //Unpause game
        //Unpausing and starting third task
        currTutorialStage += 0.5f;
        SetActiveDialogueBox(false);
        gameScript.SetPause(false);
        break;
      case 29.5f:
        //player does task
        //Waiting for player to finish third task
        if (comboValue == 5)
        {
          currTutorialStage += 0.5f;
          comboValue = 0;
          passedTutorialAction = 3;
        }
        break;
      case 30f:
        // 5 Combo Congrats
        currTutorialStage += 0.5f;
        dialogueControl.DisplayNextSentenceMultiCharacters();
        SetActiveDialogueBox(true);
        gameScript.SetPause(true);
        break;
      case 30.5f:
        //Waiting for player to click through dialogue
        CheckTouch();
        break;
      case 31f:
        // L Combo
        dialogueControl.DisplayNextSentenceMultiCharacters();
        SetActiveDialogueBox(true);
        currTutorialStage += 0.5f;
        break;
      case 31.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 32f:
        //Unpause game
        //Unpausing and starting fourth task
        currTutorialStage += 0.5f;
        SetActiveDialogueBox(false);
        gameScript.SetPause(false);
        break;
      case 32.5f:
        //player does task
        //Waiting for player to finish fourth task
        if (comboValue >= 6)
        {
          currTutorialStage += 0.5f;
          comboValue = 0;
          passedTutorialAction = 4;
        }
        break;
      case 33f:
        // Bomb
        currTutorialStage += 0.5f;
        dialogueControl.DisplayNextSentenceMultiCharacters();
        SetActiveDialogueBox(true);
        gameScript.SetPause(true);
        break;
      case 33.5f:
        //Waiting for player to click through dialogue
        CheckTouch();
        break;
      case 34f:
        //Unpause game
        //Unpausing and starting fifth task
        currTutorialStage += 0.5f;
        SetActiveDialogueBox(false);
        currGrid.PowerCharge = 1f;
        gameScript.SetPause(false);
        wasPowerUsed = false;
        break;
      case 34.5f:
        //player does task
        //Waiting for player to finish fifth task
        if (wasPowerUsed)
        {
          currTutorialStage += 0.5f;
          wasPowerUsed = false;
        }
        break;
      case 35f:
        // Bomb Congrats
        currTutorialStage += 0.5f;
        dialogueControl.DisplayNextSentenceMultiCharacters();
        SetActiveDialogueBox(true);
        gameScript.SetPause(true);
        break;
      case 35.5f:
        CheckTouch();
        break;
      case 36f:
        // load map
        currTutorialStage += 0.5f;
        StartLoad(0);
        break;
      case 36.5f:
        if (SceneManager.GetActiveScene().name == "Tutorial_Entry")
        {
          currTutorialStage += 0.5f;
        }
        break;
      case 37f:
        // Sendoff one
        currTutorialStage += 0.5f;
        CreateDialogueBox();
        SetDialogueObjects();
        dialogueControl.DisplayNextSentenceMultiCharacters();
        SetActiveDialogueBox(true);
        break;
      case 37.5f:
        //Waiting for player to click through Send-Off Dialogue
        CheckTouch();
        break;
      case 38f:
        // Sendoff two
        dialogueControl.DisplayNextSentenceMultiCharacters();
        currTutorialStage += 0.5f;
        break;
      case 38.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 39f:
        // load game
        currTutorialStage += 0.5f;
        dialogueControl.enabled = false;
        StartLoad(2);
        break;
      case 39.5f:
        // destroy self
        if (SceneManager.GetActiveScene().name == "Map_t")
        {
          string path = Path.Combine(Application.persistentDataPath, TUTORIAL_FILENAME);
          File.WriteAllText(path, true.ToString());
          Destroy(this.gameObject);
        }
        break;
    }
  }

  private void StartLoad(int destination)
  {
    if (destination == 0)
    {
      StartCoroutine(AsyncLoadIntoTutMap());
    }
    else if (destination == 1)
    {
      StartCoroutine(AsyncLoadIntoTutGame());
    }
    else if (destination == 2)
    {
      StartCoroutine(AsyncLoadIntoRealMap());
    }
    else
    {
      Debug.Log("Destination out of range");
    }

  }

  private IEnumerator AsyncLoadIntoTutMap()
  {
    AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Tutorial_Entry");

    while (!asyncLoad.isDone)
    {
      yield return null;
    }
  }

  private IEnumerator AsyncLoadIntoTutGame()
  {
    AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Tutorial_Game");

    while (!asyncLoad.isDone)
    {
      yield return null;
    }
  }

  private IEnumerator AsyncLoadIntoRealMap()
  {
    AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Map_t");

    while (!asyncLoad.isDone)
    {
      yield return null;
    }
  }

  private void CheckTouch()
  {
    if (Input.touchCount > 0)
    {
      if (Input.touchCount == 1)
      {
        // Touch Started
        if (Input.GetTouch(0).phase == TouchPhase.Began)
        {
          currTutorialStage += 0.5f;
        }
      }
    }
  }

  private void SetActiveDialogueBox(bool desActive)
  {
    for (int i = 0; i < currDiaBox.transform.childCount; i++)
    {
      currDiaBox.transform.GetChild(i).gameObject.SetActive(desActive);
    }
  }

  private void CreateDialogueBox()
  {
    GameObject desCanvas = GameObject.Find("Tut_UI");
    if (desCanvas != null)
    {
      currDiaBox = Instantiate(dialoguePrefab, desCanvas.transform);
      currDiaBox.transform.SetSiblingIndex(0);
    } else
    {
      desCanvas = GameObject.Find("Player UI");
      if (desCanvas != null)
      {
        currDiaBox = Instantiate(dialoguePrefab, desCanvas.transform);
      }
    }
  }

  static public void GetCombo(GridScript grid, int blockCount)
  {
    currGrid = grid;
    comboValue = blockCount;
  }

  static public void GetPowerUsed(bool powerUseStatus)
  {
    wasPowerUsed = powerUseStatus;
  }

  static public void ResetTutorial()
  {
    comboValue = 0;
    wasPowerUsed = false;
    stageTracker.SetDialogueIndex(startGameDialogue);
    currTutorialStage = startGameTutorialStage;
  }

  public void SkipTutorial()
  {
    SetTutorialStage(finalTutorialStage);
    StartLoad(2);
  }

  static public void SetTutorialStage(float desStage)
  {
    currTutorialStage = desStage;
  }

  private void SetDialogueIndex(int desIndex)
  {
    dialogueControl.SetDialogueIndex(desIndex);
  }

  private void SetDialogueObjects()
  {
    dialogueControl.DialoguePanel = currDiaBox;
    Image[] diaImgs = currDiaBox.GetComponentsInChildren<Image>();
    foreach (Image child in diaImgs)
    {
      if (child.name == "Character Image")
      {
        dialogueControl.characterImage = child;
        charImg = child;
        break;
      }
    }
    TMP_Text[] diaText = currDiaBox.GetComponentsInChildren<TMP_Text>();
    foreach (TMP_Text child in diaText)
    {
      if (child.name == "Name")
      {
        dialogueControl.characterName = child;
      }
      else if (child.name == "Dialogue")
      {
        dialogueControl.dialogueText = child;
      }
    }
  }
}
