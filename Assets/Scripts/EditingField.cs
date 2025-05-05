using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EditingField : MonoBehaviour
{
    public TMP_Text textDisplay;
    public TMP_Text titleText;
    public int articleId = 0; // Current article ID
    public event Action<string, int> TextSelected; // Event to notify text selection
    public LawInputController lawInputController;
    private string originalText;
    private string title;
    
    
    private int cursorIndex = 0; // Position in the text
    private int selectionStart = -1;
    private int selectionEnd = -1;
    private bool isSelecting = false;
    private List<Vector2Int> markedRanges = new List<Vector2Int>();
    
    private float repeatDelay = 0.4f; // Delay before repeat starts
    private float repeatRate = 0.08f; // Speed of continuous movement
    private float keyHoldTimer = 0f;
    private bool keyHeld = false;
    private KeyCode lastKeyPressed;
    private bool fieldSelected = false; 


    void Start()
    {
        GetNextArticle();
        //General idea of how main logic should work:
        // 1. Get the article with articleID from the GameManager
        // 2. Check if it can be edited, if there is some condition
        // 3. If it can be edited, get the text and display it in the text field, increase the articleID counter
        // 4. Wait for player to approve or prohibit the text. Change the isEdited and isApproved variable in the article class
        // 5. Start the next article
        /*
         * Here is the logic for the article editing:
         * 1. Display the article title and text from the GameManager
         * 2. For approval or prohibition, player should press a button
         * 3. There should be a reason. If the player finds a mistake, he should select the text and press a button to mark it
         * 4. The text should be marked in yellow
         * 5. The player then types a law number that is being violated
         * 6. The player can select multiple parts of the text and mark them for mistakes or recommendations, previous markings are remembered
         * 7. The special comparator checks the important parts of the article, and compares them to the marked text
         * 8. If the marked text is within the tolerance of the important part, it is considered a correct marking.
         * 9. If the player correctly approved/denied, and provided reasons, he gets money and message indicating success.
         * 
         */
        
        
        
    }

    void Update()
    {
        //Handle Field Select/Deselect
        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverText())
            {
                fieldSelected = true; 
            }
            else
            {
                fieldSelected = false; 
            }
        }

        
        if(fieldSelected)
        {
            UpdateCursorDisplay();
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
                isSelecting = true;

            if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift))
                isSelecting = false;
        
            if (Input.GetKeyDown(KeyCode.Return) && selectionStart != -1 && selectionEnd != -1)
            {
                SaveMarkedText();
            }
            HandleKeyHold(KeyCode.RightArrow, 1, false);
            HandleKeyHold(KeyCode.LeftArrow, -1, false);
            HandleKeyHold(KeyCode.UpArrow, -1, true);
            HandleKeyHold(KeyCode.DownArrow, 1, true);
        }
        else
        {
            textDisplay.text = originalText; // Reset to original text when not selected
        }
        
    }
    
    
    void HandleKeyHold(KeyCode key, int direction, bool vertical = false)
    {
        if (Input.GetKeyDown(key))
        {
            MoveCursor(direction, vertical);
            keyHoldTimer = Time.time + repeatDelay;
            keyHeld = true;
            lastKeyPressed = key;
        }

        if (Input.GetKey(key) && keyHeld && lastKeyPressed == key)
        {
            if (Time.time >= keyHoldTimer)
            {
                MoveCursor(direction, vertical);
                keyHoldTimer = Time.time + repeatRate; // Continuous movement
            }
        }

        if (Input.GetKeyUp(key))
        {
            keyHeld = false;
        }
    }

    void MoveCursor(int direction, bool vertical = false)
{
    textDisplay.ForceMeshUpdate(); // Ensure text info is up-to-date
    TMP_TextInfo textInfo = textDisplay.textInfo;

    // Check if cursorIndex is within valid bounds
    if (cursorIndex < 0 || cursorIndex >= textInfo.characterCount)
    {
        Debug.LogWarning("Cursor index out of bounds.");
        return;
    }

    if (vertical)
    {
        int currentLineIndex = textInfo.characterInfo[cursorIndex].lineNumber;
        int targetLineIndex = Mathf.Clamp(currentLineIndex + direction, 0, textInfo.lineCount - 1);

        // Ensure the target line index is valid
        if (targetLineIndex < 0 || targetLineIndex >= textInfo.lineCount)
        {
            Debug.LogWarning("Target line index out of bounds.");
            return;
        }

        if (currentLineIndex != targetLineIndex)
        {
            TMP_LineInfo currentLine = textInfo.lineInfo[currentLineIndex];
            TMP_LineInfo targetLine = textInfo.lineInfo[targetLineIndex];

            // Calculate relative position in the current line
            int charInLine = cursorIndex - currentLine.firstCharacterIndex;

            // Ensure the target line has valid characters
            if (targetLine.firstCharacterIndex <= targetLine.lastCharacterIndex)
            {
                cursorIndex = Mathf.Clamp(targetLine.firstCharacterIndex + charInLine,
                                          targetLine.firstCharacterIndex,
                                          targetLine.lastCharacterIndex + 1);
            }
            else
            {
                // If the target line is empty, move to the first character of the line
                cursorIndex = targetLine.firstCharacterIndex;
            }
        }
    }
    else
    {
        cursorIndex = Mathf.Clamp(cursorIndex + direction, 0, textInfo.characterCount - 1);
    }

    if (isSelecting)
    {
        if (selectionStart == -1) selectionStart = cursorIndex - 1;

        selectionEnd = cursorIndex;
    }
    else
    {
        selectionStart = -1;
        selectionEnd = -1;
    }
    UpdateCursorDisplay();
}

    void UpdateCursorDisplay()
    {
        // Display cursor as a "|" symbol at the current index
        string updatedText = originalText;
        
        if (selectionStart != -1 && selectionEnd != -1)
        {
            int start = Mathf.Min(selectionStart, selectionEnd);
            int end = Mathf.Max(selectionStart, selectionEnd);
            string before = originalText.Substring(0, start);
            string selected = originalText.Substring(start, end - start);
            string after = originalText.Substring(end);
            updatedText = before + "<color=yellow>" + selected + "</color>" + after;
            int cursorUpdPos = cursorIndex + 14; // 14 is the length of the <color=yellow> tag
            updatedText = updatedText.Insert(cursorUpdPos, "<color=red>|</color>");
        }
        else
        {
            updatedText = updatedText.Insert(cursorIndex, "<color=red>|</color>");
        }
        
        
        textDisplay.text = updatedText;
    }
    void SaveMarkedText()
    {
        int start = Mathf.Min(selectionStart, selectionEnd);
        int end = Mathf.Max(selectionStart, selectionEnd);
        string selectedText = originalText.Substring(start, end - start);

        markedRanges.Add(new Vector2Int(start, end));
        Debug.Log($"Marked text from {start} to {end} : " + selectedText);

        // Temporarily: Ask for law input here (you can instead show a UI input field)
        // This could trigger a UI element asking player to type the law
        
        ShowLawInput(selectedText);
        lawInputController.OnTextSelected(selectedText);

        // Reset selection
        selectionStart = -1;
        selectionEnd = -1;
        UpdateCursorDisplay();
    }

    void ShowLawInput(string selectedText)
    {
        Debug.Log("Enter a law number that this selection violates.");
        // Show a popup or input field (TMP_InputField) and confirm button
        // When confirmed, you could pair the selection with the law and store it
    }
    public void SubmitLawInput(string lawNumber)
    {
        
        Debug.Log($"Law {lawNumber} submitted for marked text.");
        
    }


    private bool IsPointerOverText()
    {
        Vector3 mousePos = Input.mousePosition;
        RectTransform textRect = textDisplay.rectTransform;

        return RectTransformUtility.RectangleContainsScreenPoint(textRect, mousePos);
    }

    private void GetNextArticle()
    {
        articleId = checker();
    
        if(articleId == -1)
        {
            Debug.Log("No article to edit.");
            return;
        }

        var article = GameManager.instance.articleList.articles[articleId];
        originalText = article.text;
        title = article.title;

        textDisplay.text = originalText;
        titleText.text = title;
    }
    
    private void StartNextArticle()
    {
        // Reset all fields
        cursorIndex = 0;
        selectionStart = -1;
        selectionEnd = -1;
        markedRanges.Clear();
        articleId++;
        GetNextArticle(); // Load next article
    }
    public void ApproveArticle()
    {
        var article = GameManager.instance.articleList.articles[articleId];
        article.isApproved = true;
        article.isEdited = true;
        Debug.Log("Article approved.");

        StartNextArticle();
    }

    public void RejectArticle()
    {
        var article = GameManager.instance.articleList.articles[articleId];
        article.isApproved = false;
        article.isEdited = true;
        Debug.Log("Article rejected.");

        StartNextArticle();
    }
    public bool IsPlayerSelectionCorrect(string playerSelectedText, int playerSelectedLawId, Article article)
    {
        foreach (var importantPart in article.importantParts)
        {
            // First check if the law ID is one of the allowed law IDs
            bool lawMatches = false;
            foreach (int lawId in importantPart.lawIds)
            {
                if (lawId == playerSelectedLawId)
                {
                    lawMatches = true;
                    break;
                }
            }
            if (!lawMatches)
            {
                continue; // this important part is not for this law, check next
            }

            // Now check text similarity
            if (IsTextSimilar(playerSelectedText, importantPart.text, importantPart.tolerance))
            {
                return true; // found a match
            }
        }

        return false; // no match found
    }
    private bool IsTextSimilar(string playerText, string importantText, int tolerance)
    {
        playerText = playerText.Trim();
        importantText = importantText.Trim();

        // Exact match
        if (playerText.Equals(importantText, StringComparison.OrdinalIgnoreCase))
            return true;

        // Check if the important part exists inside the player's text
        if (playerText.IndexOf(importantText, StringComparison.OrdinalIgnoreCase) >= 0)
            return true;

        // Check if the player's selected text is inside the important text
        if (importantText.IndexOf(playerText, StringComparison.OrdinalIgnoreCase) >= 0)
            return true;

        // Check length difference tolerance
        int lengthDifference = Math.Abs(playerText.Length - importantText.Length);
        if (lengthDifference <= tolerance)
        {
            // Optional: could still consider as matching even if slightly different
            return true;
        }

        // Otherwise not similar enough
        return false;
    }



    private int checker()
    {
        switch (GameManager.instance.day)
        {
            case 1:
                switch (articleId)
                {
                    case 0:
                        
                        return 0;
                    
                    case 1:
                        return 1;
                    
                    case 2:
                        if(DialogueManager.instance.choicesDictionary.ContainsKey("testComeLater"))
                        {
                            
                            return 2;
                        }
                        else
                        {
                            Debug.Log("You need to finish the first dialogue before editing this article.");
                            return -1;
                        }
                        return 2;
                    
                    default:
                        
                        return -1;
                }
            
            
            case 2:
                return 1;
            
            
            case 3:
                return 2;
            
            
            case 4:
                return 3;
            
            
            case 5:
                return 4;
            
            
            default:
                return -1;
            
        }
    }
}
