using UnityEngine;

/// <summary>
/// Cat reaction component for spider-sense effect.
/// Shows visual/audio feedback when secrets are detected.
/// </summary>
public class CatReaction : MonoBehaviour
{
    [Header("Visual Effects")]
    [SerializeField] private GameObject alertIcon;
    [SerializeField] private float alertDuration = 2f;
    [SerializeField] private ParticleSystem senseParticles;
    
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string reactionTrigger = "SecretSense";
    
    [Header("Audio")]
    [SerializeField] private AudioClip senseSound;
    [SerializeField] private AudioSource audioSource;
    
    [Header("Screen Effect")]
    [SerializeField] private bool useScreenFlash = true;
    [SerializeField] private Color flashColor = new Color(1f, 0.9f, 0.5f, 0.3f);
    [SerializeField] private float flashDuration = 0.3f;
    
    private float alertTimer;
    private bool isReacting;
    private Vector3 secretDirection;
    
    void Start()
    {
        if (alertIcon != null)
        {
            alertIcon.SetActive(false);
        }
        
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }
    
    void Update()
    {
        /* Handle alert icon timer */
        if (alertTimer > 0)
        {
            alertTimer -= Time.deltaTime;
            
            /* Pulse the alert icon */
            if (alertIcon != null)
            {
                float pulse = 1f + Mathf.Sin(Time.time * 10f) * 0.2f;
                alertIcon.transform.localScale = Vector3.one * pulse;
                
                /* Point towards secret */
                if (secretDirection != Vector3.zero)
                {
                    Vector3 lookDir = secretDirection - transform.position;
                    lookDir.y = 0;
                    if (lookDir.magnitude > 0.1f)
                    {
                        alertIcon.transform.rotation = Quaternion.LookRotation(lookDir);
                    }
                }
            }
            
            if (alertTimer <= 0)
            {
                EndReaction();
            }
        }
    }
    
    /// <summary>
    /// Trigger the secret sense reaction.
    /// </summary>
    public void TriggerSecretSense(Vector3 secretPosition)
    {
        if (isReacting) return;
        
        isReacting = true;
        secretDirection = secretPosition;
        alertTimer = alertDuration;
        
        Debug.Log("Kedi irkildi! Gizli bir sey var...");
        
        /* Show alert icon */
        if (alertIcon != null)
        {
            alertIcon.SetActive(true);
        }
        
        /* Play particles */
        if (senseParticles != null)
        {
            senseParticles.Play();
        }
        
        /* Trigger animation */
        if (animator != null)
        {
            animator.SetTrigger(reactionTrigger);
        }
        
        /* Play sound */
        if (senseSound != null)
        {
            if (audioSource != null)
            {
                audioSource.PlayOneShot(senseSound);
            }
            else
            {
                AudioSource.PlayClipAtPoint(senseSound, transform.position);
            }
        }
        
        /* Screen flash effect */
        if (useScreenFlash)
        {
            StartCoroutine(ScreenFlash());
        }
        
        /* Camera shake */
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(0.03f, 0.25f);
        }
    }
    
    private void EndReaction()
    {
        isReacting = false;
        
        if (alertIcon != null)
        {
            alertIcon.SetActive(false);
        }
    }
    
    private System.Collections.IEnumerator ScreenFlash()
    {
        /* This would typically use a UI overlay or post-processing */
        /* For now, we'll just log it - can be enhanced with actual UI */
        Debug.Log("Ekran flash efekti!");
        
        /* Wait for flash duration using unscaled time */
        float timer = 0;
        while (timer < flashDuration)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (isReacting && secretDirection != Vector3.zero)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position + Vector3.up, secretDirection);
        }
    }
}
