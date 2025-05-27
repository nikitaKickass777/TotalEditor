using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DisplayMoney : MonoBehaviour
{
    public TextMeshProUGUI moneyText;
    public static DisplayMoney instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // Update is called once per frame
    void Update()
    {
        if(!EndGameManager.instance.isGameEnded) moneyText.text = "Money: <color=green>" + GameManager.instance.money + " $ </color>";
    }
}