using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    [SerializeField]
    private GameObject shipObj;
    private GameObject shipRef;
    private ulong shipRefId = 0;

    private Movement shipMovement;
    private WeaponsHolder shipWeapons;
    private FollowCamera cam;

    private Vector2 inputVec = Vector2.zero;
    private NetworkVariable<Vector2> sendInputVec = new NetworkVariable<Vector2>(Vector2.zero, NetworkVariableReadPermission.Everyone ,NetworkVariableWritePermission.Owner);
    private byte inputActives = 0;
    private NetworkVariable<byte> sendInputActives = new NetworkVariable<byte>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    void Start()
    {
        if (IsServer)
        {
            shipRef = Instantiate(shipObj);
            shipRef.GetComponent<NetworkObject>().Spawn(true);
            shipMovement = shipRef.GetComponent<Movement>();
            shipWeapons = shipRef.GetComponent<WeaponsHolder>();
            OwnerFindShipRpc(shipRef.GetComponent<NetworkObject>().NetworkObjectId);
            Network.sharedInstance.PrintSpawnedObjects();
        }
    }

    void Update()
    {
        if (IsServer)
        {
            shipMovement.inputVector = sendInputVec.Value;
            shipWeapons.inputActives = sendInputActives.Value;
        }
        if (!IsOwner) return;

        if(shipRefId != 0 && shipRef == null) InitShipRef(shipRefId);

        shipMovement.inputVector = sendInputVec.Value;
        shipWeapons.inputActives = sendInputActives.Value;

        inputVec = Vector2.zero;
        if (Input.GetKey(KeyCode.W))
            inputVec.y += 1;
        if (Input.GetKey(KeyCode.S))
            inputVec.y += -1;
        if (Input.GetKey(KeyCode.A))
            inputVec.x += -1;
        if (Input.GetKey(KeyCode.D))
            inputVec.x += 1;
        sendInputVec.Value = inputVec;

        inputActives = 0b0000;
        if (Input.GetMouseButton(0))
            inputActives |= 1 << 0; 
        if (Input.GetMouseButton(1))
            inputActives |= 1 << 1;
        if (Input.GetKey(KeyCode.Q))
            inputActives |= 1 << 2;
        if (Input.GetKey(KeyCode.E))
            inputActives |= 1 << 3;
        sendInputActives.Value = inputActives;
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
        cam = Camera.main.GetComponent<FollowCamera>();
        cam.followTarget = shipRef.transform;
        cam.InitLead();
    }
}
