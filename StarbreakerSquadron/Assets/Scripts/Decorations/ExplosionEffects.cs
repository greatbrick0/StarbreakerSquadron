using UnityEngine;
using Unity.Mathematics;

public class ExplosionEffects : MonoBehaviour
{
    [SerializeField]
    private AudioSource sound;
    [SerializeField]
    private Vector2 soundBounds = Vector2.one * 14;

    [SerializeField]
    private float shakeAmplitude = 50;
    [SerializeField]
    private AnimationCurve shakeCurve;

    private void Start()
    {
        Camera cam = Camera.main;

        float dist = VecUtils.ModifiedDistance(transform.position, cam.transform.position, 4);
        float volume = Mathf.Clamp01(math.remap(soundBounds.x, soundBounds.y, 1, 0, dist));
        sound.volume = volume;
        shakeAmplitude *= volume;
        sound.Play();
        cam.GetComponent<CameraShakeHandler>().ShakeCamera(shakeAmplitude, shakeCurve);
    }
}
