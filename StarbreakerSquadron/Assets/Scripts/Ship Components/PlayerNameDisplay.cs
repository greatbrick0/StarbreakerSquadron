using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNameDisplay : NetworkBehaviour
{
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

    private void Start()
    {
        
        labelRef = Instantiate(labelObj);
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

    [Rpc(SendTo.Everyone)]
    private void SetNameRpc(string newName)
    {
        labelRef.GetComponent<TMP_Text>().text = newName;
    }
}
