using UnityEngine;

/// <summary>
/// Creates a trail effect behind the cat when moving.
/// Attach to the player cat.
/// </summary>
public class PlayerTrail : MonoBehaviour
{
    [Header("Trail Settings")]
    [SerializeField] private float baseTrailWidth = 0.2f;
    [SerializeField] private float baseTrailTime = 0.3f;
    [SerializeField] private Color heavenTrailColor = new Color(0.5f, 0.8f, 1f, 0.5f);
    [SerializeField] private Color hellTrailColor = new Color(1f, 0.3f, 0.1f, 0.5f);
    
    [Header("Speed Based Trail")]
    [SerializeField] private float maxTrailWidth = 0.5f;
    [SerializeField] private float maxTrailTime = 0.8f;
    [SerializeField] private float walkSpeed = 12f;
    [SerializeField] private float sprintSpeed = 20f;
    
    private TrailRenderer trailRenderer;
    private PlayerController playerController;
    private Vector3 lastPosition;
    
    void Start()
    {
        playerController = GetComponent<PlayerController>();
        lastPosition = transform.position;
        CreateTrail();
    }
    
    void CreateTrail()
    {
        trailRenderer = gameObject.AddComponent<TrailRenderer>();
        trailRenderer.time = baseTrailTime;
        trailRenderer.startWidth = baseTrailWidth;
        trailRenderer.endWidth = 0f;
        trailRenderer.minVertexDistance = 0.1f;
        
        /* Gradient */
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(heavenTrailColor, 0f), new GradientColorKey(heavenTrailColor, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.5f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        trailRenderer.colorGradient = gradient;
        
        /* Material */
        Material trailMat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        trailMat.color = heavenTrailColor;
        trailRenderer.material = trailMat;
        
        trailRenderer.emitting = true;
    }
    
    void Update()
    {
        if (trailRenderer == null) return;
        
        /* Speed-based trail adjustment */
        float currentSpeed = 0f;
        if (playerController != null)
        {
            currentSpeed = playerController.GetCurrentSpeed();
        }
        else
        {
            currentSpeed = (transform.position - lastPosition).magnitude / Time.deltaTime;
        }
        lastPosition = transform.position;
        
        float speedRatio = Mathf.InverseLerp(walkSpeed, sprintSpeed, currentSpeed);
        
        /* Adjust trail based on speed */
        trailRenderer.startWidth = Mathf.Lerp(baseTrailWidth, maxTrailWidth, speedRatio);
        trailRenderer.time = Mathf.Lerp(baseTrailTime, maxTrailTime, speedRatio);
        
        /* Reality-based color */
        if (RealityManager.Instance != null)
        {
            Color targetColor = RealityManager.Instance.IsRealityTorn ? hellTrailColor : heavenTrailColor;
            
            /* Boost opacity when sprinting */
            float alpha = Mathf.Lerp(0.5f, 0.8f, speedRatio);
            
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(targetColor, 0f), new GradientColorKey(targetColor, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0f), new GradientAlphaKey(0f, 1f) }
            );
            trailRenderer.colorGradient = gradient;
        }
    }
}
