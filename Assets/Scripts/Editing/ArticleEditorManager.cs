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
    int partsMatchedCount = 0;
    int moneyReward = 0; 
    string notificationText = "";
    // Evaluate selected pairs
    for (int i = 0; i < selectedText.Count; i++)
    {
        bool foundMatch = false;
        for (int j = 0; j < article.importantParts.Length; j++)
        {
            Article.ImportantPart part = article.importantParts[j];
            if (part.text.Trim() == selectedText[i])
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
    if (hasRejectReason && isRejected)
    {
        decisionCorrect = true;
        Debug.Log("Player made a correct decision based on at least one valid reason.");
        notificationText = "CORRECTLY REJECTED \nArticle violates prohibition  \n<color=#2b8a31> + 10$ </color>";
    }
    else if (hasAcceptReason && !isRejected)
    {
        decisionCorrect = true;
        Debug.Log("Player made a correct decision based on at least one valid reason.");
        notificationText = "CORRECTLY ACCEPTED \nArticle fulfills recommendation  \n<color=#2b8a31> + 10$ </color>";
    }
    
    // 2️⃣ Or if no rejection reasons existed, and player accepted
    else if (!hasImportantRejection && isRejected == false)
    {
        decisionCorrect = true;
        Debug.Log("Player correctly accepted (no prohibitions were applicable).");
        notificationText = "CORRECTLY ACCEPTED \nNo prohibitions applicable  \n<color=#2b8a31> + 10$ </color>";
        
    }

    if (decisionCorrect)
    {
        moneyReward += 10;
    }
    else
    {
        Debug.Log("Player made an incorrect decision based on the selected text and laws.");
        // If player made an incorrect decision, they lose money
        if (hasImportantRejection && isRejected == false)
        {
            notificationText = "INCORRECTLY ACCEPTED \nArticle violates prohibition  \n<color=#8a2b2b> - 10$ </color>";
            
        }
        else if (hasImportantRecommendation && isRejected)
        {
            notificationText = "INCORRECTLY REJECTED \nArticle doesnt violate any laws  \n<color=#8a2b2b> - 10$ </color>";
           
        }
        else if (!hasImportantRejection && isRejected)
        {
            notificationText = "INCORRECTLY REJECTED \nNo prohibitions applicable  \n<color=#8a2b2b> - 10$ </color>";
            
        }
        else if (hasImportantRejection && isRejected)
        {
            notificationText = "REJECTION WITHOUT REASON \nNo adequate rejection reason provided  \n<color=#8a2b2b> - 10$ </color>";
        }
        else
        {
            notificationText = "INCORRECT DECISION \nNo valid reasons found  \n<color=#8a2b2b> - 10$ </color>";
        }
        moneyReward -= 10;
    }

    foreach (bool match in importantPartMatched)
    {
        if(match) partsMatchedCount++;
        
    }
    if (partsMatchedCount > 1)
    {
        if(decisionCorrect) notificationText+="\n BONUS for "+ partsMatchedCount + " correctly assigned laws + <color=#2b8a31>"+ (partsMatchedCount-1)*5 + "$ <color=#8a2b2b>";
       moneyReward += (partsMatchedCount - 1) * 5; // bonus for multiple correct matches
    }
    //Set edited and isApproved
    GameManager.instance.articleList.articles[article.id].isApproved = !isRejected;
    GameManager.instance.articleList.articles[article.id].isEdited = true;
    GameManager.instance.money += moneyReward;
    NotificationManager.instance.AddToQueue(notificationText);
    SelectNextArticle();
}

    private void OnDestroy()
    {
        
        
        EditingField.OnArticleSubmitted -= HandleArticleSubmitted;
    }
}