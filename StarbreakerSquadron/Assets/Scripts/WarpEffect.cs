using System.Collections;
using UnityEngine;

public class WarpEffect : MonoBehaviour
{
    public bool isServer = false;
    private Material mat;
    private const string radiusProperty = "_Radius";
    [SerializeField]
    private AnimationCurve radiusCurve;
    private float animDuration = 1.0f;

    public delegate void WarpCallback();
    public WarpCallback warpCallback;

    public void Initialize(bool newIsServer, float newDuration)
    {
        isServer = newIsServer;
        animDuration = newDuration;
        if (!isServer)
        {
            mat = GetComponent<SpriteRenderer>().material;
            mat.SetFloat(radiusProperty, 0.0f);
        }
        else
        {
            GetComponent<SpriteRenderer>().enabled = false;
        }
        StartCoroutine(Progress());
    }

    private IEnumerator Progress()
    {
        //grow
        float animTime = 0.0f;
        while(animTime < animDuration)
        {
            if (!isServer)
            {
                mat.SetFloat(radiusProperty, radiusCurve.Evaluate(animTime/animDuration));
            }
            animTime += Time.deltaTime;
            yield return null;
        }

        //middle
        animTime = 0.0f;
        if (warpCallback != null) warpCallback();

        //shrink
        while (animTime < animDuration)
        {
            if (!isServer)
            {
                mat.SetFloat(radiusProperty, radiusCurve.Evaluate(1.0f - (animTime / animDuration)));
            }
            animTime += Time.deltaTime;
            yield return null;
        }

        //end
        Destroy(mat);
        Destroy(gameObject);
    }
}
