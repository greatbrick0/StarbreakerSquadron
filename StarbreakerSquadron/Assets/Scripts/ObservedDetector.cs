using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class ObservedDetector : NetworkBehaviour
{
    [SerializeField]
    private float observedRadius = 30.0f;

    private bool CheckVisibility(ulong clientId)
    {
        if (!IsSpawned) return false;
        Dictionary<ulong, ClientManager.ClientSummary> dict = ClientManager.instance.clients;
        if (!dict.ContainsKey(clientId)) return false;
        return Vector3.Distance(dict[clientId].controllerRef.transform.position, transform.position) <= observedRadius;
    }

    private void OnNetworkTick()
    {
        foreach (var clientId in NetworkManager.ConnectedClientsIds)
        {
            bool shouldBeVisibile = CheckVisibility(clientId);
            bool isVisibile = GetComponent<NetworkObject>().IsNetworkVisibleTo(clientId);
            if (shouldBeVisibile && !isVisibile)
            {
                NetworkObject.NetworkShow(clientId);
            }
            else if (!shouldBeVisibile && isVisibile)
            {
                NetworkObject.NetworkHide(clientId);
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkObject.CheckObjectVisibility += CheckVisibility;
            NetworkManager.NetworkTickSystem.Tick += OnNetworkTick;
        }
        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkObject.CheckObjectVisibility -= CheckVisibility;
            NetworkManager.NetworkTickSystem.Tick -= OnNetworkTick;
        }
        base.OnNetworkDespawn();
    }

}
