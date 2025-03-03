using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class Clickable : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Sprite spriteDefault;
    public Sprite spriteHover;
    
    public String sceneName;
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>(); 
    }

    private void OnMouseEnter()
    {
        Debug.Log("Mouse Enter");
        transform.localScale = new Vector3(1.1f, 1.1f, 1.0f);
        spriteRenderer.color = Color.HSVToRGB(0,0,60);
        spriteRenderer.sprite = spriteHover;
        spriteRenderer.sortingOrder = 1;
    }
    private void OnMouseExit()
    {
        Debug.Log("Mouse Exit");
        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        spriteRenderer.color = Color.HSVToRGB(0,0,100);
        spriteRenderer.sprite = spriteDefault;
        spriteRenderer.sortingOrder = 1;
    }
    private void OnMouseDown()
    {
    }

    private void OnMouseUp()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
