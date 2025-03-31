using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clock : MonoBehaviour
{
   public Transform clockHandTransform;
   private float time;
   public float speed = 10f;

   private void Awake()
   {
       
   }

   

    // Update is called once per frame
    void Update()
    {
        time = GameManager.instance.time;
        clockHandTransform.eulerAngles = new Vector3(0, 0, -time * speed); //  speed is the speed of the clock hand in degrees per second
    }                                                                          // - is for  the clockwise rotation
}
