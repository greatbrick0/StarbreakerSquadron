using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform followTarget;

    [SerializeField]
    private float leadSpeed = 0f;
    [SerializeField]
    private float leadDistance = 0f;
    public Vector3 leadVec;
    private Queue<Vector3> posQ = new Queue<Vector3>();
    private Queue<float> deltaQ = new Queue<float>();

    void Update()
    {
        if (followTarget == null) return;

        transform.position = VecUtils.SetZ(followTarget.position + ((leadVec.magnitude < 0.1f) ? Vector3.zero : leadVec), -10);
    }

    private void FixedUpdate()
    {
        if (followTarget == null) return;

        Vector3 targetAverageVelocity = (followTarget.position - posQ.Dequeue()) / deltaQ.Sum();
        Debug.DrawLine(followTarget.position, followTarget.position + targetAverageVelocity, Color.red, 0.1f);
        leadVec += Vector3.ClampMagnitude((targetAverageVelocity - leadVec).normalized * leadSpeed * Time.deltaTime, (targetAverageVelocity - leadVec).magnitude);
        leadVec = Vector3.ClampMagnitude(leadVec, leadDistance);
        posQ.Enqueue(followTarget.position);
        deltaQ.Enqueue(Time.deltaTime);
        deltaQ.Dequeue();
    }

    public void InitLead()
    {
        for (int ii = 0; ii < 20; ii++)
        {
            posQ.Enqueue(followTarget.position);
            deltaQ.Enqueue(0f);
        }
        leadVec = Vector3.zero;
    }
}
