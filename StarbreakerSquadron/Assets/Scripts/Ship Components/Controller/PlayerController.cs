using UnityEngine;
using Unity.Netcode;
using Unity.Collections.LowLevel.Unsafe;

public class PlayerController : NetworkBehaviour
{
    [SerializeField]
    private GameObject warpEffectObj;
    private GameObject warpEffectRef;
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

    private float respawnTime = 10.0f;
    public float respawnProgress { get; private set; } = 0.0f;

    private void Start()
    {
        if (IsServer)
        {
            
        }
        else
        {
            cam = Camera.main.GetComponent<FollowCamera>();
            gameHud = FindFirstObjectByType<GameHudManager>();
            gameHud.attemptLeaveEvent.AddListener(Network.sharedInstance.DisconnectFromSession);
            SendPasscodeRpc(Network.sharedInstance.clientPasscode, Network.sharedInstance.selectedShipIndex);
        }
    }

    private void Update()
    {
        if (IsServer && shipRef != null)
        {
            shipMovement.inputVector = sendInputVec.Value;
            shipWeapons.inputActives = sendInputActives.Value;
        }
        if (!IsOwner) return;
        if(shipRefId != 0 && shipRef == null) InitShipRef(shipRefId);
        if (shipRef == null) return;

        if (!shipHealth.isAlive)
        {
            respawnProgress -= 1.0f * Time.deltaTime;
            gameHud.respawnProgress = respawnProgress;
        }
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

    public void SpawnShip(GameObject shipObj, Transform location)
    {
        WarpEffect.WarpCallback spawnFunc = () =>
        {
            shipRef = Instantiate(shipObj);
            shipRef.transform.position = location.position;
            shipRef.transform.rotation = location.rotation;
            shipRef.GetComponent<NetworkObject>().Spawn(true);
            shipMovement = shipRef.GetComponent<Movement>();
            shipWeapons = shipRef.GetComponent<WeaponsHolder>();
            shipHealth = shipRef.GetComponent<SmallHealth>();
            OwnerFindShipRpc(shipRef.GetComponent<NetworkObject>().NetworkObjectId);
        };

        TeleportCameraRpc(location.position);
        CreateWarpEffectRpc(1.6f, 1.2f, location.position);
        warpEffectRef.GetComponent<WarpEffect>().warpCallback = spawnFunc;
    }

    private void RespawnShip(Transform location)
    {
        WarpEffect.WarpCallback spawnFunc = () =>
        {
            shipRef.transform.position = location.position;
            shipRef.transform.rotation = location.rotation;
            FinishRespawnRpc();
        };

        TeleportCameraRpc(location.position);
        CreateWarpEffectRpc(1.6f, 1.2f, location.position);
        warpEffectRef.GetComponent<WarpEffect>().warpCallback = spawnFunc;
    }

    [Rpc(SendTo.Everyone)]
    private void CreateWarpEffectRpc(float sizeMult, float duration, Vector3 location = default)
    {
        warpEffectRef = Instantiate(warpEffectObj);
        warpEffectRef.transform.position = location;
        warpEffectRef.transform.localScale = Vector3.one * sizeMult;
        warpEffectRef.GetComponent<WarpEffect>().Initialize(IsServer, duration);
    }

    [Rpc(SendTo.Server)]
    private void SendPasscodeRpc(string passcode, ushort claimedShip)
    {
        StartCoroutine(ClientManager.instance.IdentifyPlayer(this, passcode, claimedShip));
    }

    [Rpc(SendTo.Server)]
    public void AttemptRespawnRpc()
    {
        if (shipHealth.isAlive) return;
        if (respawnProgress > 0.0f) return;

        RespawnShip(ClientManager.instance.GetSpawnSpot());
    }

    [Rpc(SendTo.Everyone)]
    private void FinishRespawnRpc()
    {
        shipHealth.BecomeShown();
        if (!IsServer)
        {
            gameHud.ChangeGameHudState(GameHudState.Gameplay);
            cam.InitLead();
        }
    }

    [Rpc(SendTo.Owner)]
    private void OwnerFindShipRpc(ulong id)
    {
        shipRefId = id;
    }

    [Rpc(SendTo.Owner)]
    public void DebugServerMessageRpc(string message)
    {
        Debug.Log(message);
    }

    [Rpc(SendTo.Owner)]
    private void TeleportCameraRpc(Vector3 newPos)
    {
        cam.SnapTo(newPos);
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
        shipHealth.deathEvent.AddListener(() => respawnProgress = respawnTime);
        gameHud.respawnButton.onClick.AddListener(AttemptRespawnRpc);
        gameHud.ChangeGameHudState(GameHudState.Gameplay);
    }
}
