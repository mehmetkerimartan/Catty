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
        
        /* Optimization: Bob up and down without creating new Vector3 */
        Vector3 pos = transform.position;
        pos.y = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = pos;
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
