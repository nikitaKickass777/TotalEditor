using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PersistenceManager : MonoBehaviour
{
    public static PersistenceManager instance;
    private string saveFolderPath;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);

            // Use persistentDataPath for saving
            saveFolderPath = Path.Combine(Application.persistentDataPath, "Saves");
            if (!Directory.Exists(saveFolderPath))
            {
                Directory.CreateDirectory(saveFolderPath);
            }
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void SaveData()
    {
        // Save simple config (money, day, volumes)
        SaveToFile("config_save.json", new ConfigData()
        {
            money = GameManager.instance.money,
            day = GameManager.instance.day,
            soundVolume = AudioManager.instance.soundVolume,
            musicVolume = AudioManager.instance.musicVolume
        });

        // Save dialogue list
        SaveToFile("dialogue_save.json", DialogueManager.instance.dialogueList);
        // Save law list
        SaveToFile("law_save.json", GameManager.instance.lawList);

        // Save journalist list
        SaveToFile("journalist_save.json", GameManager.instance.journalistList);

        // Save article list
        SaveToFile("article_save.json", GameManager.instance.articleList);
        
        // Save unedited articles
        ArticleList uneditedArticles = new ArticleList();
        uneditedArticles.articles = GameManager.instance.uneditedArticles;
        Debug.Log($"Saving {uneditedArticles.articles.Count} unedited articles");
        SaveToFile("uneditedArticles_save.json", uneditedArticles);
        
        // convert dictionary to SerializableDictionary
        var serializableDictDialogue = new SerializableDictionary<int, bool>();
        serializableDictDialogue.FromDictionary(DialogueManager.instance.dialogueCompleted);
        SaveToFile("dialogueCompleted_save.json", serializableDictDialogue);
        
        // convert dictionary to SerializableDictionary
        var serializableDictChoices = new SerializableDictionary<string, bool>();
        serializableDictChoices.FromDictionary(DialogueManager.instance.choicesDictionary);
        SaveToFile("choicesDictionary_save.json", serializableDictChoices);

        Debug.Log($"Game data saved to {saveFolderPath}");
    }

    public void LoadData()
    {
        // Load config
        ConfigData config = LoadFromFile<ConfigData>("config_save.json");
        if (config != null)
        {
            GameManager.instance.money = config.money;
            GameManager.instance.day = config.day;
            AudioManager.instance.soundVolume = config.soundVolume;
            AudioManager.instance.musicVolume = config.musicVolume;
        }

        // Load dialogue list
        var dialogueList = LoadFromFile<DialogueList>("dialogue_save.json");
        if(dialogueList != null) DialogueManager.instance.dialogueList = dialogueList;
        // Load law list
        var lawList = LoadFromFile<LawList>("law_save.json");
        if (lawList != null) GameManager.instance.lawList = lawList;

        // Load journalist list
        var journalistList = LoadFromFile<JournalistList>("journalist_save.json");
        if (journalistList != null) GameManager.instance.journalistList = journalistList;

        // Load article list
        var articleList = LoadFromFile<ArticleList>("article_save.json");
        if (articleList != null) GameManager.instance.articleList = articleList;
        
        // Load unedited articles
        var uneditedArticles = LoadFromFile<ArticleList>("uneditedArticles_save.json");
        if (uneditedArticles != null) GameManager.instance.uneditedArticles = uneditedArticles.articles;

        // Load choices dictionary
        var choicesDict = LoadFromFile<SerializableDictionary<string, bool>>("choicesDictionary_save.json");
        if (choicesDict != null) DialogueManager.instance.choicesDictionary = choicesDict.ToDictionary();
        
        // Load dialogue completed dictionary
        var dialogueCompleted = LoadFromFile<SerializableDictionary<int, bool>>("dialogueCompleted_save.json");
        if (dialogueCompleted != null) DialogueManager.instance.dialogueCompleted = dialogueCompleted.ToDictionary();

        Debug.Log($"Game data loaded from {saveFolderPath}");
    }

    // Helper to save any object
    private void SaveToFile(string fileName, object data)
    {
        // Manually trigger OnBeforeSerialize if data is ISerializationCallbackReceiver
        if (data is ISerializationCallbackReceiver serializable)
        {
            Debug.Log($"Saving {fileName}");
            serializable.OnBeforeSerialize();
        }

        string json = JsonUtility.ToJson(data, true); // pretty print
        string filePath = Path.Combine(saveFolderPath, fileName);
        File.WriteAllText(filePath, json);
    }

    // Helper to load any object
    private T LoadFromFile<T>(string fileName) where T : class
    {
        string filePath = Path.Combine(saveFolderPath, fileName);
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            return JsonUtility.FromJson<T>(json);
        }
        return null;
    }

    [System.Serializable]
    public class ConfigData
    {
        public int money;
        public int day;
        public int soundVolume;
        public int musicVolume;
    }
    [System.Serializable]
    public class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver
    {
        public List<TKey> keys = new List<TKey>();
        public List<TValue> values = new List<TValue>();
        public Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();
            foreach (var kvp in dictionary)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            dictionary = new Dictionary<TKey, TValue>();
            for (int i = 0; i < keys.Count; i++)
            {
                dictionary[keys[i]] = values[i];
            }
        }
        public void FromDictionary(Dictionary<TKey, TValue> source)
        {
            dictionary = new Dictionary<TKey, TValue>(source);
        }

        public Dictionary<TKey, TValue> ToDictionary() => dictionary;
    }

}
