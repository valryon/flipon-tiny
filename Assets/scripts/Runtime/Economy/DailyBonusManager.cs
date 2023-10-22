using UnityEngine;
using System;
using System.IO;

public class DailyBonusManager : MonoBehaviour
{
    private const string DAILY_BONUS_FILENAME = "dailybonus.dat";
    private const int BASE_DAILY_LIMIT = 1000;
    private const int BASE_DAILY_BONUS = 100;

    public int DailyLimit { get; private set; }
    public int DailyPointsEarned { get; private set; }
    public int DailyStreak { get; private set; }
    private DateTime lastPlayedDate;

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
    }

    public bool CanEarnPoints(int amount)
    {
        return DailyPointsEarned + amount <= DailyLimit;
    }

    public void AwardDailyBonus()
    {
        if (DateTime.Now.Date != lastPlayedDate.Date)
        {
            if (DateTime.Now.Date == lastPlayedDate.AddDays(1).Date)
            {
                DailyStreak++;
            }
            else
            {
                DailyStreak = 1;
            }

            DailyLimit = BASE_DAILY_LIMIT + BASE_DAILY_BONUS * DailyStreak;
            DailyPointsEarned = 0;

            CurrencyManager.Instance.AddCurrencyWithLimit(BASE_DAILY_BONUS * DailyStreak);
        }
        lastPlayedDate = DateTime.Now.Date;

        SaveData();
    }

    public void AddPoints(int amount) // keep track of daily points limit
    {
        DailyPointsEarned += amount;
        SaveData();
    }

    private void SaveData()
    {
        string path = Path.Combine(Application.persistentDataPath, DAILY_BONUS_FILENAME);
        var data = new DailyBonusData
        {
            DailyStreak = DailyStreak,
            DailyPointsEarned = DailyPointsEarned,
            LastPlayedDate = lastPlayedDate
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
            DailyPointsEarned = data.DailyPointsEarned;
            lastPlayedDate = data.LastPlayedDate;
        }
        else
        {
            DailyStreak = 0;
            DailyPointsEarned = 0;
            lastPlayedDate = DateTime.MinValue;
        }
    }

    [Serializable]
    private class DailyBonusData
    {
        public int DailyStreak;
        public int DailyPointsEarned;
        public DateTime LastPlayedDate;
    }
}
