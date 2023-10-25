using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CurrencyContainerUI : MonoBehaviour
{

    public TextMeshProUGUI currencyText;
    public TextMeshProUGUI dailyPointsText;

    private void Awake()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.SetCurrencyTextReference(currencyText);
        }
        if (DailyBonusManager.Instance != null)
        {
            DailyBonusManager.Instance.SetDailyTextReference(dailyPointsText);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.SetCurrencyTextReference(currencyText);
        }
        if (DailyBonusManager.Instance != null)
        {
            DailyBonusManager.Instance.SetDailyTextReference(dailyPointsText);
        }
    }
}
