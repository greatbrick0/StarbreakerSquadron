using BrainCloud.Common;
using UnityEngine;

public class VipShipTracker : MonoBehaviour
{
    private Camera cam;
    private Targetable health;

    [SerializeField]
    private Vector2 horizontalStops = new Vector2(-0.8f, 0.8f);
    [SerializeField]
    private Vector2 verticalStops = new Vector2(-0.8f, 0.8f);
    [SerializeField]
    private GameObject arrowObj;
    private GameObject arrowRef;
    

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
        arrowRef.transform.GetChild(0).gameObject.SetActive(difference != arrowOffset);
        arrowRef.transform.GetChild(1).gameObject.SetActive(difference != arrowOffset);
        arrowRef.transform.position += arrowOffset;
    }

    private void CreateArrow()
    {
        arrowRef = Instantiate(arrowObj);
        arrowRef.transform.position = cam.transform.position.SetZ();
    }

    private void DestroyArrow()
    {
        Destroy(arrowRef);
    }
}
