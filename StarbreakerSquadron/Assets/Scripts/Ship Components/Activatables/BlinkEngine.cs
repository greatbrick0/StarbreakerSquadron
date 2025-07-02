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
    private LineRenderer blinkLine;
    [SerializeField] 
    private Color blinkLineColour;
    [SerializeField]
    private float blinkLineFadeTime = 0.3f;
    [SerializeField]
    private AnimationCurve blinkLineWidth;
    [SerializeField]
    private float cooldown = 12f;
    [SerializeField]
    private AccelMovement target;
    [SerializeField]
    private float unitRadius = 0.5f;

    [Header("Blink properties")]
    [SerializeField, Display]
    private int maxBlinkDistance = 10;
    private float timeSinceBlink = 0.0f;

    [Header("Explosion properties")]
    [SerializeField, Display]
    private int explosionDamage = 50;
    [SerializeField, Display]
    private int explosionRadius = 3;
    [SerializeField]
    private float explosionLifeTime = 0.05f;

    private void Awake()
    {
        blinkLine = GetComponent<LineRenderer>();
        blinkLine.startWidth = 0.0f;
        blinkLine.endWidth = 0.3f;
    }

    private void Start()
    {
        PropertyGetter properties = PropertyGetter.propertiesInstance;
        string statColour = gameObject.tag;
        StartCoroutine(properties.GetValue((val) => maxBlinkDistance = Mathf.RoundToInt(val), "BulletSpeed", property, statColour));
        StartCoroutine(properties.GetValue((val) => explosionDamage = Mathf.RoundToInt(val), "Damage", property, statColour));
        StartCoroutine(properties.GetValue((val) => explosionRadius = Mathf.RoundToInt(val), "AreaOfEffectRadius", property, statColour));
    }

    private void Update()
    {
        timeSinceBlink += 1.0f * Time.deltaTime;

        if (IsServer)
        {

        }
        else
        {
            if(timeSinceBlink <= blinkLineFadeTime)
            {
                blinkLine.startColor = blinkLineColour.ChangeAlpha(1 - timeSinceBlink / blinkLineFadeTime);
                blinkLine.endColor = blinkLineColour.ChangeAlpha(1 - timeSinceBlink / blinkLineFadeTime);
            }
            else
            {
                blinkLine.startColor = Color.clear;
                blinkLine.endColor = Color.clear;
            }
        }
    }

    public void Activate()
    {
        if (IsServer)
        {
            RaycastHit2D hit = GetBlinkCast();

            if (hit) 
            {
                target.transform.position = hit.centroid;
            } 
            else
            {
                target.transform.position = target.transform.position + target.transform.up * maxBlinkDistance;
            }
            CreateExplosion();
            target.InstantStopVelocity();
        }
        else
        {
            RaycastHit2D hit = GetBlinkCast();
            GetComponent<AudioSource>().Play();
            Vector3 startPoint = target.transform.position;
            Vector3 endPoint = hit ? hit.centroid : target.transform.position + target.transform.up * maxBlinkDistance;
            for(int ii = 0; ii < blinkLine.positionCount; ii++)
            {
                blinkLine.SetPosition(ii, Vector3.Lerp(startPoint, endPoint, ii / (blinkLine.positionCount - 1)));
            }
        }
        timeSinceBlink = 0.0f;
    }

    public float GetCooldown()
    {
        return cooldown;
    }

    private RaycastHit2D GetBlinkCast()
    {
        return Physics2D.Raycast(target.transform.position, target.transform.up, maxBlinkDistance, LayerMask.GetMask("Terrain"), -1, 1);
        //return Physics2D.CircleCast(target.transform.position, unitRadius, target.transform.up,LayerMask.GetMask("Terrain"), ~0, -1f, 1f);
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
