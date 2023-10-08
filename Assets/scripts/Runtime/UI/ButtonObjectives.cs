using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonObjectives : MonoBehaviour
{
    // OBJECTIVES
    public int timeLimit;
    public int timeToSurvive;
    public int numStartingLines;
    public int score;
    public int combos;
    public int fourCombos;
    public int timesPowerUsed;
    public int numBlock1Broken;
    public int numBlock2Broken;
    public int numBlock3Broken;
    public int numBlock4Broken;
    public int numBlock5Broken;
    public int numBlock6Broken;

    // MAP MANAGER (handles current level and sets objectives in game scene)


    // Start is called before the first frame update
    void Start()
    {
       
    }
    
    public void setObjectives()
    {
        if (numStartingLines > 0)
        {
            MapUIScript.mapInstance.numStartingLines = numStartingLines;
        }
        else
        {
            MapUIScript.mapInstance.numStartingLines = 3;
        }

        if (score > 0)
        {
            MapUIScript.mapInstance.score = score;
        }
        else
        {
            // default score
            MapUIScript.mapInstance.score = 0;
        }

        if (combos > 0)
        {
            MapUIScript.mapInstance.combos = combos;
        }
        else
        {
            MapUIScript.mapInstance.combos = 0;
        }

        if (fourCombos > 0)
        {
            MapUIScript.mapInstance.fourCombos = fourCombos;
        }
        else
        {
            MapUIScript.mapInstance.fourCombos = 0;
        }

        if (timesPowerUsed > 0)
        {
            MapUIScript.mapInstance.timesPowerUsed = timesPowerUsed;
        }
        else
        {
            MapUIScript.mapInstance.timesPowerUsed = 0;
        }

        if (numBlock1Broken > 0)
        {
            MapUIScript.mapInstance.numBlock1Broken = numBlock1Broken;
        }
        else
        {
            MapUIScript.mapInstance.numBlock1Broken = 0;
        }

        if (numBlock2Broken > 0)
        {
            MapUIScript.mapInstance.numBlock2Broken = numBlock2Broken;
        }
        else
        {
            MapUIScript.mapInstance.numBlock2Broken = 0;
        }

        if (numBlock3Broken > 0)
        {
            MapUIScript.mapInstance.numBlock3Broken = numBlock3Broken;
        }
        else
        {
            MapUIScript.mapInstance.numBlock3Broken = 0;
        }

        if (numBlock4Broken > 0)
        {
            MapUIScript.mapInstance.numBlock4Broken = numBlock4Broken;
        }
        else
        {
            MapUIScript.mapInstance.numBlock4Broken = 0;
        }

        if (numBlock5Broken > 0)
        {
            MapUIScript.mapInstance.numBlock5Broken = numBlock5Broken;
        }
        else
        {
            MapUIScript.mapInstance.numBlock5Broken = 0;
        }

        if (numBlock6Broken > 0)
        {
            MapUIScript.mapInstance.numBlock6Broken = numBlock6Broken;
        }
        else
        {
            MapUIScript.mapInstance.numBlock6Broken = 0;
        }
    }
}
