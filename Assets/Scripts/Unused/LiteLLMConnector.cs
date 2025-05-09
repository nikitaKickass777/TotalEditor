using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Collections;

[System.Serializable]
public class ChatMessage
{
    public string role;
    public string content;
}

[System.Serializable]
public class ChatRequest
{
    public string model;
    public ChatMessage[] messages;
    public float temperature = 0.7f;
}

public class LiteLLMConnector : MonoBehaviour
{
    public string liteLLMEndpoint = "https://lite-llm.ai.datalab.tuwien.ac.at/ui/?userID=949fe25e-fa99-4706-a378-6d485b4e18a2";

    void Start()
    {
        // Example usage
        string systemPrompt = "You are a helpful assistant.";
        string userPrompt = "What is the capital of France?";
        StartCoroutine(SendChatRequest(systemPrompt, userPrompt, (response) =>
        {
            Debug.Log("Response: " + response);
        }));
    }
    
    public IEnumerator SendChatRequest(string systemPrompt, string userPrompt, System.Action<string> callback)
    {
        ChatRequest chatRequest = new ChatRequest
        {
            model = "gpt-3.5-turbo", // or whatever alias your server provides
            messages = new ChatMessage[]
            {
                new ChatMessage { role = "system", content = systemPrompt },
                new ChatMessage { role = "user", content = userPrompt }
            }
        };

        string json = JsonUtility.ToJson(chatRequest);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(liteLLMEndpoint, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            
            string responseJson = request.downloadHandler.text;
            Debug.Log("Response JSON: " + responseJson);
            callback?.Invoke(responseJson); // You'll need to parse it after
        }
        else
        {
            Debug.LogError("LiteLLM Error: " + request.error);
        }
    }
}