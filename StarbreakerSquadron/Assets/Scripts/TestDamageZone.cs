using UnityEngine;

public class TestDamageZone : MonoBehaviour
{
    [SerializeField]
    private int damageQuantity = 15;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.TryGetComponent(out Targetable targetable))
        {
            targetable.TakeDamage(damageQuantity);
        }
    }
}
