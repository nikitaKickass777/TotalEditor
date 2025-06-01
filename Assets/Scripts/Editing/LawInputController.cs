using System;
using UnityEngine;
using TMPro;

public class LawInputController : MonoBehaviour
{
    public TMP_InputField lawInputField; // assign in Inspector

    private string selectedText = ""; // store selected text
    
    public delegate void LawSubmitted(string selectedText, int lawId);
    public static event LawSubmitted OnLawSubmitted; // event to notify (selectedText, lawId)
    
    private void Start()
    {
        EditingField.OnTextSelected += HandleTextSelected;

        lawInputField.characterLimit = 2;
        lawInputField.contentType = TMP_InputField.ContentType.IntegerNumber;

        lawInputField.gameObject.SetActive(false); // start hidden

        lawInputField.onSubmit.AddListener(SubmitLaw);
    }
    private void Update()
    {
        if (Input.anyKeyDown && !Input.GetKeyDown(KeyCode.Escape) && !Input.GetKeyDown(KeyCode.Space) && !Input.GetKeyDown(KeyCode.Return))
        {
            AudioManager.instance.PlayRandomTypingSound();
        }
    }

    private void HandleTextSelected(string text)
    {
        selectedText = text;
        lawInputField.text = ""; // clear input
        lawInputField.gameObject.SetActive(true);
        lawInputField.ActivateInputField(); // focus input
        
    }

    private void SubmitLaw(string input)
    {
        if (int.TryParse(input, out int lawId))
        {
            Debug.Log($"Law submitted: {lawId} for text: {selectedText}");
            OnLawSubmitted?.Invoke(selectedText, lawId);
        }
        else
        {
            Debug.LogWarning("Invalid input, enter a number!");
        }

        lawInputField.gameObject.SetActive(false); // hide input after submission
        
    }

    private void OnDestroy()
    {
        EditingField.OnTextSelected -= HandleTextSelected;
        lawInputField.onSubmit.RemoveListener(SubmitLaw);
    }
}