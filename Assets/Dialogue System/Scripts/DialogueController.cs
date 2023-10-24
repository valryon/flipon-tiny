using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
	public GameObject DialoguePanel;

	public TMP_Text characterName;
	public TMP_Text dialogueText;
	public Image characterImage;

	private List<string> sentences;
	private List<string> names;
	private List<Sprite> sprites;
	private bool isMulti = false;
	private int currentIndex;
	void Start()
	{
		sentences = new List<string>();
		names = new List<string>();
		sprites = new List<Sprite>();

	}

	public void StartDialogue(SingleDialogueData dialogueData)
	{
		isMulti = false;
		characterName.text = dialogueData.characterName;
		characterImage.sprite = dialogueData.characterSprite;

		sentences.Clear();

		foreach (string sentence in dialogueData.sentences)
		{
			sentences.Add(sentence);
		}
		currentIndex = 0;
		DisplayNextSentenceSingleCharacter();
	}

	public void StartMultiDialogue(MultiDialogueData dialogueData)
	{
		isMulti = true;
		if (dialogueData == null)
		{
			Debug.LogWarning("Dialogue Data is null. Cannot start dialogue.");
			return;
		}

		if (dialogueData.dialogueEntries.Count == 0)
		{
			Debug.LogWarning("Dialogue Data does not contain any dialogue entries.");
			return;
		}


		sentences.Clear();
		names.Clear();
		sprites.Clear();

		foreach (MultiDialogueData.DialogueEntry entry in dialogueData.dialogueEntries)
		{
			characterName.text = entry.characterName;
			characterImage.sprite = entry.characterSprite;
			sentences.Add(entry.sentence);
			names.Add(entry.characterName);
			sprites.Add(entry.characterSprite);

		}
		currentIndex = 0;
		DisplayNextSentenceMultiCharacters();
	}

	public void DisplayNextSentenceSingleCharacter()
	{
		if (sentences.Count == 0)
		{
			EndDialogue();
			return;
		}

		string sentence = sentences[currentIndex];
		dialogueText.text = sentence;
    currentIndex++;
  }
	public void DisplayNextSentenceMultiCharacters()
	{
		if (sentences.Count == 0)
		{
			EndDialogue();
			return;
		}

		if (currentIndex < sentences.Count)
		{
			DisplayCurrentEntry();
		}
		else
		{
			EndDialogue();
			return;
		}
    currentIndex++;
  }
	private void DisplayCurrentEntry()
	{
		characterName.text = names[currentIndex];
		dialogueText.text = sentences[currentIndex];
		characterImage.sprite = sprites[currentIndex];
	}
	private void Update()
	{
		// Add touch input detection and call DisplayNextSentence when touched.
		if (Input.GetMouseButtonDown(0))
		{
			if (isMulti)
			{
				//DisplayNextSentenceMultiCharacters();
			}
			else
			{
				DisplayNextSentenceSingleCharacter();
			}
		}
	}

	void EndDialogue()
	{
		DialoguePanel.SetActive(false);
	}

  public void SetDialogueIndex(int desIndex)
  {
		currentIndex = desIndex;
	}
}
