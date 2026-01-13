using UnityEngine;

/// <summary>
/// Ice ground zone that makes player slide with reduced friction.
/// Uses trigger-based detection for performance.
/// </summary>
public class IceGround : MonoBehaviour
{
    [Header("Ice Settings")]
    [SerializeField] private float frictionMultiplier = 0.15f;
    [SerializeField] private float slideAcceleration = 2f;
    [SerializeField] private float maxSlideSpeed = 25f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip slideSound;
    
    private static IceGround currentIceZone;
    
    public static float CurrentFriction => currentIceZone != null ? currentIceZone.frictionMultiplier : 1f;
    public static float CurrentSlideAccel => currentIceZone != null ? currentIceZone.slideAcceleration : 0f;
    public static float CurrentMaxSlide => currentIceZone != null ? currentIceZone.maxSlideSpeed : 0f;
    public static bool IsOnIce => currentIceZone != null;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            currentIceZone = this;
            Debug.Log("Buz zeminine girildi! Surtunme: " + frictionMultiplier);
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && currentIceZone == this)
        {
            currentIceZone = null;
            Debug.Log("Buz zemininden cikildi.");
        }
    }
    
    void OnDrawGizmos()
    {
        /* Draw ice zone indicator */
        Gizmos.color = new Color(0.5f, 0.8f, 1f, 0.3f);
        
        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
        }
    }
}
