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
    [SerializeField] private List<MarkedSelection> markedSelections = new List<MarkedSelection>();
    [SerializeField] private bool lawInputFieldActive = false;

    private float repeatDelay = 0.4f; // Delay before repeat starts
    private float repeatRate = 0.08f; // Speed of continuous movement
    private float keyHoldTimer = 0f;
    private bool keyHeld = false;
    private KeyCode lastKeyPressed;
    private bool fieldSelected = false;

    public GameObject approveButton;
    public GameObject rejectButton;
    public GameObject resetButton;

    public delegate void FieldSelectedDelegate(String text);

    public static event FieldSelectedDelegate OnTextSelected; // Event for field selection

    public delegate void SubmitArticleDelagate(Article article, List<MarkedSelection> markedSelections,
        bool isRejected);

    public static event SubmitArticleDelagate OnArticleSubmitted; // Event for article submission


    void Start()
    {
        ArticleEditorManager.OnArticleSelected += HandleArticleSelected;
        LawInputController.OnLawSubmitted += HandleLawSubmitted;
        SceneNavigator.OnSceneChange += HandleSceneChange;
        approveButton.SetActive(EditingState.instance.approveButtonActive);
        rejectButton.SetActive(EditingState.instance.rejectButtonActive);
        resetButton.SetActive(EditingState.instance.resetButtonActive);
        if (EditingState.instance.currentArticle.text.Length != 0)
        {
            currentArticle = EditingState.instance.currentArticle;
            cursorIndex = EditingState.instance.cursorIndex;
            selectionStart = EditingState.instance.selectionStart;
            selectionEnd = EditingState.instance.selectionEnd;
            markedSelections = EditingState.instance.markedSelections;
            if (markedSelections.Count != 0)
            {
                if (markedSelections[markedSelections.Count - 1].lawId == -1) // show input field if player left while selecting
                {
                    OnTextSelected?.Invoke(markedSelections[markedSelections.Count - 1].text);
                }
            }
            
            lawInputFieldActive = EditingState.instance.lawInputFieldActive;
            originalText = currentArticle.text;
            title = currentArticle.title;
            textDisplay.text = originalText;
            titleText.text = title;
            Debug.Log("Loaded Saved State");
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

        fieldSelected = true;
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

        int previousIndex = cursorIndex; // Save the current cursor position before moving
        if (vertical)
        {
            int currentLineIndex = textInfo.characterInfo[cursorIndex].lineNumber;
            int targetLineIndex = Mathf.Clamp(currentLineIndex + direction, 0, textInfo.lineCount - 1);

            if (targetLineIndex == currentLineIndex)
                return;

            // Get current cursor X position
            Vector3 cursorPos = textInfo.characterInfo[cursorIndex].bottomLeft;
            float targetX = cursorPos.x;

            int closestCharIndex = -1;
            float closestDistance = float.MaxValue;

            // Iterate over characters in the target line to find the closest X
            for (int i = 0; i < textInfo.characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

                if (!charInfo.isVisible || charInfo.lineNumber != targetLineIndex)
                    continue;

                float charX = charInfo.origin;
                float dist = Mathf.Abs(charX - targetX);

                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closestCharIndex = i;
                }
            }

            // Update cursorIndex if a character was found
            if (closestCharIndex != -1)
            {
                cursorIndex = closestCharIndex;
            }
        }

        else
        {
            cursorIndex = Mathf.Clamp(cursorIndex + direction, 0, textInfo.characterCount - 1);
        }

        if (isSelecting)
        {
            if (selectionStart == -1) selectionStart = previousIndex;

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
        string raw = originalText;
        List<(int index, string tag)> inserts = new List<(int, string)>();

        // Add markers
        foreach (var mark in markedSelections)
        {
            string color;
            if (mark.lawId == -1) color = "yellow";
            else if (GameManager.instance.lawList.laws.Count-1 < mark.lawId || mark.lawId < 0) color = "grey"; // Invalid law ID
            else if (!GameManager.instance.lawList.laws[mark.lawId].isProhibition) color = "#2b8a31";
            else color = "#e53e34";
            inserts.Add((mark.startIndex, $"<color={color}>"));
            inserts.Add((mark.endIndex, "</color>"));
        }

        // Add selection
        if (selectionStart != -1 && selectionEnd != -1)
        {
            int start = Mathf.Min(selectionStart, selectionEnd);
            int end = Mathf.Max(selectionStart, selectionEnd);
            inserts.Add((start, "<color=yellow>"));
            inserts.Add((end, "</color>"));
        }

        // Add cursor
        inserts.Add((cursorIndex, "<color=red>|</color>"));

        // Sort inserts in reverse order to not mess up indices
        inserts.Sort((a, b) => b.index.CompareTo(a.index));

        // Build final string
        System.Text.StringBuilder sb = new System.Text.StringBuilder(raw);
        foreach (var insert in inserts)
        {
            if (insert.index >= 0 && insert.index <= sb.Length)
                sb.Insert(insert.index, insert.tag);
        }

        textDisplay.text = sb.ToString();
    }


    void SaveMarkedText()
    {
        int start = Mathf.Min(selectionStart, selectionEnd);
        int end = Mathf.Max(selectionStart, selectionEnd);

        // Prevent overlapping marks
        foreach (var existing in markedSelections)
        {
            if (existing.startIndex == start && existing.endIndex == end)
                return; // Already marked
            if (existing.Overlaps(new MarkedSelection("", -1, start, end)))
            {
                Debug.LogWarning("Cannot mark overlapping text.");
                return;
            }
        }

        markedSelections.Add(new MarkedSelection(originalText.Substring(start, end - start), -1, start, end));
        OnTextSelected?.Invoke(originalText.Substring(start, end - start));

        lawInputFieldActive = true;
        selectionStart = -1;
        selectionEnd = -1;
        resetButton.SetActive(true);

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
            markedSelections.Clear();
        }

        return article;
    }

    private void HandleLawSubmitted(string selectedText, int lawId)
    {
        Debug.Log($"Law submitted: {lawId} for text: {selectedText}");

        foreach (var mark in markedSelections)
        {
            if (mark.text == selectedText)
            {
                mark.lawId = lawId;
                break;
            }
        }

        lawInputFieldActive = false;
        UpdateCursorDisplay();
    }


    private void HandleSceneChange(string sceneName)
    {
        if (sceneName == "Editing")
        {
            EditingState.instance.currentArticle = currentArticle;
            EditingState.instance.cursorIndex = cursorIndex;
            EditingState.instance.selectionStart = selectionStart;
            EditingState.instance.selectionEnd = selectionEnd;
            EditingState.instance.markedSelections = new List<MarkedSelection>(markedSelections);
            EditingState.instance.lawInputFieldActive = lawInputFieldActive;
            EditingState.instance.resetButtonActive = resetButton.activeSelf;
            Debug.Log("Editor state saved");
        }
    }

    public void ApproveArticle()
    {
        currentArticle.isApproved = true;
        currentArticle.isEdited = true;
        GameManager.instance.uneditedArticles.Remove(currentArticle);
        Debug.Log("Article approved.");
        OnArticleSubmitted?.Invoke(currentArticle, markedSelections, false);
        resetButton.SetActive(false);
    }

    public void RejectArticle()
    {
        currentArticle.isApproved = false;
        currentArticle.isEdited = true;
        GameManager.instance.uneditedArticles.Remove(currentArticle);
        Debug.Log("Article rejected.");
        OnArticleSubmitted?.Invoke(currentArticle, markedSelections, true);
        resetButton.SetActive(false);
    }
    public void ResetArticle()
    {
        currentArticle.text = originalText;
        currentArticle.title = title;
        markedSelections.Clear();
        cursorIndex = 0;
        selectionStart = -1;
        selectionEnd = -1;
        lawInputFieldActive = false;
        resetButton.SetActive(false);
        UpdateCursorDisplay();
        Debug.Log("Article reset to original state.");
    }

    private void OnDestroy()
    {
        LawInputController.OnLawSubmitted -= HandleLawSubmitted;
        ArticleEditorManager.OnArticleSelected -= HandleArticleSelected;
        SceneNavigator.OnSceneChange -= HandleSceneChange;
    }
}