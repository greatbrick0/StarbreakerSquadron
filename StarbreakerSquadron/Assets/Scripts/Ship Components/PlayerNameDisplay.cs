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
        if (!IsServer) return;

        playerId.OnValueChanged += DisplayName;
        if (playerId.Value != 0) DisplayName(0, playerId.Value);
    }

    private void OnEnable()
    {
        labelRef = Instantiate(labelObj);
    }

    private void OnDisable()
    {
        Destroy(labelRef);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        playerId.OnValueChanged -= DisplayName;
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
        string output = labelRef.GetComponent<TMP_Text>().text;
        output = playerId.Value.ToString() + output.Substring(1);
        labelRef.GetComponent<TMP_Text>().text = output;
    }

    public void SetPlayerId(ulong newId)
    {
        playerId.Value = newId;
    }

    private void DisplayName(ulong prevId, ulong newId)
    {
        FixedString32Bytes output;
        //if (NetworkManager.Singleton.LocalClientId == newId) output = new FixedString32Bytes(string.Empty);
        //else output = new FixedString32Bytes(ClientManager.instance.clients[ClientManager.instance.clientIds.IndexOf(newId)].username);
        output = new FixedString32Bytes(ClientManager.instance.clients[newId].username);

        SetNameRpc(output);
    }

    [Rpc(SendTo.Everyone)]
    private void SetNameRpc(FixedString32Bytes newName)
    {
        labelRef.GetComponent<TMP_Text>().text = newName.ToString();
    }
}
