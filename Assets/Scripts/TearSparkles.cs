using UnityEngine;

/// <summary>
/// Creates sparkle effects at the Reality Tear edge.
/// Attach to the player (same as RealityManager).
/// </summary>
public class TearSparkles : MonoBehaviour
{
    [Header("Sparkle Settings")]
    [SerializeField] private int sparkleCount = 20;
    [SerializeField] private float sparkleSize = 0.1f;
    [SerializeField] private Color sparkleColor = new Color(1f, 0.8f, 0.4f, 1f);
    
    private ParticleSystem sparkleSystem;
    
    void Start()
    {
        CreateSparkleSystem();
    }
    
    void CreateSparkleSystem()
    {
        GameObject sparkleObj = new GameObject("TearSparkles");
        sparkleObj.transform.SetParent(transform);
        sparkleObj.transform.localPosition = Vector3.zero;
        
        sparkleSystem = sparkleObj.AddComponent<ParticleSystem>();
        
        var main = sparkleSystem.main;
        main.loop = true;
        main.startLifetime = 0.5f;
        main.startSpeed = 2f;
        main.startSize = sparkleSize;
        main.startColor = sparkleColor;
        main.maxParticles = sparkleCount;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        var emission = sparkleSystem.emission;
        emission.rateOverTime = 0;
        
        var shape = sparkleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 5f;
        shape.radiusThickness = 0f;
        shape.rotation = new Vector3(90, 0, 0);
        
        var colorOverLifetime = sparkleSystem.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(sparkleColor, 0f), new GradientColorKey(Color.white, 0.5f), new GradientColorKey(sparkleColor, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = grad;
        
        var sizeOverLifetime = sparkleSystem.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0f);
        sizeCurve.AddKey(0.2f, 1f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        var renderer = sparkleObj.GetComponent<ParticleSystemRenderer>();
        Material sparkleMat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        sparkleMat.color = sparkleColor;
        renderer.material = sparkleMat;
        
        sparkleSystem.Stop();
    }
    
    void Update()
    {
        if (RealityManager.Instance == null || sparkleSystem == null) return;
        
        bool isActive = RealityManager.Instance.IsRealityTorn;
        float radius = RealityManager.Instance.CurrentRadius;
        
        var shape = sparkleSystem.shape;
        shape.radius = Mathf.Max(0.1f, radius);
        
        var emission = sparkleSystem.emission;
        emission.rateOverTime = isActive ? sparkleCount : 0;
        
        if (isActive && !sparkleSystem.isPlaying)
        {
            sparkleSystem.Play();
        }
        else if (!isActive && radius < 0.5f && sparkleSystem.isPlaying)
        {
            sparkleSystem.Stop();
        }
    }
}
