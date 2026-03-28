using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Wind zone that pushes the player in a direction.
/// Supports both grounded and airborne effects.
/// Supports multiple overlapping zones with stack system.
/// </summary>
public class CustomWindZone : MonoBehaviour
{
    [Header("Wind Settings")]
    [SerializeField] private Vector3 windDirection = Vector3.right;
    [SerializeField] private float windForce = 10f;
    [SerializeField] private bool affectsAirborne = true;
    [SerializeField] private float airborneMultiplier = 1.5f;
    
    [Header("Visual")]
    [SerializeField] private ParticleSystem windParticles;
    [SerializeField] private bool showGizmoArrow = true;
    
    /* Optimization: Stack system for multiple overlapping wind zones */
    private static Stack<CustomWindZone> windZoneStack = new Stack<CustomWindZone>();
    
    public static Vector3 CurrentWindForce
    {
        get
        {
            if (windZoneStack.Count == 0) return Vector3.zero;
            
            /* Combine all wind zones in stack */
            Vector3 combinedWind = Vector3.zero;
            foreach (var zone in windZoneStack)
            {
                combinedWind += zone.windDirection.normalized * zone.windForce;
            }
            return combinedWind;
        }
    }
    
    public static float AirborneMultiplier => Current != null ? Current.airborneMultiplier : 1f;
    public static bool AffectsAir => Current != null && Current.affectsAirborne;
    public static bool IsInWind => windZoneStack.Count > 0;
    private static CustomWindZone Current => windZoneStack.Count > 0 ? windZoneStack.Peek() : null;
    
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
            windZoneStack.Push(this);
            
            if (windParticles != null && !windParticles.isPlaying)
            {
                windParticles.Play();
            }
            
            Debug.Log("Ruzgar alanina girildi! Yon: " + windDirection + " Guc: " + windForce + " (Stack: " + windZoneStack.Count + ")");
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            /* Remove this zone from stack if it's on top */
            if (windZoneStack.Count > 0 && windZoneStack.Peek() == this)
            {
                windZoneStack.Pop();
                Debug.Log("Ruzgar alanindan cikildi. (Stack: " + windZoneStack.Count + ")");
            }
            else
            {
                /* Edge case: remove from middle of stack */
                var tempStack = new Stack<CustomWindZone>();
                while (windZoneStack.Count > 0)
                {
                    var zone = windZoneStack.Pop();
                    if (zone != this) tempStack.Push(zone);
                }
                while (tempStack.Count > 0)
                {
                    windZoneStack.Push(tempStack.Pop());
                }
            }
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
