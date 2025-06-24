using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Thrust : MonoBehaviour
{
    private Vector3 baseScale = Vector3.one;
    [SerializeField, Range(0, 1)]
    private float extended = 0.0f;
    [SerializeField]
    private float extendSpeed = 2.0f;
    [SerializeField]
    private float flickerPower = 0.1f;
    [SerializeField]
    private float flickerSpeed = 20.0f;

    public bool powered = false;

    private void Awake()
    {
        baseScale = transform.localScale;
        transform.localScale = new Vector3(baseScale.x, 0, baseScale.z);
    }

    private void Update()
    {
        if (powered) extended += extendSpeed * Time.deltaTime;
        else extended -= extendSpeed * Time.deltaTime;
        extended = Mathf.Clamp(extended, 0.0f, 1.0f);

        transform.localScale = new Vector3(baseScale.x, Mathf.Max(baseScale.y * (extended + Flicker()), 0.0f), baseScale.z);
    }

    private float Flicker()
    {
        return Mathf.Sin(Time.time * flickerSpeed) * flickerPower * Mathf.Pow(extended, 2);
    }
}
