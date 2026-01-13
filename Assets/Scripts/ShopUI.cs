using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Shop UI panel for purchasing perks.
/// </summary>
public class ShopUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Transform perkListContainer;
    [SerializeField] private GameObject perkItemPrefab;
    [SerializeField] private TextMeshProUGUI coinDisplay;
    [SerializeField] private TextMeshProUGUI titleText;
    
    [Header("Colors")]
    [SerializeField] private Color affordableColor = Color.white;
    [SerializeField] private Color expensiveColor = Color.gray;
    [SerializeField] private Color ownedColor = Color.green;
    
    private List<GameObject> perkItems = new List<GameObject>();
    
    void Awake()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }
    
    public void Show()
    {
        if (panel != null)
        {
            panel.SetActive(true);
        }
        
        RefreshPerkList();
        UpdateCoinDisplay();
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void Hide()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    private void RefreshPerkList()
    {
        /* Clear existing items */
        foreach (var item in perkItems)
        {
            Destroy(item);
        }
        perkItems.Clear();
        
        if (PerkManager.Instance == null || perkListContainer == null) return;
        
        /* Create perk items */
        foreach (var perkData in PerkManager.Instance.AvailablePerks)
        {
            CreatePerkItem(perkData);
        }
    }
    
    private void CreatePerkItem(PerkManager.PerkData perkData)
    {
        /* If no prefab, create simple UI */
        GameObject item;
        
        if (perkItemPrefab != null)
        {
            item = Instantiate(perkItemPrefab, perkListContainer);
        }
        else
        {
            /* Create basic item without prefab */
            item = new GameObject(perkData.displayName);
            item.transform.SetParent(perkListContainer);
            
            /* Add layout components */
            RectTransform rect = item.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(400, 60);
            
            HorizontalLayoutGroup layout = item.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10;
            layout.padding = new RectOffset(10, 10, 5, 5);
            layout.childAlignment = TextAnchor.MiddleLeft;
            
            /* Name text */
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(item.transform);
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = perkData.displayName;
            nameText.fontSize = 18;
            
            /* Cost text */
            GameObject costObj = new GameObject("Cost");
            costObj.transform.SetParent(item.transform);
            TextMeshProUGUI costText = costObj.AddComponent<TextMeshProUGUI>();
            costText.text = perkData.cost + " Coin";
            costText.fontSize = 16;
            
            /* Buy button */
            GameObject btnObj = new GameObject("BuyButton");
            btnObj.transform.SetParent(item.transform);
            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = new Color(0.2f, 0.6f, 0.2f);
            Button btn = btnObj.AddComponent<Button>();
            
            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(80, 40);
            
            GameObject btnTextObj = new GameObject("Text");
            btnTextObj.transform.SetParent(btnObj.transform);
            TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
            btnText.text = "AL";
            btnText.fontSize = 16;
            btnText.alignment = TextAlignmentOptions.Center;
            
            /* Setup button */
            bool owned = PerkManager.Instance.HasPerk(perkData.type);
            bool canAfford = GameManager.Instance != null && 
                           GameManager.Instance.CurrentCoins >= perkData.cost;
            
            if (owned)
            {
                btnText.text = "ALINDI";
                btnImage.color = ownedColor;
                btn.interactable = false;
            }
            else if (!canAfford)
            {
                btnImage.color = expensiveColor;
                btn.interactable = false;
            }
            
            /* Button click */
            PerkManager.PerkType perkType = perkData.type;
            btn.onClick.AddListener(() => OnPurchaseClick(perkType));
        }
        
        perkItems.Add(item);
    }
    
    private void OnPurchaseClick(PerkManager.PerkType perkType)
    {
        if (PerkManager.Instance.TryPurchasePerk(perkType))
        {
            RefreshPerkList();
            UpdateCoinDisplay();
            Debug.Log("Perk satin alindi: " + perkType);
        }
    }
    
    private void UpdateCoinDisplay()
    {
        if (coinDisplay != null && GameManager.Instance != null)
        {
            coinDisplay.text = GameManager.Instance.CurrentCoins + " Coin";
        }
    }
    
    void Update()
    {
        /* Keep coin display updated while shop is open */
        if (panel != null && panel.activeSelf)
        {
            UpdateCoinDisplay();
        }
    }
}
