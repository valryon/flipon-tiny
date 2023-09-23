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
    public int powerUps;

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
            // default starting lines
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
    }
}
