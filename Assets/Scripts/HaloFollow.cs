using UnityEngine;

/// <summary>
/// Makes the halo follow the cat's vertical head movement.
/// </summary>
public class HaloFollow : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Assign the cat's head bone or main transform")]
    public Transform target;
    
    [Header("Settings")]
    public float heightOffset = 0.5f;
    public float smoothSpeed = 10f;
    
    private Vector3 startLocalPos;
    
    void Start()
    {
        startLocalPos = transform.localPosition;
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        // Only follow Y position changes
        Vector3 newPos = transform.localPosition;
        float targetY = startLocalPos.y + (target.localPosition.y - target.parent.localPosition.y);
        newPos.y = Mathf.Lerp(transform.localPosition.y, startLocalPos.y, smoothSpeed * Time.deltaTime);
        
        // Just match target world position + offset
        transform.position = target.position + Vector3.up * heightOffset;
    }
}
