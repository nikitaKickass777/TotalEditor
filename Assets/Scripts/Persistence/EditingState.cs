
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
    public static EditingState instance;

    private void Awake()
    {
        
        if (instance == null)
        {
            NotificationManager.instance.AddToQueue(
                "Hold SHIFT and move with ARROWS to mark text.",5);
            NotificationManager.instance.AddToQueue(
                "Then press ENTER to select. Choose law and submit the article", 10);
            NotificationManager.instance.AddToQueue(
                "You can also use CTRL + ARROWS to move the cursor faster.", 15);
            
            instance = this;
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(this);
    }
}