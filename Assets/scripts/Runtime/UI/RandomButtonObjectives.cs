using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomButtonObjectives : ButtonObjectives
{
    public void RandomizeObjectives()
    {
        int numOfObjectives = Random.Range(2, 5); // Randomly choose between 2 and 4 objectives

        for (int i = 0; i < numOfObjectives; i++)
        {
            int objectiveType = Random.Range(0, 9); // Since there are 9 different objectives

            switch (objectiveType)
            {
                case 0:
                    score = GetRandomScore();
                    break;
                case 1:
                    combos = Random.Range(10, 31);
                    break;
                case 2:
                    fourCombos = Random.Range(3, 11);
                    break;
                case 3:
                    LCombos = Random.Range(1, 4);
                    break;
                case 4:
                    timesPowerUsed = Random.Range(1, 3);
                    break;
                case 5:
                    numBlueBlockBroken = Random.Range(15, 36);
                    break;
                case 6:
                    numRedBlockBroken = Random.Range(15, 36);
                    break;
                case 7:
                    numPinkBlockBroken = Random.Range(15, 36);
                    break;
                case 8:
                    numYellowBlock6Broken = Random.Range(15, 36);
                    break;
            }
        }

        setObjectives();
        MapUIScript.mapInstance.PlayLevel(-1);
    }

    private int GetRandomScore()
    {
        return Mathf.RoundToInt(Random.Range(5000f, 25001f) / 100f) * 100;
    }
}

