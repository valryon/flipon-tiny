using Pon;
using System.Collections;
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

  static GridScript currGrid;
  PonGameScript gameScript;

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
          Debug.Log("Displaying Narrative Tutorial Dialogue");
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
        // dialogue 1
        Debug.Log("Displaying Dialogue 1");
        dialogueControl.DisplayNextSentenceMultiCharacters();
        currTutorialStage += 0.5f;
        break;
      case 1.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 2f:
        // dialogue 2
        Debug.Log("Displaying Dialogue 2");
        dialogueControl.DisplayNextSentenceMultiCharacters();
        currTutorialStage += 0.5f;
        break;
      case 2.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 3f:
        // enter game
        currTutorialStage += 0.5f;
        StartLoad(1);
        break;
      case 3.5f:
        if (SceneManager.GetActiveScene().name == "Tutorial_Game")
        {
          currTutorialStage += 0.5f;
          CreateDialogueBox();
          SetDialogueObjects();
          gameScript = PonGameScript.instance;
          gameScript.isTutorial = true;
          gameScript.SetPause(true);
        }
        break;
      case 4f:
        // game loaded, game start dialogue
        Debug.Log("Displaying Game Tutorial Dialogue");
        currTutorialStage += 0.5f;
        dialogueControl.DisplayNextSentenceMultiCharacters();
        SetActiveDialogueBox(true);
        break;
      case 4.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 5f:
        Debug.Log("Unpausing and starting first task");
        currTutorialStage += 0.5f;
        SetActiveDialogueBox(false);
        gameScript.SetPause(false);
        break;
      case 5.5f:
        // player does task
        //Debug.Log("Waiting for player to finish first task");
        if(comboValue == 3)
        {
          currTutorialStage += 0.5f;
          comboValue = 0;
        }
        break;
      case 6f:
        // second task dialogue
        Debug.Log("Displaying Second Task Dialogue");
        currTutorialStage += 0.5f;
        dialogueControl.DisplayNextSentenceMultiCharacters();
        SetActiveDialogueBox(true);
        gameScript.SetPause(true);
        break;
      case 6.5f:
        //Waiting for player to click through dialogue
        CheckTouch();
        break;
      case 7f:
        //Unpause game
        Debug.Log("Unpausing and starting second task");
        currTutorialStage += 0.5f;
        SetActiveDialogueBox(false);
        gameScript.SetPause(false);
        break;
      case 7.5f:
        //player does task
        //Debug.Log("Waiting for player to finish second task");
        if (comboValue == 4)
        {
          currTutorialStage += 0.5f;
          comboValue = 0;
        }
        break;
      case 8f:
        // second task dialogue
        Debug.Log("Displaying Third Task Dialogue");
        currTutorialStage += 0.5f;
        dialogueControl.DisplayNextSentenceMultiCharacters();
        SetActiveDialogueBox(true);
        gameScript.SetPause(true);
        break;
      case 8.5f:
        //Waiting for player to click through dialogue
        CheckTouch();
        break;
      case 9f:
        //Unpause game
        Debug.Log("Unpausing and starting third task");
        currTutorialStage += 0.5f;
        SetActiveDialogueBox(false);
        gameScript.SetPause(false);
        break;
      case 9.5f:
        //player does task
        //Debug.Log("Waiting for player to finish third task");
        if (comboValue == 5)
        {
          currTutorialStage += 0.5f;
          comboValue = 0;
        }
        break;
      case 10f:
        // second task dialogue
        Debug.Log("Displaying Fourth Task Dialogue");
        currTutorialStage += 0.5f;
        dialogueControl.DisplayNextSentenceMultiCharacters();
        SetActiveDialogueBox(true);
        gameScript.SetPause(true);
        break;
      case 10.5f:
        //Waiting for player to click through dialogue
        CheckTouch();
        break;
      case 11f:
        //Unpause game
        Debug.Log("Unpausing and starting fourth task");
        currTutorialStage += 0.5f;
        SetActiveDialogueBox(false);
        gameScript.SetPause(false);
        break;
      case 11.5f:
        //player does task
        //Debug.Log("Waiting for player to finish fourth task");
        if (comboValue == 6)
        {
          currTutorialStage += 0.5f;
          comboValue = 0;
        }
        break;
      case 12f:
        // second task dialogue
        Debug.Log("Displaying Fifth Task Dialogue");
        currTutorialStage += 0.5f;
        dialogueControl.DisplayNextSentenceMultiCharacters();
        SetActiveDialogueBox(true);
        gameScript.SetPause(true);
        break;
      case 12.5f:
        //Waiting for player to click through dialogue
        CheckTouch();
        break;
      case 13f:
        //Unpause game
        Debug.Log("Unpausing and starting fifth task");
        currTutorialStage += 0.5f;
        SetActiveDialogueBox(false);
        currGrid.PowerCharge = 1f;
        gameScript.SetPause(false);
        break;
      case 13.5f:
        //player does task
        //Debug.Log("Waiting for player to finish fifth task");
        if (wasPowerUsed)
        {
          currTutorialStage += 0.5f;
          wasPowerUsed = false;
        }
        break;
      case 14f:
        // end of tutorial dialogue
        Debug.Log("Displaying End Game Tutorial Dialogue");
        currTutorialStage += 0.5f;
        dialogueControl.DisplayNextSentenceMultiCharacters();
        SetActiveDialogueBox(true);
        gameScript.SetPause(true);
        break;
      case 14.5f:
        CheckTouch();
        break;
      case 15f:
        // load map
        currTutorialStage += 0.5f;
        StartLoad(0);
        break;
      case 15.5f:
        if (SceneManager.GetActiveScene().name == "Tutorial_Entry")
        {
          currTutorialStage += 0.5f;
        }
        break;
      case 16f:
        Debug.Log("Displaying Send Off Dialogue");
        currTutorialStage += 0.5f;
        CreateDialogueBox();
        SetDialogueObjects();
        dialogueControl.DisplayNextSentenceMultiCharacters();
        SetActiveDialogueBox(true);
        break;
      case 16.5f:
        //Waiting for player to click through Send-Off Dialogue
        CheckTouch();
        break;
      case 17f:
        // load game
        currTutorialStage += 0.5f;
        dialogueControl.enabled = false;
        StartLoad(2);
        break;
      case 17.5f:
        // destroy self
        if (SceneManager.GetActiveScene().name == "Map_t")
        {
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
    currTutorialStage = 0;
  }

  static public void SetTutorialStage(float desStage)
  {
    currTutorialStage = desStage;
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
