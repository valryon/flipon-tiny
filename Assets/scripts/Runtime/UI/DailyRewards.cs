using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class DailyRewards : MonoBehaviour
{
    public class LoginData
    {
        public bool firstTimePlaying;
        public bool claimedReward;
        public string dateLastOpened; // calendar date (1st - 31st)
        public int dayLastOpened; // reward day number (Day 1 - 7)
    }

    private bool isNewDay;
    private float currentTime;
    private float resetTime;

    public bool firstTimePlaying;
    private int currentDay;
    private bool claimedReward;

    private string timeGameOpened;
    private string currentDate;
    private string currentTimeInGame;
    // private string timeGameClosed;

    // buttons
    public Button[] rewardButtons = new Button[7];

    // data
    private string savePath;


    // Start is called before the first frame update
    void Start()
    {
        // create a save path for data file and save initial data 
        savePath = Path.Combine(Application.persistentDataPath, "loginData.dat");

        firstTimePlaying = CheckFirstTimePlaying(); // returns boolean from login data

        // DateTime start = DateTime.UtcNow; // use for first time playing ? ? ? ?

        // check if user's first time opening game (need to track this in the title screen, store it/set boolean, check data here)
        if (firstTimePlaying)
        {
            // save some inital data here
            LoginData firstDayData = new LoginData();
            firstDayData.firstTimePlaying = true;
            firstDayData.claimedReward = false;
            firstDayData.dateLastOpened = DateTime.Now.ToString("d");
            firstDayData.dayLastOpened = 1;
            SaveLoginData(firstDayData);

            // set 1st day rewards
            currentDay = 1;
            rewardButtons[0].interactable = true;
            for (int i = 1; i < rewardButtons.Length; i++)
            {
                rewardButtons[i].interactable = false;
            }
        }
        // if not first time opening, proceed as normal
        else
        {
            // get current date when game is opened 
            currentDate = DateTime.Now.ToString("d");

            // load data to get the date that the game was last opened on 
            LoginData data = LoadLoginData();

            if (data != null) // check that save file exists
            {
                // check if the days are different (one or more days have passed)
                if (data.dateLastOpened != currentDate)
                {
                    // NOTE FOR LATER - NEED TO SAVE WHOLE DATE (WITH MONTH ETC.) TO BE ABLE TO CALCULATE TIMESPAN LATER NOT JUST THE DAY
                    int daysPassed = CalculateDateChange(currentDate, data.dateLastOpened); // how many days have passed
                }
                // same day, coming back to the game
                else
                {
                    claimedReward = data.claimedReward;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // time in game (military time 00:00 - 23:59)
        currentTimeInGame = DateTime.Now.ToString("HH");
        // print(currentTimeInGame);

        int currHour = int.Parse(currentTimeInGame);

        // if the hour has passed 00:00 and was not tracked that it is a new day, it is now a new day
        if (currHour >= 0 && isNewDay)
        {
            if (firstTimePlaying)
            {
                firstTimePlaying = false;
            }
            else
            {
                isNewDay = false;

                // new reward needs to unlock 
                if (currentDay == 7)
                {
                    currentDay = 1;
                }
                else
                {
                    currentDay++;
                }

                UnlockReward(currentDay);
            }
                
        }
        // otherwise, we are still in the same day 
    }

    // runs when user clicks on button
    public void ClaimReward(int rewardAmount)
    {
        // check if user is able to claim reward
        // check if user already claimed reward
        // check if user is on the correct reward 

        // current button
        // Button currButton = rewardButtons[currentDay - 1];

        if (!claimedReward)
        {
            // gain currency
            print(rewardAmount);

            print(currentDay);
            // make button no longer interactable 
            rewardButtons[currentDay - 1].interactable = false;

            // change image of button
            // currButton.GetComponent<Image>() = 

            // make sure color is normal?

            claimedReward = true; // NOTE FOR LATER: NEED TO UPDATE THE DATA SOMEHOW
        }
    }

    public void UnlockReward(int currentDay)
    {
        // get button and set it to interactable 
    }

    // runs when past the 7th day
    public void ResetRewards()
    {
        currentDay = 1;

        // set all buttons to uninteractable except first one
    }

    public bool CheckFirstTimePlaying()
    {
        LoginData data = LoadLoginData();
        if (data != null)
        {
            return data.firstTimePlaying;
        }
        else
        {
            return true; // probably have to fix later, but for now we assume that if there's no data saved then it IS the first time they are playing
        }
    }

    public int CalculateDateChange(string currentDate, string previousDate)
    {
        return 0;
    }

    // data saving and loading (taken from GameManager script)
    public void SaveLoginData(LoginData data)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream fileStream = File.Create(savePath);

        formatter.Serialize(fileStream, data);
        fileStream.Close();
    }

    public LoginData LoadLoginData()
    {
        if (File.Exists(savePath))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream fileStream = File.Open(savePath, FileMode.Open);

            LoginData data = (LoginData)formatter.Deserialize(fileStream);
            fileStream.Close();

            return data;
        }
        else
        {
            Debug.LogWarning("No save file found.");
            return null;
        }
    }

    /*
    private void OnApplicationQuit()
    {
        firstTimePlaying = false; // ?
    }
    */
}