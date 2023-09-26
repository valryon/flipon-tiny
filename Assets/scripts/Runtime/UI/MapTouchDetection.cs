using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MapTouchDetection : MonoBehaviour
{
  [SerializeField] Transform backgroundObj;

  RaycastHit2D[] results;
  bool boolDragging = false;
  Vector2 startPos;

  // screenFloor is the lowest the screen should scroll, whereas screenCeil is the highest the screen should scroll
  [SerializeField] Transform screenFloor;
  [SerializeField] Transform screenCeil;

  [SerializeField] Transform lvlParent;
  [SerializeField] MapUIScript mapManager;

  ContactFilter2D conFilter;

  private void Awake()
  {
    if (mapManager == null)
    {
      mapManager = FindFirstObjectByType<MapUIScript>();
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
                Debug.Log(result.transform.name);
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
          } foreach (RaycastHit2D result in results) {
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
          if ((direction.y < 0 && Camera.main.transform.position.y >= screenFloor.position.y) || (direction.y > 0 && Camera.main.transform.position.y <= screenCeil.position.y))
          {
            // Only move the camera if the camera is within acceptable ranges
            Camera.main.transform.position += new Vector3(0, direction.y, 0);
          }
        }
        else if (Input.GetTouch(0).phase == TouchPhase.Ended)
        {
          // When touch ended, also end drag
          boolDragging = false;
        }
      }
    }
  }
}
