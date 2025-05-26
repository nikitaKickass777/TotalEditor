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
    public int startOfTheDayMoney = GameManager.instance.money;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        canvas.SetActive(false);
        isOpen = false;
    }

    public void ShowEndOfTheDay()
    {
        canvas.SetActive(true);
        EndTheDay();
        isOpen = true;
        Time.timeScale = 0f;
    }
    public void EndTheDay()
    {
        int moneySpent = 35;
        int moneyEarned = GameManager.instance.money - startOfTheDayMoney;
        switch (GameManager.instance.day)
        {
            case 1:
                titleTMP.text = "Some time has passed \n";
                bodyTMP.text = "You have completed the first day. \n" +
                                "It was tiring, but satisfying. \n" +
                                "You have earned +" + moneyEarned + " money.\n"
                                + "Expenditures: \n" +
                                "You have paid for food, rent, and other expenses. -15$\n" +
                                "You have paid for your kid's present. -5$\n" +
                                "Bought new pair of shoes. -10$\n" +
                                "Bought medicine for mother. -5$\n" +
                                "You have spent " + moneySpent + " money.\n" +
                                "You have " + (GameManager.instance.money - moneySpent) + " money left.\n";
                break;
            case 2:
                titleTMP.text = "End of Day 2 \n";
                bodyTMP.text = "You have survived the second day. \n" +
                                "Keep up the good work!";
                break;
            case 3:
                titleTMP.text = "End of Day 3";
                bodyTMP.text = "You have made it to the third day. \n" +
                                "Things are getting tougher!";
                break;
            default:
                titleTMP.text = "End of Day " + GameManager.instance.day;
                bodyTMP.text = "You have reached a new milestone!";
                break;
        }
        GameManager.instance.money-= moneySpent;
        startOfTheDayMoney = GameManager.instance.money;
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