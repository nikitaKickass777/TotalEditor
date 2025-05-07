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
    public TextMeshProUGUI dialogueText;
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
    private Coroutine talkAnimation;
    
    

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
       // ShowDialogue(0);
        isDialogueOpen = false;
        choicesDictionary = new Dictionary<string, bool>();
        dialogueCompleted = new Dictionary<int, bool>();
        
    }

    private void Update()
    {
        // if game state is not paused and scene is office and player does not have a dialogue open
        // check should be performed each time the player enters the office scene
        //call correct checker function depending on the current day (maybe make a separate function for each day)
        //there should be the bool array, where each index corresponds to some important dialogue choice,
        //if it is true then the dialogue special option(one that changes some bool) has been triggered
        //checker uses these bools as one of the conditions to determine which dialogue to show
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if(DialogueTemplate.activeSelf)
            {
                EndDialogue();
            }
            else
            {
                ShowDialogue(0);
            }
        }
        checker(1);
        
    }

    void LoadDialogues()
    {
        // Load JSON file from Resources folder
        TextAsset jsonFile = Resources.Load<TextAsset>("dialogues");
        
        if (jsonFile != null)
        {
            dialogueList = JsonUtility.FromJson<DialogueList>(jsonFile.text);
            Debug.Log("Dialogues Loaded Successfully!");
        }
        else
        {
            Debug.LogError("Failed to load dialogues.json");
        }
    }
    
    
    public void ShowDialogue(int dialogueIndex)
    {
        currentDialogueIndex = dialogueIndex;
        currentLineIndex = 0;  // Start at first line
        Dialogue dialogue = dialogueList.dialogues[dialogueIndex];

        Time.timeScale = 0; 
        isDialogueOpen = true;
        DialogueTemplate.SetActive(true); // this thing makes dialogue box appear
        Journalist character = GameManager.instance.journalistList.journalists[dialogue.characterId];
        characterSprite = character.portrait;
        characterSpriteTalk = character.portraitTalking;
      
        
        // Set character portrait
       // characterPortrait.sprite = dialogue.characterName;
    
        LoadLine(0, dialogueIndex);  // Load first line
    }


    public void LoadLine(int lineIndex, int dialogueIndex)
    {
        currentLineIndex = lineIndex;
        DialogueLine line = dialogueList.dialogues[dialogueIndex].lines[lineIndex];

        if (typeCoroutine != null) StopCoroutine(typeCoroutine);
        
        typeCoroutine = StartCoroutine(TypeLine( line.lineText ));
        if (line.hasOptions)
        {
            ShowOptions(line);
        }
        else
        {
            HideOptions();
            // Set up a listener for player clicks
            StartCoroutine(WaitForPlayerClick(line.nextLineId, dialogueIndex));
        }
    }
    private IEnumerator WaitForPlayerClick(int nextLineId, int dialogueIndex)
    {
        // Wait until the player clicks
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        if (nextLineId >= 0)
        {
            LoadLine(nextLineId, dialogueIndex);
        }
        else
        {
            EndDialogue();
        }
    }

    private IEnumerator TypeLine(string line)
    {
        talkAnimation = StartCoroutine(TalkAnimation());
        dialogueText.text = "";
        foreach (char c in line.ToCharArray())
        {
            if(Input.GetMouseButtonDown(0))
            {
                StopCoroutine(talkAnimation);
                dialogueText.text = line;
                yield break;
            }
            dialogueText.text += c;
            
            yield return new WaitForSecondsRealtime(textSpeed);
        }
        StopCoroutine(talkAnimation);
        characterSpriteRenderer.sprite = characterSprite;
        
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
    
    
    public void OnOptionSelected(int optionIndex)
    {
        Debug.Log("Selected option: " + optionIndex);
        DialogueLine currentLine = dialogueList.dialogues[currentDialogueIndex].lines[currentLineIndex];
        DialogueOption selectedOption = currentLine.options[optionIndex];
        
        ApplyOptionEffects(selectedOption);
        Debug.Log("Selected option next Line: " + selectedOption.nextLineId);
        if (selectedOption.nextLineId >= 0)
        {
            LoadLine(selectedOption.nextLineId, currentDialogueIndex);
        }
        else
        {
            Debug.Log("End of dialogue");
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
        yield return new WaitForSeconds(0.1f);
        isDialogueOpen = false;

    }
    private void ApplyOptionEffects(DialogueOption option)
    {
        if(option.conditionChange != null)
        {
            choicesDictionary[option.conditionChange] = true;
        }
    }

    private void checker(int day)
    {
        // here goes all ingame logic for dialogue selection
        switch (day)
        {
            case 1:
                
                if (GameManager.instance.money > 10
                    && !choicesDictionary.ContainsKey("testComeLater")
                    && !isDialogueOpen
                    && SceneManager.GetActiveScene().name == "Office"
                    && !dialogueCompleted.ContainsKey(0))
                {
                    ShowDialogue(0);
                    dialogueCompleted[0] = true;
                }

                if (GameManager.instance.time > 10
                    && !choicesDictionary.ContainsKey("testComeLater")
                    && !isDialogueOpen
                    && SceneManager.GetActiveScene().name == "Office"
                    && !dialogueCompleted.ContainsKey(1))
                {
                    ShowDialogue(1);
                    dialogueCompleted[1] = true;
                }
                
                
                if(GameManager.instance.time > 20
                   && choicesDictionary.ContainsKey("testComeLater")
                   && !isDialogueOpen
                   && SceneManager.GetActiveScene().name == "Office"
                   && !dialogueCompleted.ContainsKey(2))
                {
                    ShowDialogue(2);
                    dialogueCompleted[2] = true;
                }
                
                
                
                break;
            
            default:
                Debug.Log("Invalid day");
                break;
            
        }
    }

}
