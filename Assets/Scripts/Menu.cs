using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
//singleton dont destroy on load key pressed listener
public class Menu : MonoBehaviour

{
    //singleton
    public static Menu instance;
    private string previousScene; 
    private void Start()
    {
        previousScene = SceneManager.GetActiveScene().name;
    }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(this);
    }
    private void Update()
    {
        
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            AudioManager.instance.PlayClip(AudioManager.instance.buttonClick);
            SceneManager.LoadScene("MainMenu");
        }
        else if(Input.GetKeyDown(KeyCode.Backspace))
        {
            AudioManager.instance.PlayClip(AudioManager.instance.buttonClick);
            string prev = previousScene;
            
            previousScene = SceneManager.GetActiveScene().name;
            Debug.Log("Previous Scene recorded: " + previousScene);
            SceneManager.LoadScene(prev);
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.G) || Input.GetKeyDown(KeyCode.H) || Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.L) || Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.N) || Input.GetKeyDown(KeyCode.O) || Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.T) || Input.GetKeyDown(KeyCode.U) || Input.GetKeyDown(KeyCode.V) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Y) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Backspace))
        {
            AudioManager.instance.PlayRandomTypingSound();
        }
    }
}
