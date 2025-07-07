using UnityEngine;
using UnityEngine.InputSystem;

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
        PipHealthBar pipBar = healthBarRef.GetComponent<PipHealthBar>();
        pipBar.SetColourData(gameObject.tag, false);
        pipBar.Initialize(health);
        health.AddHealthReactor((int prevValue, int newValue) => pipBar.UpdateHealthBar(newValue), pipBar.UpdateHealthBarMax);
    }

    private void Update()
    {
        healthBarRef.transform.position = transform.position + offset;
        healthBarRef.SetActive(health.isAlive);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(offset, new Vector3(1.2f, 0.1f, 0.1f));
    }
}
