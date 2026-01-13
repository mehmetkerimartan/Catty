using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Hades-style fairy UI with portrait and dialogue balloon.
/// Attach to a UI Canvas.
/// </summary>
public class FairyUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image portraitImage;
    [SerializeField] private Image dialogueBackground;
    [SerializeField] private TextMeshProUGUI dialogueText;
    
    [Header("Portrait Sprites")]
    [SerializeField] private Sprite normalExpression;
    [SerializeField] private Sprite worriedExpression;
    [SerializeField] private Sprite scaredExpression;
    
    [Header("Settings")]
    [SerializeField] private float typeSpeed = 0.03f;
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private float fadeSpeed = 2f;
    
    [Header("Colors")]
    [SerializeField] private Color normalBgColor = new Color(0.2f, 0.3f, 0.5f, 0.9f);
    [SerializeField] private Color worriedBgColor = new Color(0.5f, 0.3f, 0.2f, 0.9f);
    
    private CanvasGroup canvasGroup;
    private bool isShowing = false;
    private Coroutine currentDialogue;
    private bool wasRealityTorn = false;
    
    /* Singleton for easy access */
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
        /* Create canvas group for fading */
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            gameObject.AddComponent<CanvasScaler>();
            gameObject.AddComponent<GraphicRaycaster>();
        }
        
        /* Create container */
        GameObject container = new GameObject("FairyDialogue");
        container.transform.SetParent(transform, false);
        
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 0);
        containerRect.anchorMax = new Vector2(0, 0);
        containerRect.pivot = new Vector2(0, 0);
        containerRect.anchoredPosition = new Vector2(20, 20);
        containerRect.sizeDelta = new Vector2(400, 120);
        
        /* Dialogue background */
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(container.transform, false);
        dialogueBackground = bgObj.AddComponent<Image>();
        dialogueBackground.color = normalBgColor;
        
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = new Vector2(100, 0);
        bgRect.offsetMax = Vector2.zero;
        
        /* Portrait */
        GameObject portraitObj = new GameObject("Portrait");
        portraitObj.transform.SetParent(container.transform, false);
        portraitImage = portraitObj.AddComponent<Image>();
        portraitImage.color = Color.white;
        
        RectTransform portraitRect = portraitObj.GetComponent<RectTransform>();
        portraitRect.anchorMin = new Vector2(0, 0);
        portraitRect.anchorMax = new Vector2(0, 1);
        portraitRect.pivot = new Vector2(0, 0.5f);
        portraitRect.anchoredPosition = Vector2.zero;
        portraitRect.sizeDelta = new Vector2(100, 100);
        
        /* Create placeholder portrait */
        Texture2D placeholderTex = CreatePlaceholderPortrait();
        normalExpression = Sprite.Create(placeholderTex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
        worriedExpression = normalExpression;
        scaredExpression = normalExpression;
        portraitImage.sprite = normalExpression;
        
        /* Dialogue text */
        GameObject textObj = new GameObject("DialogueText");
        textObj.transform.SetParent(bgObj.transform, false);
        dialogueText = textObj.AddComponent<TextMeshProUGUI>();
        dialogueText.fontSize = 18;
        dialogueText.color = Color.white;
        dialogueText.alignment = TextAlignmentOptions.Left;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 10);
        textRect.offsetMax = new Vector2(-10, -10);
    }
    
    Texture2D CreatePlaceholderPortrait()
    {
        /* Simple glowing orb placeholder */
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        float center = size / 2f;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float intensity = 1f - Mathf.Clamp01(dist / (size * 0.4f));
                intensity = intensity * intensity;
                Color col = new Color(0.8f, 0.9f, 1f, intensity);
                tex.SetPixel(x, y, col);
            }
        }
        tex.Apply();
        return tex;
    }
    
    void Update()
    {
        if (RealityManager.Instance == null) return;
        
        bool isTorn = RealityManager.Instance.IsRealityTorn;
        
        /* Trigger dialogue on Reality Tear start */
        if (isTorn && !wasRealityTorn)
        {
            ShowDialogue("Dikkat! Gerçeği çok uzun görme...", "worried");
        }
        else if (!isTorn && wasRealityTorn)
        {
            ShowDialogue("İyi... Güvendesin.", "normal");
        }
        
        wasRealityTorn = isTorn;
    }
    
    public void ShowDialogue(string text, string expression = "normal")
    {
        if (currentDialogue != null)
        {
            StopCoroutine(currentDialogue);
        }
        currentDialogue = StartCoroutine(ShowDialogueRoutine(text, expression));
    }
    
    IEnumerator ShowDialogueRoutine(string text, string expression)
    {
        /* Set expression */
        switch (expression)
        {
            case "worried":
                if (worriedExpression != null) portraitImage.sprite = worriedExpression;
                dialogueBackground.color = worriedBgColor;
                break;
            case "scared":
                if (scaredExpression != null) portraitImage.sprite = scaredExpression;
                dialogueBackground.color = worriedBgColor;
                break;
            default:
                if (normalExpression != null) portraitImage.sprite = normalExpression;
                dialogueBackground.color = normalBgColor;
                break;
        }
        
        /* Fade in */
        isShowing = true;
        while (canvasGroup.alpha < 1f)
        {
            canvasGroup.alpha += fadeSpeed * Time.unscaledDeltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1f;
        
        /* Type text */
        dialogueText.text = "";
        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSecondsRealtime(typeSpeed);
        }
        
        /* Wait */
        yield return new WaitForSecondsRealtime(displayDuration);
        
        /* Fade out */
        while (canvasGroup.alpha > 0f)
        {
            canvasGroup.alpha -= fadeSpeed * Time.unscaledDeltaTime;
            yield return null;
        }
        canvasGroup.alpha = 0f;
        isShowing = false;
    }
    
    public void Hide()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        isShowing = false;
    }
}
