using UnityEngine;

/// <summary>
/// Collectible heart that restores player health.
/// </summary>
public class HeartPickup : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int healthAmount = 1;
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.3f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip collectSound;
    
    private Vector3 startPosition;
    
    void Start()
    {
        startPosition = transform.position;
    }
    
    void Update()
    {
        /* Rotate the heart */
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        
        /* Bob up and down */
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
    
    /// <summary>
    /// Called when player collects this heart.
    /// </summary>
    public void Collect()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddHealth(healthAmount);
        }
        
        /* Play sound if assigned */
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }
        
        /* Destroy the pickup */
        Destroy(gameObject);
    }
}
