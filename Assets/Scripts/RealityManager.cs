using UnityEngine;

/// <summary>
/// Manages the "Reality Tear" mechanic with dual-world visuals.
/// Press right-click to reveal the Hell layer (truth) through a radar effect.
/// </summary>
public class RealityManager : MonoBehaviour
{
    public static RealityManager Instance { get; private set; }
    
    [Header("Tear Charges")]
    [SerializeField] private int maxCharges = 3;
    [SerializeField] private float chargeDuration = 1.5f;  /* Each charge lasts 1.5 seconds */
    
    [Header("Reality Tear Settings")]
    [SerializeField] private float maxTearRadius = 25f;
    [SerializeField] private float tearExpandSpeed = 30f; /* Reverted to slower expansion */
    [SerializeField] private float tearShrinkSpeed = 50f;
    
    [Header("Slow Motion Settings")]
    [SerializeField] private float slowMotionScale = 0.3f;
    
    [Header("Radar Visual")]
    [SerializeField] private GameObject radarVisualPrefab;
    private GameObject radarVisual;
    
    private int currentCharges;
    private float currentChargeTimer;
    private float currentRadius = 0f;
    private bool isRealityTorn = false;
    private bool chargeUsed = false;  /* Track if current charge was spent */
    private bool needsRelease = false; /* Force release before next use */
    
    public int CurrentCharges => currentCharges;
    public int MaxCharges => maxCharges;
    public float ChargeTimerNormalized => currentChargeTimer / chargeDuration;
    public bool IsRealityTorn => isRealityTorn;
    public float CurrentRadius => currentRadius;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        currentCharges = maxCharges;
        currentChargeTimer = chargeDuration;
        SetTimeScale(1f);
        CreateRadarVisual();
    }
    
    void Update()
    {
        HandleRealityTear();
        UpdateRadarVisual();
        UpdateDualWorldObjects();
    }
    
    private void CreateRadarVisual()
    {
        /* No visual line - shaders handle edge effect */
        radarVisual = null;
    }
    
    private void HandleRealityTear()
    {
        /* Release check - mouse or controller RB */
        if (Input.GetMouseButtonUp(1) || Input.GetKeyUp(KeyCode.JoystickButton5))
        {
            needsRelease = false;
        }

        /* Check if we can start or continue a charge - mouse right-click OR controller RB */
        bool inputActive = Input.GetMouseButton(1) || Input.GetKey(KeyCode.JoystickButton5);
        bool canStart = currentCharges > 0 && !needsRelease;

        if (inputActive && canStart)
        {
            /* If this is a new click, initialize charge state */
            if (!chargeUsed)
            {
                chargeUsed = true;
                currentChargeTimer = chargeDuration;
                Debug.Log("Hak basladi! Kalan (bu dahil degil): " + (currentCharges - 1));
            }

            isRealityTorn = true;
            currentChargeTimer -= Time.unscaledDeltaTime;

            /* If time runs out while holding */
            if (currentChargeTimer <= 0)
            {
                ConsumeCharge();
                needsRelease = true;
                isRealityTorn = false;
            }
        }
        else
        {
            /* If we were using a charge but released/ran out, finalize it */
            if (chargeUsed)
            {
                ConsumeCharge();
            }
            isRealityTorn = false;
        }

        /* Handle Radius and TimeScale based on current state */
        if (isRealityTorn)
        {
            currentRadius = Mathf.MoveTowards(currentRadius, maxTearRadius, tearExpandSpeed * Time.unscaledDeltaTime);
            SetTimeScale(slowMotionScale);
        }
        else
        {
            currentRadius = Mathf.MoveTowards(currentRadius, 0f, tearShrinkSpeed * Time.unscaledDeltaTime);
            SetTimeScale(1f);
        }
    }

    private void ConsumeCharge()
    {
        if (chargeUsed)
        {
            currentCharges--;
            currentChargeTimer = chargeDuration; // Reset for next HUD display
            chargeUsed = false;
            Debug.Log("Hak bitti. Kalan: " + currentCharges);
        }
    }
    
    private void SetTimeScale(float scale)
    {
        Time.timeScale = scale;
        Time.fixedDeltaTime = 0.02f * scale;
    }
    
    private void UpdateRadarVisual()
    {
        /* Update global shader properties - this is the important part! */
        Shader.SetGlobalFloat("_TearRadius", currentRadius);
        Shader.SetGlobalVector("_TearCenter", transform.position);
        
        /* Visual ring (disabled) */
        if (radarVisual == null) return;
        
        if (currentRadius > 0.1f)
        {
            radarVisual.SetActive(true);
            radarVisual.transform.localScale = new Vector3(currentRadius, 1f, currentRadius);
        }
        else
        {
            radarVisual.SetActive(false);
        }
    }
    
    private void UpdateDualWorldObjects()
    {
        /* Update all DualWorldObject components */
        DualWorldObject[] dualObjects = FindObjectsByType<DualWorldObject>(FindObjectsSortMode.None);
        
        foreach (DualWorldObject obj in dualObjects)
        {
            /* Check distance to object's center */
            float distance = Vector3.Distance(transform.position, obj.transform.position);
            bool inRadius = currentRadius > 0 && distance <= currentRadius;
            obj.SetHellMode(inRadius);
            
            /* Debug - uncomment to see what's happening */
            /* Debug.Log(obj.name + " mesafe: " + distance + " radius: " + currentRadius + " inRadius: " + inRadius); */
        }
    }
    
    void OnDestroy()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }
}
