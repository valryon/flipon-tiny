using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTouchDetection : MonoBehaviour
{
  [SerializeField] Transform backgroundObj;

  RaycastHit2D rayTouchPos;
  bool boolDragging = false;
  Vector2 startPos;

  // screenFloor is the lowest the screen should scroll, whereas screenCeil is the highest the screen should scroll
  [SerializeField] Transform screenFloor;
  [SerializeField] Transform screenCeil;

  [SerializeField] Transform lvlParent;
  //[SerializeField] Canvas mainCanvas;
  MapUIScript mapManager;

  private void Awake()
  {
    mapManager = FindFirstObjectByType<MapUIScript>();
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
          rayTouchPos = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position), Vector2.down);

          if (rayTouchPos.transform != null)
          {
            if (rayTouchPos.transform.parent == lvlParent)
            {
              MapLvlButton lvlButton = rayTouchPos.transform.GetComponent<MapLvlButton>();
              if (lvlButton.GetUnlocked())
              {
                // set objectives 
                lvlButton.GetComponent<ButtonObjectives>().setObjectives();
                // play level
                mapManager.PlayLevel(lvlButton.GetLevel());
              }
            }
            else if (rayTouchPos.transform == backgroundObj)
            {
              // Start dragging if touching the background
              boolDragging = true;
              startPos = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
            }
          }
        }

        // Touching background or level button AND dragging screen
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
          boolDragging = false;
        }
      }
    }
  }
}
