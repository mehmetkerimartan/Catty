using UnityEngine;

/// <summary>
/// Spawns particles around the Reality Tear edge.
/// Particles spread only on horizontal plane (XZ).
/// </summary>
public class RealityParticles : MonoBehaviour
{
    [Header("Edge Particles Settings")]
    [SerializeField] private int particleCount = 15;
    [SerializeField] private Color sparkColor = new Color(1f, 0.5f, 0f, 1f);
    [SerializeField] private float particleSpeed = 0.5f;
    [SerializeField] private float particleSize = 0.15f;
    
    private ParticleSystem edgeSparks;
    private ParticleSystem burst;
    private bool wasActive = false;
    
    void Start()
    {
        CreateParticleSystems();
    }
    
    void CreateParticleSystems()
    {
        /* Create Edge Sparks */
        GameObject sparksObj = new GameObject("EdgeSparks");
        sparksObj.transform.SetParent(transform);
        sparksObj.transform.localPosition = Vector3.zero;
        
        edgeSparks = sparksObj.AddComponent<ParticleSystem>();
        var main = edgeSparks.main;
        main.loop = true;
        main.startLifetime = 1.5f;
        main.startSpeed = particleSpeed;
        main.startSize = particleSize;
        main.startColor = sparkColor;
        main.maxParticles = particleCount;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.useUnscaledTime = false;
        
        /* Force Y velocity to 0 for horizontal spread only */
        var velocityOverLifetime = edgeSparks.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
        velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);
        velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(0f, 0f); /* No vertical */
        velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);
        
        var emission = edgeSparks.emission;
        emission.rateOverTime = 0;
        
        var shape = edgeSparks.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 5f;
        shape.radiusThickness = 0f;
        shape.rotation = new Vector3(90, 0, 0); /* Horizontal circle */
        
        var colorOverLifetime = edgeSparks.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(sparkColor, 0f), new GradientColorKey(Color.red, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = grad;
        
        var sizeOverLifetime = edgeSparks.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, 0f);
        
        var renderer = sparksObj.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        renderer.material.color = sparkColor;
        
        edgeSparks.Stop();
        
        /* Create Burst effect - also horizontal */
        GameObject burstObj = new GameObject("Burst");
        burstObj.transform.SetParent(transform);
        burstObj.transform.localPosition = Vector3.zero;
        
        burst = burstObj.AddComponent<ParticleSystem>();
        var burstMain = burst.main;
        burstMain.loop = false;
        burstMain.startLifetime = 1.5f;
        burstMain.startSpeed = 5f;
        burstMain.startSize = 0.2f;
        burstMain.startColor = new Color(1f, 0.6f, 0f, 1f);
        burstMain.maxParticles = 30;
        burstMain.simulationSpace = ParticleSystemSimulationSpace.World;
        burstMain.useUnscaledTime = false;
        
        /* Burst also horizontal only */
        var burstVelocity = burst.velocityOverLifetime;
        burstVelocity.enabled = true;
        burstVelocity.space = ParticleSystemSimulationSpace.World;
        burstVelocity.y = new ParticleSystem.MinMaxCurve(0f, 0f);
        
        var burstEmission = burst.emission;
        burstEmission.rateOverTime = 0;
        burstEmission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 30) });
        
        var burstShape = burst.shape;
        burstShape.shapeType = ParticleSystemShapeType.Circle;
        burstShape.radius = 0.5f;
        burstShape.rotation = new Vector3(90, 0, 0);
        
        var burstColorOverLifetime = burst.colorOverLifetime;
        burstColorOverLifetime.enabled = true;
        Gradient burstGrad = new Gradient();
        burstGrad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.yellow, 0f), new GradientColorKey(Color.red, 0.5f), new GradientColorKey(Color.black, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.5f, 0.5f), new GradientAlphaKey(0f, 1f) }
        );
        burstColorOverLifetime.color = burstGrad;
        
        var burstRenderer = burstObj.GetComponent<ParticleSystemRenderer>();
        burstRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        
        burst.Stop();
    }
    
    void Update()
    {
        if (RealityManager.Instance == null) return;
        
        bool isActive = RealityManager.Instance.IsRealityTorn;
        float radius = RealityManager.Instance.CurrentRadius;
        
        if (edgeSparks != null)
        {
            var shape = edgeSparks.shape;
            shape.radius = Mathf.Max(0.1f, radius);
            
            var emission = edgeSparks.emission;
            emission.rateOverTime = isActive ? particleCount : 0;
            
            if (isActive && !edgeSparks.isPlaying)
            {
                edgeSparks.Play();
            }
            else if (!isActive && radius < 0.5f && edgeSparks.isPlaying)
            {
                edgeSparks.Stop();
            }
        }
        
        if (isActive && !wasActive && burst != null)
        {
            burst.Play();
        }
        
        wasActive = isActive;
    }
}
