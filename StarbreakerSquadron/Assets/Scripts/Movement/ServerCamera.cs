using UnityEngine;

public class ServerCamera : MonoBehaviour
{
    [SerializeField]
    private float speed = 5.0f;

    void Update()
    {
        Vector2 inputVec = Vector2.zero;
        if (Input.GetKey(KeyCode.W))
            inputVec.y += 1;
        if (Input.GetKey(KeyCode.S))
            inputVec.y += -1;
        if (Input.GetKey(KeyCode.A))
            inputVec.x += -1;
        if (Input.GetKey(KeyCode.D))
            inputVec.x += 1;
        transform.position += speed * Time.deltaTime * inputVec.SetZ();
    }
}
