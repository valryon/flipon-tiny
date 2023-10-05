using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    private int cUnits; //currency units
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
    }

    void Update()
    {
        
    }

    public void AddCurrency(int amount)
    {
        LoadCurrency();
        cUnits += amount;
        SaveCurrency();
        OnCurrencyChanged?.Invoke();
    }

    public bool RemoveCurrency(int amount)
    {
        LoadCurrency();
        if(CanAfford(amount)) 
        {
            cUnits -= amount;
            SaveCurrency();
            OnCurrencyChanged?.Invoke();
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
