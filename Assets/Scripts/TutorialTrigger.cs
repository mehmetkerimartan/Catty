using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class TutorialTrigger : MonoBehaviour
{
    [Header("Öğretici (Tutorial) Ayarları")]
    [TextArea(2, 4)]
    [Tooltip("Meleğin sana söyleyeceği mesaj (İngilizce tavsiye edilir)")]
    public string tutorialMessage = "Jump over the gaps using SPACE!";
    
    [Tooltip("Hangi yüz ifadesiyle gelsin? (normal, worried, scared)")]
    public string expression = "normal";
    
    [Tooltip("Yazı bittikten sonra ekranda kaç saniye kalsın?")]
    public float displayDuration = 4f;

    [Header("Tuş Açma (Unlock) Sistemi")]
    [Tooltip("Bu kutuya girince hangi mobil tuş açılsın? Yazabileceğin kelimeler: Jump, Dash, Tear (Eğer tuş açmayacaksan boş bırak!)")]
    public string unlockButtonType = "";
    
    [Header("Ekstra Ayarlar")]
    [Tooltip("Bu mesaj oyuncuya sadece 1 kere mi gösterilsin?")]
    public bool showOnlyOnce = true;
    private bool hasShown = false;

    void Start()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        box.isTrigger = true; 
        
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr != null) mr.enabled = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (showOnlyOnce && hasShown) return;

        if (other.CompareTag("Player"))
        {
            hasShown = true;
            
            // Eğer melek arayüzü varsa yazıyı yazdır
            if (FairyUI.Instance != null && !string.IsNullOrEmpty(tutorialMessage))
            {
                FairyUI.Instance.ShowDialogue(tutorialMessage, expression, displayDuration);
            }
            
            // Eğer bir mobil tuş (örn: "Jump") açılacaksa, onu aktifleştir ve kalıcı kaydet!
            if (!string.IsNullOrEmpty(unlockButtonType) && MobileUI.Instance != null)
            {
                MobileUI.Instance.UnlockButton(unlockButtonType);
            }
        }
    }
}
