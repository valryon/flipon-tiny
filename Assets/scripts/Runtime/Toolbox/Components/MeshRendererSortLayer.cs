using UnityEngine;

namespace Pon
{
  [RequireComponent(typeof(Renderer))]
  public class MeshRendererSortLayer : MonoBehaviour
  {
    [SerializeField]
    public string layerName = "UI";

    [SerializeField]
    public int sortingOrder = 1000;

    void Start()
    {
      SetLayer();
    }

    public void SetLayer()
    {
      var r = GetComponent<Renderer>();
      r.sortingLayerName = layerName;
      r.sortingOrder = sortingOrder;
    }
  }
}