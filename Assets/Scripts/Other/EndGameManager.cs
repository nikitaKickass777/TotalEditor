using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using UnityEngine.UI;

public class EndGameManager : MonoBehaviour
{
    public static EndGameManager instance;
    public bool isGameEnded = false;

    public GameObject backgroundSpriteObj;

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

    public void EndGame(string ending = "")
    {
        isGameEnded = true;
        Debug.Log(ending);
        backgroundSpriteObj.SetActive(true);
        SpriteRenderer spriteRenderer = backgroundSpriteObj.GetComponent<SpriteRenderer>();
        GameObject portrait = GameObject.Find("Portrait");
        if (portrait != null)
        {
            portrait.transform.localScale = new Vector3(1f, 1f, 1f);
            portrait.transform.position = portrait.transform.position + new Vector3(0, 0, 0);
            
            
        }
        
        GameObject.Find("laptop").SetActive(false);
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component not found on backgroundSpriteObj.");
            return;
        }

        switch (ending)
        {
            case "prison":
                spriteRenderer.sprite = Resources.Load<Sprite>("Endings/prison");
                spriteRenderer.size = new Vector2(spriteRenderer.size.x * 1.2f, spriteRenderer.size.y * 1.2f);
                break;

            case "street":
                spriteRenderer.sprite = Resources.Load<Sprite>("Endings/street");
                break;

            case "cafe":
                spriteRenderer.sprite = Resources.Load<Sprite>("Endings/cafe");
                break;

            default:
                backgroundSpriteObj.SetActive(false);
                break;
        }
    }
}