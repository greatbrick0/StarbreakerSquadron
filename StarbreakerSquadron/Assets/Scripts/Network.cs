using UnityEngine;

public class Network : MonoBehaviour
{
    public static Network sharedInstance;
    private BrainCloudWrapper _wrapper;

    public bool IsDedicatedServer;

    void Awake()
    {
        IsDedicatedServer = false;

        sharedInstance = this;
        DontDestroyOnLoad(gameObject);

        _wrapper = gameObject.AddComponent<BrainCloudWrapper>();
        _wrapper.Init();

        Debug.Log("brainCloud client version: " + _wrapper.Client.BrainCloudClientVersion);
    }

    void Update()
    {
        _wrapper.RunCallbacks();
    }

    public string BrainColudClientVersion
    {
        get { return _wrapper.Client.BrainCloudClientVersion; }
    }

    public bool IsAuthenticated()
    {
        return false;
    }
}
