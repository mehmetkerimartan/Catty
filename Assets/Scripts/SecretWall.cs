using UnityEngine;

/// <summary>
/// Secret wall/passage that triggers cat reaction when SecretSense perk is active.
/// Spider-man sense style detection.
/// </summary>
public class SecretWall : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float triggerCooldown = 3f;
    
    [Header("Secret Type")]
    [SerializeField] private bool isPassable = true;
    [SerializeField] private GameObject hiddenPassage;
    [SerializeField] private GameObject fakeWall;
    
    [Header("Visual Hint")]
    [SerializeField] private bool showHintParticles = true;
    [SerializeField] private ParticleSystem hintParticles;
    [SerializeField] private Color hintColor = new Color(1f, 0.8f, 0.2f, 0.5f);
    
    private Transform player;
    private float lastTriggerTime = -999f;
    private bool hasBeenRevealed;
    private bool playerNearby;
    
    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        /* Hide the passage initially */
        if (hiddenPassage != null)
        {
            hiddenPassage.SetActive(false);
        }
    }
    
    void Update()
    {
        if (player == null || hasBeenRevealed) return;
        
        /* Check if player has SecretSense perk */
        if (PerkManager.Instance == null || 
            !PerkManager.Instance.HasPerk(PerkManager.PerkType.SecretSense))
        {
            return;
        }
        
        float distance = Vector3.Distance(transform.position, player.position);
        bool wasNearby = playerNearby;
        playerNearby = distance <= detectionRadius;
        
        /* Trigger reaction when entering range */
        if (playerNearby && !wasNearby)
        {
            if (Time.time - lastTriggerTime >= triggerCooldown)
            {
                TriggerSecretSense();
                lastTriggerTime = Time.time;
            }
        }
    }
    
    private void TriggerSecretSense()
    {
        Debug.Log("Gizli duvar algilandi! Kedi hissediyor...");
        
        /* Trigger cat reaction */
        CatReaction catReaction = player.GetComponent<CatReaction>();
        if (catReaction == null)
        {
            catReaction = player.GetComponentInChildren<CatReaction>();
        }
        
        if (catReaction != null)
        {
            catReaction.TriggerSecretSense(transform.position);
        }
        
        /* Show hint particles */
        if (showHintParticles && hintParticles != null)
        {
            hintParticles.Play();
        }
        
        /* Camera shake for emphasis */
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(0.02f, 0.2f);
        }
    }
    
    /// <summary>
    /// Reveal the secret passage (called when player interacts).
    /// </summary>
    public void Reveal()
    {
        if (hasBeenRevealed) return;
        
        hasBeenRevealed = true;
        
        if (fakeWall != null)
        {
            fakeWall.SetActive(false);
        }
        
        if (hiddenPassage != null)
        {
            hiddenPassage.SetActive(true);
        }
        
        Debug.Log("Gizli gecit acildi!");
    }
    
    void OnTriggerEnter(Collider other)
    {
        /* Auto-reveal when player walks into it (if passable) */
        if (other.CompareTag("Player") && isPassable && 
            PerkManager.Instance != null && 
            PerkManager.Instance.HasPerk(PerkManager.PerkType.SecretSense))
        {
            Reveal();
        }
    }
    
    void OnDrawGizmosSelected()
    {
        /* Draw detection radius */
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        /* Mark secret location */
        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(transform.position, Vector3.one * 0.5f);
    }
}
