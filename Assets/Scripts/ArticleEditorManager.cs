using System;
using System.Collections.Generic;
using UnityEngine;


public class ArticleEditorManager : MonoBehaviour
{
    public int articleId = 0; // Current article ID
    public Article currentArticle;

    public delegate Article ArticleSelectedDelegate(Article article);

    public static event ArticleSelectedDelegate OnArticleSelected;

    private void Start()
    {
        //EditingField.OnTextSelected += HandleTextSelected;
        
        EditingField.OnArticleSubmitted += HandleArticleSubmitted;
        SelectNextArticle();
    }


    private Article SelectNextArticle()
    {
        Article selectedArticle = null;
        foreach (Article article in GameManager.instance.uneditedArticles)
        {
            if (ShouldBeEditedNext(article))
            {
                selectedArticle = article;
                break;
            }
        }

        GameManager.instance.uneditedArticles.Remove(selectedArticle);
        OnArticleSelected?.Invoke(selectedArticle);
        return selectedArticle;

        bool ShouldBeEditedNext(Article article)
        {
            // here is logic for article selection
            if(DialogueManager.instance.dialogueCompleted.ContainsKey(3))
            {
                return true;
            }
            return false;
        }
    }
    
    private void HandleArticleSubmitted(Article article, List<string> selectedText, List<int> selectedLaws,
        bool isRejected)
    {
        // Handle the article submission logic here
        Debug.Log($"Article submitted: {article.title}");
        Debug.Log($"Selected text: {string.Join(", ", selectedText)}");
        Debug.Log($"Selected laws: {string.Join(", ", selectedLaws)}");
        bool hasRejectReason = false;
        bool hasAcceptReason = false;
        bool [] isSelectedTextCorrect = new bool[selectedText.Count]; // array to store the correctness of each selected text
        for (int i = 0; i < selectedText.Count; i++)
        { 
            isSelectedTextCorrect[i] = false;
            foreach (Article.ImportantPart part in article.importantParts)
            {
                if (part.text == selectedText[i])
                {
                    foreach (int lawId in part.lawIds)
                    {
                        if (lawId == selectedLaws[i])
                        {
                            if (GameManager.instance.lawList.laws[lawId].isProhibition == isRejected)
                            {
                                Debug.Log("Correct!");
                                isSelectedTextCorrect[i] = true;
                                if(isRejected) 
                                    hasRejectReason = true;
                                else
                                    hasAcceptReason = true;
                                //horray! Correct choice! Everything is in order, reward player.
                            }
                        }
                    }
                }
            }
            if (!isSelectedTextCorrect[i])
            {
                Debug.Log("Incorrect!");
            }
        }

        currentArticle = SelectNextArticle();
    }
    private void OnDestroy()
    {
        //EditingField.OnTextSelected -= HandleTextSelected;
        
        EditingField.OnArticleSubmitted -= HandleArticleSubmitted;
    }
}