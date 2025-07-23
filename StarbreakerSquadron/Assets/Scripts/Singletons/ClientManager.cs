using BrainCloud.JsonFx.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
    private List<ulong> clientIds = new List<ulong>();
    private bool allPlayersAccountedFor = false;

    [SerializeField]
    private List<GameObject> playerShipObjs = new List<GameObject>();
    private List<Transform> spawnSpots = new List<Transform>();
    private int nextSpawnIndex = 0;

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
        _netManager.OnClientDisconnectCallback += OnClientLeave;
    }

    private void OnClientJoined(ulong id)
    {
        allPlayersAccountedFor = false;
        clients.Add(new ClientSummary());
        clientIds.Add(id);
        Dictionary<string, object> request = new Dictionary<string, object>
            {
                { "service", "lobby" },
                { "operation", "GET_LOBBY_DATA" },
                { "data", new Dictionary<string, object>
                {{ "lobbyId", _lobbyId }}
                }
            };
        ServerDebugMessage("Client " + id + " joined");
        //Network.StartRepeatAttemptServerRequest(request, OnLobbyDataMemberJoin, () => { return allPlayersAccountedFor; }, 3.0f);
        _bcS2S.Request(request, OnLobbyDataMemberJoin);
    }

    private void OnClientLeave(ulong id)
    {
        int removeIndex = clientIds.IndexOf(id);
        clientIds.RemoveAt(removeIndex);
        clients.RemoveAt(removeIndex);
    }

    private void OnLobbyDataMemberJoin(string responseJson)
    {
        ServerDebugMessage("Server tried to respond");

        if (allPlayersAccountedFor) return;

        ServerDebugMessage("Client joined and brainCloud responded");
        Dictionary<string, object> response = JsonReader.Deserialize<Dictionary<string, object>>(responseJson);
        var data = response["data"] as Dictionary<string, object>;
        Dictionary<string, object>[] membersData = data["members"] as Dictionary<string, object>[];
        Dictionary<string, object> newestMember = membersData[^1];

        clients[^1].username = newestMember["name"] as string;
        clients[^1].profileId = newestMember["profileId"] as string;
        clients[^1].userPasscode = newestMember["passcode"] as string;
        try { clients[^1].extraData = newestMember["extra"] as Dictionary<string, object>; }
        catch { ServerDebugMessage(clients[^1].username + " did not have extra data"); }

        allPlayersAccountedFor = true;
        ServerDebugMessage("All players accounted for");
    }

    public IEnumerator IdentifyPlayer(PlayerController givenController, string givenPasscode, int claimedShip)
    {
        ServerDebugMessage("Client attempted identification");
        yield return new WaitUntil(() => allPlayersAccountedFor || Application.isEditor);

        int selectedShipIndex = 0;
        for (int ii = 0; ii < clients.Count; ii++)
        {
            if (clients[ii].userPasscode != givenPasscode) continue;
            clients[ii].controllerRef = givenController; 
            try { selectedShipIndex = (int?)clients[ii].extraData["selectedShipIndex"] ?? 0; }
            catch { selectedShipIndex = 0; }
            
        }
        Transform spot = GetSpawnSpot();
        givenController.SpawnShip(playerShipObjs[Application.isEditor ? claimedShip : selectedShipIndex], spot);
        ServerDebugMessage("Spawned " + playerShipObjs[selectedShipIndex].name);
    }

    public void AddSpawnSpots(List<Transform> newSpots)
    {
        spawnSpots.AddRange(newSpots);
    }

    public void AddSpawnSpot(Transform newSpot)
    {
        spawnSpots.Add(newSpot);
    }

    public Transform GetSpawnSpot()
    {
        if(spawnSpots.Count == 0) return null;
        int output = nextSpawnIndex;
        nextSpawnIndex += 1;
        nextSpawnIndex %= spawnSpots.Count;
        return spawnSpots[output];
    }

    public static void ServerDebugMessage(string message)
    {
        if (instance == null) return;

        foreach (PlayerController player in FindObjectsByType(typeof(PlayerController), FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            player.DebugServerMessageRpc(message);
        }
    }
}
