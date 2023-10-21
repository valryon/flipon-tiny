using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Multi-Character Dialogue Data", menuName = "Dialogue System/Dialogue Data for Multiple Characters")]
public class MultiDialogueData : ScriptableObject
{
    [System.Serializable]
    public class DialogueEntry
    {
        public string characterName;
        [TextArea(3, 10)] public string sentence;
        public Sprite characterSprite; // Assuming you still want to include the characterSprite
    }

    public List<DialogueEntry> dialogueEntries = new List<DialogueEntry>();

    [SerializeField]
    [HideInInspector] public TextAsset csvFile;

    public void PopulateSentencesFromCSV()
    {
        dialogueEntries = ReadCSV(csvFile);
    }

    public List<DialogueEntry> ReadCSV(TextAsset csv)
    {
        List<DialogueEntry> entries = new List<DialogueEntry>();

        if (csv != null)
        {
            string[] lines = csv.text.Split('\n');
            for (int i = 1; i < lines.Length; i++) // Start from index 1 to skip the header
            {
                string line = lines[i];
                string[] fields = line.Split(',');
                if (fields.Length >= 2)
                {
                    DialogueEntry entry = new DialogueEntry
                    {
                        characterName = fields[0],
                        sentence = fields[1],
                    };
                    entries.Add(entry);
                }
            }
        }

        return entries;
    }
}

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
        dialogueData.csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", dialogueData.csvFile, typeof(TextAsset), false);

        if (GUILayout.Button("Populate Sentences"))
        {
            dialogueData.PopulateSentencesFromCSV();
        }

        GUILayout.EndHorizontal();
    }
}
