using BrainCloud.JsonFx.Json;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatMessageManager : MonoBehaviour
{
    [SerializeField]
    private GameObject messageObj;
    private List<GameObject> messageRefs = new List<GameObject>();
    [SerializeField]
    private Transform messageParent;
    [SerializeField]
    private float messageSpacing = 10;
    [SerializeField]
    private TMP_InputField chatInputField;
    [SerializeField]
    private int maxChatMessageCharacters = 128;

    private void OnDestroy()
    {
        Network.sharedInstance.shareLobbyData -= ReceiveEvent;
    }

    public void AttachCallback()
    {
        Network.sharedInstance.shareLobbyData += ReceiveEvent;
    }

    private void ReceiveEvent(string eventJson)
    {
        Dictionary<string, object> response = JsonReader.Deserialize<Dictionary<string, object>>(eventJson);
        Dictionary<string, object> data = response["data"] as Dictionary<string, object>;

        if(response["operation"] as string == "SIGNAL")
        {
            Dictionary<string, object> signalData = data["signalData"] as Dictionary<string, object>;
            Dictionary<string, object> from = data["from"] as Dictionary<string, object>;
            string message = signalData["message"] as string;
            string username = "";
            if (from.Count != 0)
            {
                username = from["name"] as string;
            }

            CreateChatMessage(username, message);
        }
    }

    private void CreateChatMessage(string user, string message)
    {
        messageRefs.Add(Instantiate(messageObj, messageParent));
        messageRefs[^1].GetComponent<ChatMessage>().SetValues(user, message);
        float additionalHeight = messageRefs[^1].GetComponent<ChatMessage>().GetPreferredHeight();
        additionalHeight += messageSpacing;
        for (int ii = 0; ii < messageParent.childCount; ii++)
        {
            RectTransform rect = messageParent.GetChild(ii).GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y + additionalHeight);
        }
    }

    public void AttemptSendMessage(string messageText)
    {
        chatInputField.text = string.Empty;
        Network.sharedInstance.StartClientSendLobbySignal(messageText);
    }

    public void TuneMessage(string messageText) 
    {
        if(chatInputField.text.Length >= maxChatMessageCharacters) chatInputField.text = chatInputField.text.Substring(0, maxChatMessageCharacters);
    }
}
