using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public GameObject menu;
    public static bool isPaused = false;
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.P) && !DialogueManager.instance.isDialogueOpen)
        {
            if(menu.activeSelf)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }
    void Start()
    {
        Time.timeScale = 1f;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
    }
    
    
    
    
    
    public void PauseGame()
    {
        // Activates the menu and pauses the game
        
        Time.timeScale = 0f;
        menu.SetActive(!menu.activeSelf);
        isPaused = true;
    }
    
    public void ResumeGame()
    {
        // deactivates the menu and resumes the game
        AudioManager.instance.PlayClip(AudioManager.instance.buttonClick);
        menu.SetActive(!menu.activeSelf);
        Time.timeScale = 1f;
        isPaused = false;
    }
    

}
