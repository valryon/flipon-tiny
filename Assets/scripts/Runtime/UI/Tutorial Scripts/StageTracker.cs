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

  static public float finalTutorialStage = 34.5f; // Change this value to match last case in tutorial switch statement if dialogue and tutorial steps change
  static float startGameTutorialStage = 16f; // Change this value to match case where gameplay is started
  static int startGameDialogue = 16;  // Change this value to the index value of the gameplay start dialogue line

  float[] tutorialActionCases = new float[] { startGameTutorialStage, 20f, 23f, 26f, 28f }; // Change this value to each of the starting cases of each game tutorial step, e.g. 3 combo, 4 combo, 5 combo, etc.
  int[] tutorialDialogueIndex = new int[] { startGameDialogue, 18, 20, 22, 23 };
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
        // Dark Clouds
        dialogueControl.DisplayNextSentenceMultiCharacters();
        charImg.enabled = false;
        currTutorialStage += 0.5f;
        break;
      case 4.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 5f:
        // LOS appears
        charImg.enabled = true;
        dialogueControl.DisplayNextSentenceMultiCharacters();
        currTutorialStage += 0.5f;
        break;
      case 5.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 6f:
        // LOS 8
        dialogueControl.DisplayNextSentenceMultiCharacters();
        currTutorialStage += 0.5f;
        break;
      case 6.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 7f:
        // LOS 9
        dialogueControl.DisplayNextSentenceMultiCharacters();
        currTutorialStage += 0.5f;
        break;
      case 7.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 8f:
        // LOS 10
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
        // LOS disappear
        dialogueControl.DisplayNextSentenceMultiCharacters();
        charImg.enabled = false;
        currTutorialStage += 0.5f;
        break;
      case 10.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 11f:
        // CK 13
        charImg.enabled = true;
        dialogueControl.DisplayNextSentenceMultiCharacters();
        currTutorialStage += 0.5f;
        break;
      case 11.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 12f:
        // CK 14
        dialogueControl.DisplayNextSentenceMultiCharacters();
        currTutorialStage += 0.5f;
        break;
      case 12.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 13f:
        // CK 15
        dialogueControl.DisplayNextSentenceMultiCharacters();
        currTutorialStage += 0.5f;
        break;
      case 13.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 14f:
        // CK 16
        dialogueControl.DisplayNextSentenceMultiCharacters();
        currTutorialStage += 0.5f;
        break;
      case 14.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 15f:
        // CK 17
        dialogueControl.DisplayNextSentenceMultiCharacters();
        currTutorialStage += 0.5f;
        break;
      case 15.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 16f:
        // enter game, used to be 3
        currTutorialStage += 0.5f;
        StartLoad(1);
        break;
      case 16.5f:
        if (SceneManager.GetActiveScene().name == "Tutorial_Game")
        {
          currTutorialStage += 0.5f;
          CreateDialogueBox();
          SetDialogueObjects();
          gameScript = PonGameScript.instance;
          gameScript.isTutorial = true;
          gameScript.SetPause(true);
          if (passedTutorialAction != 0)
          {
            currTutorialStage = tutorialActionCases[passedTutorialAction];
            SetDialogueIndex(tutorialDialogueIndex[passedTutorialAction]);
          }
        }
        break;
      case 17f:
        // game loaded, game start dialogue
        // 3 Combo
        currTutorialStage += 0.5f;
        dialogueControl.DisplayNextSentenceMultiCharacters();
        SetActiveDialogueBox(true);
        break;
      case 17.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 18f:
        //Unpausing and starting first task
        currTutorialStage += 0.5f;
        SetActiveDialogueBox(false);
        gameScript.SetPause(false);
        break;
      case 18.5f:
        // player does task
        //Waiting for player to finish first task
        if(comboValue == 3)
        {
          currTutorialStage += 0.5f;
          comboValue = 0;
          passedTutorialAction = 1;
        }
        break;
      case 19f:
        // 3 Combo Congrats
        currTutorialStage += 0.5f;
        dialogueControl.DisplayNextSentenceMultiCharacters();
        SetActiveDialogueBox(true);
        gameScript.SetPause(true);
        break;
      case 19.5f:
        //Waiting for player to click through dialogue
        CheckTouch();
        break;
      case 20f:
        // 4 Combo
        dialogueControl.DisplayNextSentenceMultiCharacters();
        currTutorialStage += 0.5f;
        break;
      case 20.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 21f:
        //Unpause game
        //Unpausing and starting second task
        currTutorialStage += 0.5f;
        SetActiveDialogueBox(false);
        gameScript.SetPause(false);
        break;
      case 21.5f:
        //player does task
        //Waiting for player to finish second task
        if (comboValue == 4)
        {
          currTutorialStage += 0.5f;
          comboValue = 0;
          passedTutorialAction = 2;
        }
        break;
      case 22f:
        // 4 Combo Congrats
        currTutorialStage += 0.5f;
        dialogueControl.DisplayNextSentenceMultiCharacters();
        SetActiveDialogueBox(true);
        gameScript.SetPause(true);
        break;
      case 22.5f:
        //Waiting for player to click through dialogue
        CheckTouch();
        break;
      case 23f:
        // 5 Combo
        dialogueControl.DisplayNextSentenceMultiCharacters();
        currTutorialStage += 0.5f;
        break;
      case 23.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 24f:
        //Unpause game
        //Unpausing and starting third task
        currTutorialStage += 0.5f;
        SetActiveDialogueBox(false);
        gameScript.SetPause(false);
        break;
      case 24.5f:
        //player does task
        //Waiting for player to finish third task
        if (comboValue == 5)
        {
          currTutorialStage += 0.5f;
          comboValue = 0;
          passedTutorialAction = 3;
        }
        break;
      case 25f:
        // 5 Combo Congrats
        currTutorialStage += 0.5f;
        dialogueControl.DisplayNextSentenceMultiCharacters();
        SetActiveDialogueBox(true);
        gameScript.SetPause(true);
        break;
      case 25.5f:
        //Waiting for player to click through dialogue
        CheckTouch();
        break;
      case 26f:
        // L Combo
        dialogueControl.DisplayNextSentenceMultiCharacters();
        currTutorialStage += 0.5f;
        break;
      case 26.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 27f:
        //Unpause game
        //Unpausing and starting fourth task
        currTutorialStage += 0.5f;
        SetActiveDialogueBox(false);
        gameScript.SetPause(false);
        break;
      case 27.5f:
        //player does task
        //Waiting for player to finish fourth task
        if (comboValue >= 6)
        {
          currTutorialStage += 0.5f;
          comboValue = 0;
          passedTutorialAction = 4;
        }
        break;
      case 28f:
        // Bomb
        currTutorialStage += 0.5f;
        dialogueControl.DisplayNextSentenceMultiCharacters();
        SetActiveDialogueBox(true);
        gameScript.SetPause(true);
        break;
      case 28.5f:
        //Waiting for player to click through dialogue
        CheckTouch();
        break;
      case 29f:
        //Unpause game
        //Unpausing and starting fifth task
        currTutorialStage += 0.5f;
        SetActiveDialogueBox(false);
        currGrid.PowerCharge = 1f;
        gameScript.SetPause(false);
        wasPowerUsed = false;
        break;
      case 29.5f:
        //player does task
        //Waiting for player to finish fifth task
        if (wasPowerUsed)
        {
          currTutorialStage += 0.5f;
          wasPowerUsed = false;
        }
        break;
      case 30f:
        // Bomb Congrats
        currTutorialStage += 0.5f;
        dialogueControl.DisplayNextSentenceMultiCharacters();
        SetActiveDialogueBox(true);
        gameScript.SetPause(true);
        break;
      case 30.5f:
        CheckTouch();
        break;
      case 31f:
        // load map
        currTutorialStage += 0.5f;
        StartLoad(0);
        break;
      case 31.5f:
        if (SceneManager.GetActiveScene().name == "Tutorial_Entry")
        {
          currTutorialStage += 0.5f;
        }
        break;
      case 32f:
        // Sendoff one
        currTutorialStage += 0.5f;
        CreateDialogueBox();
        SetDialogueObjects();
        dialogueControl.DisplayNextSentenceMultiCharacters();
        SetActiveDialogueBox(true);
        break;
      case 32.5f:
        //Waiting for player to click through Send-Off Dialogue
        CheckTouch();
        break;
      case 33f:
        // Sendoff two
        dialogueControl.DisplayNextSentenceMultiCharacters();
        currTutorialStage += 0.5f;
        break;
      case 33.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 34f:
        // load game
        currTutorialStage += 0.5f;
        dialogueControl.enabled = false;
        StartLoad(2);
        break;
      case 34.5f:
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
