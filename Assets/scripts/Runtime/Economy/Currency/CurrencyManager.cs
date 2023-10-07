using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    private int cUnits; //currency units
    public TextMeshProUGUI currencyText;
    public delegate void CurrencyChange();
    public event CurrencyChange OnCurrencyChanged;

    public static CurrencyManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (!PlayerPrefs.HasKey("cUnits"))
        {
            cUnits = 0; // or any initial amount you wish
            SaveCurrency();
        }
        else
        {
            LoadCurrency();
        }
        UpdateCurrencyDisplay();
    }

    void Update()
    {
        
    }

    public void UpdateCurrencyDisplay()
    {
        currencyText.text = "Currency: " + cUnits.ToString();
    }

    public void SetCurrencyTextReference(TextMeshProUGUI textRef)
    {
        currencyText = textRef;
        UpdateCurrencyDisplay();
    }

    public void AddCurrency(int amount)
    {
        LoadCurrency();
        cUnits += amount;
        SaveCurrency();
        OnCurrencyChanged?.Invoke();
        UpdateCurrencyDisplay();
    }

    public bool RemoveCurrency(int amount)
    {
        LoadCurrency();
        if(CanAfford(amount)) 
        {
            cUnits -= amount;
            SaveCurrency();
            OnCurrencyChanged?.Invoke();
            UpdateCurrencyDisplay();
            return true;
        }
        else { return false; }
    }

    public bool CanAfford(int amount)
    {
        return cUnits >= amount;
    }

    public int GetCurrentCurrency()
    {
        return cUnits;
    }


    void SaveCurrency()
    {
        PlayerPrefs.SetInt("cUnits", cUnits);
    }

    void LoadCurrency()
    {
        if (PlayerPrefs.HasKey("cUnits"))
        {
            cUnits = PlayerPrefs.GetInt("cUnits");
        }
    }
}
