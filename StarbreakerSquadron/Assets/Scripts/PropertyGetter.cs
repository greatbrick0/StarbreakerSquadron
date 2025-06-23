using System;
using System.Collections.Generic;
using UnityEngine;
using BrainCloud.JsonFx.Json;
using System.Collections;

public class PropertyGetter : MonoBehaviour
{
    public static PropertyGetter propertiesInstance;

    private Network _bcNetwork;
    private bool isDedicatedServer;

    private Dictionary<string, Dictionary<string, float>> adjustMultipliers = new Dictionary<string, Dictionary<string, float>>();
    private Dictionary<string, Dictionary<string, float>> baseStats = new Dictionary<string, Dictionary<string, float>>();
    public bool adjustMultsPrepped { get; private set; } = false;
    public bool baseStatsPrepped { get; private set; } = false;

    public delegate void StatHandover(float value);

    private void Awake()
    {
        propertiesInstance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _bcNetwork = Network.sharedInstance;
        isDedicatedServer = _bcNetwork.IsDedicatedServer;

        SetValues();
    }

    public IEnumerator GetValue(StatHandover callback, string category, string property, string colour)
    {
        yield return new WaitUntil(() => adjustMultsPrepped && baseStatsPrepped);
        float output = baseStats[category][property];
        output = MultiplyProperty(output, category + "Mult", colour);
        callback(output);
    }

    public float MultiplyProperty(float value, string category, string colour)
    {
        return (value * adjustMultipliers[category][colour]);
    }

    private void SetValues()
    {
        adjustMultsPrepped = false;
        baseStatsPrepped = false;

        if (isDedicatedServer)
        {
            _bcNetwork._bcS2S.Request(FormatPropertyRequest(new string[] { "adjustmult" }), HandleMultipliers);
            _bcNetwork._bcS2S.Request(FormatPropertyRequest(new string[] { "basestat" }), HandleBaseStats);
        }
        else
        {
            _bcNetwork._wrapper.GlobalAppService.ReadPropertiesInCategories(new string[] { "adjustmult" }, HandleMultipliers, null);
            _bcNetwork._wrapper.GlobalAppService.ReadPropertiesInCategories(new string[] { "basestat" }, HandleBaseStats, null);
        }
    }

    private Dictionary<string, object> FormatPropertyRequest(string[] categories)
    {
        Dictionary<string, object> request = new Dictionary<string, object>
            {
                { "service", "globalApp" },
                { "operation", "READ_PROPERTIES_IN_CATEGORIES" },
                { "data", new Dictionary<string, object>
                {{ "categories", categories }}
                }
            };
        return request;
    }

    private void HandleMultipliers(string jsonResponse, object cbObject)
    {
        HandleMultipliers(jsonResponse);
    }

    private void HandleMultipliers(string jsonResponse)
    {
        var response = JsonReader.Deserialize<Dictionary<string, object>>(jsonResponse);
        var data = response["data"] as Dictionary<string, object>;
        foreach (KeyValuePair<string, object> ii in data)
        {
            var group = ii.Value as Dictionary<string, object>;
            var properties = JsonReader.Deserialize<Dictionary<string, float>>(group["value"] as string);
            Dictionary<string, float> output = new Dictionary<string, float>();
            foreach (KeyValuePair<string, float> jj in properties)
            {
                output.Add(jj.Key, jj.Value);
            }
            adjustMultipliers.Add(ii.Key, output);
        }

        adjustMultsPrepped = true;
    }

    private void HandleBaseStats(string jsonResponse, object cbObject)
    {
        HandleBaseStats(jsonResponse);
    }

    private void HandleBaseStats(string jsonResponse)
    {
        var response = JsonReader.Deserialize<Dictionary<string, object>>(jsonResponse);
        var data = response["data"] as Dictionary<string, object>;
        foreach (KeyValuePair<string, object> ii in data)
        {
            var group = ii.Value as Dictionary<string, object>;
            var properties = JsonReader.Deserialize<Dictionary<string, float>>(group["value"] as string);
            Dictionary<string, float> output = new Dictionary<string, float>();
            foreach (KeyValuePair<string, float> jj in properties)
            {
                output.Add(jj.Key, jj.Value);
            }
            baseStats.Add(ii.Key, output);
        }

        baseStatsPrepped = true;
        if (isDedicatedServer) ClientManager.instance.ServerDebugMessage("Got properies");
    }
}
