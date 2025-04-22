using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    [SerializeField]
    private GameObject shipObj;
    private GameObject shipRef;

    private Movement shipMovement;

    private Vector2 inputVec = Vector2.zero;
    private NetworkVariable<Vector2> sendInputVec = new NetworkVariable<Vector2>(Vector2.zero, NetworkVariableReadPermission.Everyone ,NetworkVariableWritePermission.Owner);

    void Start()
    {
        if (IsServer)
        {
            shipRef = Instantiate(shipObj);
            shipRef.GetComponent<NetworkObject>().Spawn(true);
            shipMovement = shipRef.GetComponent<Movement>();
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
}
