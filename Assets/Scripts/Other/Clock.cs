using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Clock : MonoBehaviour
{
    public Transform clockHandTransform;
    private float time;
    [SerializeField] private float speed = 1f;
    private float endTime = 9f;
    private bool hasShownEndOfDay = false;
    internal static object instance;

    void Update()
    {
        if (!hasShownEndOfDay)
        {
            time = GameManager.instance.time * speed;

            if (time < endTime)
            {
                float angle = 360f - (time % 12f) * 30f + 90f;
                clockHandTransform.eulerAngles = new Vector3(0, 0, angle);
            }
            else
            {
                // hasShownEndOfDay = true;
                time = 0f;
                clockHandTransform.eulerAngles = new Vector3(0, 0, 180f);
                // EndOfDayScreen.instance.ShowEndOfTheDay();
            }
        }
    }

    public void ResetClock()
    {
        time = 0f;
        hasShownEndOfDay = false;
    }
}
