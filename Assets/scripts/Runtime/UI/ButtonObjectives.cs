using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonObjectives : MonoBehaviour
{
    // OBJECTIVES
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
        MapUIScript.mapInstance.numStartingLines = numStartingLines;
    }
}
