using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Shop keeper NPC that opens shop UI when player presses E.
/// Follows LevelPortal interaction pattern.
/// </summary>
public class ShopKeeper : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private string promptText = "E - Dukkan";
    
    [Header("Visual")]
    [SerializeField] private GameObject shopIcon;
    [SerializeField] private float iconBobSpeed = 2f;
    [SerializeField] private float iconBobHeight = 0.2f;
    
    [Header("Events")]
    public UnityEvent onShopOpen;
    public UnityEvent onShopClose;
    
    private Transform player;
    private bool playerInRange;
    private bool shopOpen;
    private Vector3 iconStartPos;
    private HUDManager hudManager;
    
    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        hudManager = FindFirstObjectByType<HUDManager>();
        
        if (shopIcon != null)
        {
            iconStartPos = shopIcon.transform.localPosition;
        }
    }
    
    void Update()
    {
        if (player == null) return;
        
        /* Check distance */
        float distance = Vector3.Distance(transform.position, player.position);
        bool wasInRange = playerInRange;
        playerInRange = distance <= interactionRange;
        
        /* Show/hide prompt */
        if (playerInRange && !wasInRange)
        {
            if (hudManager != null)
            {
                hudManager.ShowInteractPrompt(promptText);
            }
        }
        else if (!playerInRange && wasInRange)
        {
            if (hudManager != null)
            {
                hudManager.HideInteractPrompt();
            }
            
            /* Close shop if player walks away */
            if (shopOpen)
            {
                CloseShop();
            }
        }
        
        /* Handle interaction */
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            if (shopOpen)
            {
                CloseShop();
            }
            else
            {
                OpenShop();
            }
        }
        
        /* Close with Escape */
        if (shopOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseShop();
        }
        
        /* Animate shop icon */
        if (shopIcon != null && playerInRange)
        {
            float bob = Mathf.Sin(Time.time * iconBobSpeed) * iconBobHeight;
            shopIcon.transform.localPosition = iconStartPos + Vector3.up * bob;
        }
    }
    
    private void OpenShop()
    {
        shopOpen = true;
        Time.timeScale = 0f;
        
        onShopOpen?.Invoke();
        
        /* Find and show ShopUI */
        ShopUI shopUI = FindFirstObjectByType<ShopUI>(FindObjectsInactive.Include);
        if (shopUI != null)
        {
            shopUI.Show();
        }
        
        Debug.Log("Dukkan acildi!");
    }
    
    private void CloseShop()
    {
        shopOpen = false;
        Time.timeScale = 1f;
        
        onShopClose?.Invoke();
        
        /* Find and hide ShopUI */
        ShopUI shopUI = FindFirstObjectByType<ShopUI>(FindObjectsInactive.Include);
        if (shopUI != null)
        {
            shopUI.Hide();
        }
        
        Debug.Log("Dukkan kapandi!");
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
