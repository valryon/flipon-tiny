using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SingleDialogueData))]
public class SingleDialogueDataEditor : Editor
{
	public override void OnInspectorGUI()
	{
		SingleDialogueData dialogueData = (SingleDialogueData)target;

		// Display the default fields
		base.OnInspectorGUI();

		GUILayout.Space(10);

		GUILayout.Label("CSV Data");
		GUILayout.Space(10);
		GUILayout.BeginHorizontal();
		// Allow specifying the sprite folder.

		SingleDialogueData.spriteFolder = EditorGUILayout.TextField("Sprite Folder", SingleDialogueData.spriteFolder);
		GUILayout.Space(10);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();

		//  Add a drop-down menu for selecting and set the file delimiter.
		dialogueData.csvDelimiter = (SingleDialogueData.CSVDelimiter)EditorGUILayout.EnumPopup("CSV Delimiter", dialogueData.csvDelimiter);
		GUILayout.Space(10);
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		dialogueData.csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", dialogueData.csvFile, typeof(TextAsset), false);
		GUILayout.Space(10);

		if (GUILayout.Button("Populate Sentences"))
		{
			dialogueData.PopulateSentencesFromCSV();
		}

		GUILayout.EndHorizontal();
	}
}
