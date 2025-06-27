using BrainCloud.Common;
using UnityEngine;

public class VipShipTracker : MonoBehaviour
{
    private Camera cam;
    private Targetable health;

    [SerializeField]
    private Sprite symbolSprite;
    [SerializeField]
    private Color pointerColour = Color.gray;
    [SerializeField]
    private Vector2 horizontalStops = new Vector2(-0.8f, 0.8f);
    [SerializeField]
    private Vector2 verticalStops = new Vector2(-0.8f, 0.8f);
    [SerializeField]
    private float tooCloseBuffer = 3.0f;
    [SerializeField]
    private float fadeSpeed = 10.0f;
    [SerializeField]
    private GameObject arrowObj;
    private GameObject arrowRef;
    private SpriteRenderer pointerSpriteRef;
    private SpriteRenderer symbolSpriteRef;

    private bool tooClose = false;
    private float opacity = 0.0f;
    

    void Start()
    {
        cam = Camera.main;
        health = GetComponent<Targetable>();
        health.respawnEvent.AddListener(CreateArrow);
        health.deathEvent.AddListener(DestroyArrow);
        CreateArrow();
    }

    private void LateUpdate()
    {
        if (arrowRef == null) return;

        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;
        Vector3 difference = transform.position - cam.transform.position.SetZ();
        arrowRef.transform.GetChild(0).rotation = Quaternion.FromToRotation(Vector3.up, difference);
        arrowRef.transform.position = cam.transform.position.SetZ();
        Vector3 arrowOffset = new Vector3(
            Mathf.Clamp(difference.x, horizontalStops.x * camWidth, horizontalStops.y * camWidth),
            Mathf.Clamp(difference.y, verticalStops.x * camHeight, verticalStops.y * camHeight),
            0);
        tooClose = !(difference.magnitude > arrowOffset.magnitude + tooCloseBuffer);
        arrowRef.transform.position += arrowOffset;


        opacity += (tooClose ? -fadeSpeed : fadeSpeed) * Time.deltaTime;
        opacity = Mathf.Clamp01(opacity);
        pointerSpriteRef.color = ChangeAlpha(pointerSpriteRef.color, opacity);
        symbolSpriteRef.color = ChangeAlpha(symbolSpriteRef.color, opacity);
    }

    private void CreateArrow()
    {
        arrowRef = Instantiate(arrowObj);
        arrowRef.transform.position = cam.transform.position.SetZ();
        symbolSpriteRef = arrowRef.transform.GetChild(1).GetComponent<SpriteRenderer>();
        pointerSpriteRef = arrowRef.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
        symbolSpriteRef.sprite = symbolSprite;
        pointerSpriteRef.color = pointerColour;
        opacity = 0.0f;
    }

    private Color ChangeAlpha(Color oldColour, float newAlpha)
    {
        return new Color(oldColour.r, oldColour.g, oldColour.b, newAlpha);
    }

    private void DestroyArrow()
    {
        Destroy(arrowRef);
    }
}
