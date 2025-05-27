using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class NotificationManager : MonoBehaviour
{
    public TMP_Text popupText;

    public GameObject window;
    [SerializeField]
    private Animator popupAnimator;
    [SerializeField]
    private Queue<NotificationData> popupQueue; 
    [SerializeField]
    private Coroutine queueChecker;

    public static NotificationManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            SceneNavigator.OnSceneChange += HandleSceneChange;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        popupAnimator = window.GetComponent<Animator>();
        window.SetActive(false);
        popupQueue = new Queue<NotificationData>();
    }

    public void AddToQueue(string text, float duration = 4.5f)
    {
        popupQueue.Enqueue(new NotificationData(text, duration));
        if (queueChecker == null)
            queueChecker = StartCoroutine(CheckQueue());
    }


    private void ShowPopup(string text)
    {
        Debug.Log(text);
        //parameter the same type as queue
        window.SetActive(true);

        popupText.text = text;
        popupAnimator.Play("NotificationAnimation");
    }

    private IEnumerator CheckQueue()
    {
        do
        {
            NotificationData data = popupQueue.Dequeue();
            ShowPopup(data.message);

            popupAnimator.speed = 1f;
            popupAnimator.Play("NotificationAnimation", 0, 0f);

            // Wait for the duration of the animation (fixed at 4.5s)
            float animationDuration = 4.5f;
            float idleTime = Mathf.Max(0, data.duration - animationDuration);

            // Wait during fixed animation
            yield return new WaitForSeconds(3.0f);
            // If player wants longer duration, we just wait here
            if (idleTime > 0)
            {
                popupAnimator.speed = 0.0f; // Pause the animation
                yield return new WaitForSeconds(idleTime);
                popupAnimator.speed = 1.0f; // Resume the animation
            }

            yield return new WaitForSeconds(1.5f);
        } while (popupQueue.Count > 0);

        window.SetActive(false);
        queueChecker = null;
    }

    private void HandleSceneChange(string sceneName)
    {
        Debug.Log("Destroyed notifications in queue: " + popupQueue.Count);
        popupAnimator.StopPlayback();
        popupQueue.Clear();
        window.SetActive(false);
    }
    private void OnDestroy()
    {
        if (queueChecker != null)
        {
            StopCoroutine(queueChecker);
            queueChecker = null;
        }
        SceneNavigator.OnSceneChange -= HandleSceneChange;
    }
    
    private struct NotificationData
    {
        public string message;
        public float duration;

        public NotificationData(string message, float duration)
        {
            this.message = message;
            this.duration = duration;
        }
    }
}