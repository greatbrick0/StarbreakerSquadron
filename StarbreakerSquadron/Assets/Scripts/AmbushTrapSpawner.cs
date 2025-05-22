using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class AmbushTrapSpawner : NetworkBehaviour
{
    private GameObject trapRef;
    private Targetable trapHealth;

    [SerializeField]
    private float timeInactive = 0.0f;
    private NetworkVariable<bool> isActivated = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField]
    private float kickPower = 40.0f;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        trapRef = transform.GetChild(transform.childCount - 1).gameObject;
        trapHealth = trapRef.GetComponent<Targetable>();
        trapHealth.deathEvent.AddListener(ResetTrap);
        if (IsServer)
        {
            trapRef.transform.position = transform.position;
            trapRef.transform.rotation = transform.rotation;
        }
        else
        {
            if (isActivated.Value) trapHealth.BecomeShown();
            else trapHealth.BecomeHidden();
        }
    }

    void Update()
    {
        if (IsServer)
        {
            isActivated.Value = trapHealth.isAlive;

            if (!trapHealth.isAlive)
            {
                timeInactive += 1.0f * Time.deltaTime;
                if (timeInactive > 10.0f) ActivateTrapRpc();
            }
        }
    }

    [Rpc(SendTo.Everyone)]
    public void ActivateTrapRpc()
    {
        Debug.Log("ambush");
        trapHealth.BecomeShown();
        trapRef.GetComponent<Movement>().Stun(0.3f, true, transform.up * kickPower);
    }

    private void ResetTrap()
    {
        timeInactive = 0.0f;
        trapRef.transform.position = transform.position;
        trapRef.transform.rotation = transform.rotation;
    }
}
