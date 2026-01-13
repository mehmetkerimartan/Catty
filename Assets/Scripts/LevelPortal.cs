using UnityEngine;

/// <summary>
/// Portal/door that completes the level when player interacts with E key.
/// </summary>
public class LevelPortal : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactionRange = 2f;
    
    [Header("Visual")]
    [SerializeField] private float rotationSpeed = 30f;
    [SerializeField] private ParticleSystem portalParticles;
    
    private Transform player;
    private bool playerInRange = false;
    
    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }
    
    void Update()
    {
        /* Rotate the portal for visual effect */
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        
        /* Check if player is in range */
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            playerInRange = distance <= interactionRange;
            
            /* Check for interaction */
            if (playerInRange && Input.GetKeyDown(interactKey))
            {
                CompleteLevel();
            }
        }
    }
    
    private void CompleteLevel()
    {
        if (portalParticles != null)
        {
            portalParticles.Play();
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LevelComplete();
        }
    }
    
    void OnDrawGizmosSelected()
    {
        /* Draw interaction range in editor */
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
