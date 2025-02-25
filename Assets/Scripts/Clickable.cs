using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clickable : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Sprite spriteDefault;
    public Sprite spriteHover;
    private Transform transform;
    public String sceneName;
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>(); 
        transform = GetComponent<Transform>();
    }

    private void OnMouseEnter()
    {
        transform.localScale = new Vector3(1.1f, 1.1f, 1.0f);
        spriteRenderer.color = Color.HSVToRGB(0,0,60);
        spriteRenderer.sprite = spriteHover;
    }
    private void OnMouseExit()
    {
        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        spriteRenderer.color = Color.HSVToRGB(0,0,100);
        spriteRenderer.sprite = spriteDefault;
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
