using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DailyRewards : MonoBehaviour
{
    private bool isNewDay;
    private float currentTime;
    private float resetTime;

    public bool firstTimePlaying;
    private int currentDay;
    private bool claimedReward;

    private string timeGameOpened;
    private string currentTimeInGame;
    // private string timeGameClosed;

    // buttons
    public Button[] rewardButtons = new Button[7];


    // Start is called before the first frame update
    void Start()
    {
        // DateTime start = DateTime.UtcNow; // use for first time playing ? ? ? ?

        // check if user's first time opening game (need to track this in the title screen, store it/set boolean, check data here)
        if (firstTimePlaying)
        {
            currentDay = 1;
            // set rewards
            rewardButtons[0].interactable = true;
            for (int i = 1; i < rewardButtons.Length; i++)
            {
                rewardButtons[i].interactable = false;
            }

            // no longer first time playing
            // firstTimePlaying = false;
            claimedReward = false;
            isNewDay = false;
        }
        // if not first time opening, proceed as normal
        else
        {
            // get current time 
            timeGameOpened = DateTime.Now.ToString("HH:mm");
            // check if new day
            // if (timeGameOpened[:2])


            // get time that the player last opened the game

            // check if new day when game is reopened

        }

        // reset time is midnight
        // resetTime = 

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

            claimedReward = true;
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

    /*
    private void OnApplicationQuit()
    {
        firstTimePlaying = false; // ?
    }
    */
}