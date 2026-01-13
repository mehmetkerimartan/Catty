using UnityEngine;

/// <summary>
/// Coin pickup that adds currency for the shop.
/// Similar pattern to HeartPickup for consistency.
/// </summary>
public class CoinPickup : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int coinValue = 10;
    [SerializeField] private float rotationSpeed = 120f;
    [SerializeField] private float bobSpeed = 3f;
    [SerializeField] private float bobHeight = 0.2f;
    
    [Header("Visual")]
    [SerializeField] private Color coinColor = new Color(1f, 0.85f, 0f);
    [SerializeField] private ParticleSystem collectParticles;
    
    [Header("Audio")]
    [SerializeField] private AudioClip collectSound;
    
    private Vector3 startPosition;
    private Renderer rend;
    
    void Start()
    {
        startPosition = transform.position;
        rend = GetComponent<Renderer>();
        
        /* Apply coin color */
        if (rend != null)
        {
            rend.material.color = coinColor;
        }
    }
    
    void Update()
    {
        /* Rotate the coin */
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        
        /* Bob up and down */
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }
    
    private void Collect()
    {
        /* Add coins to GameManager */
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCoins(coinValue);
        }
        
        /* Play particles */
        if (collectParticles != null)
        {
            collectParticles.transform.SetParent(null);
            collectParticles.Play();
            Destroy(collectParticles.gameObject, 2f);
        }
        
        /* Play sound */
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }
        
        Debug.Log("Coin toplandi! Deger: " + coinValue);
        Destroy(gameObject);
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.85f, 0f, 0.7f);
        Gizmos.DrawSphere(transform.position, 0.3f);
    }
}
