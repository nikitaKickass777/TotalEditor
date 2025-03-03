using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public GameObject menu;
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
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
        // Try to find the object again after scene change
        menu = GameObject.Find("PauseMenu");

        if (menu == null)
        {
            Debug.LogWarning("PauseMenu not found in scene!");
        }
    }
    
    
    
    
    
    public void PauseGame()
    {
        // Activates the menu and pauses the game
        Time.timeScale = 0f;
        menu.SetActive(!menu.activeSelf);
        
    }
    
    public void ResumeGame()
    {
        // deactivates the menu and resumes the game
        AudioManager.instance.PlayClip(AudioManager.instance.buttonClick);
        menu.SetActive(!menu.activeSelf);
        Time.timeScale = 1f;
    }
    public void MainMenuReturn()
    {
        // Load the main menu scene
        AudioManager.instance.PlayClip(AudioManager.instance.buttonClick);
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void Settings()
    {
        // For now just load the dialogue system scene, but in the future this will be the settings scene
        AudioManager.instance.PlayClip(AudioManager.instance.buttonClick);
        UnityEngine.SceneManagement.SceneManager.LoadScene("DialogueSystem");
    }

}
