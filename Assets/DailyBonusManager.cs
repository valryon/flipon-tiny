using UnityEngine;
using System;
using System.IO;
using TMPro;

public class DailyBonusManager : MonoBehaviour
{
    private const string DAILY_BONUS_FILENAME = "dailybonus.dat";
    private const int BASE_DAILY_LIMIT = 1000;
    private const int BASE_DAILY_BONUS = 100;

    public int DailyLimit;
    public int DailyPointsEarned;
    public int DailyStreak;
    private TextMeshProUGUI dailyPointsText;
    public DateTime lastPlayedDate;

    public static DailyBonusManager Instance;

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

        LoadData();
        // Initialize DailyLimit after loading the data
        DailyLimit = BASE_DAILY_LIMIT + BASE_DAILY_BONUS * Math.Min(1,DailyStreak);
        UpdateDailyPointsText();
    }

    public void UpdateDailyPointsText()
    {
        if (dailyPointsText != null)
        {
            Debug.Log("Updating Daily Points Text to be "+ DailyPointsEarned + " / " + DailyLimit);
            dailyPointsText.text = $"{DailyPointsEarned} / {DailyLimit}";
        }
    }

    public void SetDailyTextReference(TextMeshProUGUI textRef)
    {
        dailyPointsText = textRef;
        UpdateDailyPointsText();
    }

    public bool CanEarnPoints(int amount)
    {
        return DailyPointsEarned + amount <= DailyLimit;
    }

    public void AwardDailyBonus()
    {
        if (DateTime.Now.Date != lastPlayedDate.Date)
        {
            Debug.Log("New Day, awarding daily bonus!");
            if (DateTime.Now.Date == lastPlayedDate.AddDays(1).Date)
            {
                DailyStreak++;
                Debug.Log("Daily streak is now " + DailyStreak);
            }
            else
            {
                DailyStreak = 1;
                Debug.Log("Daily streak is now " + DailyStreak);
            }

            DailyLimit = BASE_DAILY_LIMIT + BASE_DAILY_BONUS * DailyStreak;
            DailyPointsEarned = 0;

            CurrencyManager.Instance.AddCurrencyWithLimit(BASE_DAILY_BONUS * DailyStreak);
        }
        lastPlayedDate = DateTime.Now.Date;

        SaveData();
        UpdateDailyPointsText();
    }

    public void AddPoints(int amount) // keep track of daily points limit
    {
        DailyPointsEarned += amount;
        SaveData();
        UpdateDailyPointsText();
    }

    public void SaveData()
    {
        string path = Path.Combine(Application.persistentDataPath, DAILY_BONUS_FILENAME);
        var data = new DailyBonusData
        {
            DailyStreak = DailyStreak,
            DailyPointsEarned = DailyPointsEarned,
            LastPlayedDateString = lastPlayedDate.ToString("yyyy-MM-dd") // Store date in "yyyy-MM-dd" format
        };
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(path, json);
    }
    private void LoadData()
    {
        string path = Path.Combine(Application.persistentDataPath, DAILY_BONUS_FILENAME);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            var data = JsonUtility.FromJson<DailyBonusData>(json);
            DailyStreak = data.DailyStreak;

            // Parse the DateTime from the stored string
            lastPlayedDate = DateTime.ParseExact(data.LastPlayedDateString, "yyyy-MM-dd", null);

            if (DateTime.Now.Date != lastPlayedDate.Date)
            {
                DailyPointsEarned = 0;
                Debug.Log("New day, setting daily points earned to 0.");
            }
            else
            {
                DailyPointsEarned = data.DailyPointsEarned;
                Debug.Log("Resuming today, loading daily points earned.");
            }
        }
        else
        {
            DailyStreak = 0;
            DailyPointsEarned = 0;
            lastPlayedDate = DateTime.Now.Date;
            SaveData();
        }
    }

    [Serializable]
    private class DailyBonusData
    {
        public int DailyStreak;
        public int DailyPointsEarned;
        public string LastPlayedDateString; // Store date as a string
    }

}
