using UnityEngine;

/// <summary>
/// Smoothly transitions camera background color and fog during Reality Tear.
/// Sets global shader variables for synchronized fog color.
/// </summary>
public class BackgroundTransition : MonoBehaviour
{
    [Header("Colors")]
    [SerializeField] private Color heavenColor = new Color(0.95f, 0.95f, 0.95f);
    [SerializeField] private Color hellColor = new Color(0.1f, 0.02f, 0.02f);
    
    [Header("Transition")]
    [SerializeField] private float transitionSpeed = 3f;
    
    private Camera mainCamera;
    private float currentBlend = 0f;
    
    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = heavenColor;
        }
        
        /* Initialize shader globals */
        Shader.SetGlobalFloat("_RealityBlend", 0f);
        Shader.SetGlobalVector("_FogColorHeaven", new Vector4(heavenColor.r, heavenColor.g, heavenColor.b, 1));
        Shader.SetGlobalVector("_FogColorHell", new Vector4(hellColor.r, hellColor.g, hellColor.b, 1));
    }
    
    void Update()
    {
        if (mainCamera == null || RealityManager.Instance == null) return;
        
        float targetBlend = RealityManager.Instance.IsRealityTorn ? 1f : 0f;
        
        currentBlend = Mathf.MoveTowards(currentBlend, targetBlend, transitionSpeed * Time.unscaledDeltaTime);
        
        /* Update camera background */
        mainCamera.backgroundColor = Color.Lerp(heavenColor, hellColor, currentBlend);
        
        /* Update global shader variables */
        Shader.SetGlobalFloat("_RealityBlend", currentBlend);
        Shader.SetGlobalVector("_FogColorHeaven", new Vector4(heavenColor.r, heavenColor.g, heavenColor.b, 1));
        Shader.SetGlobalVector("_FogColorHell", new Vector4(hellColor.r, hellColor.g, hellColor.b, 1));
    }
}
