using UnityEngine;

[RequireComponent(typeof(SmallHealth))]
public class SmallHealthDisplay : MonoBehaviour
{
    [SerializeField]
    private GameObject healthBarObj;
    private GameObject healthBarRef;

    private SmallHealth health;

    [SerializeField]
    private Vector3 offset;

    private void Awake()
    {
        health = GetComponent<SmallHealth>();
    }

    private void Start()
    {
        healthBarRef = Instantiate(healthBarObj);
        healthBarRef.GetComponent<PipHealthBar>().Initialize(health);
    }

    private void Update()
    {
        healthBarRef.transform.position = transform.position + offset;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(offset, new Vector3(1.2f, 0.1f, 0.1f));
    }
}
