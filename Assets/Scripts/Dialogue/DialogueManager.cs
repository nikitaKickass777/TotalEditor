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
    public GameObject NarratorDialogueTemplate;
    public TextMeshProUGUI selectedTMP;
    public TextMeshProUGUI narratorDialogueTMP;
    public TextMeshProUGUI dialogueTMP;

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

    private float timeLeftDialogue;


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
            if (DialogueTemplate.activeSelf || NarratorDialogueTemplate.activeSelf)
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
                selectedTMP.text = dialogueList.dialogues[currentDialogueIndex].lines[currentLineIndex].lineText;
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
        Journalist character = GameManager.instance.journalistList.journalists[dialogue.characterId];
        
        if(character.id == 6){ // narrator
            NarratorDialogueTemplate.SetActive(true); // this thing makes dialogue box appear 
            selectedTMP = narratorDialogueTMP;
        }
        else
        {
           DialogueTemplate.SetActive(true); // this thing makes dialogue box appear 
           selectedTMP = dialogueTMP;
           DialogueTemplate.GetComponentInChildren<TextMeshProUGUI>().text = character.name;
        }
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
        selectedTMP.text = "";
        foreach (char c in line.ToCharArray())
        {
            selectedTMP.text += c;
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
        NarratorDialogueTemplate.gameObject.SetActive(false);
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
                // DAY 1 (Tutorial) - laws 0,1
                if (!dialogueCompleted.ContainsKey(0))
                {
                    ShowDialogue(0);
                    dialogueCompleted[0] = true;
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[0]);
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[1]);
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
                    && GameManager.instance.articleList.articles[0].isEdited
                    && GameManager.instance.articleList.articles[1].isEdited)
                {
                    ShowDialogue(7);
                    dialogueCompleted[7] = true;
                    break;
                }

                // DAY 2 (Tetiana + Lev + Officer) - laws 0,1,2,3

                if (!dialogueCompleted.ContainsKey(8)
                    && dialogueCompleted.ContainsKey(7))
                {
                    ShowDialogue(8);
                    dialogueCompleted[8] = true;
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[2]);
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[3]);
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[4]);
                    break;
                }
                if (!dialogueCompleted.ContainsKey(9)
                    && dialogueCompleted.ContainsKey(8)
                    && GameManager.instance.articleList.articles[2].isEdited == true
                    && GameManager.instance.articleList.articles[3].isEdited == true
                    && GameManager.instance.articleList.articles[4].isEdited == true)
                {
                    ShowDialogue(9);
                    dialogueCompleted[9] = true;
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[5]);
                    GameManager.instance.lawList.laws[2].isActive = true;
                    GameManager.instance.lawList.laws[3].isActive = true;
                    timeLeftDialogue = Time.time;
                    break;
                }

                if (SceneNavigator.instance.previousSceneName == "Editing"
                    && !dialogueCompleted.ContainsKey(10)
                    && !dialogueCompleted.ContainsKey(11)
                    && !dialogueCompleted.ContainsKey(12)
                    && dialogueCompleted.ContainsKey(9)
                    && GameManager.instance.articleList.articles[5].isEdited == false
                    && timeLeftDialogue <= SceneNavigator.instance.timeLeftPreviousScene)
                {
                    ShowDialogue(10);
                    dialogueCompleted[10] = true;

                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[6]);
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[7]);
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[8]);
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[9]);
                    break;
                }

                if (SceneNavigator.instance.previousSceneName == "Editing"
                    && !dialogueCompleted.ContainsKey(10)
                    && dialogueCompleted.ContainsKey(9)
                    && !dialogueCompleted.ContainsKey(11)
                    && !dialogueCompleted.ContainsKey(12)
                    && GameManager.instance.articleList.articles[5].isEdited == true
                    && GameManager.instance.articleList.articles[5].isApproved == false
                    && timeLeftDialogue <= SceneNavigator.instance.timeLeftPreviousScene)
                {
                    ShowDialogue(11);
                    dialogueCompleted[11] = true;
                    

                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[6]);
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[7]);
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[8]);
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[9]);
                    break;
                }

                
                if (SceneNavigator.instance.previousSceneName == "Editing"
                    && !dialogueCompleted.ContainsKey(10)
                    && dialogueCompleted.ContainsKey(9)
                    && !dialogueCompleted.ContainsKey(11)
                    && !dialogueCompleted.ContainsKey(12)
                    && GameManager.instance.articleList.articles[5].isEdited == true
                    && GameManager.instance.articleList.articles[5].isApproved == true
                    && timeLeftDialogue <= SceneNavigator.instance.timeLeftPreviousScene)
                {
                    ShowDialogue(12);
                    dialogueCompleted[12] = true;

                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[6]);
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[7]);
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[8]);
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[9]);
                    break;
                }

                if (!dialogueCompleted.ContainsKey(13)
                    && GameManager.instance.articleList.articles[5].isEdited == true
                    && GameManager.instance.articleList.articles[6].isEdited == true
                    && GameManager.instance.articleList.articles[7].isEdited == true
                    && GameManager.instance.articleList.articles[8].isEdited == true
                    && GameManager.instance.articleList.articles[9].isEdited == true)
                { 
                    ShowDialogue(13);
                    dialogueCompleted[13] = true;

                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[10]);
                    break;
                }

                if (!dialogueCompleted.ContainsKey(14)
                    && dialogueCompleted.ContainsKey(13)
                    && GameManager.instance.articleList.articles[10].isEdited == true
                    && choicesDictionary.ContainsKey("BeginTetianaOfficerTalk")
                    && GameManager.instance.articleList.articles[5].isApproved == false)
                {
                    ShowDialogue(14);
                    dialogueCompleted[14] = true;

                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[11]);
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[12]);
                    break;
                }

                if (!dialogueCompleted.ContainsKey(16)
                    && !dialogueCompleted.ContainsKey(14)
                    && dialogueCompleted.ContainsKey(13)
                    && GameManager.instance.articleList.articles[10].isEdited == true
                    && GameManager.instance.articleList.articles[5].isApproved == false)
                {
                    ShowDialogue(16);
                    dialogueCompleted[16] = true;

                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[11]);
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[12]);
                    break;
                }

                if (!dialogueCompleted.ContainsKey(15)
                    && !dialogueCompleted.ContainsKey(14)
                    && dialogueCompleted.ContainsKey(13)
                    && GameManager.instance.articleList.articles[10].isEdited == true
                    && GameManager.instance.articleList.articles[5].isApproved == true)
                {
                    ShowDialogue(15);
                    dialogueCompleted[15] = true;

                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[11]);
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[12]);
                    break;
                }

                // DAY 3 (Dialogue with Abraham) - laws 0,1,2,3,4
                if (!dialogueCompleted.ContainsKey(17)
                    && GameManager.instance.articleList.articles[11].isEdited == true
                    && GameManager.instance.articleList.articles[12].isEdited == true
                    && GameManager.instance.articleList.articles[5].isApproved == true)
                {
                    ShowDialogue(17);
                    dialogueCompleted[17] = true;

                    GameManager.instance.lawList.laws[4].isActive = true;
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[13]);
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[14]);
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[15]);
                    break;
                }

                if (!dialogueCompleted.ContainsKey(18)
                    && GameManager.instance.articleList.articles[11].isEdited == true
                    && GameManager.instance.articleList.articles[12].isEdited == true
                    && GameManager.instance.articleList.articles[5].isApproved == false)
                {
                    ShowDialogue(18);
                    dialogueCompleted[18] = true;

                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[13]);
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[14]);
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[15]);
                    break;
                }

                if (!dialogueCompleted.ContainsKey(19)
                    && GameManager.instance.articleList.articles[13].isEdited == true
                    && GameManager.instance.articleList.articles[14].isEdited == true
                    && GameManager.instance.articleList.articles[15].isEdited == true)
                {
                    ShowDialogue(19);
                    dialogueCompleted[19] = true;

                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[16]);
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[17]);
                    break;
                }

                if (!dialogueCompleted.ContainsKey(20)
                    && GameManager.instance.articleList.articles[16].isEdited == true
                    && GameManager.instance.articleList.articles[17].isEdited == true)
                {
                    ShowDialogue(20);
                    dialogueCompleted[20] = true;
                    break;
                }

                if (dialogueCompleted.ContainsKey(20)
                    && !dialogueCompleted.ContainsKey(21))
                {
                    ShowDialogue(21);
                    dialogueCompleted[21] = true;
                    break;
                }

                if (dialogueCompleted.ContainsKey(21)
                    && !dialogueCompleted.ContainsKey(22))
                {
                    ShowDialogue(22);
                    dialogueCompleted[22] = true;
                    break;
                }
                
                if (dialogueCompleted.ContainsKey(22)
                    && !dialogueCompleted.ContainsKey(23))
                {
                    ShowDialogue(23);
                    dialogueCompleted[23] = true;
                    break;
                }

                if (dialogueCompleted.ContainsKey(23)
                    && !dialogueCompleted.ContainsKey(24))
                {
                    ShowDialogue(24);
                    dialogueCompleted[24] = true;
                    break;
                }

                if (dialogueCompleted.ContainsKey(24)
                    && !dialogueCompleted.ContainsKey(25))
                {
                    ShowDialogue(25);
                    dialogueCompleted[25] = true;
                    break;
                }

                if (dialogueCompleted.ContainsKey(25)
                    && !dialogueCompleted.ContainsKey(26))
                {
                    ShowDialogue(26);
                    dialogueCompleted[26] = true;
                    break;
                }

                if (dialogueCompleted.ContainsKey(26)
                    && !dialogueCompleted.ContainsKey(27))
                {
                    ShowDialogue(27);
                    dialogueCompleted[27] = true;
                    break;
                }

                if (dialogueCompleted.ContainsKey(27)
                    && !dialogueCompleted.ContainsKey(28))
                {
                    ShowDialogue(28);
                    dialogueCompleted[28] = true;
                    break;
                }

                if (dialogueCompleted.ContainsKey(28)
                    && !dialogueCompleted.ContainsKey(29))
                {
                    ShowDialogue(29);
                    dialogueCompleted[29] = true;
                    break;
                }

                if (dialogueCompleted.ContainsKey(29)
                    && !dialogueCompleted.ContainsKey(30))
                {
                    ShowDialogue(30);
                    dialogueCompleted[30] = true;
                    break;
                }

                if (dialogueCompleted.ContainsKey(30)
                    && !dialogueCompleted.ContainsKey(31))
                {
                    ShowDialogue(31);
                    dialogueCompleted[31] = true;
                    break;
                }

                if (dialogueCompleted.ContainsKey(31)
                    && !dialogueCompleted.ContainsKey(32))
                {
                    ShowDialogue(32);
                    dialogueCompleted[32] = true;
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