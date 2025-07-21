using UnityEngine;
using UnityEngine.UI;

public class CooldownDisplay : MonoBehaviour
{
    [SerializeField]
    private Slider cooldownBar;
    [SerializeField]
    private Image cooldownBarFill;

    private float cooldownTime = 20.0f;
    public float remainingTime = 0.0f;

    void Start()
    {
        
    }

    void Update()
    {
        cooldownBar.value = 1.0f - (remainingTime / cooldownTime);
    }

    public void SetColour(Color newColour)
    {
        cooldownBarFill.color = newColour;
    }
}
