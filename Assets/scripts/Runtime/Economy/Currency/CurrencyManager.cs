using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using TMPro;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    private int cUnits; //currency units
    public TextMeshProUGUI currencyText;
    public delegate void CurrencyChange();
    public event CurrencyChange OnCurrencyChanged;

    private const string CURRENCY_FILENAME = "currency.dat";

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
        currencyText.text = cUnits.ToString();
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
        string path = Path.Combine(Application.persistentDataPath, CURRENCY_FILENAME);
        File.WriteAllText(path, cUnits.ToString());
    }

    void LoadCurrency()
    {
        string path = Path.Combine(Application.persistentDataPath, CURRENCY_FILENAME);
        if (File.Exists(path))
        {
            string content = File.ReadAllText(path);
            if (int.TryParse(content, out int loadedCurrency))
            {
                cUnits = loadedCurrency;
            }
            else
            {
                Debug.LogError("Failed to parse saved currency data.");
            }
        }
    }
}
