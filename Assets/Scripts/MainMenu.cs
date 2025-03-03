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
        // For now just load the dialogue system scene, but in the future this will be the settings scene
        AudioManager.instance.PlayClip(AudioManager.instance.buttonClick);
        UnityEngine.SceneManagement.SceneManager.LoadScene("DialogueSystem");
    }

    public void QuitGame()
    {
        // Quit the game
        AudioManager.instance.PlayClip(AudioManager.instance.buttonClick);
        Application.Quit();
    }
}
