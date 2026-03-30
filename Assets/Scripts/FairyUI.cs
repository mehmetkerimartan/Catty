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
    
    [Header("Aesthetics & Özel Çizimler")]
    [Tooltip("ÖZEL YAZI TİPİ: İnternetten indirdiğin süslü bir fontu (TextMeshPro) sürükleyip bırak!")]
    public TMP_FontAsset customFont;
    
    [Tooltip("YENİ TASARIM: O gri arka plan resmini (Metin Balonunu) buraya sürükle!")]
    public Sprite customBackgroundSprite;
    
    [Header("Arka Plan (Oyun İçinde Test Ederek Ayarla)")]
    public Vector2 backgroundSize = new Vector2(1000f, 2584.5f);
    public Vector2 backgroundOffset = new Vector2(3.57f, 551.44f);
    [Range(0f, 1f)]
    [Tooltip("0 = Tamamen Şeffaf, 1 = Katı (Solid)")]
    public float backgroundOpacity = 1f;

    [Header("Yazılar (Oyun İçinde Test Ederek Ayarla)")]
    public Vector2 textSize = new Vector2(664.9f, 250f);
    public Vector2 textOffset = new Vector2(250f, 78.3f);

    [Header("Baykuş/Melek (Oyun İçinde Test Ederek Ayarla)")]
    public Vector2 portraitSize = new Vector2(954.2f, 2660f);
    public Vector2 portraitPosition = new Vector2(-10.07f, 537.6f);
    
    [Tooltip("Oyun çalışırken Resimleri/Yazıları sıfıra sıfır eşleştirmek için bunu TİKLİ BIRAK ve sayıları değiştir!")]
    public bool updateLayoutInRealtime = true; 
    
    [Header("Portrait Sprites (Dinamik Konuşma)")]
    [Tooltip("Sabit Durma (Kanatları/Elleri Kapalı Hal)")]
    [SerializeField] private Sprite normalExpression;
    [Tooltip("Yazı Akarken (Kanatları/Elleri Açık Konuşma Hali)")]
    [SerializeField] private Sprite talkingExpression;
    [SerializeField] private Sprite worriedExpression;
    [SerializeField] private Sprite scaredExpression;
    
    [Header("Settings")]
    [SerializeField] private float typeSpeed = 0.03f;
    [SerializeField] private float displayDuration = 4f;
    [SerializeField] private float fadeSpeed = 3f;
    
    [Header("Eski Kod Renkleri (Otomatik devredışı kalır)")]
    [SerializeField] private Color normalBgColor = new Color(0.05f, 0.05f, 0.08f, 0.9f); 
    [SerializeField] private Color worriedBgColor = new Color(0.15f, 0.02f, 0.02f, 0.9f); 
    
    private CanvasGroup canvasGroup;
    private bool isShowing = false;
    private Coroutine currentDialogue;
    
    private RectTransform containerRect;
    private RectTransform bgRect;
    private RectTransform myPortraitRect;
    private RectTransform textRect;
    private RectTransform shadowRect;
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

        /* 1. Canvas ve Scaler (TÜM GÖRÜNTÜ BUGLARINI BİTİREN OVERLAY MODU) */
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
        
        // ÖNEMLİ: Oyun dünyasının ARKASINDA veya çok garip yerlerde belirmeyi önler.
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        if (GetComponent<GraphicRaycaster>() == null) gameObject.AddComponent<GraphicRaycaster>();

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler == null) scaler = gameObject.AddComponent<CanvasScaler>();
        
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920); 
        scaler.matchWidthOrHeight = 0.5f;
        
        /* 2. Ana Taşıyıcı Kutu */
        GameObject container = new GameObject("FairyDialogue");
        container.transform.SetParent(transform, false);
        containerRect = container.AddComponent<RectTransform>();
        
        containerRect.anchorMin = new Vector2(0.5f, 0.75f); // Biraz daha aşağı aldık
        containerRect.anchorMax = new Vector2(0.5f, 0.75f); 
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        
        /* 3. Arka Plan Kutusu (Gri Resim) */
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(container.transform, false);
        dialogueBackground = bgObj.AddComponent<Image>();
        
        bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.5f, 0.5f);
        bgRect.anchorMax = new Vector2(0.5f, 0.5f);
        bgRect.pivot = new Vector2(0.5f, 0.5f); 
        
        if (customBackgroundSprite != null)
        {
            dialogueBackground.sprite = customBackgroundSprite;
            dialogueBackground.color = new Color(1f, 1f, 1f, backgroundOpacity); 
            dialogueBackground.preserveAspect = true; // Şişmeyi önle, kutu boyutuna sıkışsın
        }
        else
        {
            dialogueBackground.color = normalBgColor;
            Outline bgOutline = bgObj.AddComponent<Outline>();
            bgOutline.effectColor = new Color(1f, 1f, 1f, 0.3f);
            bgOutline.effectDistance = new Vector2(3, -3);
        }
        
        /* 4. Karakter Portresi (Baykuş / Melek) */
        GameObject portraitObj = new GameObject("Portrait");
        portraitObj.transform.SetParent(container.transform, false);
        portraitImage = portraitObj.AddComponent<Image>();
        portraitImage.color = Color.white;
        portraitImage.preserveAspect = true; 
        
        myPortraitRect = portraitObj.GetComponent<RectTransform>();
        myPortraitRect.anchorMin = new Vector2(0.5f, 0.5f); 
        myPortraitRect.anchorMax = new Vector2(0.5f, 0.5f);
        myPortraitRect.pivot = new Vector2(0.5f, 0.5f); 
        
        if (normalExpression == null)
        {
            Texture2D placeholderTex = CreatePlaceholderPortrait();
            normalExpression = Sprite.Create(placeholderTex, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f));
            worriedExpression = normalExpression;
            scaredExpression = normalExpression;
        }
        portraitImage.sprite = normalExpression;
        
        /* 5. GÖLGE YAZISI */
        GameObject shadowObj = new GameObject("DialogueText_Shadow");
        // Gölgeler artık Ana Container içinde! (Arka plandan tamamen bağımsız haraket edebilir!)
        shadowObj.transform.SetParent(container.transform, false);
        dialogueTextShadow = shadowObj.AddComponent<TextMeshProUGUI>();
        
        dialogueTextShadow.enableAutoSizing = true;
        dialogueTextShadow.fontSizeMin = 14;
        dialogueTextShadow.fontSizeMax = 55;
        dialogueTextShadow.fontSize = 55; 
        
        dialogueTextShadow.color = new Color(0f, 0f, 0f, 0.95f); 
        dialogueTextShadow.alignment = TextAlignmentOptions.MidlineLeft; 
        dialogueTextShadow.enableWordWrapping = true;
        if (customFont != null) dialogueTextShadow.font = customFont; 
        
        shadowRect = shadowObj.GetComponent<RectTransform>();
        shadowRect.anchorMin = new Vector2(0.5f, 0.5f);
        shadowRect.anchorMax = new Vector2(0.5f, 0.5f);
        // PIVOT u Merkezden (0.5), SOLA (0) ÇEKTİK! Artık yazı asla baykuşun tarafına taşamaz!
        shadowRect.pivot = new Vector2(0f, 0.5f);

        /* 6. ASIL KONUŞMA YAZISI (BEYAZ) */
        GameObject textObj = new GameObject("DialogueText");
        textObj.transform.SetParent(container.transform, false);
        dialogueText = textObj.AddComponent<TextMeshProUGUI>();
        
        // Uzun metinlerde yazıyı küçültüp kutuya tıkıştırır
        dialogueText.enableAutoSizing = true;
        dialogueText.fontSizeMin = 14;
        dialogueText.fontSizeMax = 55;
        dialogueText.fontSize = 55; 
        
        dialogueText.color = new Color(0.95f, 0.95f, 0.95f, 1f); 
        dialogueText.alignment = TextAlignmentOptions.MidlineLeft; 
        dialogueText.enableWordWrapping = true;
        if (customFont != null) dialogueText.font = customFont; 
        
        textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        // Aynı şekilde yazının başlangıç duvarını Sola mühürledik.
        textRect.pivot = new Vector2(0f, 0.5f);
        
        // İlk Değer Atamaları (Update içine bırakmadan önce)
        bgRect.sizeDelta = backgroundSize;
        bgRect.anchoredPosition = backgroundOffset;
        textRect.sizeDelta = textSize;
        textRect.anchoredPosition = textOffset;
        shadowRect.sizeDelta = textSize;
        shadowRect.anchoredPosition = textOffset + new Vector2(4, -4);
        myPortraitRect.sizeDelta = portraitSize;
        myPortraitRect.anchoredPosition = portraitPosition;
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
            
            // Oyun oynanırken resimlerin yeri milimetrik test ediliyorsa:
            if (updateLayoutInRealtime)
            {
                if (bgRect != null) { bgRect.sizeDelta = backgroundSize; bgRect.anchoredPosition = backgroundOffset; }
                if (textRect != null) { textRect.sizeDelta = textSize; textRect.anchoredPosition = textOffset; }
                if (shadowRect != null) { shadowRect.sizeDelta = textSize; shadowRect.anchoredPosition = textOffset + new Vector2(4, -4); }
                if (myPortraitRect != null) { myPortraitRect.sizeDelta = portraitSize; myPortraitRect.anchoredPosition = portraitPosition; }
                
                // Opacity / Saydamlık güncelleme
                if (dialogueBackground != null)
                {
                    Color bgColor = dialogueBackground.color;
                    bgColor.a = backgroundOpacity;
                    dialogueBackground.color = bgColor;
                }
            }
        }
    }
    
    public void ShowDialogue(string text, string expression = "normal", float customDuration = -1f)
    {
        if (currentDialogue != null) StopCoroutine(currentDialogue);
        float waitTime = customDuration > 0f ? customDuration : displayDuration;
        currentDialogue = StartCoroutine(ShowDialogueRoutine(text, expression, waitTime));
    }
    
    IEnumerator ShowDialogueRoutine(string text, string expression, float waitTime)
    {
        Sprite idleSprite = normalExpression;
        Sprite talkSprite = (talkingExpression != null) ? talkingExpression : normalExpression;
        
        switch (expression)
        {
            case "worried":
            case "scared":
                if (worriedExpression != null) { idleSprite = worriedExpression; talkSprite = worriedExpression; }
                if (customBackgroundSprite == null) dialogueBackground.color = worriedBgColor;
                break;
            default:
                if (normalExpression != null) idleSprite = normalExpression;
                if (customBackgroundSprite == null) dialogueBackground.color = normalBgColor;
                break;
        }
        
        isShowing = true;
        floatTimer = 0f; 
        
        float t = 0;
        dialogueText.text = ""; 
        if (dialogueTextShadow != null) dialogueTextShadow.text = ""; 
        
        // 1. Ekran açılırken SABİT (Elleri Kapalı) Dursun
        portraitImage.sprite = idleSprite;
        
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
        
        // 2. YAZILAR AKMAYA BAŞLIYOR -> ELLERİ (Kanatları) AÇ!
        if (talkingExpression != null) portraitImage.sprite = talkSprite;
        
        foreach (char c in text)
        {
            dialogueText.text += c;
            if (dialogueTextShadow != null) dialogueTextShadow.text = dialogueText.text;
            yield return new WaitForSecondsRealtime(typeSpeed);
        }
        
        // 3. YAZI BİTTİ (Bekleme Süresi Başladı) -> ELLERİ İNDİR / SABİTLEN
        portraitImage.sprite = idleSprite;
        
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
