using UnityEngine;
using Unity.Netcode;
using Unity.Collections.LowLevel.Unsafe;

public class PlayerController : NetworkBehaviour
{
    [SerializeField]
    private GameObject shipObj;
    private GameObject shipRef;
    private ulong shipRefId = 0;

    private Movement shipMovement;
    private WeaponsHolder shipWeapons;
    private SmallHealth shipHealth;
    private FollowCamera cam;
    private GameHudManager gameHud;

    private Vector2 inputVec = Vector2.zero;
    private NetworkVariable<Vector2> sendInputVec = new NetworkVariable<Vector2>(Vector2.zero, NetworkVariableReadPermission.Everyone ,NetworkVariableWritePermission.Owner);
    private byte inputActives = 0;
    private NetworkVariable<byte> sendInputActives = new NetworkVariable<byte>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    void Start()
    {
        if (IsServer)
        {
            
        }
        else
        {
            cam = Camera.main.GetComponent<FollowCamera>();
            gameHud = FindFirstObjectByType<GameHudManager>();
            gameHud.attemptLeaveEvent.AddListener(Network.sharedInstance.DisconnectFromSession);
            SendPasscodeRpc(Network.sharedInstance.clientPasscode);
        }
    }

    void Update()
    {
        if (IsServer && shipRef != null)
        {
            shipMovement.inputVector = sendInputVec.Value;
            shipWeapons.inputActives = sendInputActives.Value;
        }
        if (!IsOwner) return;
        if(shipRefId != 0 && shipRef == null) InitShipRef(shipRefId);
        if (shipRef == null) return;

        shipMovement.inputVector = sendInputVec.Value;
        shipWeapons.inputActives = sendInputActives.Value;
        inputVec = Vector2.zero;
        inputActives = 0b0000;

        if (Input.GetKeyUp(KeyCode.Tab)) gameHud.ToggleMenuHotKey();
        if (gameHud.state == GameHudState.Gameplay)
        {
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                inputVec.y += 1;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                inputVec.y += -1;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                inputVec.x += -1;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                inputVec.x += 1;

            if (Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.RightControl))
                inputActives |= 1 << 0;
            if (Input.GetMouseButton(1) || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                inputActives |= 1 << 1;
            if (Input.GetKey(KeyCode.Q))
                inputActives |= 1 << 2;
            if (Input.GetKey(KeyCode.E))
                inputActives |= 1 << 3;
        }

        sendInputVec.Value = inputVec;
        sendInputActives.Value = inputActives;
    }

    public void SpawnShip(GameObject shipObj)
    {
        shipRef = Instantiate(shipObj);
        shipRef.GetComponent<NetworkObject>().Spawn(true);
        shipMovement = shipRef.GetComponent<Movement>();
        shipWeapons = shipRef.GetComponent<WeaponsHolder>();
        OwnerFindShipRpc(shipRef.GetComponent<NetworkObject>().NetworkObjectId);
    }

    [Rpc(SendTo.Server)]
    private void SendPasscodeRpc(string passcode)
    {
        StartCoroutine(ClientManager.instance.IdentifyPlayer(this, passcode));
    }

    [Rpc(SendTo.Owner)]
    private void OwnerFindShipRpc(ulong id)
    {
        shipRefId = id;
    }

    private void InitShipRef(ulong id)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(shipRefId)) return;

        shipRef = NetworkManager.Singleton.SpawnManager.SpawnedObjects[id].gameObject;
        shipMovement = shipRef.GetComponent<Movement>();
        shipWeapons = shipRef.GetComponent<WeaponsHolder>();
        shipHealth = shipRef.GetComponent<SmallHealth>();
        cam.followTarget = shipRef.transform;
        cam.InitLead();
        gameHud.maxHealth = shipHealth.maxHealth;
        shipHealth.AddHealthReactor((int prevValue, int newValue) => gameHud.UpdateHealthBar(newValue), gameHud.UpdateHealthBarMax);
        gameHud.UpdateHealthBarMax(shipHealth.maxHealth);
        gameHud.UpdateHealthBar(gameHud.maxHealth);
        shipHealth.deathEvent.AddListener(() => StartCoroutine(gameHud.StartRespawningHud()));
        gameHud.ChangeGameHudState(GameHudState.Gameplay);
    }
}
