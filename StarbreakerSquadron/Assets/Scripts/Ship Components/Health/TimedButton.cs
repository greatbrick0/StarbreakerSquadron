using UnityEngine;
using UnityEngine.Events;

public class TimedButton : Targetable
{
    [SerializeField]
    private bool pressed = false;

    [SerializeField]
    private float activeDuration = 20.0f;

    [SerializeField]
    public UnityEvent activateEvent;
    [SerializeField]
    public UnityEvent deactivateEvent;

    public override void TakeDamage(int amount)
    {
        timeSinceLastDamage = 0.0f;

        if (!pressed)
        {
            pressed = true;
            activateEvent.Invoke();
        }
    }
    
    void Update()
    {
        timeSinceLastDamage += 1.0f * Time.deltaTime;
        if (timeSinceLastDamage > activeDuration && pressed) 
        {
            pressed = false;
            deactivateEvent.Invoke();
        }
    }
}
