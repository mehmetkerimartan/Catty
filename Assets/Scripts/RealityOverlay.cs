using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Creates a full-screen color overlay with smooth fade during Reality Tear.
/// Attach to a UI Canvas.
/// </summary>
public class RealityOverlay : MonoBehaviour
{
    [Header("Overlay Settings")]
    [SerializeField] private Color overlayColor = new Color(0.5f, 0f, 0f, 0.3f);
    [SerializeField] private float fadeSpeed = 3f;
    
    [Header("Vignette")]
    [SerializeField] private bool useVignette = true;
    [SerializeField] private float vignetteIntensity = 0.5f;
    
    private Image overlayImage;
    private Image vignetteImage;
    private float currentAlpha = 0f;
    
    void Start()
    {
        CreateOverlay();
    }
    
    void CreateOverlay()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            gameObject.AddComponent<CanvasScaler>();
        }
        
        /* Create vignette */
        if (useVignette)
        {
            GameObject vignetteObj = new GameObject("Vignette");
            vignetteObj.transform.SetParent(transform, false);
            
            vignetteImage = vignetteObj.AddComponent<Image>();
            vignetteImage.raycastTarget = false;
            
            Texture2D vignetteTex = CreateVignetteTexture(256);
            Sprite vignetteSprite = Sprite.Create(vignetteTex, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f));
            vignetteImage.sprite = vignetteSprite;
            vignetteImage.color = new Color(0f, 0f, 0f, 0f);
            
            RectTransform vignetteRect = vignetteImage.rectTransform;
            vignetteRect.anchorMin = Vector2.zero;
            vignetteRect.anchorMax = Vector2.one;
            vignetteRect.offsetMin = Vector2.zero;
            vignetteRect.offsetMax = Vector2.zero;
        }
        
        /* Create overlay */
        GameObject overlayObj = new GameObject("RealityOverlay");
        overlayObj.transform.SetParent(transform, false);
        
        overlayImage = overlayObj.AddComponent<Image>();
        overlayImage.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, 0f);
        overlayImage.raycastTarget = false;
        
        RectTransform rect = overlayImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
    
    Texture2D CreateVignetteTexture(int size)
    {
        Texture2D tex = new Texture2D(size, size);
        float center = size / 2f;
        float maxDist = center;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float alpha = Mathf.Clamp01((dist / maxDist - 0.3f) / 0.7f);
                alpha = alpha * alpha;
                tex.SetPixel(x, y, new Color(0, 0, 0, alpha));
            }
        }
        tex.Apply();
        return tex;
    }
    
    void Update()
    {
        if (RealityManager.Instance == null) return;
        
        float targetAlpha = RealityManager.Instance.IsRealityTorn ? overlayColor.a : 0f;
        
        /* Simple smooth fade - no pulse */
        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, fadeSpeed * Time.unscaledDeltaTime);
        
        if (overlayImage != null)
        {
            overlayImage.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, currentAlpha);
        }
        
        if (vignetteImage != null)
        {
            vignetteImage.color = new Color(0.2f, 0f, 0f, currentAlpha * vignetteIntensity);
        }
    }
}
