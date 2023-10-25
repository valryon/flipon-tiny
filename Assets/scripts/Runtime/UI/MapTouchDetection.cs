using System;
using UnityEngine;
using Input = UnityEngine.Input;

public class MapTouchDetection : MonoBehaviour
{
	[SerializeField] Transform backgroundObj;

	RaycastHit2D[] results;
	bool boolDragging = false;
	Vector2 startPos;

	// screenFloor is the lowest the screen should scroll, whereas screenCeil is the highest the screen should scroll
	[SerializeField] Transform screenFloor;
	[SerializeField] Transform screenCeil;

	public Transform lvlParent;

	MapUIScript mapManager;
	LvlUnlockContainer lvlUnlocks;

	Transform cameraTransform;

	float t = 0;
	float SCROLL_SPEED = 3f;
	float scrollDestination;
	bool isScrolling = false;

	ContactFilter2D conFilter;


	private void Start()
	{
		AssignMapScripts();
		cameraTransform = Camera.main.transform;


		if (!mapManager.currentLevelName.Equals(""))
		{
			GameManager.gameManager.LoadUnlocks();
			lvlUnlocks = GameManager.gameManager.lvlUnlocks;
			// prevLvl = Object's assigned level, also equals next level's index; prevLvl - 1 = previous level's index;
			int prevLvl = int.Parse(mapManager.currentLevelName.Remove(0, 5));
			if (prevLvl > 1 && prevLvl <= lvlParent.childCount - 1)
			{
        prevLvl = prevLvl - 1;
      } else if (prevLvl >= lvlParent.childCount)
			{
        prevLvl = lvlParent.childCount - 1;
      }
			MapLvlButton prevButton = lvlParent.GetChild(prevLvl - 1).GetComponent<MapLvlButton>();
			MapLvlButton nextButton = lvlParent.GetChild(prevLvl).GetComponent<MapLvlButton>();
      //Debug.Log("Button To Unlock: " + nextButton.name + "; Previously Played Level: " + prevButton.name);

      if (mapManager.wonLastGame && !nextButton.GetUnlocked() && prevLvl < lvlParent.childCount)
			{
        nextButton.SetUnlocked(true);

				GameManager.gameManager.lvlUnlocks.LvlUnlockStates[prevLvl] = nextButton.GetUnlocked();
				GameManager.gameManager.SaveUnlocks();

				// vv Below handles where camera spawns & scrolls to when scene starts vv
				if (prevLvl > 2 && prevLvl < lvlParent.childCount - 1)
				{
					cameraTransform.position = new Vector3(cameraTransform.position.x, prevButton.transform.position.y, cameraTransform.position.z);
					t = 0;
					scrollDestination = nextButton.transform.position.y;
					isScrolling = true;
				}
				else if (prevLvl >= lvlParent.childCount - 1)
				{
					cameraTransform.position = new Vector3(cameraTransform.position.x, screenCeil.position.y, cameraTransform.position.z);
				}
			}
			else if (!mapManager.wonLastGame || nextButton.GetUnlocked())
			{
				if (prevLvl > 2)
				{
					if (prevLvl < lvlParent.childCount - 1)
					{
						cameraTransform.position = new Vector3(cameraTransform.position.x, prevButton.transform.position.y, cameraTransform.position.z);
					}
					else
					{
						cameraTransform.position = new Vector3(cameraTransform.position.x, screenCeil.position.y, cameraTransform.position.z);
					}
				}
			}
		}
		else
		{
			GameManager.gameManager.SaveUnlocks();
		}

		conFilter.NoFilter();
	}

	void Update()
	{
		if (Input.touchCount > 0)
		{
			if (Input.touchCount == 1)
			{
				// Touch Started
				if (Input.GetTouch(0).phase == TouchPhase.Began)
				{
					results = new RaycastHit2D[2];
					Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position), Vector2.zero, conFilter, results);
					foreach (RaycastHit2D result in results)
					{
						if (result.transform != null)
						{
							if (result.transform.IsChildOf(lvlParent))
							{
								MapLvlButton lvlButton = result.transform.GetComponent<MapLvlButton>();
								if (lvlButton.GetUnlocked())
								{
									// set objectives 
									lvlButton.GetComponent<ButtonObjectives>().setObjectives();
									// play level
									mapManager.PlayLevel(lvlButton.GetLevel());
									return;
								}
								else
								{
									// level not unlocked
									return;
								}
							}
						}
					}
					foreach (RaycastHit2D result in results)
					{
						if (result.transform == backgroundObj)
						{
							// Start dragging if touching the background
							boolDragging = true;
							startPos = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
						}
					}
				}

				// Touching background AND dragging screen
				else if (Input.GetTouch(0).phase == TouchPhase.Moved && boolDragging)
				{
					Vector2 direction = startPos - (Vector2)Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
					if ((direction.y < 0 && cameraTransform.position.y >= screenFloor.position.y) || (direction.y > 0 && cameraTransform.position.y <= screenCeil.position.y))
					{
						// Only move the camera if the camera is within acceptable ranges
						cameraTransform.position += new Vector3(0, direction.y, 0);
					}
				}
				else if (Input.GetTouch(0).phase == TouchPhase.Ended)
				{
					// When touch ended, also end drag
					boolDragging = false;
				}
			}
		}

		if (isScrolling)
		{
			ScrollToUnlockedLevel();
		}

	}

	private void ScrollToUnlockedLevel()
	{
		if (t < SCROLL_SPEED)
		{
			float cameraYPos = Mathf.Lerp(cameraTransform.position.y, scrollDestination, t / SCROLL_SPEED);
			cameraTransform.position = new Vector3(cameraTransform.position.x, cameraYPos, cameraTransform.position.z);
		}
		else
		{
			isScrolling = false;
		}

		t += Time.deltaTime;
	}



	public void AssignMapScripts()
	{
		//mapManager = FindFirstObjectByType<MapUIScript>();
		mapManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<MapUIScript>();
		lvlUnlocks = mapManager.gameObject.GetComponent<LvlUnlockContainer>();
	}
}
