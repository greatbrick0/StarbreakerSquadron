using BrainCloud.JsonFx.Json;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ClientManager : MonoBehaviour
{
    public class ClientSummary
    {
        public string username;
        public string profileId;
        public string userPasscode;
        public Dictionary<string, object> extraData;
        public PlayerController controllerRef;
    }

    public static ClientManager instance;
    private NetworkManager _netManager;
    private BrainCloudS2S _bcS2S;

    private string _lobbyId;
    private List<ClientSummary> clients = new List<ClientSummary>();

    [SerializeField]
    private List<GameObject> playerShipObjs = new List<GameObject>();

    public void Initialize(bool isServer, string lobbyId)
    {
        if (instance != null || !isServer)
        {
            Destroy(this);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        _lobbyId = lobbyId;

        _bcS2S = Network.sharedInstance._bcS2S;
        _netManager = GetComponent<NetworkManager>();
        _netManager.OnClientConnectedCallback += OnClientJoined;
    }

    private void OnClientJoined(ulong id)
    {
        clients.Add(new ClientSummary());

        Dictionary<string, object> request = new Dictionary<string, object>
            {
                { "service", "lobby" },
                { "operation", "GET_LOBBY_DATA" },
                { "data", new Dictionary<string, object>
                {{ "lobbyId", _lobbyId }}
                }
            };
        _bcS2S.Request(request, OnLobbyDataMemberJoin); 
    }

    private void OnLobbyDataMemberJoin(string responseJson)
    {
        Dictionary<string, object> response = JsonReader.Deserialize<Dictionary<string, object>>(responseJson);
        var data = response["data"] as Dictionary<string, object>;
        var membersData = data["members"] as List<Dictionary<string, object>>;

        clients[^1].username = membersData[^1]["name"] as string;
        clients[^1].profileId = membersData[^1]["profileId"] as string;
        clients[^1].userPasscode = membersData[^1]["passcode"] as string;
        clients[^1].extraData = membersData[^1]["extra"] as Dictionary<string, object>;
    }

    public void IdentifyPlayer(PlayerController givenController, string givenPasscode)
    {
        int selectedShipIndex = 0;
        for (int ii = 0; ii < clients.Count; ii++)
        {
            if (clients[ii].userPasscode != givenPasscode) continue;

            clients[ii].controllerRef = givenController;
            try { selectedShipIndex = (int?)clients[ii].extraData["selectedShipIndex"] ?? 0; }
            catch { selectedShipIndex = 0; }
            
        }
        givenController.SpawnShip(playerShipObjs[selectedShipIndex]);
    }
}
