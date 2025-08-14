using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CooldownDisplay : MonoBehaviour
{
    [SerializeField]
    private Slider cooldownBar;
    [SerializeField]
    private Image cooldownBarFill;
    [SerializeField]
    private TMP_Text remainingTimeLabel;

    private WeaponsHolder.WeaponSlot slot;

    private float cooldownTime = 20.0f;
    public float remainingTime = 0.0f;

    void Update()
    {
        remainingTime = slot.remainingCooldown;
        cooldownBar.value = 1.0f - (remainingTime / cooldownTime);
        if(remainingTime > 0.0f)
            remainingTimeLabel.text = string.Format(StringUtils.COOLDOWN_TIME_LABEL_FORMAT, remainingTime);
        else
            remainingTimeLabel.text = string.Empty;
    }

    public void AssignSlot(WeaponsHolder.WeaponSlot newSlot)
    {
        slot = newSlot;
        cooldownBarFill.color = slot.GetCooldownColour();
        slot.onActivated += RestartCooldown;
    }

    private void RestartCooldown(float newCooldownTime)
    {
        cooldownTime = slot.GetCooldown();
        remainingTime = slot.remainingCooldown;
    }
}
