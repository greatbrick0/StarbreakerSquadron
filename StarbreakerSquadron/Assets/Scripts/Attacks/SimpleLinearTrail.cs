using UnityEngine;

public class SimpleLinearTrail : MonoBehaviour
{
    [SerializeField] private SpriteRenderer trailSprite;
    [SerializeField] private float trailTime = 0.05f;
    [SerializeField] private float rotationOffset = 0f;
    [SerializeField] private float trailWidth = 0.1f;

    public void SetColor(Color color)
    {
        if (trailSprite != null)
        {
            trailSprite.color = color;
        }
    }

    public void SetOpacity(float opacity)
    {
        if (trailSprite != null)
        {
            Color c = trailSprite.color;
            c.a = opacity;
            trailSprite.color = c;
        }
    }

    public void UpdateTrail(Vector2 direction, float speed, Vector2 extraVelocity)
    {
        if (trailSprite == null) return;

        Vector2 velocity = (direction * speed) + extraVelocity;
        float currentSpeed = velocity.magnitude;
        float length = currentSpeed * trailTime;

        if (length <= 0.01f)
        {
            trailSprite.gameObject.SetActive(false);
            return;
        }

        trailSprite.gameObject.SetActive(true);
        
        // Scale the trail (assuming the sprite is 1 unit wide)
        Vector3 scale = transform.localScale;
        scale.x = trailWidth;
        scale.y = length;
        transform.localScale = scale;

        // Calculate rotation and position in parent space
        // We MUST use the parent transform to inverse the world velocity, 
        // because this object's own rotation changes during this method.
        Vector2 localVelocity = velocity;
        if (transform.parent != null)
        {
            localVelocity = transform.parent.InverseTransformDirection(velocity);
        }

        float localAngle = (Mathf.Atan2(localVelocity.y, localVelocity.x) * Mathf.Rad2Deg) + rotationOffset;
        transform.localRotation = Quaternion.Euler(0, 0, localAngle);
        transform.localPosition = -localVelocity.normalized * (length * 0.5f);
        }
    
    public void Clear()
    {
        if (trailSprite != null)
        {
            trailSprite.gameObject.SetActive(false);
        }
    }

    public void SetActive(bool active)
    {
        if (trailSprite != null)
        {
            trailSprite.gameObject.SetActive(active);
        }
    }
}
