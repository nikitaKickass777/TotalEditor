using System;
using System.Collections.Generic;

[Serializable]
public class Article
{
    public int id;
    public string title;
    public string text;
    public int journalistId;
    public Journalist author;
    public ImportantPart[] importantParts; // array of important parts in the article
    public bool isEdited = false; // if the article was edited by the player
    public bool isApproved = false; // if the article was approved
    
    public Article(string title, string text, Journalist author)
    {
        this.title = title;
        this.text = text;
        this.author = author;
    }
    
    
    [Serializable]
    public class ImportantPart
    {
        public string text; // text of the important part
        public int tolerance; // tolerance for if the player selects a bit more or less of the text.
        public int[] lawIds; // index array of the applicable relevant laws
        public ImportantPart(string text, int tolerance, int[] lawIds)
        {
            
            this.text = text;
            this.tolerance = tolerance;
            this.lawIds = lawIds;
        }
    }
}

[Serializable]
public class ArticleList
{
    public List<Article> articles;
    public ArticleList()
    {
        articles = new List<Article>();
    }
}