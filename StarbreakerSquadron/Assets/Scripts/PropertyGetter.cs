using System;
using System.Collections.Generic;
using UnityEngine;

public class PropertyGetter : MonoBehaviour
{

    private Network _bcNetwork;
    private bool isDedicatedServer;

    public Dictionary<string, Dictionary<string, float>> teamMultipliers = new Dictionary<string, Dictionary<string, float>>();

    void Start()
    {
        _bcNetwork = Network.sharedInstance;
        isDedicatedServer = _bcNetwork.IsDedicatedServer;

        if (isDedicatedServer)
        {
            _bcNetwork._bcS2S.Request(FormatPropertyRequest(new string[] { "adjustmult" }), HandleMultipliers);
        }
        else
        {
            _bcNetwork._wrapper.GlobalAppService.ReadPropertiesInCategories(new string[] { "adjustmult" }, HandleMultipliers, null);
        }
    }

    private Dictionary<string, object> FormatPropertyRequest(string[] categories)
    {
        Dictionary<string, object> request = new Dictionary<string, object>
            {
                { "service", "globalApp" },
                { "operation", "READ_PROPERTIES_IN_CATEGORIES" },
                { "data", new Dictionary<string, object>
                {
                    { "categories", categories }
                }}
            };
        return request;
    }

    private void HandleMultipliers(string jsonResponse, object cbObject)
    {
        HandleMultipliers(jsonResponse);
    }

    private void HandleMultipliers(string jsonResponse)
    {
        Debug.Log(jsonResponse);
    }
}
