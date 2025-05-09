
using System;
using System.Collections.Generic;
using UnityEngine;

public class EditingState : MonoBehaviour
{
    public Article currentArticle;
    public int cursorIndex;
    public int selectionStart;
    public int selectionEnd;
    public List<string> selectedTexts = new List<string>();
    public List<int> selectedLawIds = new List<int>();
    public bool lawInputFieldActive;
    public static EditingState instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(this);
    }
}