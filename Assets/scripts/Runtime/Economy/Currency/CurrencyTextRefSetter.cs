using TMPro;
using UnityEngine;

public class CurrencyTextRefSetter : MonoBehaviour
{
    private void Start()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.SetCurrencyTextReference(GetComponent<TextMeshProUGUI>());
        }
    }
}
