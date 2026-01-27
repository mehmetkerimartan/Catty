using UnityEngine;

/// <summary>
/// Adds pixelation effect to the camera render.
/// Attach to the Main Camera.
/// </summary>
[ExecuteAlways]
public class PixelationEffect : MonoBehaviour
{
    [Header("Pixelation Settings")]
    [SerializeField] private int pixelHeight = 180; /* Lower = more pixelated */
    [SerializeField] private bool isEffectEnabled = true;
    
    private Camera cam;
    private RenderTexture pixelTexture;
    
    void Start()
    {
        cam = GetComponent<Camera>();
    }
    
    void OnPreRender()
    {
        if (!isEffectEnabled || cam == null) return;
        
        int width = (int)(pixelHeight * cam.aspect);
        
        if (pixelTexture == null || pixelTexture.height != pixelHeight)
        {
            if (pixelTexture != null) pixelTexture.Release();
            pixelTexture = new RenderTexture(width, pixelHeight, 24);
            pixelTexture.filterMode = FilterMode.Point; /* No smoothing - sharp pixels */
        }
        
        cam.targetTexture = pixelTexture;
    }
    
    void OnPostRender()
    {
        if (!isEffectEnabled || cam == null) return;
        
        cam.targetTexture = null;
        Graphics.Blit(pixelTexture, null as RenderTexture);
    }
    
    void OnDisable()
    {
        if (pixelTexture != null)
        {
            pixelTexture.Release();
            pixelTexture = null;
        }
    }
}
