using UnityEngine;

/// <summary>
/// Attach this to objects that have dual appearances (Heaven/Hell).
/// The object should have two child objects: "Heaven" and "Hell".
/// Shaders handle the per-pixel visibility based on tear radius.
/// </summary>
public class DualWorldObject : MonoBehaviour
{
    [Header("World Versions")]
    [Tooltip("The Heaven (fake/illusion) version - visible normally")]
    [SerializeField] private GameObject heavenVersion;
    
    [Tooltip("The Hell (true/reality) version - visible when Reality Tear active")]
    [SerializeField] private GameObject hellVersion;
    
    [Header("Object Type")]
    [SerializeField] private DualWorldType objectType = DualWorldType.Normal;
    
    [Header("Shader Mode")]
    [Tooltip("If true, both versions stay visible and shaders handle per-pixel visibility")]
    [SerializeField] private bool useShaderMode = true;
    
    public enum DualWorldType
    {
        Normal,         /* Has both versions, switches between them */
        GizliYol,       /* Hidden path - always walkable, only visible in Hell mode */
        SahteDuvar      /* Fake wall - never blocks, only visible in Heaven mode */
    }
    
    void Start()
    {
        /* Auto-find children if not assigned */
        if (heavenVersion == null)
        {
            Transform heaven = transform.Find("Heaven");
            if (heaven != null) heavenVersion = heaven.gameObject;
        }
        
        if (hellVersion == null)
        {
            Transform hell = transform.Find("Hell");
            if (hell != null) hellVersion = hell.gameObject;
        }
        
        if (useShaderMode)
        {
            /* Shader mode: Keep both visible, let shaders handle it */
            EnableShaderMode();
        }
        else
        {
            /* Script mode: Start in Heaven */
            SetHellMode(false);
        }
    }
    
    private void EnableShaderMode()
    {
        /* Enable all renderers - shaders will handle per-pixel visibility */
        switch (objectType)
        {
            case DualWorldType.Normal:
                if (heavenVersion != null) SetRenderersEnabled(heavenVersion, true);
                if (hellVersion != null) SetRenderersEnabled(hellVersion, true);
                break;
                
            case DualWorldType.GizliYol:
                /* Hell version with RealityReveal shader - always enabled, shader hides outside radius */
                if (hellVersion != null) SetRenderersEnabled(hellVersion, true);
                break;
                
            case DualWorldType.SahteDuvar:
                /* Heaven version with RealityHide shader - always enabled, shader hides inside radius */
                if (heavenVersion != null) SetRenderersEnabled(heavenVersion, true);
                break;
        }
    }
    
    private void SetRenderersEnabled(GameObject obj, bool enabled)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.enabled = enabled;
        }
    }
    
    /// <summary>
    /// Called by RealityManager - only used if useShaderMode is false.
    /// </summary>
    public void SetHellMode(bool hellMode)
    {
        if (useShaderMode) return; /* Let shaders handle it */
        
        switch (objectType)
        {
            case DualWorldType.Normal:
                if (heavenVersion != null) SetRenderersEnabled(heavenVersion, !hellMode);
                if (hellVersion != null) SetRenderersEnabled(hellVersion, hellMode);
                break;
                
            case DualWorldType.GizliYol:
                if (hellVersion != null) SetRenderersEnabled(hellVersion, hellMode);
                break;
                
            case DualWorldType.SahteDuvar:
                if (heavenVersion != null) SetRenderersEnabled(heavenVersion, !hellMode);
                break;
        }
    }
}
