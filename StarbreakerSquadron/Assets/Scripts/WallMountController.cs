using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class WallMountController : NetworkBehaviour
{
    private Vector2 inputVec = Vector2.zero;
    private byte inputActives = 0;

    private void Update()
    {
        if (!IsServer) return;

        inputActives = 0b1111;

        GetComponent<WeaponsHolder>().inputActives = inputActives;
    }
}
