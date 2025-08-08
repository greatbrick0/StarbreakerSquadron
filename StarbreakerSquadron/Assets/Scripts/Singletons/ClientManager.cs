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
        Dictionary<string, object> request = new Dictionary<string, object>
            {
                { "service", "lobby" },
                { "operation", "GET_LOBBY_DATA" },
                { "data", new Dictionary<string, object>
                {{ "lobbyId", _lobbyId }}
                }
            };
        clientIdToProfileId.Add(id, string.Empty);
        ServerDebugMessage("Client " + id + " joined");
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
        ServerDebugMessage("Server tried to respond");

        if (allPlayersAccountedFor) return;

        ServerDebugMessage("Client joined and brainCloud responded");
        Dictionary<string, object> response = JsonReader.Deserialize<Dictionary<string, object>>(responseJson);
        var data = response["data"] as Dictionary<string, object>;
        Dictionary<string, object>[] membersData = data["members"] as Dictionary<string, object>[];
        foreach(Dictionary<string, object> member in membersData)
        {
            ClientSummary output = new ClientSummary();
            output.username = member["name"] as string;
            output.profileId = member["profileId"] as string;
            output.userPasscode = member["passcode"] as string;
            try { output.extraData = member["extra"] as Dictionary<string, object>; }
            catch { ServerDebugMessage(output.username + " did not have extra data"); }
            AddClient(id, output);
        }

        allPlayersAccountedFor = true;
        ServerDebugMessage("All players accounted for");
    }

    private void AddClient(ulong newId, ClientSummary newSummary)
    {
        if (!clients.ContainsKey(newId))
        {
            ServerDebugMessage("adding id " + newId + " player " + newSummary.username);
            clients.Add(newId, newSummary);
        }  
        else clients[newId] = newSummary;
    }

    private void RemoveClient(ulong removedId)
    {
        if (clients.ContainsKey(removedId))
        {
            ServerDebugMessage("removing id " + removedId);
            clients.Remove(removedId);
        }
    }

    public IEnumerator IdentifyPlayer(PlayerController givenController, string givenPasscode, string givenProfileId, ulong givenCLientId, int claimedShip)
    {
        ServerDebugMessage("Client started identification");
        yield return new WaitUntil(() => allPlayersAccountedFor || Application.isEditor);

        int selectedShipIndex = 0;

        KeyValuePair<ulong, ClientSummary> matchingProfile = clients.FirstOrDefault(kvp => kvp.Value.profileId == givenProfileId);
        if (Application.isEditor)
        {
            AllowSpawnPlayerShip(givenController, claimedShip);
        }
        else if (matchingProfile.Value.userPasscode == givenPasscode)
        {
            clientIdToProfileId[givenCLientId] = givenProfileId;
            clients[givenCLientId].controllerRef = givenController;
            try { selectedShipIndex = (int?)clients[givenCLientId].extraData["selectedShipIndex"] ?? 0; }
            catch { selectedShipIndex = 0; }

            AllowSpawnPlayerShip(givenController, selectedShipIndex);
        }
        else
        {
            ServerDebugMessage("Could not confirm identity! ");
        }
    }

    private void AllowSpawnPlayerShip(PlayerController controller, int shipIndex)
    {
        controller.SpawnShip(playerShipObjs[shipIndex], GetSpawnSpot());
        ServerDebugMessage("Spawned " + playerShipObjs[shipIndex].name);
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

    public static void ServerDebugMessage(string message)
    {
        if (instance == null) return;

        foreach (PlayerController player in FindObjectsByType(typeof(PlayerController), FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            player.DebugServerMessageRpc(message);
        }
    }
}
