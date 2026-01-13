using UnityEngine;

/// <summary>
/// Platform that sinks when player stands on it.
/// Optimized with minimal Update overhead.
/// </summary>
public class SinkingPlatform : MonoBehaviour
{
    [Header("Sink Settings")]
    [SerializeField] private float sinkDelay = 0.5f;
    [SerializeField] private float sinkSpeed = 3f;
    [SerializeField] private float sinkDistance = 10f;
    
    [Header("Reset Settings")]
    [SerializeField] private bool resetAfterFall = true;
    [SerializeField] private float resetDelay = 3f;
    
    [Header("Visual Feedback")]
    [SerializeField] private bool shakeBeforeSink = true;
    [SerializeField] private float shakeIntensity = 0.05f;
    [SerializeField] private float shakeFrequency = 20f;
    
    private Vector3 originalPosition;
    private float sinkTimer;
    private float resetTimer;
    private bool playerOnPlatform;
    private bool isSinking;
    private bool hasFallen;
    private Renderer rend;
    private Color originalColor;
    
    void Awake()
    {
        originalPosition = transform.position;
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            originalColor = rend.material.color;
        }
    }
    
    void Update()
    {
        /* Early exit if not active - optimization */
        if (!playerOnPlatform && !isSinking && !hasFallen) return;
        
        if (playerOnPlatform && !isSinking)
        {
            /* Countdown to sink */
            sinkTimer += Time.deltaTime;
            
            /* Shake effect before sinking */
            if (shakeBeforeSink)
            {
                float shake = Mathf.Sin(Time.time * shakeFrequency) * shakeIntensity;
                transform.position = originalPosition + new Vector3(shake, 0, shake);
                
                /* Visual warning */
                if (rend != null)
                {
                    float flash = Mathf.PingPong(Time.time * 5f, 1f);
                    rend.material.color = Color.Lerp(originalColor, Color.red, flash * 0.5f);
                }
            }
            
            if (sinkTimer >= sinkDelay)
            {
                isSinking = true;
                Debug.Log("Platform batmaya basladi!");
            }
        }
        
        if (isSinking)
        {
            /* Move platform down */
            transform.Translate(Vector3.down * sinkSpeed * Time.deltaTime);
            
            /* Check if fallen enough */
            if (originalPosition.y - transform.position.y >= sinkDistance)
            {
                hasFallen = true;
                isSinking = false;
                
                if (resetAfterFall)
                {
                    resetTimer = resetDelay;
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
        }
        
        if (hasFallen && resetAfterFall)
        {
            resetTimer -= Time.deltaTime;
            if (resetTimer <= 0)
            {
                ResetPlatform();
            }
        }
    }
    
    private void ResetPlatform()
    {
        transform.position = originalPosition;
        sinkTimer = 0f;
        isSinking = false;
        hasFallen = false;
        playerOnPlatform = false;
        
        if (rend != null)
        {
            rend.material.color = originalColor;
        }
        
        Debug.Log("Platform sifirlandi!");
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasFallen)
        {
            playerOnPlatform = true;
            Debug.Log("Oyuncu batan platforma basti!");
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerOnPlatform = false;
            
            /* Reset shake if player leaves before sinking */
            if (!isSinking && !hasFallen)
            {
                transform.position = originalPosition;
                if (rend != null)
                {
                    rend.material.color = originalColor;
                }
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 fallPos = (Application.isPlaying ? originalPosition : transform.position) + Vector3.down * sinkDistance;
        Gizmos.DrawWireCube(fallPos, transform.localScale);
        Gizmos.DrawLine(transform.position, fallPos);
    }
}
