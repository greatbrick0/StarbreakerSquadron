using UnityEngine;

public class ServerCamera : MonoBehaviour
{
    [SerializeField]
    private float speed = 5.0f;

    [SerializeField]
    private float zoomSpeed = 0.1f;
    private float defaultZoom = 8.0f;
    private float zoomMult = 1.0f;

    private void Start()
    {
        if(!Network.sharedInstance.IsDedicatedServer) Destroy(this);
        defaultZoom = GetComponent<Camera>().orthographicSize;
    }

    void Update()
    {
        if (Input.mouseScrollDelta.y > 0)
        {
            zoomMult -= zoomSpeed;
            GetComponent<Camera>().orthographicSize = defaultZoom * zoomMult;
        }
        else if(Input.mouseScrollDelta.y < 0)
        {
            zoomMult += zoomSpeed;
            GetComponent<Camera>().orthographicSize = defaultZoom * zoomMult;
        }

        Vector2 inputVec = Vector2.zero;
        if (Input.GetKey(KeyCode.W))
            inputVec.y += 1;
        if (Input.GetKey(KeyCode.S))
            inputVec.y += -1;
        if (Input.GetKey(KeyCode.A))
            inputVec.x += -1;
        if (Input.GetKey(KeyCode.D))
            inputVec.x += 1;
        transform.position += speed * zoomMult * Time.deltaTime * inputVec.SetZ();
    }
}
