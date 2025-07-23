using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNameDisplay : MonoBehaviour
{
    [SerializeField]
    private GameObject labelObj;
    private GameObject labelRef;

    private SmallHealth health;

    public bool isOwner = false;

    [SerializeField]
    private Vector3 offset;

    private void Awake()
    {
        health = GetComponent<SmallHealth>();
    }

    private void Start()
    {
        if (isOwner) return;
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
}
