using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
//singleton dont destroy on load key pressed listener
public class SceneNavigator : MonoBehaviour

{
    //singleton
    public static SceneNavigator instance;
    private string currentSceneName;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            currentSceneName = SceneManager.GetActiveScene().name;
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(this);
    }
    private void Update()
    {
        
        if(Input.GetKeyDown(KeyCode.Backspace))
        {
            currentSceneName = SceneManager.GetActiveScene().name;
            AudioManager.instance.PlayClip(AudioManager.instance.buttonClick);
            Debug.Log("Scene was: " + currentSceneName);
            currentSceneName = getPreviousScene();
            Debug.Log("New scene: " + currentSceneName);
            SceneManager.LoadScene(currentSceneName);
        }
    }
    private string getPreviousScene()
    {
        switch (currentSceneName)
        {
            case "MainMenu":
                return "MainMenu"; ;
            case "Office":
                return "MainMenu";
            case "Reprimands":
                return "Office";
            case "Editing":
                return "Office";
            case "DialogueSystem":
                return "Office";
            case "Board":
                return "Office";
            default:
                Debug.Log("Current Scene not found");
                return "MainMenu";
        }
    }
}
