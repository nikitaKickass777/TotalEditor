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
    int expenses = 0;
    int baseCost = 0;
    int motherCost = 0;
    string flavorText = "";
    string loanText = "";
    string title = "";
    int day = GameManager.instance.day;

    switch (day)
    {
        case 1:
            title = "Some time has passed";
            flavorText = "You have completed the first day.\nIt was tiring, but you managed to get into the workflow.";
            baseCost = 15;
            motherCost = 5;
            break;
        case 2:
            title = "End of Day 2";
            flavorText = "You have worked some more.\nCant stop thinking about Tetianas situation...";
            baseCost = 20;
            motherCost = 10;
            break;
        case 3:
            title = "End of Day 3";
            flavorText = "This day was a complete shock, poor Margaret.\nTomorrow you will have to make a hard decision.";
            baseCost = 25;
            motherCost = 10;
            break;
        default:
            title = $"End of Day {day}";
            flavorText = "You have reached a new milestone!";
            baseCost = 25;
            motherCost = 10;
            break;
    }

    expenses = baseCost + motherCost;
    int finalMoney = GameManager.instance.money - expenses;

    titleTMP.text = title;
    bodyTMP.text =
        $"{flavorText}\n\n" +
        $"<b>Financial Summary:</b>\n" +
        $"Start of the day: <color=#2b8a31>{startOfTheDayMoney}$</color>\n" +
        $"Earned today: <color=#2b8a31>{moneyEarned}$</color>\n" +
        $"<color=#a83232>Expenditures:</color>\n" +
        $" - Food, rent, and utilities: <color=#a83232>-{baseCost}$</color>\n" +
        $" - Medicine for your sick mother: <color=#a83232>-{motherCost}$</color>\n";

    if (finalMoney < 0)
    {
        loanText = "You didn't have enough money.\nYou borrowed the rest from your friend.\nYou will have to pay it back later.\n";
        GameManager.instance.money = 0;
    }
    else
    {
        GameManager.instance.money = finalMoney;
    }

    bodyTMP.text += $"\n{loanText}" +
                    $"Money left: <color=#2b8a31>{GameManager.instance.money}$</color>\n";

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