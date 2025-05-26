using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using TMPro;

public class BoardLaws : MonoBehaviour
{
    LawList lawList; 
    GameObject[] lawPapers; // Array of law papers
    List<Law> activeLaws = new List<Law>(); // List of law texts
    int lawsPerPaper = 1; // Number of laws per paper
    private void Start()
    {
        lawList = GameManager.instance.lawList; // Get the law list from the GameManager
        lawPapers = GameObject.FindGameObjectsWithTag("Paper"); // Find all law papers in the scene
        DisplayLaws();
    }

    private void DisplayLaws()
    {
        foreach (Law law in lawList.laws)
        {
            if (law.isActive)
            {
                activeLaws.Add(law); // Add the law text to the list
            }
        }
        int papersActive =  activeLaws.Count / lawsPerPaper; 
        if( activeLaws.Count % lawsPerPaper != 0)
        {
            papersActive++;
        }
        for(int i = 0; i < papersActive; i++)
        {
            lawPapers[i].SetActive(true); // Activate the law papers
            TMP_Text[] tmpTexts = lawPapers[i].GetComponentsInChildren<TMP_Text>();
            for (int j = 0; j < tmpTexts.Length; j++)
            {
                if (j < lawsPerPaper && i * lawsPerPaper + j < activeLaws.Count)
                {
                    tmpTexts[j].text = activeLaws[i * lawsPerPaper + j].text; // Set the text of the law paper
                    if(activeLaws[i * lawsPerPaper + j].isProhibition) tmpTexts[j].color = Color.red;
                    else tmpTexts[j].color = Color.green;
                }
                else
                {
                    tmpTexts[j].gameObject.SetActive(false); // Deactivate the extra text fields
                }
            }
            
        }
        for(int i = papersActive; i < lawPapers.Length; i++)
        {
            lawPapers[i].SetActive(false); // Deactivate the extra law papers
        } 
        
    }
}