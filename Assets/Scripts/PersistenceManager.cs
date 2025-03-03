using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistenceManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    public void SaveData()
    {
        PlayerPrefs.SetInt("volume", 100);
        PlayerPrefs.SetString("playerName", "Player 1");
        PlayerPrefs.SetInt("day", 1);
        PlayerPrefs.SetInt("time", 8);
        PlayerPrefs.SetString("scene", "Office");
        
        PlayerPrefs.Save();
        // Save the data
    }
}
