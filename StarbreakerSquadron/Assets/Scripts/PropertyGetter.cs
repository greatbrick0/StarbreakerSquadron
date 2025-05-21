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

    public Dictionary<string, Dictionary<string, float>> adjustMultipliers { get; private set; } = new Dictionary<string, Dictionary<string, float>>();
    public bool adjustMultsPrepped { get; private set; } = false;

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
        //StartCoroutine(GetValue((val) => Debug.Log(Mathf.RoundToInt(val)), "HealthMult", "red"));
    }

    public IEnumerator GetValue(StatHandover callback, string property, string colour, float backup = 100.0f)
    {
        yield return new WaitUntil(() => adjustMultsPrepped && true);
        float output = MultiplyProperty(backup, property, colour);
        callback(output);
    }

    public float MultiplyProperty(float value, string property, string colour)
    {
        return (value * adjustMultipliers[property][colour]);
    }

    private void SetValues()
    {
        adjustMultsPrepped = false;

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
}
