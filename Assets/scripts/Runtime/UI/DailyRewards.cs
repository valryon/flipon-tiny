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
    [System.Serializable]
    public class LoginData
    {
        public bool firstTimePlaying;
        public bool claimedReward;
        public string dateLastOpened; // calendar date (1st - 31st)
        public int dayLastOpened; // reward day number (Day 1 - 7)
        public int daysRemaining;
    }

    private bool isNewDay;
    private float currentTime;
    private float resetTime;

    public bool firstTimePlaying; // local bool to track if it's still the player's first time playing/seeing rewards
    private int currentDay;
    private int daysRemaining;
    private bool claimedReward;

    private string timeGameOpened;
    public string currentDate;
    private string currentTimeInGame;
    // private string timeGameClosed;

    // buttons
    public Button[] rewardButtons = new Button[7];

    // data
    private string savePath;


    // Start is called before the first frame update
    void Start()
    {
        // create a save path for data file (data has not been saved yet)
        savePath = Path.Combine(Application.persistentDataPath, "loginData.dat");
        print(savePath);

        firstTimePlaying = CheckFirstTimePlaying(); // returns boolean from login data
        // firstTimePlaying = true;

        // DateTime start = DateTime.UtcNow; // use for first time playing ? ? ? ?

        // check if user's first time opening game (need to track this in the title screen, store it/set boolean, check data here)
        if (firstTimePlaying)
        {
            print("HEREEEEEEEEHASFKJSDKF");
            // save some inital data here
            LoginData firstDayData = new LoginData();
            // track that it is currently the player's first time playing, this will change when they collect their reward
            firstDayData.firstTimePlaying = true;
            // at this point, they have not collected anything 
            firstDayData.claimedReward = false;
            // current date is stored ? to track when they have opened the game. Might need to do this at the end of gameplay 
            firstDayData.dateLastOpened = DateTime.Now.ToString("d");
            // current day is DAY 1 for reward (first day)
            firstDayData.dayLastOpened = 1;
            // days remaining for the whole week (includes today)
            firstDayData.daysRemaining = 7;

            // DATA IS SAVED
            SaveLoginData(firstDayData);
            print("DATA SAVED");

            // set 1st day rewards (should replace this with reset rewards)
            ResetRewards();
        }
        // if not first time opening, check how much time has passed since last log-in
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

                    // check if claimed reward
                    claimedReward = data.claimedReward;

                    // if only one day has passed, WE ARE GOOD YAY
                    if (daysPassed == 1)
                    {
                        if (claimedReward)
                        {
                            currentDay = data.dayLastOpened++; // current day is updated 
                            if (currentDay > 7)
                            {
                                currentDay = 1;
                            }
                            UnlockReward(currentDay); // only unlock the NEXT day IF they have already claimed the reward, otherwise keep the original day unlocked
                        }
                        daysRemaining = 7 - currentDay;
                    }
                    // if MORE THAN ONE DAY has passed, calculate days remaining in the week OR reset
                    else if (daysPassed > 1)
                    {
                        currentDay = data.dayLastOpened; // reward they should be still on
                        daysRemaining = 7 - currentDay - daysPassed;
                        // days remaining == 1 means they still have one more day in the week 
                    }
                }
                // same day, coming back to the game
                else
                {
                    // check if they have claimed their reward already or not
                    claimedReward = data.claimedReward;
                    if (claimedReward)
                    {
                        // red notification needs to be enabled
                        print("Your reward was already claimed for today!");
                        
                    }
                    LoadRewards(data.claimedReward, data.dayLastOpened); // WORKS YAY
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
    public void ClaimReward(int rewardAmount) // pass login data here, need to update so reward amount is in an array or something else that is editable
    {
        // check if user is able to claim reward
        // check if user already claimed reward
        // check if user is on the correct reward 

        // current button
        // Button currButton = rewardButtons[currentDay - 1];

        if (!claimedReward)
        {
            // gain currency (use michael's system here)
            print(rewardAmount);

            print(currentDay);
            // make button no longer interactable 
            rewardButtons[currentDay - 1].interactable = false;

            // change image of button
            // currButton.GetComponent<Image>() = 

            // make sure color is normal?
            /*
            var colors = btn.colors;
            colors.pressedColor = new Color(0f, 0f, 0f, 0f);
            btn.colors = colors;
            */

            // remove notification/red dot thing

            claimedReward = true; // NOTE FOR LATER: NEED TO UPDATE THE DATA SOMEHOW
            firstTimePlaying = false;

            // load data, update it, and save
            LoginData data = LoadLoginData();
            data.claimedReward = claimedReward;
            data.firstTimePlaying = firstTimePlaying;
            SaveLoginData(data);
        }
    }

    public void UnlockReward(int currentDay)
    {
        // get button and set it to interactable 
        if (currentDay == 1)
        {
            ResetRewards();
        }
        else
        {
            rewardButtons[currentDay - 1].interactable = true;
        }
    }

    // NOTE FOR LATER: NEED TO EDIT ICONS IN THIS FUNCTION, DIFFERENT THAN JUST UNLOCKING I THINK IDK
    public void LoadRewards(bool claimedReward, int currentDay)
    {
        // if claimed reward, everything is not interactable but some buttons need a different icon
        if (claimedReward)
        {
            for (int i = 0; i < rewardButtons.Length; i++)
            {
                rewardButtons[i].interactable = false;
                if (i + 1 <= currentDay) // include the current day
                {
                    // change the icon to "collected" or soemthing

                }
            }
        }
        // if not claimed reward, only unlock the current day reward and make sure icons are correct 
        else
        {
            for (int i = 0; i < rewardButtons.Length; i++)
            {
                rewardButtons[i].interactable = false;
                if (i + 1 < currentDay) // exclude the current day
                {
                    // change the icon to "collected" or soemthing
                }
            }
            UnlockReward(currentDay); // unlock the current day
        }
    }

    // runs when past the 7th day
    public void ResetRewards()
    {
        currentDay = 1;
        daysRemaining = 7;
        rewardButtons[0].interactable = true; // day 1 button
        for (int i = 1; i < rewardButtons.Length; i++)
        {
            rewardButtons[i].interactable = false; // all other buttons
        }
    }

    public bool CheckFirstTimePlaying()
    {
        LoginData data = LoadLoginData();
        if (data != null)
        {
            print("HERE RETURNING DATA FROM LOADED FILE HERE HER HERE RHEHEHR");
            return data.firstTimePlaying;
        }
        else
        {
            print("RETURNING TRUE: FIRSTTIMEPLAYING");
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