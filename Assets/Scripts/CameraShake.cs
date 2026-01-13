using UnityEngine;

/// <summary>
/// Adds camera shake effects on events like jumping and Reality Tear.
/// Attach to the Main Camera.
/// </summary>
public class CameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float tearShakeIntensity = 0.03f;
    [SerializeField] private float jumpShakeIntensity = 0.02f;
    [SerializeField] private float shakeDuration = 0.15f;
    [SerializeField] private float shakeFrequency = 15f;
    
    private float currentShake = 0f;
    private float currentDuration = 0f;
    private float maxDuration = 0f;
    private bool wasRealityTorn = false;
    private Vector3 shakeOffset = Vector3.zero;
    
    public static CameraShake Instance { get; private set; }
    
    void Awake()
    {
        Instance = this;
    }
    
    void LateUpdate()
    {
        /* Auto shake on Reality Tear toggle */
        if (RealityManager.Instance != null)
        {
            bool isTorn = RealityManager.Instance.IsRealityTorn;
            if (isTorn != wasRealityTorn)
            {
                Shake(tearShakeIntensity, shakeDuration);
            }
            wasRealityTorn = isTorn;
        }
        
        /* Calculate shake offset (additive, doesn't change base position) */
        if (currentDuration > 0)
        {
            currentDuration -= Time.unscaledDeltaTime;
            
            float x = (Mathf.PerlinNoise(Time.unscaledTime * shakeFrequency, 0) - 0.5f) * 2f;
            float y = (Mathf.PerlinNoise(0, Time.unscaledTime * shakeFrequency) - 0.5f) * 2f;
            
            float decay = currentDuration / maxDuration;
            shakeOffset = new Vector3(x, y, 0) * currentShake * decay;
            
            /* Apply offset additively */
            transform.localPosition += shakeOffset;
        }
    }
    
    public void Shake(float intensity, float duration)
    {
        currentShake = intensity;
        currentDuration = duration;
        maxDuration = duration;
    }
    
    public void ShakeOnJump()
    {
        Shake(jumpShakeIntensity, 0.1f);
    }
}
