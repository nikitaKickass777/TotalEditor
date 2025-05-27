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
    public GameObject NarratorOptionsDialogueTemplate;
    public TextMeshProUGUI selectedTMP;
    public TextMeshProUGUI narratorDialogueTMP;
    
    public TextMeshProUGUI narratorOptionsDialogueTMP;
    public TextMeshProUGUI dialogueTMP;

    public Sprite characterSprite;
    public Sprite characterSpriteTalk;
    public SpriteRenderer characterSpriteRenderer;
    public Button[] selectedOptionButtons;
    public Button[] optionButtons;
    public Button[] narratorOptionButtons;

    public int currentDialogueIndex;
    public int currentLineIndex;
    public DialogueList dialogueList;
    public Dictionary<string, bool> choicesDictionary;
    public Dictionary<int, bool> dialogueCompleted;
    public bool isDialogueOpen;
    public bool showArticle;

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
            if (character.id == 7)
            {
                NarratorOptionsDialogueTemplate.SetActive(true);
                selectedTMP = narratorOptionsDialogueTMP;
                selectedOptionButtons = narratorOptionButtons;
            }
            else
            {
                DialogueTemplate.SetActive(true); // this thing makes dialogue box appear 
                selectedTMP = dialogueTMP;
                DialogueTemplate.GetComponentInChildren<TextMeshProUGUI>().text = character.name;
                selectedOptionButtons = optionButtons;
            }

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
        for (int i = 0; i < selectedOptionButtons.Length; i++)
        {
            if (i < line.options.Count)
            {
                Debug.Log("Showing option " + i);
                selectedOptionButtons[i].gameObject.SetActive(true);
                selectedOptionButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = line.options[i].optionText;

                // Capture the correct index
                int capturedIndex = i;
                selectedOptionButtons[i].onClick.RemoveAllListeners();
                selectedOptionButtons[i].onClick.AddListener(() => OnOptionSelected(capturedIndex));
            }
            else
            {
                selectedOptionButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void HideOptions()
    {
        foreach (Button button in selectedOptionButtons)
        {
            button.gameObject.SetActive(false);
        }
    }

    private void EndDialogue()
    {
        Time.timeScale = 1;
        DialogueTemplate.gameObject.SetActive(false);
        NarratorDialogueTemplate.gameObject.SetActive(false);
        NarratorOptionsDialogueTemplate.gameObject.SetActive(false);
        StartCoroutine(WaitBeforeClosingDialogue());
    }

    private IEnumerator WaitBeforeClosingDialogue()
    {
        yield return null;
        isDialogueOpen = false;
    }

    private void ApplyOptionEffects(DialogueOption option)
    {
        option.isSelected = true;
        if (option.conditionChange != null)
        {
            choicesDictionary[option.conditionChange] = true;
        }
    }

// Base conditions to be checked before showing any dialogue
    private bool baseConditionsMet()
    {
        return (!isDialogueOpen
                && SceneManager.GetActiveScene().name == "Office"
                && EndOfDayScreen.instance.isOpen == false);
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
                    NotificationManager.instance.AddToQueue("You have new articles to edit");
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
                   // EndGameManager.instance.EndGame("street"); // IMPORTANT: call AFTER ShowDialogue()
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
                    EndOfDayScreen.instance.ShowEndOfTheDay();
                }

                break;
            case 2:

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
                        && dialogueCompleted.ContainsKey(7)
                        )
                    {

                        ShowDialogue(8);
                        dialogueCompleted[8] = true;
                        GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[2]);
                        GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[3]);
                        GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[4]);
                        NotificationManager.instance.AddToQueue("You have new articles to edit");
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
                    NotificationManager.instance.AddToQueue("Tetiana asked you not to edit her article, but just read");
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[5]);
                    GameManager.instance.lawList.laws[2].isActive = true;
                    GameManager.instance.lawList.laws[3].isActive = true;
                    timeLeftDialogue = Time.time;
                    EditingState.instance.approveButtonActive = false;
                    EditingState.instance.rejectButtonActive = false;
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
                    EditingState.instance.approveButtonActive = true;
                    EditingState.instance.rejectButtonActive = true;
                    NotificationManager.instance.AddToQueue("Check out the law board");
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[6]);
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[7]);
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[8]);
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[9]);
                    NotificationManager.instance.AddToQueue("You have new articles to edit");
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
                    
                    NotificationManager.instance.AddToQueue("Check out the law board");
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[6]);
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[7]);
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[8]);
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[9]);
                    NotificationManager.instance.AddToQueue("You have new articles to edit");
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

                    NotificationManager.instance.AddToQueue("Check out the law board");
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[6]);
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[7]);
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[8]);
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[9]);
                    NotificationManager.instance.AddToQueue("You have new articles to edit");
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
                    NotificationManager.instance.AddToQueue("You have a new article to edit");
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
                    NotificationManager.instance.AddToQueue("You have new articles to edit");
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
                    NotificationManager.instance.AddToQueue("You have new articles to edit");
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
                    NotificationManager.instance.AddToQueue("You have new articles to edit");
                    break;
                }

                if (GameManager.instance.articleList.articles[11].isEdited == true
                    && GameManager.instance.articleList.articles[12].isEdited == true)
                {
                    EndOfDayScreen.instance.ShowEndOfTheDay();
                }

                break;

            case 3:
                // DAY 3 (Dialogue with Abraham) - laws 0,1,2,3,4
                if (!dialogueCompleted.ContainsKey(17)
                    && GameManager.instance.articleList.articles[11].isEdited == true
                    && GameManager.instance.articleList.articles[12].isEdited == true
                    && GameManager.instance.articleList.articles[5].isApproved == true)
                {
                    ShowDialogue(17);
                    dialogueCompleted[17] = true;

                    GameManager.instance.lawList.laws[4].isActive = true;
                    NotificationManager.instance.AddToQueue("Check out the law board");
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[13]);
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[14]);
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[15]);
                    NotificationManager.instance.AddToQueue("You have new articles to edit");
                    break;
                }

                if (!dialogueCompleted.ContainsKey(18)
                    && GameManager.instance.articleList.articles[11].isEdited == true
                    && GameManager.instance.articleList.articles[12].isEdited == true
                    && GameManager.instance.articleList.articles[5].isApproved == false)
                {
                    ShowDialogue(18);
                    dialogueCompleted[18] = true;

                    GameManager.instance.lawList.laws[4].isActive = true;
                    NotificationManager.instance.AddToQueue("Check out the law board");
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[13]);
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[14]);
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[15]);
                    NotificationManager.instance.AddToQueue("You have new articles to edit");
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
                    NotificationManager.instance.AddToQueue("You have new articles to edit");
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

                if (dialogueCompleted.ContainsKey(32)
                    && !dialogueCompleted.ContainsKey(33))
                {
                    ShowDialogue(33);
                    dialogueCompleted[33] = true;
                    break;
                }

                if (dialogueCompleted.ContainsKey(33)
                    && !dialogueCompleted.ContainsKey(34))
                {
                    ShowDialogue(34);
                    dialogueCompleted[34] = true;
                    break;
                }

                if (dialogueCompleted.ContainsKey(34)
                    && !dialogueCompleted.ContainsKey(35))
                {
                    ShowDialogue(35);
                    dialogueCompleted[35] = true;
                    break;
                }

                if (dialogueCompleted.ContainsKey(35)
                    && !dialogueCompleted.ContainsKey(36))
                {
                    ShowDialogue(36);
                    dialogueCompleted[36] = true;
                    break;
                }

                if (dialogueCompleted.ContainsKey(36)
                    && !dialogueCompleted.ContainsKey(37))
                {
                    ShowDialogue(37);
                    dialogueCompleted[37] = true;
                    break;
                }

                if (dialogueCompleted.ContainsKey(37)
                    && !dialogueCompleted.ContainsKey(38))
                {
                    ShowDialogue(38);
                    dialogueCompleted[38] = true;
                    break;
                }

                if (dialogueCompleted.ContainsKey(38)
                    && !dialogueCompleted.ContainsKey(39))
                {
                    ShowDialogue(39);
                    dialogueCompleted[39] = true;
                    break;
                }

                if (dialogueCompleted.ContainsKey(39)
                    && !dialogueCompleted.ContainsKey(40))
                {
                    ShowDialogue(40);
                    dialogueCompleted[40] = true;
                    break;
                }

                if (dialogueCompleted.ContainsKey(40))
                {
                    EndOfDayScreen.instance.ShowEndOfTheDay();
                }

                break;

            case 4:
                // DAY 4 
                if (dialogueCompleted.ContainsKey(40)
                    && !dialogueCompleted.ContainsKey(41))
                {
                    ShowDialogue(41);
                    dialogueCompleted[41] = true;
                    showArticle = false;
                    break;
                }
                if (dialogueCompleted.ContainsKey(41)
                    && !showArticle)
                {
                    if (choicesDictionary.ContainsKey("IWrite"))
                    {
                        GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[21]);
                    }
                    if (choicesDictionary.ContainsKey("AbrahamWrite"))
                    {
                        GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[22]);
                    }
                    if (choicesDictionary.ContainsKey("MargaretWrite"))
                    {
                        GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[23]);
                    }
                    showArticle = true;
                    EditingState.instance.rejectButtonActive = false;
                    NotificationManager.instance.AddToQueue("You have a new article to edit");
                    break;
                }

                if (dialogueCompleted.ContainsKey(41)
                    && !choicesDictionary.ContainsKey("TurnIn")
                    && !choicesDictionary.ContainsKey("RefuseHelp")
                    && !choicesDictionary.ContainsKey("IWrite")
                    && !choicesDictionary.ContainsKey("AbrahamWrite")
                    && !choicesDictionary.ContainsKey("MargaretWrite")
                    && !dialogueCompleted.ContainsKey(42)
                    && !dialogueCompleted.ContainsKey(43)
                    && ((GameManager.instance.articleList.articles[5].isApproved == true
                    && GameManager.instance.articleList.articles[16].isApproved == true)
                    || (GameManager.instance.articleList.articles[5].isApproved == false
                    && GameManager.instance.articleList.articles[16].isApproved == true)
                    || (GameManager.instance.articleList.articles[5].isApproved == true
                    && GameManager.instance.articleList.articles[16].isApproved == false)))
                {
                    ShowDialogue(42);
                    dialogueCompleted[42] = true;
                    break;
                }

                if (dialogueCompleted.ContainsKey(41)
                    && !choicesDictionary.ContainsKey("TurnIn")
                    && !choicesDictionary.ContainsKey("RefuseHelp")
                    && !choicesDictionary.ContainsKey("IWrite")
                    && !choicesDictionary.ContainsKey("AbrahamWrite")
                    && !choicesDictionary.ContainsKey("MargaretWrite")
                    && !dialogueCompleted.ContainsKey(42)
                    && !dialogueCompleted.ContainsKey(43)
                    && ((GameManager.instance.articleList.articles[5].isApproved == false
                    && GameManager.instance.articleList.articles[16].isApproved == false)))
                {
                    ShowDialogue(43);
                    dialogueCompleted[43] = true;
                    break;
                }

                if ((dialogueCompleted.ContainsKey(42)
                    || dialogueCompleted.ContainsKey(43))
                    && !dialogueCompleted.ContainsKey(44))
                {
                    ShowDialogue(44);
                    dialogueCompleted[44] = true;
                    break;
                }
                
                if (dialogueCompleted.ContainsKey(44)
                    && !dialogueCompleted.ContainsKey(45)
                    && ((GameManager.instance.articleList.articles[5].isApproved == true
                    && GameManager.instance.articleList.articles[16].isApproved == true)
                    || (GameManager.instance.articleList.articles[5].isApproved == false
                    && GameManager.instance.articleList.articles[16].isApproved == true)
                    || (GameManager.instance.articleList.articles[5].isApproved == true
                    && GameManager.instance.articleList.articles[16].isApproved == false))
                    && choicesDictionary.ContainsKey("BigBribe"))
                {
                    ShowDialogue(45);
                    dialogueCompleted[45] = true;
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[18]);
                    EditingState.instance.rejectButtonActive = false;
                    NotificationManager.instance.AddToQueue("You have a new article to edit");
                    break;
                }

                if (dialogueCompleted.ContainsKey(44)
                    && !dialogueCompleted.ContainsKey(46)
                    && ((GameManager.instance.articleList.articles[5].isApproved == false
                    && GameManager.instance.articleList.articles[16].isApproved == false))
                    && choicesDictionary.ContainsKey("BigBribe"))
                {
                    ShowDialogue(46);
                    dialogueCompleted[46] = true;
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[18]);
                    EditingState.instance.rejectButtonActive = false;
                    NotificationManager.instance.AddToQueue("You have a new article to edit");
                    break;
                }

                if (dialogueCompleted.ContainsKey(44)
                    && !dialogueCompleted.ContainsKey(47)
                    && ((GameManager.instance.articleList.articles[5].isApproved == true
                    && GameManager.instance.articleList.articles[16].isApproved == true)
                    || (GameManager.instance.articleList.articles[5].isApproved == false
                    && GameManager.instance.articleList.articles[16].isApproved == true)
                    || (GameManager.instance.articleList.articles[5].isApproved == true
                    && GameManager.instance.articleList.articles[16].isApproved == false))
                    && choicesDictionary.ContainsKey("SmallBribe"))
                {
                    ShowDialogue(47);
                    dialogueCompleted[47] = true;
                    break;
                }

                if (dialogueCompleted.ContainsKey(44)
                    && !dialogueCompleted.ContainsKey(48)
                    && ((GameManager.instance.articleList.articles[5].isApproved == false
                    && GameManager.instance.articleList.articles[16].isApproved == false))
                    && choicesDictionary.ContainsKey("SmallBribe"))
                {
                    ShowDialogue(48);
                    dialogueCompleted[48] = true;
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[18]);
                    EditingState.instance.rejectButtonActive = false;
                    NotificationManager.instance.AddToQueue("You have a new article to edit");
                    break;
                }

                if (!dialogueCompleted.ContainsKey(59)
                    && dialogueCompleted.ContainsKey(47))
                {
                    ShowDialogue(59);
                    dialogueCompleted[59] = true;
                    break;
                }

                if (!dialogueCompleted.ContainsKey(80)
                    && dialogueCompleted.ContainsKey(59))
                {
                    EndGameManager.instance.EndGame("prison");
                    ShowDialogue(80);
                    dialogueCompleted[80] = true;
                    break;
                }

                if (dialogueCompleted.ContainsKey(80)
                    && !dialogueCompleted.ContainsKey(56)
                    && (GameManager.instance.articleList.articles[5].isApproved == true
                    || GameManager.instance.articleList.articles[16].isApproved == true))
                {
                    ShowDialogue(56);
                    dialogueCompleted[56] = true;
                    break;
                }

                if (dialogueCompleted.ContainsKey(80)
                    && dialogueCompleted.ContainsKey(56)
                    && !dialogueCompleted.ContainsKey(57))
                {
                    ShowDialogue(57);
                    dialogueCompleted[57] = true;
                    break;
                }

                if (!dialogueCompleted.ContainsKey(56)
                    && dialogueCompleted.ContainsKey(55)
                    && (GameManager.instance.articleList.articles[5].isApproved == true
                    || GameManager.instance.articleList.articles[16].isApproved == true))
                {
                    ShowDialogue(56);
                    dialogueCompleted[56] = true;
                    break;
                }
         
                if (!dialogueCompleted.ContainsKey(57)
                    && dialogueCompleted.ContainsKey(56)
                    && (GameManager.instance.articleList.articles[5].isApproved == true
                    || GameManager.instance.articleList.articles[16].isApproved == true))
                {
                    ShowDialogue(57);
                    dialogueCompleted[57] = true;
                    break;
                }

                if (!dialogueCompleted.ContainsKey(49)
                    && (dialogueCompleted.ContainsKey(48)
                    || dialogueCompleted.ContainsKey(45)
                    || dialogueCompleted.ContainsKey(46))
                    && GameManager.instance.articleList.articles[18].isEdited == true)
                {
                    ShowDialogue(49);
                    dialogueCompleted[49] = true;
                    break;
                }

                if (!dialogueCompleted.ContainsKey(60)
                    && dialogueCompleted.ContainsKey(49)
                    && dialogueCompleted.ContainsKey(48))
                {
                    ShowDialogue(60);
                    dialogueCompleted[60] = true;
                    break;
                }

                if (!dialogueCompleted.ContainsKey(58)
                    && dialogueCompleted.ContainsKey(60)
                    && dialogueCompleted.ContainsKey(48)
                    && choicesDictionary.ContainsKey("Escape")
                    && (GameManager.instance.articleList.articles[5].isApproved == true
                    || GameManager.instance.articleList.articles[16].isApproved == true))
                {
                    EndGameManager.instance.EndGame("cafe");
                    ShowDialogue(58);
                    dialogueCompleted[58] = true;
                    break;
                }

                if (!dialogueCompleted.ContainsKey(51)
                    && dialogueCompleted.ContainsKey(60)
                    && dialogueCompleted.ContainsKey(48)
                    && !choicesDictionary.ContainsKey("Escape")
                    && (GameManager.instance.articleList.articles[5].isApproved == false
                    && GameManager.instance.articleList.articles[16].isApproved == false))
                {
                    ShowDialogue(51);
                    dialogueCompleted[51] = true;
                    break;
                }

                if (!dialogueCompleted.ContainsKey(51)
                    && dialogueCompleted.ContainsKey(60)
                    && dialogueCompleted.ContainsKey(48)
                    && !choicesDictionary.ContainsKey("Escape")
                    && (GameManager.instance.articleList.articles[5].isApproved == true
                    || GameManager.instance.articleList.articles[16].isApproved == true))
                {
                    ShowDialogue(51);
                    dialogueCompleted[51] = true;
                    break;
                }
                if (!dialogueCompleted.ContainsKey(52)
                    && dialogueCompleted.ContainsKey(60)
                    && dialogueCompleted.ContainsKey(51)
                    && dialogueCompleted.ContainsKey(48)
                    && !choicesDictionary.ContainsKey("Escape")
                    && (GameManager.instance.articleList.articles[5].isApproved == true
                    || GameManager.instance.articleList.articles[16].isApproved == true))
                {
                    ShowDialogue(52);
                    dialogueCompleted[52] = true;
                    break;
                }
                if (!dialogueCompleted.ContainsKey(53)
                    && dialogueCompleted.ContainsKey(60)
                    && dialogueCompleted.ContainsKey(51)
                    && dialogueCompleted.ContainsKey(52)
                    && dialogueCompleted.ContainsKey(48)
                    && !choicesDictionary.ContainsKey("Escape")
                    && (GameManager.instance.articleList.articles[5].isApproved == true
                    || GameManager.instance.articleList.articles[16].isApproved == true))
                {
                    ShowDialogue(53);
                    dialogueCompleted[53] = true;
                    break;
                }

                if (!dialogueCompleted.ContainsKey(61)
                    && dialogueCompleted.ContainsKey(60)
                    && dialogueCompleted.ContainsKey(48)
                    && choicesDictionary.ContainsKey("Escape")
                    && (GameManager.instance.articleList.articles[5].isApproved == false
                    && GameManager.instance.articleList.articles[16].isApproved == false))
                {
                    EndGameManager.instance.EndGame("cafe");
                    ShowDialogue(61);
                    dialogueCompleted[61] = true;
                    break;
                }







                if (!dialogueCompleted.ContainsKey(50)
                    && dialogueCompleted.ContainsKey(49)
                    && (dialogueCompleted.ContainsKey(45)
                    || dialogueCompleted.ContainsKey(46))
                    && (GameManager.instance.articleList.articles[5].isApproved == false
                    && GameManager.instance.articleList.articles[16].isApproved == false))
                {
                    ShowDialogue(50);
                    dialogueCompleted[50] = true;
                    break;
                }

                if (!dialogueCompleted.ContainsKey(51)
                    && dialogueCompleted.ContainsKey(49)
                    && (dialogueCompleted.ContainsKey(45)
                    || dialogueCompleted.ContainsKey(46))
                    && (GameManager.instance.articleList.articles[5].isApproved == true
                    || GameManager.instance.articleList.articles[16].isApproved == true))
                {
                    ShowDialogue(51);
                    dialogueCompleted[51] = true;
                    break;
                }

                if (!dialogueCompleted.ContainsKey(52)
                    && dialogueCompleted.ContainsKey(51)
                    && (dialogueCompleted.ContainsKey(45)
                    || dialogueCompleted.ContainsKey(46))
                    && (GameManager.instance.articleList.articles[5].isApproved == true
                    || GameManager.instance.articleList.articles[16].isApproved == true))
                {
                    ShowDialogue(52);
                    dialogueCompleted[52] = true;
                    break;
                }

                if (!dialogueCompleted.ContainsKey(53)
                    && dialogueCompleted.ContainsKey(52)
                    && (dialogueCompleted.ContainsKey(45)
                    || dialogueCompleted.ContainsKey(46))
                    && (GameManager.instance.articleList.articles[5].isApproved == true
                    || GameManager.instance.articleList.articles[16].isApproved == true))
                {
                    ShowDialogue(53);
                    dialogueCompleted[53] = true;
                    break;
                }




                if (dialogueCompleted.ContainsKey(41)
                    && !dialogueCompleted.ContainsKey(62)
                    && choicesDictionary.ContainsKey("TurnIn")
                    && !choicesDictionary.ContainsKey("RefuseHelp")
                    && !choicesDictionary.ContainsKey("IWrite")
                    && !choicesDictionary.ContainsKey("AbrahamWrite")
                    && !choicesDictionary.ContainsKey("MargaretWrite"))
                {
                    ShowDialogue(62);
                    dialogueCompleted[62] = true;
                    break;
                }

                if (dialogueCompleted.ContainsKey(62)
                    && !dialogueCompleted.ContainsKey(63)
                    && choicesDictionary.ContainsKey("TurnIn")
                    && !choicesDictionary.ContainsKey("RefuseHelp")
                    && !choicesDictionary.ContainsKey("IWrite")
                    && !choicesDictionary.ContainsKey("AbrahamWrite")
                    && !choicesDictionary.ContainsKey("MargaretWrite"))
                {
                    ShowDialogue(63);
                    dialogueCompleted[63] = true;
                    break;
                }

                if (dialogueCompleted.ContainsKey(63)
                    && !dialogueCompleted.ContainsKey(64)
                    && choicesDictionary.ContainsKey("TurnIn")
                    && !choicesDictionary.ContainsKey("RefuseHelp")
                    && !choicesDictionary.ContainsKey("IWrite")
                    && !choicesDictionary.ContainsKey("AbrahamWrite")
                    && !choicesDictionary.ContainsKey("MargaretWrite"))
                {
                    ShowDialogue(64);
                    dialogueCompleted[64] = true;
                    GameManager.instance.uneditedArticles.Add(GameManager.instance.articleList.articles[22]);
                    EditingState.instance.rejectButtonActive = false;
                    NotificationManager.instance.AddToQueue("You have a new article to edit");
                    break;
                }

                if (dialogueCompleted.ContainsKey(64)
                    && !dialogueCompleted.ContainsKey(65)
                    && choicesDictionary.ContainsKey("TurnIn")
                    && !choicesDictionary.ContainsKey("RefuseHelp")
                    && !choicesDictionary.ContainsKey("IWrite")
                    && !choicesDictionary.ContainsKey("AbrahamWrite")
                    && !choicesDictionary.ContainsKey("MargaretWrite")
                    && (GameManager.instance.articleList.articles[5].isApproved == false
                    && GameManager.instance.articleList.articles[16].isApproved == false)
                    && GameManager.instance.articleList.articles[22].isEdited == true)
                {
                    ShowDialogue(65);
                    dialogueCompleted[65] = true;
                    break;
                }

                if (dialogueCompleted.ContainsKey(64)
                    && !dialogueCompleted.ContainsKey(66)
                    && choicesDictionary.ContainsKey("TurnIn")
                    && !choicesDictionary.ContainsKey("RefuseHelp")
                    && !choicesDictionary.ContainsKey("IWrite")
                    && !choicesDictionary.ContainsKey("AbrahamWrite")
                    && !choicesDictionary.ContainsKey("MargaretWrite")
                    && (GameManager.instance.articleList.articles[5].isApproved == true
                    || GameManager.instance.articleList.articles[16].isApproved == true)
                    && GameManager.instance.articleList.articles[22].isEdited == true)
                {
                    ShowDialogue(66);
                    dialogueCompleted[66] = true;
                    break;
                }




                if (dialogueCompleted.ContainsKey(41)
                    && !dialogueCompleted.ContainsKey(67)
                    && !choicesDictionary.ContainsKey("TurnIn")
                    && choicesDictionary.ContainsKey("RefuseHelp")
                    && !choicesDictionary.ContainsKey("IWrite")
                    && !choicesDictionary.ContainsKey("AbrahamWrite")
                    && !choicesDictionary.ContainsKey("MargaretWrite"))
                {
                    ShowDialogue(67);
                    dialogueCompleted[67] = true;
                    break;
                }

                if (dialogueCompleted.ContainsKey(67)
                    && !dialogueCompleted.ContainsKey(68)
                    && !choicesDictionary.ContainsKey("TurnIn")
                    && choicesDictionary.ContainsKey("RefuseHelp")
                    && !choicesDictionary.ContainsKey("IWrite")
                    && !choicesDictionary.ContainsKey("AbrahamWrite")
                    && !choicesDictionary.ContainsKey("MargaretWrite")
                    && (GameManager.instance.articleList.articles[5].isApproved == true
                    || GameManager.instance.articleList.articles[16].isApproved == true))
                {
                    EndGameManager.instance.EndGame("street");
                    ShowDialogue(68);
                    dialogueCompleted[68] = true;
                    break;
                }

                if (dialogueCompleted.ContainsKey(67)
                    && !dialogueCompleted.ContainsKey(69)
                    && !dialogueCompleted.ContainsKey(68)
                    && !choicesDictionary.ContainsKey("TurnIn")
                    && choicesDictionary.ContainsKey("RefuseHelp")
                    && !choicesDictionary.ContainsKey("IWrite")
                    && !choicesDictionary.ContainsKey("AbrahamWrite")
                    && !choicesDictionary.ContainsKey("MargaretWrite")
                    && (GameManager.instance.articleList.articles[5].isApproved == false
                    && GameManager.instance.articleList.articles[16].isApproved == false))
                {
                    EndGameManager.instance.EndGame("street");
                    ShowDialogue(69);
                    dialogueCompleted[69] = true;
                    break;
                }




                if (dialogueCompleted.ContainsKey(41)
                    && !dialogueCompleted.ContainsKey(70)
                    && !choicesDictionary.ContainsKey("TurnIn")
                    && !choicesDictionary.ContainsKey("RefuseHelp")
                    && !choicesDictionary.ContainsKey("IWrite")
                    && choicesDictionary.ContainsKey("AbrahamWrite")
                    && !choicesDictionary.ContainsKey("MargaretWrite")
                    && GameManager.instance.articleList.articles[22].isEdited == true)
                {
                    ShowDialogue(70);
                    dialogueCompleted[70] = true;
                    break;
                }
                if (dialogueCompleted.ContainsKey(70)
                    && !dialogueCompleted.ContainsKey(74)
                    && !choicesDictionary.ContainsKey("TurnIn")
                    && !choicesDictionary.ContainsKey("RefuseHelp")
                    && !choicesDictionary.ContainsKey("IWrite")
                    && choicesDictionary.ContainsKey("AbrahamWrite")
                    && !choicesDictionary.ContainsKey("MargaretWrite"))
                {
                    ShowDialogue(74);
                    dialogueCompleted[74] = true;
                    break;
                }

                if (dialogueCompleted.ContainsKey(41)
                    && !dialogueCompleted.ContainsKey(71)
                    && !choicesDictionary.ContainsKey("TurnIn")
                    && !choicesDictionary.ContainsKey("RefuseHelp")
                    && !choicesDictionary.ContainsKey("IWrite")
                    && !choicesDictionary.ContainsKey("AbrahamWrite")
                    && choicesDictionary.ContainsKey("MargaretWrite")
                    && GameManager.instance.articleList.articles[23].isEdited == true)
                {
                    ShowDialogue(71);
                    dialogueCompleted[71] = true;
                    break;
                }
                if (dialogueCompleted.ContainsKey(71)
                    && !dialogueCompleted.ContainsKey(74)
                    && !choicesDictionary.ContainsKey("TurnIn")
                    && !choicesDictionary.ContainsKey("RefuseHelp")
                    && !choicesDictionary.ContainsKey("IWrite")
                    && !choicesDictionary.ContainsKey("AbrahamWrite")
                    && choicesDictionary.ContainsKey("MargaretWrite"))
                {
                    ShowDialogue(74);
                    dialogueCompleted[74] = true;
                    break;
                }



                if (dialogueCompleted.ContainsKey(41)
                    && !dialogueCompleted.ContainsKey(72)
                    && !choicesDictionary.ContainsKey("TurnIn")
                    && !choicesDictionary.ContainsKey("RefuseHelp")
                    && choicesDictionary.ContainsKey("IWrite")
                    && !choicesDictionary.ContainsKey("AbrahamWrite")
                    && !choicesDictionary.ContainsKey("MargaretWrite")
                    && GameManager.instance.articleList.articles[21].isEdited == true)
                {
                    ShowDialogue(72);
                    dialogueCompleted[72] = true;
                    break;
                }
                if (dialogueCompleted.ContainsKey(72)
                    && !dialogueCompleted.ContainsKey(73)
                    && !choicesDictionary.ContainsKey("TurnIn")
                    && !choicesDictionary.ContainsKey("RefuseHelp")
                    && choicesDictionary.ContainsKey("IWrite")
                    && !choicesDictionary.ContainsKey("AbrahamWrite")
                    && !choicesDictionary.ContainsKey("MargaretWrite")
                    && GameManager.instance.articleList.articles[21].isEdited == true)
                {
                    ShowDialogue(73);
                    dialogueCompleted[73] = true;

                    break;
                }

                if (dialogueCompleted.ContainsKey(71)
                    && !dialogueCompleted.ContainsKey(75)
                    && choicesDictionary.ContainsKey("Escape")
                    && (GameManager.instance.articleList.articles[5].isApproved == true
                    || GameManager.instance.articleList.articles[16].isApproved == true))
                {
                    EndGameManager.instance.EndGame("cafe");
                    ShowDialogue(75);
                    dialogueCompleted[75] = true;
                    break;
                }

                if (dialogueCompleted.ContainsKey(70)
                    && !dialogueCompleted.ContainsKey(81)
                    && choicesDictionary.ContainsKey("Escape")
                    && (GameManager.instance.articleList.articles[5].isApproved == true
                    || GameManager.instance.articleList.articles[16].isApproved == true))
                {
                    EndGameManager.instance.EndGame("cafe");
                    ShowDialogue(81);
                    dialogueCompleted[81] = true;
                    break;
                }

                if (dialogueCompleted.ContainsKey(72)
                    && !dialogueCompleted.ContainsKey(82)
                    && choicesDictionary.ContainsKey("Escape")
                    && (GameManager.instance.articleList.articles[5].isApproved == true
                    || GameManager.instance.articleList.articles[16].isApproved == true))
                {
                    EndGameManager.instance.EndGame("cafe");
                    ShowDialogue(82);
                    dialogueCompleted[82] = true;
                    break;
                }

                if (dialogueCompleted.ContainsKey(70)
                    && !dialogueCompleted.ContainsKey(75)
                    && !dialogueCompleted.ContainsKey(76)
                    && choicesDictionary.ContainsKey("Escape")
                    && (GameManager.instance.articleList.articles[5].isApproved == false
                    && GameManager.instance.articleList.articles[16].isApproved == false))
                {
                    EndGameManager.instance.EndGame("cafe");
                    ShowDialogue(76);
                    dialogueCompleted[76] = true;
                    break;
                }

                if (dialogueCompleted.ContainsKey(71)
                    && !dialogueCompleted.ContainsKey(75)
                    && !dialogueCompleted.ContainsKey(83)
                    && choicesDictionary.ContainsKey("Escape")
                    && (GameManager.instance.articleList.articles[5].isApproved == false
                    && GameManager.instance.articleList.articles[16].isApproved == false))
                {
                    EndGameManager.instance.EndGame("cafe");
                    ShowDialogue(83);
                    dialogueCompleted[83] = true;
                    break;
                }

                if (dialogueCompleted.ContainsKey(72)
                    && !dialogueCompleted.ContainsKey(75)
                    && !dialogueCompleted.ContainsKey(84)
                    && choicesDictionary.ContainsKey("Escape")
                    && (GameManager.instance.articleList.articles[5].isApproved == false
                    && GameManager.instance.articleList.articles[16].isApproved == false))
                {
                    EndGameManager.instance.EndGame("cafe");
                    ShowDialogue(84);
                    dialogueCompleted[84] = true;
                    break;
                }

                if ((dialogueCompleted.ContainsKey(70)
                    || dialogueCompleted.ContainsKey(71))
                    && dialogueCompleted.ContainsKey(74)
                    && !dialogueCompleted.ContainsKey(72)
                    && !dialogueCompleted.ContainsKey(77)
                    && !choicesDictionary.ContainsKey("Escape")
                    && (GameManager.instance.articleList.articles[5].isApproved == false
                    && GameManager.instance.articleList.articles[16].isApproved == false))
                {
                    ShowDialogue(77);
                    dialogueCompleted[77] = true;
                    break;
                }

                if (dialogueCompleted.ContainsKey(72)
                    && !dialogueCompleted.ContainsKey(55)
                    && !choicesDictionary.ContainsKey("Escape")
                    && (GameManager.instance.articleList.articles[5].isApproved == true
                    || GameManager.instance.articleList.articles[16].isApproved == true))
                {
                    EndGameManager.instance.EndGame("prison");
                    ShowDialogue(55);
                    dialogueCompleted[55] = true;
                    break;
                }

                if (dialogueCompleted.ContainsKey(72)
                    && !dialogueCompleted.ContainsKey(79)
                    && !choicesDictionary.ContainsKey("Escape")
                    && (GameManager.instance.articleList.articles[5].isApproved == false
                    && GameManager.instance.articleList.articles[16].isApproved == false))
                {
                    EndGameManager.instance.EndGame("prison");
                    ShowDialogue(79);
                    dialogueCompleted[79] = true;
                    break;
                }

                if ((dialogueCompleted.ContainsKey(70)
                    || dialogueCompleted.ContainsKey(71))
                    && dialogueCompleted.ContainsKey(74)
                    && !dialogueCompleted.ContainsKey(72)
                    && !dialogueCompleted.ContainsKey(78)
                    && !choicesDictionary.ContainsKey("Escape")
                    && (GameManager.instance.articleList.articles[5].isApproved == true
                    || GameManager.instance.articleList.articles[16].isApproved == true))
                {
                    ShowDialogue(78);
                    dialogueCompleted[78] = true;
                    break;
                }

                if (dialogueCompleted.ContainsKey(78)
                    && !dialogueCompleted.ContainsKey(52)
                    && !choicesDictionary.ContainsKey("Escape")
                    && (GameManager.instance.articleList.articles[5].isApproved == true
                    || GameManager.instance.articleList.articles[16].isApproved == true))
                {
                    ShowDialogue(52);
                    dialogueCompleted[52] = true;
                    break;
                }

                if (dialogueCompleted.ContainsKey(78)
                    && dialogueCompleted.ContainsKey(52)
                    && !dialogueCompleted.ContainsKey(53)
                    && !choicesDictionary.ContainsKey("Escape")
                    && (GameManager.instance.articleList.articles[5].isApproved == true
                    || GameManager.instance.articleList.articles[16].isApproved == true))
                {
                    ShowDialogue(53);
                    dialogueCompleted[53] = true;
                    break;
                }
                
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