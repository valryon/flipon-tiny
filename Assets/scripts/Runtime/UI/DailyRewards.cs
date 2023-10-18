using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


    // Start is called before the first frame update
    void Start()
    {
        // DateTime start = DateTime.UtcNow; // use for first time playing ? ? ? ?
        
        // check if user's first time opening game (need to track this in the title screen, store it/set boolean, check data here)
        if (firstTimePlaying)
        {
            currentDay = 1;
        }
        // if not first time opening, proceed as normal
        else
        {
            // get current time 
            timeGameOpened = DateTime.Now.ToString("HH:mm");
            // check if new day
            // if (timeGameOpened[:2])
        }

        // reset time is midnight
        // resetTime = 

    }

    // Update is called once per frame
    void Update()
    {
        currentTimeInGame = DateTime.Now.ToString("HH");
        // print(currentTimeInGame);

        int currHour = int.Parse(currentTimeInGame);

        // if the hour has passed 00:00 and was not tracked that it is a new day, it is now a new day
        if (currHour >= 0 && !isNewDay)
        {
            isNewDay = true;

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
        // otherwise, we are still in the same day 
    }

    // runs when user clicks on button
    public void ClaimReward()
    {
        // check if user is able to claim reward
        // check if user already claimed reward
        // check if user is on the correct reward 
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
}
