using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EndOfDayScreen : MonoBehaviour
{
    public static EndOfDayScreen instance;
    public GameObject canvas;
    public TextMeshProUGUI titleTMP;
    public TextMeshProUGUI bodyTMP;
    public bool isOpen;
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
    private void Start()
    {
        
        canvas = GameObject.Find("EndOfTheDayCanvas");
        canvas.SetActive(false);
        isOpen = false;
    }
    public void ShowEndOfTheDay()
    {
        canvas.SetActive(true);
        bodyTMP.text = "Day " + GameManager.instance.day + " is over! \n" +
                       "You have " + GameManager.instance.money + " money.\n" +
                       "You have " + GameManager.instance.uneditedArticles.Count + " unedited articles.";
        isOpen = true;
        Time.timeScale = 0f;
    }
    public void HideEndOfTheDay()
    {
        StartCoroutine(HideEndOfTheDayCoroutine());
    }
    public IEnumerator HideEndOfTheDayCoroutine()
    {
        yield return new WaitForSecondsRealtime(0.2f);
        Time.timeScale = 1f;
        GameManager.instance.day++;
        GameManager.instance.time = 0f;
        Clock clockInstance = FindObjectOfType<Clock>();
        if (clockInstance != null)
        {
            clockInstance.ResetClock();
        }
        else
        {
            Debug.LogError("Clock instance not found!");
        }
        PersistenceManager.instance.SaveData();
        canvas.SetActive(false);
        isOpen = false;
    }
    
}