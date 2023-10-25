using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New  single character Dialogue Data", menuName = "Dialogue System/Dialogue Data for a single character")]
public class SingleDialogueData : ScriptableObject
{
	public string characterName;
	[TextArea(3, 10)] public string[] sentences;
	public Sprite characterSprite;
	[HideInInspector] public CSVDelimiter csvDelimiter = CSVDelimiter.Comma; //set to comma as default 
	public Dictionary<string, Sprite> characterToSpriteMap = new Dictionary<string, Sprite>();

	// Use an enum to define the delimiter options.
	public enum CSVDelimiter
	{
		Tab,
		Space,
		Comma,
		Period,
		Semicolon
		// Add more options as needed.
	}
	[SerializeField]
	[HideInInspector] public TextAsset csvFile;
	[HideInInspector] public static string spriteFolder { get; set; }
	SingleDialogueData()
	{
		spriteFolder = "Assets/Resources/Characters";
	}

	public char GetDelimiterCharacter()
	{
		// Get the selected delimiter character based on the enum value.
		switch (csvDelimiter)
		{
			case CSVDelimiter.Tab:
				return '\t';
			case CSVDelimiter.Space:
				return ' ';
			case CSVDelimiter.Comma:
				return ',';
			case CSVDelimiter.Period:
				return '.';
			case CSVDelimiter.Semicolon:
				return ';';
			// Add more cases as needed.
			default:
				return ',';
		}
	}
	public Sprite[] LoadSpritesFromFolder()
	{
		Sprite[] sprites = (Sprite[])Resources.LoadAll(spriteFolder);
		return sprites;
	}
	public void PopulateCharacterToSpriteMap()
	{
		Sprite[] sprites = LoadSpritesFromFolder();
		characterToSpriteMap.Clear();

		foreach (Sprite sprite in sprites)
		{
			characterToSpriteMap[sprite.name] = sprite;
		}
	}
	public void PopulateSentencesFromCSV()
	{
		PopulateCharacterToSpriteMap(); // Load sprites before populating sentences.
		sentences = ReadCSV(csvFile, GetDelimiterCharacter()); // Pass the delimiter.
	}

	public string[] ReadCSV(TextAsset csv, char delimiter)
	{
		string defaultCharacterName = null;
		List<string> dialogueEntries = new List<string>();

		if (csv != null)
		{
			string[] lines = csv.text.Split('\n');
			for (int i = 1; i < lines.Length; i++) // Start from index 1 to skip the header
			{
				string line = lines[i];
				if (line == null || string.IsNullOrWhiteSpace(line))
				{
					i++;
					continue;
				}
				string[] fields = line.Split(delimiter);
				if (fields.Length >= 2)
				{
					characterName = fields[0];
					if (characterName != null && !string.IsNullOrWhiteSpace(characterName))
					{
						defaultCharacterName = characterName;
					}
					else
					{
						characterName = defaultCharacterName;
					}
					characterSprite = GetCharacterSprite(fields[1]);
					if (characterSprite == null)
					{
						if (fields[1].Contains("_"))
						{
							Debug.LogWarning("Sprite not found for character: " + fields[1]);

							string[] parts = fields[1].Split('_');
							if (parts.Length > 1)
							{
								characterSprite = GetCharacterSprite(parts[0]); // Use the part before "_"
							}
						}
					}
					string sentence = fields[2];
					dialogueEntries.Add(sentence);
				}
				else
				{
					string sentence = fields[1];
					dialogueEntries.Add(sentence);
				}
			}
		}

		return dialogueEntries.ToArray();
	}
	public Sprite GetCharacterSprite(string characterName)
	{
		if (characterToSpriteMap.ContainsKey(characterName))
		{
			return characterToSpriteMap[characterName];
		}
		return null;
	}
}

