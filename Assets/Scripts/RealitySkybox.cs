using UnityEngine;

/// <summary>
/// Controls skybox color and environment changes during Reality Tear.
/// Attach to any GameObject in the scene.
/// </summary>
public class RealitySkybox : MonoBehaviour
{
    [Header("Skybox Colors")]
    [SerializeField] private Color normalSkyColor = new Color(0.5f, 0.7f, 1f);
    [SerializeField] private Color hellSkyColor = new Color(0.4f, 0.1f, 0.05f);
    
    [Header("Ambient Light")]
    [SerializeField] private Color normalAmbient = new Color(0.5f, 0.5f, 0.5f);
    [SerializeField] private Color hellAmbient = new Color(0.3f, 0.1f, 0.1f);
    
    [Header("Fog")]
    [SerializeField] private bool useFog = true;
    [SerializeField] private Color normalFogColor = new Color(0.7f, 0.8f, 0.9f);
    [SerializeField] private Color hellFogColor = new Color(0.2f, 0.05f, 0.02f);
    [SerializeField] private float normalFogDensity = 0.01f;
    [SerializeField] private float hellFogDensity = 0.03f;
    
    [Header("Transition")]
    [SerializeField] private float transitionSpeed = 3f;
    
    private float currentBlend = 0f;
    private Material skyboxMaterial;
    
    void Start()
    {
        /* Store original skybox or create procedural one */
        if (RenderSettings.skybox != null)
        {
            skyboxMaterial = new Material(RenderSettings.skybox);
            RenderSettings.skybox = skyboxMaterial;
        }
        
        if (useFog)
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
        }
    }
    
    void Update()
    {
        if (RealityManager.Instance == null) return;
        
        float targetBlend = RealityManager.Instance.IsRealityTorn ? 1f : 0f;
        
        /* Smooth transition */
        currentBlend = Mathf.MoveTowards(currentBlend, targetBlend, transitionSpeed * Time.unscaledDeltaTime);
        
        /* Apply skybox tint if possible */
        if (skyboxMaterial != null)
        {
            if (skyboxMaterial.HasProperty("_Tint"))
            {
                skyboxMaterial.SetColor("_Tint", Color.Lerp(normalSkyColor, hellSkyColor, currentBlend));
            }
            else if (skyboxMaterial.HasProperty("_SkyTint"))
            {
                skyboxMaterial.SetColor("_SkyTint", Color.Lerp(normalSkyColor, hellSkyColor, currentBlend));
            }
        }
        
        /* Apply ambient light */
        RenderSettings.ambientLight = Color.Lerp(normalAmbient, hellAmbient, currentBlend);
        
        /* Apply fog */
        if (useFog)
        {
            RenderSettings.fogColor = Color.Lerp(normalFogColor, hellFogColor, currentBlend);
            RenderSettings.fogDensity = Mathf.Lerp(normalFogDensity, hellFogDensity, currentBlend);
        }
    }
    
    void OnDisable()
    {
        /* Reset to normal on disable */
        RenderSettings.ambientLight = normalAmbient;
        if (useFog)
        {
            RenderSettings.fogColor = normalFogColor;
            RenderSettings.fogDensity = normalFogDensity;
        }
    }
}
