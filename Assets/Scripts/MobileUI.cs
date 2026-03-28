using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// Auto-generates a canvas with a Mobile Joystick and 3 Action Buttons.
/// PROGRESSIVE UNLOCK: Tuşlar kapalı başlar ve sonradan açılarak kalıcı olarak kaydedilir!
/// </summary>
public class MobileUI : MonoBehaviour
{
    public static MobileUI Instance { get; private set; }

    private RectTransform joystickHandle;
    private float joystickRadius = 150f;
    
    // Tuş referansları
    private GameObject btnJumpObj;
    private GameObject btnDashObj;
    private GameObject btnTearObj;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        CreateMobileCanvas();
    }

    void CreateMobileCanvas()
    {
        Transform oldUI = transform.Find("MobileUICanvas");
        if (oldUI != null) Destroy(oldUI.gameObject);

        GameObject canvasObj = new GameObject("MobileUICanvas");
        canvasObj.transform.SetParent(transform, false);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920); 
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        Color bgCol = new Color(0, 0, 0, 0.2f);
        Color handleCol = new Color(1, 1, 1, 0.5f);

        Texture2D circleTex = CreateCircleTexture(128, Color.white);
        Sprite circleSprite = Sprite.Create(circleTex, new Rect(0,0,128,128), new Vector2(0.5f,0.5f));

        /* --- SOL ALT: VIRTUAL JOYSTICK (Bu her zaman açık) --- */
        GameObject joyBgObj = new GameObject("Joystick_BG");
        joyBgObj.transform.SetParent(canvasObj.transform, false);
        Image joyBgImg = joyBgObj.AddComponent<Image>();
        joyBgImg.sprite = circleSprite;
        joyBgImg.color = bgCol;

        Outline joyOutline = joyBgObj.AddComponent<Outline>();
        joyOutline.effectColor = new Color(1, 1, 1, 0.2f);
        joyOutline.effectDistance = new Vector2(2, -2);

        RectTransform joyBgRect = joyBgObj.GetComponent<RectTransform>();
        joyBgRect.anchorMin = new Vector2(0, 0);
        joyBgRect.anchorMax = new Vector2(0, 0);
        joyBgRect.pivot = new Vector2(0.5f, 0.5f);
        joyBgRect.anchoredPosition = new Vector2(220, 250); 
        joyBgRect.sizeDelta = new Vector2(joystickRadius * 2, joystickRadius * 2);

        GameObject joyHandleObj = new GameObject("Joystick_Handle");
        joyHandleObj.transform.SetParent(joyBgObj.transform, false);
        Image joyHandleImg = joyHandleObj.AddComponent<Image>();
        joyHandleImg.sprite = circleSprite;
        joyHandleImg.color = handleCol;

        joystickHandle = joyHandleObj.GetComponent<RectTransform>();
        joystickHandle.anchorMin = new Vector2(0.5f, 0.5f);
        joystickHandle.anchorMax = new Vector2(0.5f, 0.5f);
        joystickHandle.pivot = new Vector2(0.5f, 0.5f);
        joystickHandle.anchoredPosition = Vector2.zero;
        joystickHandle.sizeDelta = new Vector2(120, 120);

        EventTrigger joyTrigger = joyBgObj.AddComponent<EventTrigger>();
        
        EventTrigger.Entry pointerDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        pointerDown.callback.AddListener((data) => { OnJoystickDrag((PointerEventData)data); });
        joyTrigger.triggers.Add(pointerDown);

        EventTrigger.Entry drag = new EventTrigger.Entry { eventID = EventTriggerType.Drag };
        drag.callback.AddListener((data) => { OnJoystickDrag((PointerEventData)data); });
        joyTrigger.triggers.Add(drag);

        EventTrigger.Entry pointerUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        pointerUp.callback.AddListener((data) => { 
            joystickHandle.anchoredPosition = Vector2.zero;
            MobileInput.Joystick = Vector2.zero;
        });
        joyTrigger.triggers.Add(pointerUp);


        /* --- SAĞ ALT: AKSİYON BUTONLARI (BAŞLANGIÇTA GİZLİ) --- */
        Color jumpCol = new Color(0.2f, 0.9f, 0.3f, 0.45f); 
        Color dashCol = new Color(0.2f, 0.6f, 1f, 0.45f);   
        Color tearCol = new Color(1f, 0.2f, 0.3f, 0.45f);   
        
        btnJumpObj = CreateButton(canvasObj.transform, "Btn_Jump", circleSprite, new Vector2(-220, 250), 220, jumpCol, 
            () => MobileInput.JumpPressed = true, 
            () => { });

        btnDashObj = CreateToggleButton(canvasObj.transform, "Btn_Dash_Toggle", circleSprite, new Vector2(-460, 200), 160, dashCol);

        btnTearObj = CreateButton(canvasObj.transform, "Btn_Tear", circleSprite, new Vector2(-250, 480), 160, tearCol, 
            () => MobileInput.TearHeld = true, 
            () => MobileInput.TearHeld = false);

        // PlayerPrefs Kontrolü: Eğer tuş henüz açılmadıysa görünmez yap
        if (PlayerPrefs.GetInt("MobileUI_Unlocked_Jump", 0) == 0) btnJumpObj.SetActive(false);
        if (PlayerPrefs.GetInt("MobileUI_Unlocked_Dash", 0) == 0) btnDashObj.SetActive(false);
        if (PlayerPrefs.GetInt("MobileUI_Unlocked_Tear", 0) == 0) btnTearObj.SetActive(false);
    }

    /// <summary>
    /// Animasyonlu şekilde belirlenen tuşu açar ve kalıcı olarak kaydeder.
    /// Kullanım: MobileUI.Instance.UnlockButton("Jump");
    /// Tipler: "Jump", "Dash", "Tear"
    /// </summary>
    public void UnlockButton(string type)
    {
        GameObject targetBtn = null;
        if (type == "Jump") targetBtn = btnJumpObj;
        else if (type == "Dash") targetBtn = btnDashObj;
        else if (type == "Tear") targetBtn = btnTearObj;

        if (targetBtn != null && !targetBtn.activeSelf)
        {
            // Oynanışı kaydet
            PlayerPrefs.SetInt("MobileUI_Unlocked_" + type, 1);
            PlayerPrefs.Save();
            
            // Görünür yap ve Patlama efektiyle aç
            targetBtn.SetActive(true);
            StartCoroutine(PopAnimation(targetBtn.transform));
        }
    }

    private IEnumerator PopAnimation(Transform t)
    {
        float timer = 0f;
        while (timer < 1f)
        {
            timer += Time.unscaledDeltaTime * 4f; // Çok hızlı açılır
            float eased = Mathf.Clamp01(timer);
            // Yaylanma matematiği (Overshoot pop)
            float scale = Mathf.Lerp(0f, 1.2f, eased);
            if (eased > 0.8f) scale = Mathf.Lerp(1.2f, 1f, (eased - 0.8f) * 5f);
            
            t.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }
        t.localScale = Vector3.one;
    }

    private GameObject CreateButton(Transform parent, string name, Sprite sprite, Vector2 pos, float size, Color col, UnityEngine.Events.UnityAction onDown, UnityEngine.Events.UnityAction onUp)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        Image img = btnObj.AddComponent<Image>();
        img.sprite = sprite;
        img.color = col;

        Outline outline = btnObj.AddComponent<Outline>();
        outline.effectColor = new Color(1, 1, 1, 0.2f);
        outline.effectDistance = new Vector2(2, -2);

        RectTransform rect = btnObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 0); 
        rect.anchorMax = new Vector2(1, 0);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = pos;
        rect.sizeDelta = new Vector2(size, size);

        EventTrigger trigger = btnObj.AddComponent<EventTrigger>();
        
        EventTrigger.Entry down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        down.callback.AddListener((data) => { onDown(); img.color = new Color(1f, 1f, 1f, 0.8f); }); 
        trigger.triggers.Add(down);

        EventTrigger.Entry up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        up.callback.AddListener((data) => { onUp(); img.color = col; }); 
        trigger.triggers.Add(up);

        return btnObj;
    }

    private GameObject CreateToggleButton(Transform parent, string name, Sprite sprite, Vector2 pos, float size, Color offCol)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        Image img = btnObj.AddComponent<Image>();
        img.sprite = sprite;
        img.color = offCol;

        Outline outline = btnObj.AddComponent<Outline>();
        outline.effectColor = new Color(1, 1, 1, 0.2f);
        outline.effectDistance = new Vector2(2, -2);

        RectTransform rect = btnObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 0); 
        rect.anchorMax = new Vector2(1, 0);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = pos;
        rect.sizeDelta = new Vector2(size, size);

        EventTrigger trigger = btnObj.AddComponent<EventTrigger>();
        
        EventTrigger.Entry down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        down.callback.AddListener((data) => { 
            MobileInput.DashHeld = !MobileInput.DashHeld; 
            img.color = MobileInput.DashHeld ? new Color(1f, 1f, 1f, 0.8f) : offCol; 
        }); 
        trigger.triggers.Add(down);

        return btnObj;
    }

    private void OnJoystickDrag(PointerEventData data)
    {
        RectTransform background = joystickHandle.parent.GetComponent<RectTransform>();
        Vector2 position = Vector2.zero;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(background, data.position, data.pressEventCamera, out position);

        if (position.magnitude > joystickRadius)
        {
            position = position.normalized * joystickRadius;
        }

        joystickHandle.anchoredPosition = position;
        MobileInput.Joystick = position / joystickRadius;
    }

    Texture2D CreateCircleTexture(int size, Color color)
    {
        Texture2D tex = new Texture2D(size, size);
        float center = size / 2f;
        float radius = size / 2f;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                Color pCol = color;
                
                float alpha = Mathf.Clamp01((radius - dist) / 1.5f);
                pCol.a *= alpha;
                
                tex.SetPixel(x, y, pCol); 
            }
        }
        tex.Apply();
        return tex;
    }
}
