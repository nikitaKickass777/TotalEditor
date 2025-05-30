﻿
using System;
using System.Collections.Generic;
using UnityEngine;

public class EditingState : MonoBehaviour
{
    public Article currentArticle;
    public int cursorIndex;
    public int selectionStart;
    public int selectionEnd;
    [SerializeField] public List<MarkedSelection> markedSelections = new List<MarkedSelection>();
    public bool lawInputFieldActive;
    public bool approveButtonActive;
    public bool rejectButtonActive;
    public bool resetButtonActive;
    public static EditingState instance;

    private void Awake()
    {
        
        if (instance == null)
        {
            instance = this;
            rejectButtonActive = true;
            approveButtonActive = true;
            resetButtonActive = false;
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(this);
    }
    
}