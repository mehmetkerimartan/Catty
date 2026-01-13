using UnityEngine;

/// <summary>
/// Creates a trail effect behind the cat when moving.
/// Attach to the player cat.
/// </summary>
public class PlayerTrail : MonoBehaviour
{
    [Header("Trail Settings")]
    [SerializeField] private float trailWidth = 0.2f;
    [SerializeField] private float trailTime = 0.3f;
    [SerializeField] private Color heavenTrailColor = new Color(0.5f, 0.8f, 1f, 0.5f);
    [SerializeField] private Color hellTrailColor = new Color(1f, 0.3f, 0.1f, 0.5f);
    
    private TrailRenderer trailRenderer;
    
    void Start()
    {
        CreateTrail();
    }
    
    void CreateTrail()
    {
        trailRenderer = gameObject.AddComponent<TrailRenderer>();
        trailRenderer.time = trailTime;
        trailRenderer.startWidth = trailWidth;
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
        if (RealityManager.Instance == null || trailRenderer == null) return;
        
        Color targetColor = RealityManager.Instance.IsRealityTorn ? hellTrailColor : heavenTrailColor;
        
        /* Update gradient */
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(targetColor, 0f), new GradientColorKey(targetColor, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.5f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        trailRenderer.colorGradient = gradient;
    }
}
