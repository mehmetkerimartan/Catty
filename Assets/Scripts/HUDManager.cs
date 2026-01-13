using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// HUD Manager that displays lives, health, and spirit power.
/// </summary>
public class HUDManager : MonoBehaviour
{
    [Header("Lives Display")]
    [SerializeField] private TextMeshProUGUI livesText;
    
    [Header("Health Display")]
    [SerializeField] private Image[] healthHearts;
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite emptyHeart;
    
    [Header("Spirit Power Bar")]
    [SerializeField] private Image spiritPowerFill;
    [SerializeField] private Color normalColor = Color.cyan;
    [SerializeField] private Color lowColor = Color.red;
    
    [Header("Interaction Prompt")]
    [SerializeField] private GameObject interactPrompt;
    [SerializeField] private TextMeshProUGUI interactText;
    
    [Header("Coin Display")]
    [SerializeField] private TextMeshProUGUI coinText;
    
    void Update()
    {
        UpdateLivesDisplay();
        UpdateHealthDisplay();
        UpdateSpiritPowerDisplay();
        UpdateCoinDisplay();
    }
    
    private void UpdateCoinDisplay()
    {
        if (coinText != null && GameManager.Instance != null)
        {
            coinText.text = GameManager.Instance.CurrentCoins.ToString();
        }
    }
    
    private void UpdateLivesDisplay()
    {
        if (livesText != null && GameManager.Instance != null)
        {
            livesText.text = "x " + GameManager.Instance.CurrentLives;
        }
    }
    
    private void UpdateHealthDisplay()
    {
        if (healthHearts == null || GameManager.Instance == null) return;
        
        int currentHealth = GameManager.Instance.CurrentHealth;
        
        for (int i = 0; i < healthHearts.Length; i++)
        {
            if (healthHearts[i] != null)
            {
                if (i < currentHealth)
                {
                    healthHearts[i].sprite = fullHeart;
                    healthHearts[i].color = Color.white;
                }
                else
                {
                    healthHearts[i].sprite = emptyHeart;
                    healthHearts[i].color = new Color(1f, 1f, 1f, 0.5f);
                }
            }
        }
    }
    
    private void UpdateSpiritPowerDisplay()
    {
        if (spiritPowerFill != null && RealityManager.Instance != null)
        {
            /* Show current charge timer progress */
            float normalized = RealityManager.Instance.ChargeTimerNormalized;
            spiritPowerFill.fillAmount = normalized;
            
            /* Change color based on remaining charges */
            int charges = RealityManager.Instance.CurrentCharges;
            if (charges <= 0)
            {
                spiritPowerFill.color = Color.gray;
            }
            else if (charges == 1)
            {
                spiritPowerFill.color = lowColor;
            }
            else
            {
                spiritPowerFill.color = normalColor;
            }
        }
    }
    
    /// <summary>
    /// Show interaction prompt (e.g., "Press E to enter portal")
    /// </summary>
    public void ShowInteractPrompt(string text)
    {
        if (interactPrompt != null)
        {
            interactPrompt.SetActive(true);
            if (interactText != null)
            {
                interactText.text = text;
            }
        }
    }
    
    /// <summary>
    /// Hide interaction prompt
    /// </summary>
    public void HideInteractPrompt()
    {
        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }
    }
}
