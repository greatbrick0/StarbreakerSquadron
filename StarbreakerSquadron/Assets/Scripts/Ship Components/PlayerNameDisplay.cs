using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNameDisplay : NetworkBehaviour
{
    public NetworkVariable<ulong> playerId { get; private set; } = new NetworkVariable<ulong>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField]
    private GameObject labelObj;
    private GameObject labelRef;

    private SmallHealth health;

    [SerializeField]
    private Vector3 offset;

    private void Awake()
    {
        health = GetComponent<SmallHealth>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            playerId.OnValueChanged += (prevId, newId) => RequestNameDisplayRpc(NetworkManager.Singleton.LocalClientId);
            if (playerId.Value != 0) RequestNameDisplayRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    private void OnEnable()
    {
        labelRef = Instantiate(labelObj);
    }

    private void OnDisable()
    {
        Destroy(labelRef);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(offset + transform.position, new Vector3(2.4f, 0.35f, 0.1f));
    }

    private void Update()
    {
        labelRef.transform.position = transform.position + offset;
        labelRef.SetActive(health.isAlive);
    }

    public void SetPlayerId(ulong newId)
    {
        playerId.Value = newId;
    }

    [Rpc(SendTo.Server)]
    private void RequestNameDisplayRpc(ulong id)
    {
        DisplayNameTargeted(0, playerId.Value, id);
    }

    private void DisplayNameTargeted(ulong prevId, ulong newId, ulong target)
    {
        FixedString32Bytes output = new FixedString32Bytes(ClientManager.instance.clients[newId].username);

        SetNameRpc(output, newId, RpcTarget.Single(target, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void SetNameRpc(FixedString32Bytes newName, ulong id, RpcParams rpcParams = default)
    {
        if(id == NetworkManager.Singleton.LocalClientId && false)
        {
            labelRef.GetComponent<TMP_Text>().text = string.Empty;
        }
        else
        {
            labelRef.GetComponent<TMP_Text>().text = newName.ToString();
        }
    }
}
