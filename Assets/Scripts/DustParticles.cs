using UnityEngine;

/// <summary>
/// Creates floating dust particles in the air for atmosphere.
/// Attach to an empty GameObject or the player.
/// </summary>
public class DustParticles : MonoBehaviour
{
    [Header("Dust Settings")]
    [SerializeField] private int particleCount = 50;
    [SerializeField] private float spawnRadius = 15f;
    [SerializeField] private float particleSize = 0.05f;
    [SerializeField] private float floatSpeed = 0.3f;
    
    [Header("Colors")]
    [SerializeField] private Color heavenDustColor = new Color(1f, 1f, 1f, 0.3f);
    [SerializeField] private Color hellDustColor = new Color(1f, 0.3f, 0.1f, 0.3f);
    
    private ParticleSystem dustSystem;
    
    void Start()
    {
        CreateDustSystem();
    }
    
    void CreateDustSystem()
    {
        GameObject dustObj = new GameObject("DustParticles");
        dustObj.transform.SetParent(transform);
        dustObj.transform.localPosition = Vector3.zero;
        
        dustSystem = dustObj.AddComponent<ParticleSystem>();
        
        var main = dustSystem.main;
        main.loop = true;
        main.startLifetime = 8f;
        main.startSpeed = floatSpeed;
        main.startSize = particleSize;
        main.startColor = heavenDustColor;
        main.maxParticles = particleCount;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        var emission = dustSystem.emission;
        emission.rateOverTime = particleCount / 4f;
        
        var shape = dustSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(spawnRadius * 2, 5f, spawnRadius * 2);
        
        var velocityOverLifetime = dustSystem.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
        velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-0.1f, 0.1f);
        velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-0.1f, 0.1f);
        
        var colorOverLifetime = dustSystem.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.5f, 0.3f), new GradientAlphaKey(0.5f, 0.7f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = grad;
        
        var renderer = dustObj.GetComponent<ParticleSystemRenderer>();
        Material dustMat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        dustMat.color = heavenDustColor;
        renderer.material = dustMat;
    }
    
    void Update()
    {
        if (RealityManager.Instance == null || dustSystem == null) return;
        
        var main = dustSystem.main;
        Color targetColor = RealityManager.Instance.IsRealityTorn ? hellDustColor : heavenDustColor;
        main.startColor = Color.Lerp(main.startColor.color, targetColor, Time.deltaTime * 2f);
    }
}
