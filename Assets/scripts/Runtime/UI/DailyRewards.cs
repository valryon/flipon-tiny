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

    private bool firstTimePlaying; // local bool to track if it's still the player's first time playing/seeing rewards
    private int currentDay;
    private int daysRemaining;
    private bool claimedReward;

    private string timeGameOpened;
    private string currentDate; 
    private string currentTimeInGame;
    // private string timeGameClosed;


    public string testDate; // date that can be changed for testing

    // buttons
    public Button[] rewardButtons = new Button[7];

    // data
    private string savePath;


    // Start is called before the first frame update
    void Start()
    {
        // create a save path for data file (data has not been saved yet)
        savePath = Path.Combine(Application.persistentDataPath, "loginData.dat");

        // returns boolean from login data
        firstTimePlaying = CheckFirstTimePlaying();

        // test time
        DateTime testDateTime =  DateTime.Parse(testDate);

        // check if user's first time opening game (need to track this in the title screen, store it/set boolean, check data here)
        if (firstTimePlaying)
        {
            // save some inital data here
            LoginData firstDayData = new LoginData();
            firstDayData.firstTimePlaying = true;
            firstDayData.claimedReward = false;
            // current date is stored ? to track when they have opened the game. Might need to do this at the end of gameplay 
            firstDayData.dateLastOpened = DateTime.Now.ToString("d");
            firstDayData.dayLastOpened = 1;
            firstDayData.daysRemaining = 7;

            // DATA IS SAVED
            SaveLoginData(firstDayData);
            print("LOGIN DATA SAVED");

            // set 1st day rewards (should replace this with reset rewards)
            ResetRewards();
        }
        // if not first time opening, check how much time has passed since last log-in
        else
        {
            // get current date when game is opened 
            currentDate = DateTime.Now.ToString("d");
            DateTime currDateTime = DateTime.Now;

            // load data to get the date that the game was last opened on 
            LoginData data = LoadLoginData();

            if (data != null) // check that save file exists
            {
                // check if the days are different (one or more days have passed)
                // CHANGE TESTDATE BACK TO DATA.DATELASTOPENED
                if (testDate != currentDate)
                {
                    // test time
                    DateTime dateLastOpenedData = DateTime.Parse(data.dateLastOpened);

                    // NOTE FOR LATER - NEED TO SAVE WHOLE DATE (WITH MONTH ETC.) TO BE ABLE TO CALCULATE TIMESPAN LATER NOT JUST THE DAY
                    // CHANGE TESTDATE BACK TO DATELASTOPENEDDATA
                    int daysPassed = CalculateDateChange(currDateTime, testDateTime); // how many days have passed

                    // check if claimed reward
                    claimedReward = data.claimedReward;

                    // if only one day has passed, WE ARE GOOD YAY
                    if (daysPassed == 1)
                    {
                        print("ONE DAY PASSED");
                        if (claimedReward)
                        {
                            // reward was already claimed last time, just load the next day reward
                            currentDay = ++data.dayLastOpened; // current day is updated 
                            print("CURRENT REWARD DAY IS");
                            print(currentDay);
                            if (currentDay > 7)
                            {
                                currentDay = 1;
                                daysRemaining = 7;
                            }
                            else
                            {
                                daysRemaining = 8 - currentDay;
                            }
                            claimedReward = UnlockReward(currentDay); // only unlock the NEXT day IF they have already claimed the reward, otherwise keep the original day unlocked
                            // change it in the data too and save
                            
                        }
                        else
                        {
                            // have not yet claimed reward, keep last day's reward unlocked only and decrement days remaining
                        }
                        
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
                    LoadRewards(); // WORKS YAY
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

                // UnlockReward(currentDay);
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

    public bool UnlockReward(int currentDay)
    {
        print(currentDay);
        // get button and set it to interactable 
        if (currentDay == 1)
        {
            ResetRewards();
        }
        else
        {
            for (int i = 0; i < rewardButtons.Length; i++)
            {
                rewardButtons[i].interactable = false;
            }
            rewardButtons[currentDay - 1].interactable = true;
        }

        // load data, update it, and save
        LoginData data = LoadLoginData();
        data.claimedReward = false;
        SaveLoginData(data);

        // update data, reward is no longer claimed bc it just unlocked
        return false;
    }


    // NOTE FOR LATER: NEED TO EDIT ICONS IN THIS FUNCTION, DIFFERENT THAN JUST UNLOCKING I THINK IDK
    public void LoadRewards() // WANT TO LOAD EACH TIME THE BUTTON IS PRESSED AGH
    {
        LoginData loadedData = LoadLoginData();
        claimedReward = loadedData.claimedReward;
        currentDay = loadedData.dayLastOpened;

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
            return data.firstTimePlaying;
        }
        else
        {
            return true; // probably have to fix later, but for now we assume that if there's no data saved then it IS the first time they are playing
        }
    }

    public int CalculateDateChange(DateTime currentDate, DateTime previousDate)
    {
        TimeSpan interval = currentDate - previousDate;
        print("Days passed");
        print(interval.Days);
        return interval.Days;
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