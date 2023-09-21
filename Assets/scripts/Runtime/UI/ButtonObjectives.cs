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
    // crystal type

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
    }
}
