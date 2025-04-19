using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
   //This class will handle remembering current state, handling in-game logic.
   public int money;
   public int day;
   public float time;
   public float dayLength; // day length in seconds
   public LawList lawList; // array of laws
   public JournalistList journalistList; // array of journalists
   public ArticleList articleList; // array of articles
   public static GameManager instance;
   
   
   //singleton pattern
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

   void Start()
   {
         // initialize the game state
       InitializeData();
       money = 20;
       day = 1;
       time = 0;
   }
   void Update()
   {
       time += Time.deltaTime;
   }
    public void changeMoney(int amount)
    {
         money += amount;
    }
    private void gameLogic()
    {
        if(time >= dayLength)
        {
            Debug.Log("Time is up!");
            // put the end of the day logic here
        }
    }

    private void InitializeData()
    {
        LoadJournalists();
        LoadLaws();
        LoadArticles();
    }
    
    void LoadJournalists()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("journalists");
        
        if (jsonFile != null)
        {
            journalistList = JsonUtility.FromJson<JournalistList>(jsonFile.text);
            Debug.Log(journalistList.journalists.Count + " journalists loaded.");
            LoadPortraits();
            Debug.Log("Journalists Loaded Successfully!");
        }
        else
        {
            Debug.LogError("Failed to load journalists.json");
        }
    }
    void LoadLaws()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("laws");
        
        if (jsonFile != null)
        {
            lawList = JsonUtility.FromJson<LawList>(jsonFile.text);
            Debug.Log("Laws Loaded Successfully!");
        }
        else
        {
            Debug.LogError("Failed to load laws.json");
        }
    }
    void LoadArticles()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("articles");
        
        if (jsonFile != null)
        {
            articleList = JsonUtility.FromJson<ArticleList>(jsonFile.text);
            foreach (var article in articleList.articles)
            {
                article.author = journalistList.journalists.Find(j => j.id == article.journalistId);
            }
            Debug.Log("Articles Loaded Successfully!");
        }
        else
        {
            Debug.LogError("Failed to load articles.json");
        }
        
    }
    
    void LoadPortraits()
    {
        foreach (var journalist in journalistList.journalists)
        {
            journalist.portrait = Resources.Load<Sprite>("Portraits/" + journalist.name);
            journalist.portraitTalking = Resources.Load<Sprite>("Portraits/" + journalist.name + "_talks");
            journalist.portraitDoor = Resources.Load<Sprite>("Portraits/" + journalist.name + "_door");
        }
    }
    
}

