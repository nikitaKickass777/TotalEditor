using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EditingField : MonoBehaviour
{
    public TMP_Text textDisplay;
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
            HandleKeyHold(KeyCode.RightArrow, 1);
            HandleKeyHold(KeyCode.LeftArrow, -1);
        }
        
    }
    
    
    void HandleKeyHold(KeyCode key, int direction)
    {
        if (Input.GetKeyDown(key))
        {
            MoveCursor(direction);
            keyHoldTimer = Time.time + repeatDelay;
            keyHeld = true;
            lastKeyPressed = key;
        }

        if (Input.GetKey(key) && keyHeld && lastKeyPressed == key)
        {
            if (Time.time >= keyHoldTimer)
            {
                MoveCursor(direction);
                keyHoldTimer = Time.time + repeatRate; // Continuous movement
            }
        }

        if (Input.GetKeyUp(key))
        {
            keyHeld = false;
        }
    }

    void MoveCursor(int direction)
    {
        cursorIndex = Mathf.Clamp(cursorIndex + direction, 0, originalText.Length);
        if (isSelecting)
        {
            if (selectionStart == -1) selectionStart = cursorIndex-1;
            
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
        Debug.Log($"Marked text from {start} to {end}");

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
