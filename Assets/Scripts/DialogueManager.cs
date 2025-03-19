using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

using UnityEngine.UI;


public class DialogueManager : MonoBehaviour
{
    
    
    public GameObject DialogueTemplate;
    public TextMeshProUGUI dialogueText;
    public Image characterPortrait;
    public Button[] optionButtons;
    public int currentDialogueIndex;
    public int currentLineIndex;
    public DialogueData dialogueData;
    bool[] choices = new bool[100];

    void Start()
    {
        LoadDialogues();
       // ShowDialogue(0);
        choices[0] = false;
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
        
    }

    void LoadDialogues()
    {
        // Load JSON file from Resources folder
        TextAsset jsonFile = Resources.Load<TextAsset>("dialogues");
        
        if (jsonFile != null)
        {
            dialogueData = JsonUtility.FromJson<DialogueData>(jsonFile.text);
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
        Dialogue dialogue = dialogueData.dialogues[dialogueIndex];

        Time.timeScale = 0; // TODO: replace with method that stops clock
        DialogueTemplate.SetActive(true); // this thing makes dialogue box appear
    
        // Set character portrait
       // characterPortrait.sprite = dialogue.characterName;
    
        LoadLine(0, dialogueIndex);  // Load first line
    }


    public void LoadLine(int lineIndex, int dialogueIndex)
    {
        currentLineIndex = lineIndex;
        DialogueLine line = dialogueData.dialogues[dialogueIndex].lines[lineIndex];

        dialogueText.text = line.lineText;

        if (line.hasOptions)
        {
            ShowOptions(line);
        }
        else
        {
            HideOptions();
        }
    }
    public void OnOptionSelected(int optionIndex)
    {
        Debug.Log("Selected option: " + optionIndex);
        DialogueLine currentLine = dialogueData.dialogues[currentDialogueIndex].lines[currentLineIndex];
        DialogueOption selectedOption = currentLine.options[optionIndex];
        

        // Apply effects
        //ModifyMoney(selectedOption.moneyChange);
        //ModifyRelationship(selectedOption.relationshipChange, selectedOption.affectedCharacter);

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
        //if(line)
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
    }

    private int checker(int day)
    {
        // here goes all ingame logic for dialogue selection
        switch (day)
        {
            case 1:
                if(choices[0])
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
                
                break;
            
            default:
                Debug.Log("Invalid day");
                return -1;
                break;
            
        }
    }

}
