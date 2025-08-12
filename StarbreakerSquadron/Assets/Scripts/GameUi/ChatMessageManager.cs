using BrainCloud.JsonFx.Json;
using System.Collections.Generic;
using UnityEngine;

public class ChatMessageManager : MonoBehaviour
{
    [SerializeField]
    private GameObject messageObj;
    [SerializeField]
    private Transform messageParent;

    private void OnDestroy()
    {
        //Network.sharedInstance.shareLobbyData -= ReceiveEvent;
    }

    public void AttachCallback()
    {
        //Network.sharedInstance.shareLobbyData += ReceiveEvent;
    }

    private void ReceiveEvent(string eventJson)
    {
        Dictionary<string, object> response = JsonReader.Deserialize<Dictionary<string, object>>(eventJson);
        Dictionary<string, object> data = response["data"] as Dictionary<string, object>;

        if(response["operation"] as string == "SIGNAL")
        {
            Dictionary<string, object> signalData = data["signalData"] as Dictionary<string, object>;
            string message = signalData["message"] as string;
            Debug.Log("chat: " + message);
        }
    }
}
