using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class NotificationManager : MonoBehaviour
{
    public TMP_Text popupText;

    public GameObject window;
    private Animator popupAnimator;

    private Queue<string> popupQueue; //make it different type for more detailed popups, you can add different types, titles, descriptions etc
    private Coroutine queueChecker;
    
    public static NotificationManager instance;
     void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    private void Start() {
        popupAnimator = window.GetComponent<Animator>();
        window.SetActive(false);
        popupQueue = new Queue<string>();
    }

    public void AddToQueue(string text) {//parameter the same type as queue
        popupQueue.Enqueue(text);
        if (queueChecker == null)
            queueChecker = StartCoroutine(CheckQueue());
    }

    private void ShowPopup(string text) { //parameter the same type as queue
        window.SetActive(true);
        if(SceneManager.GetActiveScene().name != "Editing") window.SetActive(false);

        popupText.text = text;
        popupAnimator.Play("NotificationAnimation");
    }

    private IEnumerator CheckQueue() {
        do {
            ShowPopup(popupQueue.Dequeue());
            do {
                if (SceneManager.GetActiveScene().name != "Editing")
                {
                    window.SetActive(false);
                    //popupAnimator.StopPlayback();
                }
                yield return null;
            } while (!popupAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Idle"));

        } while (popupQueue.Count > 0);
        window.SetActive(false);
        queueChecker = null;
    }

}
