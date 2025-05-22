using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameHudManager : MonoBehaviour
{
    [SerializeField]
    GameObject versionLabel;

    [Header("Health Display")]
    [Display]
    public int maxHealth = 100;
    [SerializeField]
    private Slider healthBar;
    [SerializeField]
    private TMP_Text healthLabel;

    private void Awake()
    {
        if(Network.sharedInstance.IsDedicatedServer) Destroy(gameObject);
    }

    public void UpdateHealthBar(int newHealth)
    {
        Debug.Log(newHealth.ToString() + " " + maxHealth.ToString() + " " + (newHealth / maxHealth).ToString());
        healthBar.value = (1.0f * newHealth) / maxHealth;
        healthLabel.text = newHealth.ToString();
    }
}
