using UnityEngine;

/// <summary>
/// Creates a rotating halo effect above the cat's head.
/// Attach to an empty GameObject that is a child of the cat.
/// The halo follows the cat's rotation but stays horizontal.
/// </summary>
public class HaloEffect : MonoBehaviour
{
    [Header("Halo Settings")]
    [SerializeField] private float radius = 0.4f;
    [SerializeField] private float rotationSpeed = 60f;
    [SerializeField] private float thickness = 0.15f;
    [SerializeField] private Vector3 localOffset = new Vector3(0, 1.2f, 0);
    
    [Header("Colors")]
    [SerializeField] private Color normalColor = new Color(1f, 0.9f, 0.5f, 0.9f);
    [SerializeField] private Color tearColor = new Color(0.5f, 0.2f, 0.2f, 0.5f);
    
    [Header("Reality Tear Effects")]
    [SerializeField] private float shakeIntensity = 0.1f;
    [SerializeField] private float flickerSpeed = 10f;
    
    private LineRenderer lineRenderer;
    private int segments = 64;
    private float currentColorBlend = 0f;
    private float rotationAngle = 0f;
    private Transform catTransform;
    
    void Start()
    {
        catTransform = transform.parent;
        if (catTransform == null)
        {
            catTransform = transform;
        }
        CreateHalo();
    }
    
    void CreateHalo()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = true;
        lineRenderer.positionCount = segments;
        lineRenderer.startWidth = thickness;
        lineRenderer.endWidth = thickness;
        
        Material haloMat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        haloMat.color = normalColor;
        lineRenderer.material = haloMat;
    }
    
    void Update()
    {
        if (lineRenderer == null || catTransform == null) return;
        
        rotationAngle += rotationSpeed * Time.deltaTime;
        
        /* Apply offset in cat's local space so it follows rotation */
        Vector3 worldCenter = catTransform.TransformPoint(localOffset);
        
        /* Check Reality Tear state */
        bool isTorn = RealityManager.Instance != null && RealityManager.Instance.IsRealityTorn;
        
        float targetBlend = isTorn ? 1f : 0f;
        currentColorBlend = Mathf.MoveTowards(currentColorBlend, targetBlend, Time.unscaledDeltaTime * 3f);
        
        Color currentColor = Color.Lerp(normalColor, tearColor, currentColorBlend);
        
        float wobble = 0f;
        if (isTorn)
        {
            float flicker = Mathf.Sin(Time.unscaledTime * flickerSpeed) * 0.3f + 0.7f;
            currentColor.a *= flicker;
            wobble = shakeIntensity;
        }
        
        /* Draw circle - always horizontal in world but centered on cat's local offset */
        for (int i = 0; i < segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f + rotationAngle * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            float y = wobble * Mathf.Sin(angle * 2f + Time.time * 3f);
            
            lineRenderer.SetPosition(i, worldCenter + new Vector3(x, y, z));
        }
        
        lineRenderer.startWidth = thickness;
        lineRenderer.endWidth = thickness;
        
        if (lineRenderer.material != null)
        {
            lineRenderer.material.color = currentColor;
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Transform parent = transform.parent != null ? transform.parent : transform;
        Vector3 center = parent.TransformPoint(localOffset);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, radius);
    }
}
