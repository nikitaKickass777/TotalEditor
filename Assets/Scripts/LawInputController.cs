using UnityEngine;
using TMPro; // if you use TextMeshPro

public class LawInputController : MonoBehaviour
{
    public TMP_InputField lawInputField; // assign in Inspector
    private bool isSelectingText = false;

    private string selectedText = ""; // the text player selected

    // Call this when player selects text
    public void OnTextSelected(string text)
    {
        selectedText = text;
        isSelectingText = true;
        lawInputField.gameObject.SetActive(true);
        lawInputField.text = ""; // clear previous input
        lawInputField.ActivateInputField(); // focus
    }

    // Call this when player submits the law input
    public void OnLawSubmitted()
    {
        if (!isSelectingText) return;

        string playerInput = lawInputField.text.Trim();
        if (int.TryParse(playerInput, out int lawId))
        {
            Debug.Log($"Player entered law ID: {lawId} for text: {selectedText}");

            // Here you can now check if this lawId is correct for the selected text
            ValidateLawSelection(selectedText, lawId);
        }
        else
        {
            Debug.LogWarning("Invalid law ID input. Please enter a number.");
        }

        // Deactivate input field again
        lawInputField.gameObject.SetActive(false);
        isSelectingText = false;
        selectedText = "";
    }

    // Validate if the law ID matches the selected text
    private void ValidateLawSelection(string selectedText, int enteredLawId)
    {
        // Example: loop through your article's important parts and check
        // (I can show you the full check if you want next)
        Debug.Log($"Validating selected text: '{selectedText}' with law ID: {enteredLawId}");
    }
}