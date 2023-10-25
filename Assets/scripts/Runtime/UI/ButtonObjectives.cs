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
    public int fiveCombos;
    public int LCombos;
    public int timesPowerUsed;
    public int numBlueBlockBroken;
    public int numBlock2Broken;
    public int numRedBlockBroken;
    public int numPinkBlockBroken;
    public int numBlock5Broken;
    public int numYellowBlock6Broken;

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
        if (fiveCombos > 0)
        {
            MapUIScript.mapInstance.fiveCombos = fiveCombos;
        }
        else
        {
            MapUIScript.mapInstance.fiveCombos = 0;
        }
        if (LCombos > 0)
        {
            MapUIScript.mapInstance.LCombos = LCombos;
        }
        else
        {
            MapUIScript.mapInstance.LCombos = 0;
        }

        if (timesPowerUsed > 0)
        {
            MapUIScript.mapInstance.timesPowerUsed = timesPowerUsed;
        }
        else
        {
            MapUIScript.mapInstance.timesPowerUsed = 0;
        }

        if (numBlueBlockBroken > 0)
        {
            MapUIScript.mapInstance.numBlock1Broken = numBlueBlockBroken;
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

        if (numRedBlockBroken > 0)
        {
            MapUIScript.mapInstance.numBlock3Broken = numRedBlockBroken;
        }
        else
        {
            MapUIScript.mapInstance.numBlock3Broken = 0;
        }

        if (numPinkBlockBroken > 0)
        {
            MapUIScript.mapInstance.numBlock4Broken = numPinkBlockBroken;
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

        if (numYellowBlock6Broken > 0)
        {
            MapUIScript.mapInstance.numBlock6Broken = numYellowBlock6Broken;
        }
        else
        {
            MapUIScript.mapInstance.numBlock6Broken = 0;
        }
    }
}
