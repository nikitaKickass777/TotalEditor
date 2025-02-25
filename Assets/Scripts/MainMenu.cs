using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.G) || Input.GetKeyDown(KeyCode.H) || Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.L) || Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.N) || Input.GetKeyDown(KeyCode.O) || Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.T) || Input.GetKeyDown(KeyCode.U) || Input.GetKeyDown(KeyCode.V) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Y) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Backspace))
        {
            AudioManager.instance.PlayRandomTypingSound();
        }
    }

    public void PlayGame()
    {
        // Load the office scene
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
