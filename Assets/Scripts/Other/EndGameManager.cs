using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using UnityEngine.UI;

public class EndGameManager : MonoBehaviour
{
    public static EndGameManager instance;
    public bool isGameEnded = false;
    
    public GameObject imageObj ;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(this);
    }
    /**
     * Ends the game with a specific ending.
     * @param ending The ending to be applied.
     * IMPORTANT: Dasha. add more parameters and use them in switch statement to make the complex ending 
     */

    private void EndGame(int ending) 
    {
        imageObj.SetActive(true);
        Image imageComponent = imageObj.GetComponent<Image>();
        switch (ending)
        {
            case 1:
                imageComponent.sprite = Resources.Load<Sprite>("Endings/prison");
                break;
            
            case 2:
                imageComponent.sprite = Resources.Load<Sprite>("Endings/street");
                break;
            
            default:
                imageComponent.sprite = Resources.Load<Sprite>("Endings/cafe");
                break;
        }
    }
}