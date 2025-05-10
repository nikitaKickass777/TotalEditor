using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;


public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    public GameObject DialogueTemplate;
    [FormerlySerializedAs("dialogueText")] public TextMeshProUGUI dialogueTMP;
    public Sprite characterSprite;
    public Sprite characterSpriteTalk;
    public SpriteRenderer characterSpriteRenderer;
    public Button[] optionButtons;

    public int currentDialogueIndex;
    public int currentLineIndex;
    public DialogueList dialogueList;
    public Dictionary<string, bool> choicesDictionary;
    public Dictionary<int, bool> dialogueCompleted;
    public bool isDialogueOpen;

    public float textSpeed = 0.05f;
    private Coroutine typeCoroutine;
    private Coroutine talkAnimationCoroutine;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        LoadDialogues();
        isDialogueOpen = false;
        choicesDictionary = new Dictionary<string, bool>();
        dialogueCompleted = new Dictionary<int, bool>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (DialogueTemplate.activeSelf)
            {
                EndDialogue();
            }
            else
            {
                ShowDialogue(0);
            }
        }

        if (Input.GetMouseButtonDown(0) && isDialogueOpen)
        {
            if (typeCoroutine != null)
            {
                Debug.Log("Stopping typing coroutine");
                try
                {
                    StopCoroutine(typeCoroutine);
                    StopCoroutine(talkAnimationCoroutine);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }

                typeCoroutine = null;
                talkAnimationCoroutine = null;
                characterSpriteRenderer.sprite = characterSprite;
                dialogueTMP.text = dialogueList.dialogues[currentDialogueIndex].lines[currentLineIndex].lineText;
            }
            else if (!dialogueList.dialogues[currentDialogueIndex].lines[currentLineIndex].hasOptions)
            {
                Debug.Log("Loading next line");
                if (dialogueList.dialogues[currentDialogueIndex].lines[currentLineIndex].nextLineId != -1)
                {
                    LoadLine(dialogueList.dialogues[currentDialogueIndex].lines[currentLineIndex].nextLineId,
                        currentDialogueIndex);
                }
                else
                {
                    EndDialogue();
                }
            }
        }

        checker(GameManager.instance.day);
    }


    public void ShowDialogue(int dialogueIndex)
    {
        currentDialogueIndex = dialogueIndex;
        currentLineIndex = 0; // Start at first line
        Dialogue dialogue = dialogueList.dialogues[dialogueIndex];

        Time.timeScale = 0;
        isDialogueOpen = true;
        DialogueTemplate.SetActive(true); // this thing makes dialogue box appear
        Journalist character = GameManager.instance.journalistList.journalists[dialogue.characterId];
        // if(character.id == 6){ // narrator

        // }
        characterSprite = character.portrait;
        characterSpriteTalk = character.portraitTalking;

        LoadLine(0, dialogueIndex); // Load first line
    }


    public void LoadLine(int lineIndex, int dialogueIndex)
    {
        Debug.Log("Loading line " + lineIndex + " from dialogue " + dialogueIndex);
        currentLineIndex = lineIndex;
        DialogueLine line = dialogueList.dialogues[dialogueIndex].lines[lineIndex];
        if (typeCoroutine != null)
        {
            StopCoroutine(typeCoroutine);
            typeCoroutine = null;
        }

        if (talkAnimationCoroutine != null)
        {
            StopCoroutine(talkAnimationCoroutine);
            talkAnimationCoroutine = null;
        }

        typeCoroutine = StartCoroutine(TypeLine(line.lineText));
        if (line.hasOptions)
        {
            ShowOptions(line);
        }
        else
        {
            HideOptions();
        }
    }

    private IEnumerator TypeLine(string line)
    {
        talkAnimationCoroutine = StartCoroutine(TalkAnimation());
        dialogueTMP.text = "";
        foreach (char c in line.ToCharArray())
        {
            dialogueTMP.text += c;
            yield return new WaitForSecondsRealtime(textSpeed);
        }

        StopCoroutine(talkAnimationCoroutine);
        talkAnimationCoroutine = null;
        characterSpriteRenderer.sprite = characterSprite;
        typeCoroutine = null;
    }

    private IEnumerator TalkAnimation()
    {
        bool animation = false;
        while (true)
        {
            if (animation)
            {
                characterSpriteRenderer.sprite = characterSpriteTalk;
                animation = false;
            }
            else
            {
                characterSpriteRenderer.sprite = characterSprite;
                animation = true;
            }

            yield return new WaitForSecondsRealtime(textSpeed * 2);
        }
    }

    private void OnOptionSelected(int optionIndex)
    {
        DialogueLine currentLine = dialogueList.dialogues[currentDialogueIndex].lines[currentLineIndex];
        DialogueOption selectedOption = currentLine.options[optionIndex];
        ApplyOptionEffects(selectedOption);
        if (selectedOption.nextLineId >= 0)
        {
            LoadLine(selectedOption.nextLineId, currentDialogueIndex);
        }
        else
        {
            EndDialogue();
        }
    }

    private void ShowOptions(DialogueLine line)
    {
        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (i < line.options.Count)
            {
                Debug.Log("Showing option " + i);
                optionButtons[i].gameObject.SetActive(true);
                optionButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = line.options[i].optionText;

                // Capture the correct index
                int capturedIndex = i;
                optionButtons[i].onClick.RemoveAllListeners();
                optionButtons[i].onClick.AddListener(() => OnOptionSelected(capturedIndex));
            }
            else
            {
                optionButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void HideOptions()
    {
        foreach (Button button in optionButtons)
        {
            button.gameObject.SetActive(false);
        }
    }

    private void EndDialogue()
    {
        Time.timeScale = 1;
        DialogueTemplate.gameObject.SetActive(false);

        StartCoroutine(WaitBeforeClosingDialogue());
    }

    private IEnumerator WaitBeforeClosingDialogue()
    {
        yield return null;
        isDialogueOpen = false;
    }

    private void ApplyOptionEffects(DialogueOption option)
    {
        if (option.conditionChange != null)
        {
            choicesDictionary[option.conditionChange] = true;
        }
    }

// Base conditions to be checked before showing any dialogue
    private bool baseConditionsMet()
    {
        return (!isDialogueOpen
                && SceneManager.GetActiveScene().name == "Office");
    }

    private void checker(int day)
    {
        // here goes all ingame logic for dialogue selection
        if (!baseConditionsMet()) return;
        switch (day)
        {
            case 1:

                if (!dialogueCompleted.ContainsKey(0))
                {
                    ShowDialogue(0);
                    dialogueCompleted[0] = true;
                    break;
                }

                if (!dialogueCompleted.ContainsKey(1)
                    && dialogueCompleted.ContainsKey(0))
                {
                    ShowDialogue(1);
                    dialogueCompleted[1] = true;
                    break;
                }

                if (!dialogueCompleted.ContainsKey(2)
                    && dialogueCompleted.ContainsKey(1))
                {
                    ShowDialogue(2);
                    dialogueCompleted[2] = true;
                    break;
                }

                if (!dialogueCompleted.ContainsKey(3)
                    && dialogueCompleted.ContainsKey(2))
                {
                    ShowDialogue(3);
                    dialogueCompleted[3] = true;
                    break;
                }

                if (!dialogueCompleted.ContainsKey(4)
                    && dialogueCompleted.ContainsKey(3))
                {
                    ShowDialogue(4);
                    dialogueCompleted[4] = true;
                    break;
                }

                if (!dialogueCompleted.ContainsKey(5)
                    && dialogueCompleted.ContainsKey(4))
                {
                    ShowDialogue(5);
                    dialogueCompleted[5] = true;
                    break;
                }

                if (!dialogueCompleted.ContainsKey(6)
                    && dialogueCompleted.ContainsKey(5))
                {
                    ShowDialogue(6);
                    dialogueCompleted[6] = true;
                    break;
                }

                if (!dialogueCompleted.ContainsKey(7)
                    && dialogueCompleted.ContainsKey(6)
                    && GameManager.instance.articleList.articles[0].isEdited)
                {
                    ShowDialogue(7);
                    dialogueCompleted[7] = true;
                    break;
                }


                // if (GameManager.instance.time > 10
                //     && !choicesDictionary.ContainsKey("testComeLater")
                //     && !isDialogueOpen
                //     && SceneManager.GetActiveScene().name == "Office"
                //     && !dialogueCompleted.ContainsKey(1))
                // {
                //     ShowDialogue(1);
                //     dialogueCompleted[1] = true;
                // }


                // if(GameManager.instance.time > 20
                //    && choicesDictionary.ContainsKey("testComeLater")
                //    && !isDialogueOpen
                //    && SceneManager.GetActiveScene().name == "Office"
                //    && !dialogueCompleted.ContainsKey(2))
                // {
                //     ShowDialogue(2);
                //     dialogueCompleted[2] = true;
                // }


                break;

            default:
                Debug.Log("Invalid day");
                break;
        }
    }

    void LoadDialogues()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("dialogues");
        dialogueList = JsonUtility.FromJson<DialogueList>(jsonFile.text);
        
    }
}