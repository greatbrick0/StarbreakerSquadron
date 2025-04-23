using UnityEngine;
using Unity.Netcode;

public class Movement : NetworkBehaviour
{
    public Vector2 inputVector;

    public float maxSpeed = 5f;
}
