using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        
        AudioManager.instance.PlayClip(AudioManager.instance.buttonClick);
        UnityEngine.SceneManagement.SceneManager.LoadScene("Office");
    }
    public void Settings()
    {
        
        AudioManager.instance.PlayClip(AudioManager.instance.buttonClick);
        UnityEngine.SceneManagement.SceneManager.LoadScene("Settings");
    }

    public void QuitGame()
    {
        // Quit the game
        AudioManager.instance.PlayClip(AudioManager.instance.buttonClick);
        Application.Quit();
    }
}
