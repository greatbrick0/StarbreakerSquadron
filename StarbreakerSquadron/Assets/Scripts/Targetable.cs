using UnityEngine;

public class Targetable : MonoBehaviour
{
    public enum Teams
    {
        Green,
        Yellow,
    }

    [SerializeField]
    public Teams team;
}
