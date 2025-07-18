using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class TimedButton : Targetable
{
    private NetworkVariable<bool> pressed = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField]
    private float activeDuration = 20.0f;

    [SerializeField]
    public UnityEvent activateEvent;
    [SerializeField]
    public UnityEvent deactivateEvent;

    private void Start()
    {
        if (!IsServer)
        {
            pressed.OnValueChanged += (bool prevValue, bool newValue) =>
            {
                if(newValue) activateEvent.Invoke();
                else deactivateEvent.Invoke();
            };
        }
    }

    private void Update()
    {
        if (!IsServer) return;

        timeSinceLastDamage += 1.0f * Time.deltaTime;
        if (timeSinceLastDamage > activeDuration && pressed.Value)
        {
            pressed.Value = false;
            deactivateEvent.Invoke();
        }
    }

    public override void TakeDamage(int amount)
    {
        if (!IsServer) return;

        timeSinceLastDamage = 0.0f;

        if (!pressed.Value)
        {
            Debug.Log("pressed");
            pressed.Value = true;
            activateEvent.Invoke();
        }
    }
}
