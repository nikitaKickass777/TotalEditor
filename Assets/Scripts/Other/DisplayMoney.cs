using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DisplayMoney : MonoBehaviour
{
    public TextMeshProUGUI moneyText;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!EndGameManager.instance.isGameEnded) moneyText.text = "Money: " + GameManager.instance.money + " $";
    }
}