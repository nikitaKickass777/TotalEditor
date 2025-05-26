using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class ArticleEditorManager : MonoBehaviour
{

    public delegate Article ArticleSelectedDelegate(Article article);

    public static event ArticleSelectedDelegate OnArticleSelected;

    private void Start()
    {
        //EditingField.OnTextSelected += HandleTextSelected;
        
        EditingField.OnArticleSubmitted += HandleArticleSubmitted;
    }


    public static Article SelectNextArticle()
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
        if (selectedArticle == null)
        {
            Debug.Log("No more articles to edit.");
            selectedArticle = new Article("", "", new Journalist());
        }

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
    
    private void HandleArticleSubmitted(Article article, List<MarkedSelection> markedSelections, bool isRejected)
{
    List<string> selectedText = markedSelections.Select(m => m.text).ToList();
    List<int> selectedLaws = markedSelections.Select(m => m.lawId).ToList();
    
    Debug.Log($"Article submitted: {article.title}");
    Debug.Log($"Selected text: {string.Join(", ", selectedText)}");
    Debug.Log($"Selected laws: {string.Join(", ", selectedLaws)}");

    bool hasRejectReason = false;
    bool hasAcceptReason = false;
    bool[] importantPartMatched = new bool[article.importantParts.Length]; // Track if each important part was matched

    // Evaluate selected pairs
    for (int i = 0; i < selectedText.Count; i++)
    {
        bool foundMatch = false;
        for (int j = 0; j < article.importantParts.Length; j++)
        {
            Article.ImportantPart part = article.importantParts[j];
            if (part.text == selectedText[i])
            {
                foreach (int lawId in part.lawIds)
                {
                    if (lawId == selectedLaws[i])
                    {
                        bool lawIsProhibition = GameManager.instance.lawList.laws[lawId].isProhibition;

                        // Check if player's rejection/acceptance aligns with law type
                        if (lawIsProhibition == isRejected)
                        {
                            Debug.Log($"Correct selection: '{selectedText[i]}' under law {lawId}");
                            importantPartMatched[j] = true; // mark this important part as found

                            if (lawIsProhibition)
                                hasRejectReason = true;
                            else
                                hasAcceptReason = true;

                            foundMatch = true;
                            GameManager.instance.money += 10; // base reward for finding a valid reason
                        }
                        else
                        {
                            Debug.Log($"Law {lawId} type mismatch for selected text '{selectedText[i]}'.");
                        }
                    }
                }
            }
        }

        if (!foundMatch)
        {
            Debug.Log($"Incorrect or irrelevant selection: '{selectedText[i]}'");
        }
    }

    // Evaluate overall correctness

    bool hasImportantRejection = false;
    bool hasImportantRecommendation = false;
    foreach (Article.ImportantPart part in article.importantParts)
    {
        foreach (int lawId in part.lawIds)
        {
            if (GameManager.instance.lawList.laws[lawId].isProhibition)
                hasImportantRejection = true;
            else
                hasImportantRecommendation = true;
        }
    }

    bool allImportantPartsFound = importantPartMatched.All(matched => matched);

    bool decisionCorrect = false;

    // 1️⃣ Decision correct if:
    // → player found at least one valid rejection reason AND rejected
    // → OR player found at least one valid acceptance reason AND accepted
    if ((hasRejectReason && isRejected) || (hasAcceptReason && !isRejected))
    {
        decisionCorrect = true;
        Debug.Log("Player made a correct decision based on at least one valid reason.");
        NotificationManager.instance.AddToQueue(
            "Correct Descision! + 20$");
    }
    // 2️⃣ Or if no rejection reasons existed, and player accepted
    else if (!hasImportantRejection && isRejected == false)
    {
        decisionCorrect = true;
        Debug.Log("Player correctly accepted (no prohibitions were applicable).");
        NotificationManager.instance.AddToQueue(
            "No prohibitions were applicable, you accepted correctly! + 10$");
        
    }

    if (decisionCorrect)
    {
        Debug.Log("✅ Player rewarded for correct decision.");
        GameManager.instance.money += 20; // base decision reward
    }
    else
    {
        NotificationManager.instance.AddToQueue(
            "Incorrect descision! ");
        Debug.Log("❌ Player penalized for incorrect decision.");
        // If player made an incorrect decision, they lose money
        if (hasImportantRejection && isRejected == false)
        {
            NotificationManager.instance.AddToQueue(
                "You should have rejected this article! -10$");
            Debug.Log("❌ Player should have rejected the article.");
        }
        else if (hasImportantRecommendation && isRejected)
        {
            NotificationManager.instance.AddToQueue(
                "You should have accepted this article! -10$");
            Debug.Log("❌ Player should have accepted the article.");
        }
        GameManager.instance.money -= 10; // optional penalty
    }

    // BONUS if player found ALL important parts when there was more than one
    if (allImportantPartsFound && article.importantParts.Length > 1)
    {
        NotificationManager.instance.AddToQueue(
            "Found all the applicable laws! Good work! + 20$");
        Debug.Log("🏆 Bonus! Player found ALL important parts.");
        NotificationManager.instance.AddToQueue("You found all the important parts! + 50$");
        GameManager.instance.money += 50; // bonus reward
    }
    //Set edited and isApproved
    GameManager.instance.articleList.articles[article.id].isApproved = !isRejected;
    GameManager.instance.articleList.articles[article.id].isEdited = true;
    SelectNextArticle();
}

    private void OnDestroy()
    {
        //EditingField.OnTextSelected -= HandleTextSelected;
        
        EditingField.OnArticleSubmitted -= HandleArticleSubmitted;
    }
}