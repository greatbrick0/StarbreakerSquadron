using UnityEngine;

public class DeathExplosion : MonoBehaviour
{
    [SerializeField]
    private GameObject explosionObj;
    private GameObject explosionRef;

    [SerializeField]
    private float explosionSize = 1.0f;
    [SerializeField]
    private float explosionDuration = 3.0f;

    private void Awake()
    {
        if(TryGetComponent(out SmallHealth smallHealth))
        {
            smallHealth.deathEvent.AddListener(CreateExplosion);
        }
    }

    public void CreateExplosion()
    {
        explosionRef = Instantiate(explosionObj);
        explosionRef.transform.localScale = Vector3.one * explosionSize;
        explosionRef.transform.position = transform.position;
        Invoke(nameof(CleanUpExplosion), explosionDuration);
    }

    public void CleanUpExplosion()
    {
        Destroy(explosionRef);
    }
}
