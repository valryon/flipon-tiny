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

    private Queue<string> sentences;
    private Queue<string> names;
    private Queue<Sprite> sprites;
    private bool isMulti = false;
    void Start()
    {
        sentences = new Queue<string>();
        names = new Queue<string>();
        sprites = new Queue<Sprite>();

    }

    public void StartDialogue(SingleDialogueData dialogueData)
    {
        isMulti = false;
        characterName.text = dialogueData.characterName;
        characterImage.sprite = dialogueData.characterSprite;

        sentences.Clear();

        foreach (string sentence in dialogueData.sentences)
        {
            sentences.Enqueue(sentence);
        }

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

        foreach (MultiDialogueData.DialogueEntry entry in dialogueData.dialogueEntries)
        {
            characterName.text = entry.characterName;
            characterImage.sprite = entry.characterSprite;
            sentences.Enqueue(entry.sentence);
            names.Enqueue(entry.characterName);
            sprites.Enqueue(entry.characterSprite);

        }

        DisplayNextSentenceMultiCharacters();
    }

    public void DisplayNextSentenceSingleCharacter()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        string sentence = sentences.Dequeue();
        dialogueText.text = sentence;
    }
    public void DisplayNextSentenceMultiCharacters()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        string sentence = sentences.Dequeue();
        dialogueText.text = sentence;
        string name = names.Dequeue();
        characterName.text = name;
        Sprite sprite = sprites.Dequeue();
        characterImage.sprite = sprite;
    }
    private void Update()
    {
      /*  // Add touch input detection and call DisplayNextSentence when touched.
        if (Input.GetMouseButtonDown(0))
        {
			if (isMulti)
			{
                DisplayNextSentenceMultiCharacters();
			}
			else
			{
                DisplayNextSentenceSingleCharacter();
            }
        } */
    }

    void EndDialogue()
    {
        DialoguePanel.SetActive(false);
    }
}
