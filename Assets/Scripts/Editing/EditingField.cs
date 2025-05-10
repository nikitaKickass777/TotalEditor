using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.Events;


public class EditingField : MonoBehaviour
{
    public TMP_Text textDisplay;
    public TMP_Text titleText;


    public Article currentArticle;
    private string originalText;
    private string title;

    [SerializeField] private int cursorIndex = 0; // Position in the text
    [SerializeField] private int selectionStart = -1;
    [SerializeField] private int selectionEnd = -1;
    [SerializeField] private bool isSelecting = false;
    [SerializeField] private List<string> selectedTexts = new List<string>();
    [SerializeField] private List<int> selectedLawIds = new List<int>();
    [SerializeField] private bool lawInputFieldActive = false;

    private float repeatDelay = 0.4f; // Delay before repeat starts
    private float repeatRate = 0.08f; // Speed of continuous movement
    private float keyHoldTimer = 0f;
    private bool keyHeld = false;
    private KeyCode lastKeyPressed;
    private bool fieldSelected = false;


    public delegate void FieldSelectedDelegate(String text);

    public static event FieldSelectedDelegate OnTextSelected; // Event for field selection

    public delegate void SubmitArticleDelagate(Article article, List<string> selectedText, List<int> selectedLaws,
        bool isRejected);

    public static event SubmitArticleDelagate OnArticleSubmitted; // Event for article submission


    void Start()
    {
        ArticleEditorManager.OnArticleSelected += HandleArticleSelected;
        LawInputController.OnLawSubmitted += HandleLawSubmitted;
        SceneNavigator.OnSceneChange += HandleSceneChange;
        if (EditingState.instance.currentArticle.text.Length != 0)
        {
            currentArticle = EditingState.instance.currentArticle;
            cursorIndex = EditingState.instance.cursorIndex;
            selectionStart = EditingState.instance.selectionStart;
            selectionEnd = EditingState.instance.selectionEnd;
            selectedTexts = EditingState.instance.selectedTexts;
            selectedLawIds = EditingState.instance.selectedLawIds;
            lawInputFieldActive = EditingState.instance.lawInputFieldActive;
            originalText = currentArticle.text;
            title = currentArticle.title;
            textDisplay.text = originalText;
            titleText.text = title;
            Debug.Log("Loaded Saved State");
            if (selectedTexts.Count != selectedLawIds.Count && lawInputFieldActive)
            {
                Debug.Log("Law input field active, but not all laws submitted");
                Debug.Log("Selected texts: " + string.Join(", ", selectedTexts));
                OnTextSelected?.Invoke(selectedTexts[selectedTexts.Count - 1]);
                Debug.Log("Selected texts after invocation: " + string.Join(", ", selectedTexts));
            }
        }
        else
        {
            ArticleEditorManager.SelectNextArticle();
        }
        fieldSelected = true;


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


        if (fieldSelected)
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

        Debug.Log($"Marked text from {start} to {end} : " + selectedText);

        // Temporarily: Ask for law input here (you can instead show a UI input field)
        // This could trigger a UI element asking player to type the law
        selectedTexts.Add(selectedText);
        OnTextSelected?.Invoke(selectedText);
        lawInputFieldActive = true;
        // Reset selection
        selectionStart = -1;
        selectionEnd = -1;
        UpdateCursorDisplay();
    }

    private bool IsPointerOverText()
    {
        Vector3 mousePos = Input.mousePosition;
        RectTransform textRect = textDisplay.rectTransform;
        RectTransform titleRect = titleText.rectTransform;

        return (RectTransformUtility.RectangleContainsScreenPoint(textRect, mousePos) || 
                RectTransformUtility.RectangleContainsScreenPoint(titleRect, mousePos));
    }

    private Article HandleArticleSelected(Article article)
    {
        Debug.Log("Article selected: " + article.title + " with text: " + article.text);
        currentArticle = article;
        title = article.title;
        originalText = article.text;
        textDisplay.text = originalText;
        titleText.text = title;
        // Reset cursor and selection
        cursorIndex = 0;
        selectionStart = -1;
        selectionEnd = -1;
        if (currentArticle != EditingState.instance.currentArticle)
        {
            selectedTexts.Clear();
            selectedLawIds.Clear();
        }
        return article;
    }

    private void HandleLawSubmitted(string selectedText, int lawId)
    {
        Debug.Log($"Law submitted: {lawId} for text: {selectedText}");
        selectedLawIds.Add(lawId);
        lawInputFieldActive = false;
    }

    private void HandleSceneChange(string sceneName)
    {
        EditingState.instance.currentArticle = currentArticle;
        EditingState.instance.cursorIndex = cursorIndex;
        EditingState.instance.selectionStart = selectionStart;
        EditingState.instance.selectionEnd = selectionEnd;
        EditingState.instance.selectedTexts = new List<string>(selectedTexts);
        EditingState.instance.selectedLawIds = new List<int>(selectedLawIds);
        EditingState.instance.lawInputFieldActive = lawInputFieldActive;

        Debug.Log("Editor state saved");
    }

    public void ApproveArticle()
    {
        currentArticle.isApproved = true;
        currentArticle.isEdited = true;
        GameManager.instance.uneditedArticles.Remove(currentArticle);
        Debug.Log("Article approved.");
        OnArticleSubmitted?.Invoke(currentArticle, selectedTexts, selectedLawIds, false);
    }

    public void RejectArticle()
    {
        currentArticle.isApproved = false;
        currentArticle.isEdited = true;
        GameManager.instance.uneditedArticles.Remove(currentArticle);
        Debug.Log("Article rejected.");
        OnArticleSubmitted?.Invoke(currentArticle, selectedTexts, selectedLawIds, true);
    }

    private void OnDestroy()
    {
        LawInputController.OnLawSubmitted -= HandleLawSubmitted;
        ArticleEditorManager.OnArticleSelected -= HandleArticleSelected;
        SceneNavigator.OnSceneChange -= HandleSceneChange;
    }
}