using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//singleton dont destroy on load key pressed listener
public class SceneNavigator : MonoBehaviour

{
    //singleton
    public static SceneNavigator instance;
    public float timeLeftPreviousScene;
    public string previousSceneName;
    private string currentSceneName;
    public bool isNewGame = true;
    public GameObject ConfirmExitPopup;
    public bool isPopupOpen = false;

    public delegate void SceneChangeDelegate(string sceneName);

    public static event SceneChangeDelegate OnSceneChange;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            currentSceneName = SceneManager.GetActiveScene().name;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(this);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            currentSceneName = SceneManager.GetActiveScene().name;
            AudioManager.instance.PlayClip(AudioManager.instance.buttonClick);
            Debug.Log("Scene was: " + currentSceneName);
            previousSceneName = currentSceneName;
            timeLeftPreviousScene = Time.time;
            if (currentSceneName == getPreviousScene()) return;
            if (getPreviousScene() == "MainMenu" && currentSceneName == "Office")
            {
                isPopupOpen = true;
                ConfirmExitPopup.SetActive(true);
                return;
            }
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
                return "MainMenu";
                
            case "Office":
            {
                if (DialogueManager.instance.isDialogueOpen)
                {
                    // Some notifier class needed for all popups!
                    Debug.Log("Dialogue is open, cannot go back to office");
                    return "Office";
                }

                return "MainMenu";
            }

            case "Reprimands":
                return "Office";
            case "Editing":
            {
                OnSceneChange?.Invoke("Editing");
            }
                return "Office";
            case "DialogueSystem":
                return "Office";
            case "Board":
                return "Office";
            case "Settings":
                return "MainMenu";
            default:
                Debug.Log("Current Scene not found");
                return "MainMenu";
        }
    }
    public void LoadScene(string sceneName)
    {
        isPopupOpen = false;
        ConfirmExitPopup.SetActive(false);
        SceneManager.LoadScene(sceneName);
    }
}