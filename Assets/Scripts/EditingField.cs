using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EditingField : MonoBehaviour
{
    public TMP_Text textDisplay;
    public int articleId = -1; // Current article ID
    public event Action<string, int> TextSelected; // Event to notify text selection
    private string originalText;
    
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
        originalText = textDisplay.text;
        //UpdateCursorDisplay();
        
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

        markedRanges.Add(new Vector2Int(start, end));
        // Extract the selected text
        string selectedText = originalText.Substring(start, end - start);

        Debug.Log($"Marked text from {start} to {end} : " + selectedText);

        // Trigger the TextSelected event
        TextSelected?.Invoke(selectedText, articleId);
        
        // Reset selection
        selectionStart = -1;
        selectionEnd = -1;
        UpdateCursorDisplay();
    }

    private bool IsPointerOverText()
    {
        Vector3 mousePos = Input.mousePosition;
        RectTransform textRect = textDisplay.rectTransform;

        return RectTransformUtility.RectangleContainsScreenPoint(textRect, mousePos);
    }
}
