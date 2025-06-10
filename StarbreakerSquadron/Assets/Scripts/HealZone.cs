using System.Runtime.InteropServices;
using UnityEngine;

public class HealZone : MonoBehaviour
{
    [field: SerializeField]
    public Teams team { get; protected set; }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        
    }
}
