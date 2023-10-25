#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DailyBonusManager))]
public class DailyBonusManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        DailyBonusManager manager = (DailyBonusManager)target;

        // Display the current data
        EditorGUILayout.LabelField("Current Daily Streak", manager.DailyStreak.ToString());
        EditorGUILayout.LabelField("Daily Points Earned", manager.DailyPointsEarned.ToString());
        EditorGUILayout.LabelField("Last Played Date", manager.lastPlayedDate.ToString());

        // Create fields to modify the data
        manager.DailyStreak = EditorGUILayout.IntField("Set Daily Streak", manager.DailyStreak);
        manager.DailyPointsEarned = EditorGUILayout.IntField("Set Daily Points Earned", manager.DailyPointsEarned);
        string lastPlayedDateString = EditorGUILayout.TextField("Set Last Played Date", manager.lastPlayedDate.ToString("yyyy-MM-dd"));
        if (DateTime.TryParse(lastPlayedDateString, out DateTime newDate))
        {
            manager.lastPlayedDate = newDate;
        }


        // Button to save changes
        if (GUILayout.Button("Save Changes"))
        {
            manager.SaveData();
        }
    }
}
#endif
