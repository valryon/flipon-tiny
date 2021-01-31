using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
  /// <summary>
  /// Trigger an event on press instead of on click/tap
  /// </summary>
  [RequireComponent(typeof(Button))]
  public class PressButtonScript : MonoBehaviour
  {
    public UnityEvent onPress = new UnityEvent();

    private bool isPointerDown = false;

    private void Update()
    {
      if (isPointerDown)
      {
        onPress.Invoke();
      }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
      isPointerDown = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
      isPointerDown = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
      isPointerDown = false;
    }

    void OnDisable()
    {
      isPointerDown = false;
    }

    void OnDestroy()
    {
      isPointerDown = false;
    }
  }
}