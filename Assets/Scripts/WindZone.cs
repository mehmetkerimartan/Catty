using UnityEngine;

/// <summary>
/// Wind zone that pushes the player in a direction.
/// Supports both grounded and airborne effects.
/// </summary>
public class WindZone : MonoBehaviour
{
    [Header("Wind Settings")]
    [SerializeField] private Vector3 windDirection = Vector3.right;
    [SerializeField] private float windForce = 10f;
    [SerializeField] private bool affectsAirborne = true;
    [SerializeField] private float airborneMultiplier = 1.5f;
    
    [Header("Visual")]
    [SerializeField] private ParticleSystem windParticles;
    [SerializeField] private bool showGizmoArrow = true;
    
    private static WindZone currentWindZone;
    
    public static Vector3 CurrentWindForce
    {
        get
        {
            if (currentWindZone == null) return Vector3.zero;
            return currentWindZone.windDirection.normalized * currentWindZone.windForce;
        }
    }
    
    public static float AirborneMultiplier => currentWindZone != null ? currentWindZone.airborneMultiplier : 1f;
    public static bool AffectsAir => currentWindZone != null && currentWindZone.affectsAirborne;
    public static bool IsInWind => currentWindZone != null;
    
    void Start()
    {
        /* Auto-rotate particles to match wind direction */
        if (windParticles != null)
        {
            windParticles.transform.rotation = Quaternion.LookRotation(windDirection);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            currentWindZone = this;
            
            if (windParticles != null && !windParticles.isPlaying)
            {
                windParticles.Play();
            }
            
            Debug.Log("Ruzgar alanina girildi! Yon: " + windDirection + " Guc: " + windForce);
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && currentWindZone == this)
        {
            currentWindZone = null;
            Debug.Log("Ruzgar alanindan cikildi.");
        }
    }
    
    void OnDrawGizmos()
    {
        if (!showGizmoArrow) return;
        
        /* Draw wind zone box */
        Gizmos.color = new Color(0.3f, 0.7f, 1f, 0.3f);
        
        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.matrix = Matrix4x4.identity;
        }
        
        /* Draw wind direction arrow */
        Gizmos.color = Color.cyan;
        Vector3 start = transform.position;
        Vector3 end = start + windDirection.normalized * 3f;
        Gizmos.DrawLine(start, end);
        
        /* Arrow head */
        Vector3 right = Vector3.Cross(windDirection.normalized, Vector3.up).normalized;
        Gizmos.DrawLine(end, end - windDirection.normalized * 0.5f + right * 0.3f);
        Gizmos.DrawLine(end, end - windDirection.normalized * 0.5f - right * 0.3f);
    }
    
    void OnValidate()
    {
        /* Ensure direction is not zero */
        if (windDirection.magnitude < 0.01f)
        {
            windDirection = Vector3.right;
        }
    }
}
