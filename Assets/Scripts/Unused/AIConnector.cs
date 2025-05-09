using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class AIConnector : MonoBehaviour
{
    private System.Action<string> callback;
    private void Start()
    {
        Debug.Log("AIConnector Start");
        // Initialize the callback to handle the response
        callback = (response) => Debug.Log("Response received: " + response);
        StartCoroutine(EvaluateArticle("dolphin-llama3", "hello how are you", callback));
    }

    public IEnumerator EvaluateArticle(string model, string promptJson, System.Action<string> callback)
    {
        Debug.Log("sending request " + promptJson);
        string url = "http://localhost:11434/api/generate";

        // Correctly format the JSON body
        string body = $"{{\"model\": \"{model}\", \"prompt\": \"{promptJson}\"}}";

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(body);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log(request.downloadHandler.text);
            callback?.Invoke(request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("AI Evaluation failed: " + request.error);
        }
    }
}