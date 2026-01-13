using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages perks that the player can purchase from the shop.
/// Singleton pattern for global access.
/// </summary>
public class PerkManager : MonoBehaviour
{
    public static PerkManager Instance { get; private set; }
    
    /* Available perks */
    public enum PerkType
    {
        SecretSense,
        DoubleJump,
        WallClimb,
        DashAbility
    }
    
    [System.Serializable]
    public class PerkData
    {
        public PerkType type;
        public string displayName;
        public string description;
        public int cost;
        public Sprite icon;
    }
    
    [Header("Perk Definitions")]
    [SerializeField] private List<PerkData> availablePerks = new List<PerkData>();
    
    private HashSet<PerkType> unlockedPerks = new HashSet<PerkType>();
    
    public List<PerkData> AvailablePerks => availablePerks;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDefaultPerks();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeDefaultPerks()
    {
        /* Add default perks if none defined in inspector */
        if (availablePerks.Count == 0)
        {
            availablePerks.Add(new PerkData
            {
                type = PerkType.SecretSense,
                displayName = "Gizli His",
                description = "Gizli duvarlara yaklasinca kedi irkilir",
                cost = 50
            });
            
            availablePerks.Add(new PerkData
            {
                type = PerkType.DoubleJump,
                displayName = "Cift Ziplama",
                description = "Havadayken tekrar zipla",
                cost = 100
            });
            
            availablePerks.Add(new PerkData
            {
                type = PerkType.DashAbility,
                displayName = "Atilim",
                description = "Shift ile hizli atilim yap",
                cost = 75
            });
        }
    }
    
    public bool HasPerk(PerkType perk)
    {
        return unlockedPerks.Contains(perk);
    }
    
    public bool TryPurchasePerk(PerkType perk)
    {
        if (HasPerk(perk))
        {
            Debug.Log("Bu perk zaten acik: " + perk);
            return false;
        }
        
        PerkData data = availablePerks.Find(p => p.type == perk);
        if (data == null)
        {
            Debug.LogError("Perk bulunamadi: " + perk);
            return false;
        }
        
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager bulunamadi!");
            return false;
        }
        
        if (GameManager.Instance.SpendCoins(data.cost))
        {
            unlockedPerks.Add(perk);
            Debug.Log("Perk satin alindi: " + data.displayName);
            return true;
        }
        
        Debug.Log("Yeterli coin yok! Gereken: " + data.cost);
        return false;
    }
    
    public void UnlockPerkFree(PerkType perk)
    {
        unlockedPerks.Add(perk);
        Debug.Log("Perk ucretsiz acildi: " + perk);
    }
    
    public int GetPerkCost(PerkType perk)
    {
        PerkData data = availablePerks.Find(p => p.type == perk);
        return data != null ? data.cost : 0;
    }
}
