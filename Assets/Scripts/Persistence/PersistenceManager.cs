using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class PersistenceManager : MonoBehaviour
{
    public static PersistenceManager instance;
    public const string SAVE_PATH = "saveData.json";
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    public void SaveData()
    {
        PlayerPrefs.SetInt("money", GameManager.instance.money);
        PlayerPrefs.SetInt("day", GameManager.instance.day);
        
        PlayerPrefs.SetInt("soundVolume", GameManager.instance.soundVolume);
        PlayerPrefs.SetInt("musicVolume", GameManager.instance.musicVolume);
        
        // Save the law list
        string lawListJson = JsonUtility.ToJson(GameManager.instance.lawList);
        PlayerPrefs.SetString("lawList", lawListJson);
        
        // Save the journalist list
        string journalistListJson = JsonUtility.ToJson(GameManager.instance.journalistList);
        PlayerPrefs.SetString("journalistList", journalistListJson);
       
        // Save the article list
        string articleListJson = JsonUtility.ToJson(GameManager.instance.articleList);
        PlayerPrefs.SetString("articleList", articleListJson);
        
        string choicesDictionaryJson = JsonUtility.ToJson(DialogueManager.instance.choicesDictionary);
        PlayerPrefs.SetString("choicesDictionary", choicesDictionaryJson);
        Debug.Log("Game data saved");
        PlayerPrefs.Save();
    }
    public void LoadData()
    {
        GameManager.instance.money = PlayerPrefs.GetInt("money", GameManager.instance.money);
        GameManager.instance.day = PlayerPrefs.GetInt("day", GameManager.instance.day);
        
        GameManager.instance.soundVolume = PlayerPrefs.GetInt("soundVolume", GameManager.instance.soundVolume);
        GameManager.instance.musicVolume = PlayerPrefs.GetInt("musicVolume", GameManager.instance.musicVolume);
        // Load the law list
        string lawListJson = PlayerPrefs.GetString("lawList", JsonUtility.ToJson(GameManager.instance.lawList));
        GameManager.instance.lawList = JsonUtility.FromJson<LawList>(lawListJson);
        // Load the journalist list
        string journalistListJson = PlayerPrefs.GetString("journalistList", JsonUtility.ToJson(GameManager.instance.journalistList));
        GameManager.instance.journalistList = JsonUtility.FromJson<JournalistList>(journalistListJson);
        // Load the article list
        string articleListJson = PlayerPrefs.GetString("articleList", JsonUtility.ToJson(GameManager.instance.articleList));
        GameManager.instance.articleList = JsonUtility.FromJson<ArticleList>(articleListJson);
        // Load the choices dictionary
        string choicesDictionaryJson = PlayerPrefs.GetString("choicesDictionary", JsonUtility.ToJson(DialogueManager.instance.choicesDictionary));
        DialogueManager.instance.choicesDictionary = JsonUtility.FromJson<Dictionary<string, bool>>(choicesDictionaryJson);
        Debug.Log("Game data loaded");
    }
    
    /*
        PlayerPrefs.SetFloat("margaretRelationship", GameManager.instance.journalistList.journalists[0].relationship);
        PlayerPrefs.SetFloat("levRelationship", GameManager.instance.journalistList.journalists[1].relationship);
        PlayerPrefs.SetFloat("tetianaRelationship", GameManager.instance.journalistList.journalists[2].relationship);
        PlayerPrefs.SetFloat("abrahamRelationship", GameManager.instance.journalistList.journalists[3].relationship);
        PlayerPrefs.SetInt("margaretReprimands", GameManager.instance.journalistList.journalists[0].reprimands);
        PlayerPrefs.SetInt("levReprimands", GameManager.instance.journalistList.journalists[1].reprimands);
        PlayerPrefs.SetInt("tetianaReprimands", GameManager.instance.journalistList.journalists[2].reprimands);
        PlayerPrefs.SetInt("abrahamReprimands", GameManager.instance.journalistList.journalists[3].reprimands);
     * 
     */
}
