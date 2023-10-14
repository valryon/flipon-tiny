using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageTracker : MonoBehaviour
{
  public static StageTracker stageTracker;
  float currTutorialStage = 0f;
  int FINAL_STEP; // Set this value when tutorial steps decided on

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
  }

    private void Update()
  {
    switch (currTutorialStage)
    {
      case 0f:
        // start of tutorial
        Debug.Log("Displaying Narrative Tutorial Dialogue");
        currTutorialStage += 0.5f;
        break;
      case 0.5f:
        //Debug.Log("Waiting for press 0.5");
        CheckTouch();
        break;
      case 1f:
        // dialogue 1
        Debug.Log("Displaying Dialogue 1");
        currTutorialStage += 0.5f;
        break;
      case 1.5f:
        //Debug.Log("Waiting for press 1.5");
        CheckTouch();
        break;
      case 2f:
        // dialogue 2
        Debug.Log("Displaying Dialogue 2");
        currTutorialStage += 0.5f;
        break;
      case 2.5f:
        //Debug.Log("Waiting for press 2.5");
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
        }
        break;
      case 4f:
        // game loaded, game start dialogue
        Debug.Log("Displaying Game Tutorial Dialogue");
        currTutorialStage += 0.5f;
        break;
      case 4.5f:
        //Waiting for player to click through Dialogue
        CheckTouch();
        break;
      case 5f:
        Debug.Log("Unpausing and starting first task");
        currTutorialStage += 0.5f;
        break;
      case 5.5f:
        // player does task
        //Debug.Log("Waiting for player task complete");
        CheckTouch();
        break;
      case 6f:
        // second task dialogue
        Debug.Log("Displaying Second Task Dialogue");
        currTutorialStage += 0.5f;
        break;
      case 6.5f:
        //Waiting for player to click through dialogue
        CheckTouch();
        break;
      case 7f:
        //Unpause game
        Debug.Log("Unpausing and starting second task");
        currTutorialStage += 0.5f;
        break;
      case 7.5f:
        //player does task
        //Debug.Log("Waiting for player to finish second task");
        CheckTouch();
        break;
      case 8f:
        // end of tutorial dialogue
        Debug.Log("Displaying End Game Tutorial Dialogue");
        currTutorialStage += 0.5f;
        break;
      case 8.5f:
        //Debug.Log("Waiting for press 6.5");
        CheckTouch();
        break;
      case 9f:
        // load map
        currTutorialStage += 0.5f;
        StartLoad(0);
        break;
      case 9.5f:
        if (SceneManager.GetActiveScene().name == "Tutorial_Entry")
        {
          currTutorialStage += 0.5f;
        }
        break;
      case 10f:
        Debug.Log("Displaying Send Off Dialogue");
        currTutorialStage += 0.5f;
        break;
      case 10.5f:
        // end send off dialogue
        //Debug.Log("Waiting for press 8.5");
        CheckTouch();
        break;
      case 11f:
        // load game
        currTutorialStage += 0.5f;
        StartLoad(2);
        break;
      case 11.5f:
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

}
