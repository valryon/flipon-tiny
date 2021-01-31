using System.Linq;
using UnityEngine;

namespace Pon
{
  public class ChildrenSingleSelector : MonoBehaviour
  {
    void Awake()
    {
      var children = transform.GetChildren();
      if (children.Count > 1)
      {
        children.ForEach(c => c.gameObject.SetActive(false));
        children.PickRandom().gameObject.SetActive(true);
      }
      else
      {
        var c = children.FirstOrDefault();
        if (c != null) c.gameObject.SetActive(true);
      }
    }
  }
}