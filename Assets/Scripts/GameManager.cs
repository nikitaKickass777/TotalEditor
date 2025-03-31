using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
   //This class will handle remembering current state, handling in-game logic.
   public int money;
   public int day;
   public float time;
   public static GameManager instance;
   
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
       money = 20;
       day = 1;
       time = 0;
   }
   void Update()
   {
       time += Time.deltaTime;
   }
}

