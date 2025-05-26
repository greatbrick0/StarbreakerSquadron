using System.Runtime.CompilerServices;
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
    [SerializeField, Display]
    private int currentHealth = 100;
    [SerializeField]
    private Color baseHealthColour = Color.green;
    [SerializeField]
    private int criticalHealthAmount = 20;
    [SerializeField]
    private Color criticalHealthColour = Color.red;
    [SerializeField, Range(0f, 1f)]
    private float warningHealthAmount = 0.4f;
    [SerializeField]
    private Color warningHealthColour = Color.yellow;
    [SerializeField]
    private Slider healthBar;
    [SerializeField]
    private Image healthBarFill;
    [SerializeField]
    private Image healthBarCritical;
    [SerializeField]
    private Image healthBarWarning;
    [SerializeField]
    private TMP_Text healthLabel;

    private float animTime = 0.0f;

    private void Awake()
    {
        if(Network.sharedInstance.IsDedicatedServer) Destroy(gameObject);
    }

    private void Update()
    {
        animTime += Time.deltaTime;
        if (animTime > 100) animTime -= Mathf.PI * 30;

        if (currentHealth <= criticalHealthAmount)
        {
            healthBarFill.color = Color.Lerp(baseHealthColour, criticalHealthColour, Mathf.Clamp01(Mathf.Sin(12 * animTime) + 0.7f));
            healthBarCritical.color = new Color(1, 1, 1, 1 - Mathf.Pow(Mathf.Sin(8 * animTime), 6));
            healthBarWarning.color = Color.clear;
        }
        else if ((1.0f * currentHealth) / maxHealth < warningHealthAmount)
        {
            healthBarFill.color = Color.Lerp(baseHealthColour, warningHealthColour, Mathf.Clamp01(Mathf.Sin(12 * animTime) + 0.7f));
            healthBarCritical.color = Color.clear;
            healthBarWarning.color = new Color(1, 1, 1, 1 - Mathf.Pow(Mathf.Sin(4 * animTime), 4));
        }
        else
        {
            healthBarFill.color = baseHealthColour;
            healthBarCritical.color = Color.clear;
            healthBarWarning.color = Color.clear;
        }
    }

    public void UpdateHealthBar(int newHealth)
    {
        currentHealth = newHealth;
        healthBar.value = (1.0f * newHealth) / maxHealth;
        healthLabel.text = newHealth.ToString();
    }
}
