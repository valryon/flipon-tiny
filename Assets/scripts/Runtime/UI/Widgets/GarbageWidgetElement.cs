using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pon
{
  public class GarbageWidgetElement : MonoBehaviour
  {
    public Image fill;
    public TextMeshProUGUI text;

    public GridScript.GarbageStored GarbageStored { get; set; }
  }
}