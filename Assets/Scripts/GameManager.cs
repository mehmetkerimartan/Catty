using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton Game Manager that handles lives, health, respawning, and game state.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Health Settings")]
    [SerializeField] private int maxLives = 9;
    [SerializeField] private int maxHealth = 3;
    
    [Header("Currency")]
    [SerializeField] private int startingCoins = 0;
    
    [Header("References")]
    [SerializeField] private Transform player;
    
    private int currentLives;
    private int currentHealth;
    private int currentCoins;
    private Vector3 lastCheckpointPosition;
    private Quaternion lastCheckpointRotation;
    
    public int CurrentLives => currentLives;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public int CurrentCoins => currentCoins;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        InitializeGame();
    }
    
    private void InitializeGame()
    {
        currentLives = maxLives;
        currentHealth = maxHealth;
        currentCoins = startingCoins;
        
        if (player != null)
        {
            lastCheckpointPosition = player.position;
            lastCheckpointRotation = player.rotation;
        }
    }
    
    /// <summary>
    /// Called when player reaches a checkpoint.
    /// </summary>
    public void SetCheckpoint(Vector3 position, Quaternion rotation)
    {
        lastCheckpointPosition = position;
        lastCheckpointRotation = rotation;
        Debug.Log("Checkpoint kaydedildi: " + position);
    }
    
    /// <summary>
    /// Called when player takes damage.
    /// </summary>
    public void TakeDamage(int damage = 1)
    {
        currentHealth -= damage;
        
        if (currentHealth <= 0)
        {
            PlayerDied();
        }
    }
    
    /// <summary>
    /// Called when player collects a heart.
    /// </summary>
    public void AddHealth(int amount = 1)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log("Can eklendi. Mevcut can: " + currentHealth);
    }
    
    /// <summary>
    /// Add coins to player's wallet.
    /// </summary>
    public void AddCoins(int amount)
    {
        currentCoins += amount;
        Debug.Log("Coin eklendi: " + amount + " Toplam: " + currentCoins);
    }
    
    /// <summary>
    /// Try to spend coins. Returns true if successful.
    /// </summary>
    public bool SpendCoins(int amount)
    {
        if (currentCoins >= amount)
        {
            currentCoins -= amount;
            Debug.Log("Coin harcandi: " + amount + " Kalan: " + currentCoins);
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Called when player dies (falls or loses all health).
    /// </summary>
    public void PlayerDied()
    {
        currentLives--;
        
        if (currentLives <= 0)
        {
            GameOver();
            return;
        }
        
        /* Reset health and respawn */
        currentHealth = maxHealth;
        RespawnPlayer();
    }
    
    private void RespawnPlayer()
    {
        /* Auto-find player if not assigned */
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log("Player otomatik bulundu!");
            }
            else
            {
                Debug.LogError("HATA: Player bulunamadi! Player tag'i atanmis mi?");
                return;
            }
        }
        
        Debug.Log("Respawn basliyor. Hedef pozisyon: " + lastCheckpointPosition);
        
        /* Disable CharacterController temporarily to teleport */
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) 
        {
            cc.enabled = false;
            Debug.Log("CharacterController deaktif edildi");
        }
        
        player.position = lastCheckpointPosition;
        player.rotation = lastCheckpointRotation;
        
        Debug.Log("Oyuncu yeni pozisyona taşındı: " + player.position);
        
        if (cc != null) 
        {
            cc.enabled = true;
            Debug.Log("CharacterController aktif edildi");
        }
        
        Debug.Log("Oyuncu yeniden dogdu. Kalan can: " + currentLives);
    }
    
    private void GameOver()
    {
        Debug.Log("OYUN BITTI - Tum canlar tukendi!");
        /* TODO: Show game over UI */
        Time.timeScale = 0f;
    }
    
    /// <summary>
    /// Called when player completes the level.
    /// </summary>
    public void LevelComplete()
    {
        Debug.Log("BOLUM TAMAMLANDI!");
        /* TODO: Load next level or show completion UI */
        Time.timeScale = 0f;
    }
    
    /// <summary>
    /// Restart the current level.
    /// </summary>
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
