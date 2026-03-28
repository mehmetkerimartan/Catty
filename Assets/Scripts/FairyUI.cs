using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Mükemmel responsive ve mobil uyumlu Fairy UI.
/// Ekrandan taşmaları önler ve profesyonel bir görünüm sunar.
/// Artık Tutorial sistemi için kullanılıyor!
/// </summary>
public class FairyUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image portraitImage;
    [SerializeField] private Image dialogueBackground;
    [SerializeField] private TextMeshProUGUI dialogueText;
    private TextMeshProUGUI dialogueTextShadow; 
    
    [Header("Aesthetics & Fonts")]
    [Tooltip("ÖZEL YAZI TİPİ: İnternetten indirdiğin süslü bir fontu (TextMeshPro uyumlu) sürükleyip buraya bırakabilirsin!")]
    public TMP_FontAsset customFont;
    
    [Header("Portrait Sprites")]
    [SerializeField] private Sprite normalExpression;
    [SerializeField] private Sprite worriedExpression;
    [SerializeField] private Sprite scaredExpression;
    
    [Header("Settings")]
    [SerializeField] private float typeSpeed = 0.03f;
    [SerializeField] private float displayDuration = 4f;
    [SerializeField] private float fadeSpeed = 3f;
    
    [Header("Colors & Aesthetics")]
    [SerializeField] private Color normalBgColor = new Color(0.05f, 0.05f, 0.08f, 0.9f); 
    [SerializeField] private Color worriedBgColor = new Color(0.15f, 0.02f, 0.02f, 0.9f); 
    
    private CanvasGroup canvasGroup;
    private bool isShowing = false;
    private Coroutine currentDialogue;
    
    private RectTransform containerRect;
    private float floatTimer = 0f;

    public static FairyUI Instance { get; private set; }
    
    void Awake()
    {
        Instance = this;
    }
    
    void Start()
    {
        CreateUI();
        Hide();
    }
    
    void CreateUI()
    {
        /* 0. Temizlik */
        Transform oldUI = transform.Find("FairyDialogue");
        if (oldUI != null) Destroy(oldUI.gameObject);

        /* 1. Canvas ve Scaler */
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            gameObject.AddComponent<GraphicRaycaster>();
        }

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler == null) scaler = gameObject.AddComponent<CanvasScaler>();
        
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920); 
        scaler.matchWidthOrHeight = 0.5f;
        
        /* 2. Ana Taşıyıcı Kutu */
        GameObject container = new GameObject("FairyDialogue");
        container.transform.SetParent(transform, false);
        containerRect = container.AddComponent<RectTransform>();
        
        // Zıplama ve yürüme tuşlarıyla (alt kısım) üst üste binmemesi için EKRANIN ÜSTÜNE alındı
        containerRect.anchorMin = new Vector2(0.05f, 0.82f); 
        containerRect.anchorMax = new Vector2(0.95f, 0.95f); 
        containerRect.offsetMin = Vector2.zero;
        containerRect.offsetMax = Vector2.zero;
        
        /* 3. Arka Plan Kutusu */
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(container.transform, false);
        dialogueBackground = bgObj.AddComponent<Image>();
        dialogueBackground.color = normalBgColor;
        
        Outline bgOutline = bgObj.AddComponent<Outline>();
        bgOutline.effectColor = new Color(1f, 1f, 1f, 0.3f);
        bgOutline.effectDistance = new Vector2(3, -3);
        
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = new Vector2(250, 0); 
        bgRect.offsetMax = Vector2.zero;
        
        /* 4. Karakter Portresi */
        GameObject portraitObj = new GameObject("Portrait");
        portraitObj.transform.SetParent(container.transform, false);
        portraitImage = portraitObj.AddComponent<Image>();
        portraitImage.color = Color.white;
        
        RectTransform portraitRect = portraitObj.GetComponent<RectTransform>();
        portraitRect.anchorMin = new Vector2(0, 0.5f);
        portraitRect.anchorMax = new Vector2(0, 0.5f);
        portraitRect.pivot = new Vector2(0, 0.5f);
        portraitRect.sizeDelta = new Vector2(350, 350); 
        portraitRect.anchoredPosition = new Vector2(-50, 20); 
        
        Texture2D placeholderTex = CreatePlaceholderPortrait();
        normalExpression = Sprite.Create(placeholderTex, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f));
        worriedExpression = normalExpression;
        scaredExpression = normalExpression;
        portraitImage.sprite = normalExpression;
        
        /* 5. GÖLGE YAZISI */
        GameObject shadowObj = new GameObject("DialogueText_Shadow");
        shadowObj.transform.SetParent(bgObj.transform, false);
        dialogueTextShadow = shadowObj.AddComponent<TextMeshProUGUI>();
        
        dialogueTextShadow.enableAutoSizing = true;
        dialogueTextShadow.fontSizeMin = 20;
        dialogueTextShadow.fontSizeMax = 50;
        dialogueTextShadow.fontSize = 50; 
        
        dialogueTextShadow.color = new Color(0f, 0f, 0f, 0.95f); 
        dialogueTextShadow.alignment = TextAlignmentOptions.TopLeft;
        dialogueTextShadow.enableWordWrapping = true;
        if (customFont != null) dialogueTextShadow.font = customFont;
        
        RectTransform shadowRect = shadowObj.GetComponent<RectTransform>();
        shadowRect.anchorMin = Vector2.zero;
        shadowRect.anchorMax = Vector2.one;
        shadowRect.offsetMin = new Vector2(44, 26); 
        shadowRect.offsetMax = new Vector2(-36, -34);

        /* 6. ASIL KONUŞMA YAZISI (BEYAZ) */
        GameObject textObj = new GameObject("DialogueText");
        textObj.transform.SetParent(bgObj.transform, false);
        dialogueText = textObj.AddComponent<TextMeshProUGUI>();
        
        dialogueText.enableAutoSizing = true;
        dialogueText.fontSizeMin = 20;
        dialogueText.fontSizeMax = 50;
        dialogueText.fontSize = 50; 
        
        dialogueText.color = new Color(0.95f, 0.95f, 0.95f, 1f); 
        dialogueText.alignment = TextAlignmentOptions.TopLeft; 
        dialogueText.enableWordWrapping = true;
        if (customFont != null) dialogueText.font = customFont;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(40, 30); 
        textRect.offsetMax = new Vector2(-40, -30);
    }
    
    Texture2D CreatePlaceholderPortrait()
    {
        int size = 128;
        Texture2D tex = new Texture2D(size, size);
        float center = size / 2f;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float intensity = 1f - Mathf.Clamp01(dist / (size * 0.45f));
                intensity = Mathf.Pow(intensity, 2f);
                Color col = new Color(0.8f, 0.9f, 1f, intensity * 0.5f); 
                tex.SetPixel(x, y, col);
            }
        }
        tex.Apply();
        return tex;
    }
    
    void Update()
    {
        if (isShowing && containerRect != null)
        {
            floatTimer += Time.unscaledDeltaTime;
            float floatOffset = Mathf.Sin(floatTimer * 3f) * 15f; 
            containerRect.anchoredPosition = new Vector2(0, floatOffset);
        }
        // * RealityManager entegrasyonu TEMA yüzünden tamamen silindi.
    }
    
    public void ShowDialogue(string text, string expression = "normal", float customDuration = -1f)
    {
        if (currentDialogue != null) StopCoroutine(currentDialogue);
        float waitTime = customDuration > 0f ? customDuration : displayDuration;
        currentDialogue = StartCoroutine(ShowDialogueRoutine(text, expression, waitTime));
    }
    
    IEnumerator ShowDialogueRoutine(string text, string expression, float waitTime)
    {
        switch (expression)
        {
            case "worried":
            case "scared":
                if (worriedExpression != null) portraitImage.sprite = worriedExpression;
                dialogueBackground.color = worriedBgColor;
                break;
            default:
                if (normalExpression != null) portraitImage.sprite = normalExpression;
                dialogueBackground.color = normalBgColor;
                break;
        }
        
        isShowing = true;
        floatTimer = 0f; 
        
        float t = 0;
        dialogueText.text = ""; 
        if (dialogueTextShadow != null) dialogueTextShadow.text = ""; 
        
        while (t < 1f)
        {
            t += fadeSpeed * Time.unscaledDeltaTime;
            float easedT = Mathf.Clamp01(t);
            
            canvasGroup.alpha = easedT;
            
            float scale = Mathf.Lerp(0.8f, 1.05f, easedT);
            if (easedT > 0.8f) scale = Mathf.Lerp(1.05f, 1f, (easedT - 0.8f) * 5f);
            
            containerRect.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }
        canvasGroup.alpha = 1f;
        containerRect.localScale = Vector3.one;
        
        foreach (char c in text)
        {
            dialogueText.text += c;
            if (dialogueTextShadow != null) dialogueTextShadow.text = dialogueText.text;
            yield return new WaitForSecondsRealtime(typeSpeed);
        }
        
        yield return new WaitForSecondsRealtime(waitTime);
        
        t = 1f;
        while (t > 0f)
        {
            t -= fadeSpeed * Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Clamp01(t);
            float scale = Mathf.Lerp(0.9f, 1f, t);
            containerRect.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        isShowing = false;
    }
    
    public void Hide()
    {
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        isShowing = false;
    }
}
