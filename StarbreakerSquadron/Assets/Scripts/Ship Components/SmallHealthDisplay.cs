using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SmallHealth))]
public class SmallHealthDisplay : MonoBehaviour
{
    [SerializeField]
    private GameObject healthBarObj;
    public GameObject healthBarRef { get; private set; }

    private SmallHealth health;

    public bool isOwner = false;

    [SerializeField]
    private Vector3 offset;

    private void Awake()
    {
        health = GetComponent<SmallHealth>();
    }

    private void OnEnable()
    {
        healthBarRef = Instantiate(healthBarObj);
        PipHealthBar pipBar = healthBarRef.GetComponent<PipHealthBar>();
        pipBar.SetColourData(gameObject.tag, isOwner);
        pipBar.Initialize(health);
        health.AddHealthReactor((int prevValue, int newValue) => pipBar.UpdateHealthBar(newValue), pipBar.UpdateHealthBarMax);
    }

    private void OnDisable()
    {
        Destroy(healthBarRef);
    }

    private void Update()
    {
        healthBarRef.transform.position = transform.position + offset;
        healthBarRef.SetActive(health.isAlive);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(offset + transform.position, new Vector3(1.2f, 0.1f, 0.1f));
    }
}
