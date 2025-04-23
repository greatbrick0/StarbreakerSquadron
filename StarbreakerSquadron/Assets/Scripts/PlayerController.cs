using UnityEngine;
using Unity.Netcode;
using static UnityEditor.Progress;
using System.Collections.Generic;
using System.Linq;

public class PlayerController : NetworkBehaviour
{
    [SerializeField]
    private GameObject shipObj;
    private GameObject shipRef;

    private Movement shipMovement;
    private FollowCamera cam;

    private Vector2 inputVec = Vector2.zero;
    private NetworkVariable<Vector2> sendInputVec = new NetworkVariable<Vector2>(Vector2.zero, NetworkVariableReadPermission.Everyone ,NetworkVariableWritePermission.Owner);

    void Start()
    {
        if (IsServer)
        {
            shipRef = Instantiate(shipObj);
            shipRef.GetComponent<NetworkObject>().Spawn(true);
            shipMovement = shipRef.GetComponent<Movement>();
            OwnerFindShipRpc(shipRef.GetComponent<NetworkObject>().NetworkObjectId);
        }
    }

    void Update()
    {
        if(IsServer)
        {
            shipMovement.inputVector = sendInputVec.Value;
        }
        if (!IsOwner) return;

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
    }

    [Rpc(SendTo.Owner)]
    private void OwnerFindShipRpc(ulong id)
    {
        shipRef = NetworkManager.Singleton.SpawnManager.SpawnedObjects[id].gameObject;
        shipMovement = shipRef.GetComponent<Movement>();
        cam = Camera.main.GetComponent<FollowCamera>();
        cam.followTarget = shipRef.transform;
        cam.InitLead();
    }
}
