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
    public int startOfTheDayMoney = 20;

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
        
        int moneyEarned = GameManager.instance.money - startOfTheDayMoney;
        switch (GameManager.instance.day)
        {
            case 1:
                titleTMP.text = "Some time has passed \n";
                bodyTMP.text = "You have completed the first day. \n" +
                               "It was tiring, but satisfying. \n" +
                               "You have earned <color=green> " + moneyEarned + "</color> money.\n"
                               + "Expenditures: \n" +
                               "You have paid for food, rent, and other expenses. -15$\n" +
                               "Bought medicine for mother. -5$\n";
                GameManager.instance.money -= 20;
                if(GameManager.instance.money < 0)
                {
                    bodyTMP.text += "You loaned missing money from your close friend. You will have to pay it back later.\n";
                    GameManager.instance.money = 0;
                }
                bodyTMP.text +=  "You have " + (GameManager.instance.money) + " money left.\n";
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