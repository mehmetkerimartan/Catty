using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Controls post-processing effects during Reality Tear.
/// Requires a Volume component with overrides on the same GameObject.
/// </summary>
[RequireComponent(typeof(Volume))]
public class RealityPostProcessing : MonoBehaviour
{
    [Header("Effect Intensities")]
    [SerializeField] private float maxChromaticAberration = 0.5f;
    [SerializeField] private float maxVignette = 0.4f;
    [SerializeField] private float normalBloom = 1f;
    [SerializeField] private float maxBloom = 3f;
    
    [Header("Transition")]
    [SerializeField] private float transitionSpeed = 5f;
    
    private Volume volume;
    private ChromaticAberration chromaticAberration;
    private Vignette vignette;
    private Bloom bloom;
    
    private float targetIntensity = 0f;
    private float currentIntensity = 0f;
    
    void Start()
    {
        volume = GetComponent<Volume>();
        
        /* Try to get or add overrides */
        if (volume.profile == null)
        {
            volume.profile = ScriptableObject.CreateInstance<VolumeProfile>();
        }
        
        /* Get or add Chromatic Aberration */
        if (!volume.profile.TryGet(out chromaticAberration))
        {
            chromaticAberration = volume.profile.Add<ChromaticAberration>(true);
        }
        chromaticAberration.intensity.overrideState = true;
        
        /* Get or add Vignette */
        if (!volume.profile.TryGet(out vignette))
        {
            vignette = volume.profile.Add<Vignette>(true);
        }
        vignette.intensity.overrideState = true;
        
        /* Get or add Bloom */
        if (!volume.profile.TryGet(out bloom))
        {
            bloom = volume.profile.Add<Bloom>(true);
        }
        bloom.intensity.overrideState = true;
    }
    
    void Update()
    {
        if (RealityManager.Instance == null) return;
        
        /* Target intensity based on tear state */
        targetIntensity = RealityManager.Instance.IsRealityTorn ? 1f : 0f;
        
        /* Smooth transition */
        currentIntensity = Mathf.MoveTowards(currentIntensity, targetIntensity, transitionSpeed * Time.unscaledDeltaTime);
        
        /* Apply effects */
        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = currentIntensity * maxChromaticAberration;
        }
        
        if (vignette != null)
        {
            vignette.intensity.value = currentIntensity * maxVignette;
            vignette.color.value = Color.Lerp(Color.black, new Color(0.3f, 0f, 0f), currentIntensity);
        }
        
        if (bloom != null)
        {
            bloom.intensity.value = Mathf.Lerp(normalBloom, maxBloom, currentIntensity);
        }
    }
}
