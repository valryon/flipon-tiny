// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using UnityEngine;

namespace Pon
{
  public enum GarbagesType
  {
    Normal,
    Low,
    None
  }

  public enum PlayMode
  {
    Singleplayer,
    Versus
  }

  /// <summary>
  /// Grid parameters as an object you pass from menu scene to game scene.
  /// </summary>
  public class GameSettings : MonoBehaviour
  {
    #region Members

    public GridSettings gridSettings;
    public Player[] players;
    public int seed;
    public GarbagesType garbagesType = GarbagesType.Normal;
    public bool disableChains;
    public bool enableObjectives;
    public Objective objective;
    public PlayMode playMode;
    public int currencyReward = 100;

    public static GameSettings gameSettingsInstance;

    #endregion

    #region Timeline

    void Awake()
    {
            /*
           if (gameSettingsInstance != null)
           {
               Destroy(gameObject);
               return;
           }
           else
           {
               gameSettingsInstance = this;
               DontDestroyOnLoad(gameObject);
           }
            */
            gameSettingsInstance = this;

 

            if (MapUIScript.mapInstance != null)
            {
                // set game settings based on level
                gridSettings.startLines = MapUIScript.mapInstance.numStartingLines;
                objective.stats.score = MapUIScript.mapInstance.score;
                objective.stats.totalCombos = MapUIScript.mapInstance.combos;
                objective.stats.total4Combos = MapUIScript.mapInstance.fourCombos;
                objective.stats.total5Combos = MapUIScript.mapInstance.fiveCombos;
                objective.stats.totalLCombos = MapUIScript.mapInstance.LCombos;
                objective.stats.timesPowerUsed = MapUIScript.mapInstance.timesPowerUsed;
                objective.stats.numBlock1Broken = MapUIScript.mapInstance.numBlock1Broken;
                objective.stats.numBlock2Broken = MapUIScript.mapInstance.numBlock2Broken;
                objective.stats.numBlock3Broken = MapUIScript.mapInstance.numBlock3Broken;
                objective.stats.numBlock4Broken = MapUIScript.mapInstance.numBlock4Broken;
                objective.stats.numBlock5Broken = MapUIScript.mapInstance.numBlock5Broken;
                objective.stats.numBlock6Broken = MapUIScript.mapInstance.numBlock6Broken;
            }

            if (gameSettingsInstance != null)
            {
                Debug.Log(gridSettings.startLines);
            }

            //DontDestroyOnLoad(gameObject);


        }

        #endregion
    }
}