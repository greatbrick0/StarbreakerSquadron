using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNameDisplay : NetworkBehaviour
{
    public ulong id { get; private set; }

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
        Gizmos.DrawWireCube(offset, new Vector3(2.0f, 0.2f, 0.1f));
    }

    private void Update()
    {
        labelRef.transform.position = transform.position + offset;
        labelRef.SetActive(health.isAlive);
    }

    public void SetNameFromId(ulong newId)
    {
        id = newId;
        FixedString32Bytes output = new FixedString32Bytes(ClientManager.instance.clients[ClientManager.instance.clientIds.IndexOf(id)].username);
        SetNameRpc(output, newId);
    }

    [Rpc(SendTo.Everyone)]
    private void SetNameRpc(FixedString32Bytes newName, ulong clientId)
    {
        if(NetworkManager.Singleton.LocalClientId == clientId)
        {
            labelRef.GetComponent<TMP_Text>().text = "";
            return;
        }

        Debug.Log("Set name");
        labelRef.GetComponent<TMP_Text>().text = newName.ToString();
    }
}
