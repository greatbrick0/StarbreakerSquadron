using UnityEngine;

public class CameraShakeHandler : MonoBehaviour
{
    public delegate void CameraShook(float amplitude, AnimationCurve curve);
    public CameraShook cameraShook;

    public void ShakeCamera(float amplitude, AnimationCurve yCurve)
    {
        if(cameraShook != null) cameraShook(amplitude, yCurve);
    }
}
