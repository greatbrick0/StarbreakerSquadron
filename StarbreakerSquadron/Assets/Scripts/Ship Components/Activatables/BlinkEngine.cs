using Unity.Netcode;
using UnityEngine;

public class BlinkEngine : NetworkBehaviour, IActivatable
{
    [SerializeField]
    private GameObject explosionObj;
    private GameObject explosionRef;

    [SerializeField]
    private string property = "BlinkEngine";
    [SerializeField]
    private Teams team = Teams.Environment;
    [SerializeField]
    private string explosionColour = "#cccccc";
    [SerializeField]
    private float cooldown = 12f;
    [SerializeField]
    private AccelMovement target;

    [Header("Blink properties")]
    [SerializeField, Display]
    private int maxBlinkDistance = 10;

    [Header("Explosion properties")]
    [SerializeField, Display]
    private int explosionDamage = 50;
    [SerializeField, Display]
    private int explosionRadius = 3;
    [SerializeField]
    private float explosionLifeTime = 0.05f;

    private void Start()
    {
        PropertyGetter properties = PropertyGetter.propertiesInstance;
        string statColour = gameObject.tag;
        StartCoroutine(properties.GetValue((val) => maxBlinkDistance = Mathf.RoundToInt(val), "BulletSpeed", property, statColour));
        StartCoroutine(properties.GetValue((val) => explosionDamage = Mathf.RoundToInt(val), "Damage", property, statColour));
        StartCoroutine(properties.GetValue((val) => explosionRadius = Mathf.RoundToInt(val), "AreaOfEffectRadius", property, statColour));
    }

    public void Activate()
    {
        if (IsServer)
        {
            target.transform.position += target.transform.up * maxBlinkDistance;
            CreateExplosion();
            target.InstantStopVelocity();
        }
        else
        {
            GetComponent<AudioSource>().Play();
        }
    }

    public float GetCooldown()
    {
        return cooldown;
    }

    public void Preview()
    {

    }

    private void CreateExplosion()
    {
        AttackInfo attackInfo;
        attackInfo = new AttackInfo(
            team,
            explosionDamage,
            transform.position,
            explosionLifeTime,
            explosionColour,
            explosionRadius
            );
        explosionRef = Instantiate(explosionObj);
        explosionRef.transform.position = attackInfo.originPos;
        explosionRef.GetComponent<NetworkObject>().Spawn(true);
        explosionRef.GetComponent<Attack>().SetValuesRpc(attackInfo);
    }
}
