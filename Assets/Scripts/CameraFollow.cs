using UnityEngine;

/// <summary>
/// Isometric camera that follows the player smoothly.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    
    [Header("Camera Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 10f, -10f);
    [SerializeField] private float smoothSpeed = 5f;
    
    [Header("Isometric Angle")]
    [SerializeField] private float pitch = 45f;
    [SerializeField] private float yaw = 45f;
    
    void Start()
    {
        /* Set initial rotation for isometric view */
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        
        /* Find player if not assigned */
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        /* Calculate desired position */
        Vector3 desiredPosition = target.position + Quaternion.Euler(0, yaw, 0) * offset;
        
        /* Smoothly move camera */
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
        
        /* Maintain isometric rotation */
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}
