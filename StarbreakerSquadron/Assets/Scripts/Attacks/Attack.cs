using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class Attack : NetworkBehaviour
{
    [SerializeField]
    protected Teams team;
    [SerializeField]
    protected int primaryPower;
    [SerializeField]
    protected int secondaryPower;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return;

        if(collision.gameObject.layer == 3)
        {
            HitTerrain();
        }
        else if (collision.gameObject.TryGetComponent(out Targetable targetable))
        {
            HitTargetable(targetable);
        }
    }

    protected virtual void HitTerrain()
    {
        ResetToHiddenRpc();
    }

    protected virtual void HitTargetable(Targetable targetable)
    {
        if(targetable.team != team)
        {
            targetable.TakeDamage(0);
            ResetToHiddenRpc();
        }
    }

    [Rpc(SendTo.Everyone)]
    private void SetValuesRpc(Teams newTeam, int newPower, string newColour = "cccccc", int newSecondaryPower = 0)
    {
        GetComponent<Collider2D>().enabled = true;
        transform.GetChild(0).gameObject.SetActive(true);
        team = newTeam;
        ColorUtility.TryParseHtmlString(newColour, out Color parsedColour);
        transform.GetChild(0).GetComponent<SpriteRenderer>().color = parsedColour;
        primaryPower = newPower;
        secondaryPower = newSecondaryPower;
    }

    [Rpc(SendTo.Everyone)]
    private void ResetToHiddenRpc()
    {
        GetComponent<Collider2D>().enabled = false;
        transform.GetChild(0).gameObject.SetActive(false);
        team = Teams.Environment;
        transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.white;
        primaryPower = 0;
        secondaryPower = 0;
    }
}
