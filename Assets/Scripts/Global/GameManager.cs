using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using TMPro;



public class GameManager : MonoBehaviour
{
   //This class will handle remembering current state, handling in-game logic.
   public int soundVolume = 100;
   public int musicVolume = 100;
   public int money;
   public int day;
   public float time;
   public float dayLength; // day length in seconds
   public LawList lawList; // array of laws
   public JournalistList journalistList; // array of journalists
   public ArticleList articleList; // array of articles
   public static GameManager instance;
   public List<Article> uneditedArticles = new List<Article>();
   public GameObject EndOfTheDayTemplate;
   public bool isEndOfTheDayOpen;
   public TextMeshProUGUI endOfDayText;
   

   
   
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
       if(SceneManager.GetActiveScene().name == "Office" || SceneManager.GetActiveScene().name == "Reprimands" || SceneManager.GetActiveScene().name == "Editing" || SceneManager.GetActiveScene().name == "Board")
       {
           gameLogic();
           time += Time.deltaTime;
       }
      
   }
    public void changeMoney(int amount)
    {
         money += amount;
    }
    private void gameLogic()
    {
        if(time >= dayLength)
        {
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
                uneditedArticles.Add(article);
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

    public void ShowEndOfTheDay()
    {
        isEndOfTheDayOpen = true;
        Time.timeScale = 0;
        // isDialogueOpen = true;
        EndOfTheDayTemplate.SetActive(true); // this thing makes dialogue box appear

        endOfDayText.text =
            "Results of the Day " + day + "\n" +
            "Earnings: $" + money + "\n";
            
    }

    public void HideEndOfTheDay()
    {
        print("End of the day closed");

        isEndOfTheDayOpen = false;
        EndOfTheDayTemplate.SetActive(false);
        Clock clockInstance = FindObjectOfType<Clock>();
        if (clockInstance != null)
        {
            clockInstance.ResetClock();
        }
        else
        {
            Debug.LogError("Clock instance not found!");
        }
        time = 0;
        day += 1;
        Time.timeScale = 1;
    }
}

