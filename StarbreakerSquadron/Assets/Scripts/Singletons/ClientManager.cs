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
        public string username = "---";
        public string profileId;
        public string userPasscode;
        public Dictionary<string, object> extraData;
        public PlayerController controllerRef;
    }

    public static ClientManager instance;
    private NetworkManager _netManager;
    private BrainCloudS2S _bcS2S;

    private string _lobbyId;
    public Dictionary<ulong, ClientSummary> clients { get; private set; } = new Dictionary<ulong, ClientSummary>();
    public Dictionary<ulong, string> clientIdToProfileId = new Dictionary<ulong, string>();
    private int untrackedPlayers = 0;

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
        untrackedPlayers += 1;
        Dictionary<string, object> request = new Dictionary<string, object>
            {
                { "service", "lobby" },
                { "operation", "GET_LOBBY_DATA" },
                { "data", new Dictionary<string, object>
                {{ "lobbyId", _lobbyId }}
                }
            };
        clientIdToProfileId.Add(id, string.Empty);
        ServerMessage("Client " + id + " joined");
        _bcS2S.Request(request, (string responseJson) => { OnLobbyDataMemberJoin(responseJson, id); });
        if (Application.isEditor) AddClient(id, new ClientSummary());
    }

    private void OnClientLeave(ulong id)
    {
        Debug.Log("client left");
        RemoveClient(id);
    }

    private void OnLobbyDataMemberJoin(string responseJson, ulong id)
    {
        Dictionary<string, object> response = JsonReader.Deserialize<Dictionary<string, object>>(responseJson);
        Dictionary<string, object> data = response["data"] as Dictionary<string, object>;
        Dictionary<string, object>[] membersData = data["members"] as Dictionary<string, object>[];
        foreach(Dictionary<string, object> member in membersData)
        {
            ClientSummary output = new ClientSummary();
            output.username = member["name"] as string;
            output.profileId = member["profileId"] as string;
            output.userPasscode = member["passcode"] as string;
            try { output.extraData = member["extra"] as Dictionary<string, object>; }
            catch { Debug.LogError("No extra data for " + output.username); }
            AddClient(id, output);
        }
    }

    private void AddClient(ulong newId, ClientSummary newSummary)
    {
        if (!clients.ContainsKey(newId))
        {
            ServerMessage("Adding ID " + newId + ", player " + newSummary.username);
            newSummary.controllerRef = NetworkManager.Singleton.ConnectedClients[newId].PlayerObject.GetComponent<PlayerController>();
            clients.Add(newId, newSummary);
            untrackedPlayers -= 1;
        }
        else
        {
            ServerMessage("Already had ID " + newId, true, false);
        }
    }

    private void RemoveClient(ulong removedId)
    {
        if (clients.ContainsKey(removedId))
        {
            ServerMessage("removing id " + removedId, true, false);
            clients.Remove(removedId);
        }
    }

    public IEnumerator IdentifyPlayer(string givenPasscode, string givenProfileId, ulong givenCLientId, int claimedShip)
    {
        yield return new WaitUntil(() => untrackedPlayers == 0);
        ServerMessage("Identification condition met", true);

        int selectedShipIndex = 0;

        KeyValuePair<ulong, ClientSummary> matchingProfile = clients.FirstOrDefault(kvp => kvp.Value.profileId == givenProfileId);
        if (Application.isEditor)
        {
            AllowSpawnPlayerShip(clients[givenCLientId].controllerRef, claimedShip);
        }
        else if (matchingProfile.Value.userPasscode == givenPasscode)
        {
            clientIdToProfileId[givenCLientId] = givenProfileId;
            try { selectedShipIndex = (int?)clients[givenCLientId].extraData["selectedShipIndex"] ?? 0; }
            catch { selectedShipIndex = 0; }

            AllowSpawnPlayerShip(clients[givenCLientId].controllerRef, selectedShipIndex);
        }
        else
        {
            ServerMessage("Could not confirm identity! ");
        }
        ServerMessage("Server finished identification");
    }

    private void AllowSpawnPlayerShip(PlayerController controller, int shipIndex)
    {
        controller.SpawnShip(playerShipObjs[shipIndex], GetSpawnSpot());
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

    public ClientSummary GetSummaryFromId(ulong id)
    {
        ClientSummary output = null;
        return output;
    }

    public static void ServerMessage(string message, bool useRpc = false, bool useSignal = true)
    {
        if (instance == null) return;

        if (useRpc)
        {
            foreach (PlayerController player in FindObjectsByType(typeof(PlayerController), FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                player.DebugServerMessageRpc(message);
            }
        }
        if (useSignal) 
        {
            if(instance.clients.Count > 0)
            {
                Network.sharedInstance.StartServerSendLobbySignal(message);
            }
        }
    }
}
