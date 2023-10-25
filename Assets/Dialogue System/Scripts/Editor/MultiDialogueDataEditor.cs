using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MultiDialogueData))]
public class MultiDialogueDataEditor : Editor
{
	public override void OnInspectorGUI()
	{
		MultiDialogueData dialogueData = (MultiDialogueData)target;

		// Display the default fields
		base.OnInspectorGUI();

		GUILayout.Space(10);

		GUILayout.Label("CSV Data");
		GUILayout.Space(10);
		GUILayout.BeginHorizontal();
		// Allow specifying the sprite folder.

		MultiDialogueData.spriteFolder = EditorGUILayout.TextField("Sprite Folder", MultiDialogueData.spriteFolder);
		GUILayout.Space(10);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();

		//  Add a drop-down menu for selecting and set the file delimiter.
		dialogueData.csvDelimiter = (MultiDialogueData.CSVDelimiter)EditorGUILayout.EnumPopup("CSV Delimiter", dialogueData.csvDelimiter);
		GUILayout.Space(10);
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		dialogueData.csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", dialogueData.csvFile, typeof(TextAsset), false);
		GUILayout.Space(10);

		if (GUILayout.Button("Populate Sentences"))
		{
			dialogueData.PopulateSentencesFromCSV();
			EditorUtility.SetDirty(dialogueData);
		}

		GUILayout.EndHorizontal();
	}
}
