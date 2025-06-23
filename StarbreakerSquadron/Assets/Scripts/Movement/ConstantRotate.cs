using UnityEngine;

public class ConstantRotate : MonoBehaviour
{
    [SerializeField]
    private float rotateSpeed = 90.0f;

    private void Update()
    {
        transform.Rotate(rotateSpeed * Time.deltaTime * Vector3.back);
    }
}
