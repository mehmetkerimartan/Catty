using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Ice ground zone that makes player slide with reduced friction.
/// Uses trigger-based detection for performance.
/// Supports multiple overlapping zones with stack system.
/// </summary>
public class IceGround : MonoBehaviour
{
    [Header("Ice Settings")]
    [SerializeField] private float frictionMultiplier = 0.15f;
    [SerializeField] private float slideAcceleration = 2f;
    [SerializeField] private float maxSlideSpeed = 25f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip slideSound;
    
    /* Optimization: Stack system for multiple overlapping ice zones */
    private static Stack<IceGround> iceZoneStack = new Stack<IceGround>();
    
    public static float CurrentFriction => Current != null ? Current.frictionMultiplier : 1f;
    public static float CurrentSlideAccel => Current != null ? Current.slideAcceleration : 0f;
    public static float CurrentMaxSlide => Current != null ? Current.maxSlideSpeed : 0f;
    public static bool IsOnIce => iceZoneStack.Count > 0;
    private static IceGround Current => iceZoneStack.Count > 0 ? iceZoneStack.Peek() : null;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            iceZoneStack.Push(this);
            Debug.Log("Buz zeminine girildi! Surtunme: " + frictionMultiplier + " (Stack: " + iceZoneStack.Count + ")");
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            /* Remove this zone from stack if it's there */
            if (iceZoneStack.Count > 0 && iceZoneStack.Peek() == this)
            {
                iceZoneStack.Pop();
                Debug.Log("Buz zemininden cikildi. (Stack: " + iceZoneStack.Count + ")");
            }
            else
            {
                /* Edge case: remove from middle of stack (shouldn't happen normally) */
                var tempStack = new Stack<IceGround>();
                while (iceZoneStack.Count > 0)
                {
                    var zone = iceZoneStack.Pop();
                    if (zone != this) tempStack.Push(zone);
                }
                while (tempStack.Count > 0)
                {
                    iceZoneStack.Push(tempStack.Pop());
                }
            }
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
