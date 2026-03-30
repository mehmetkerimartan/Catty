using UnityEngine;

/// <summary>
/// Ekranda parmak kaydırarak (Yukarı, Aşağı, Sağa, Sola) komut alan modern Swipe Manager.
/// Editör testleri için PC Faresinin (Mouse Click) bas-çek olayını da destekler.
/// </summary>
public class SwipeManager : MonoBehaviour
{
    private Vector2 startTouch, swipeDelta;
    private bool isDragging = false;
    
    // Her kareden (Frame) sadece 1 kereliğine okunabilen, public static bool "Olay" Değişkenleri
    public static bool swipeLeft, swipeRight, swipeUp, swipeDown;
    
    private void Update()
    {
        // Her frame'de komutları sıfırla ki aralıksız yana uçmasın
        swipeLeft = swipeRight = swipeUp = swipeDown = false;
        
        // 1. BİLGİSAYAR FARESİ İLE TEST ETME (Sol tık basılı tutup sürükleme)
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            startTouch = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            ResetVariables();
        }
        
        // 2. MOBİL DOKUNMATİK EKRAN (Telefondan Parmağı Sürtme)
        if (Input.touches.Length > 0)
        {
            if (Input.touches[0].phase == TouchPhase.Began)
            {
                isDragging = true;
                startTouch = Input.touches[0].position;
            }
            else if (Input.touches[0].phase == TouchPhase.Ended || Input.touches[0].phase == TouchPhase.Canceled)
            {
                isDragging = false;
                ResetVariables();
            }
        }
        
        // 3. FARK MESAFESİNİ HESAPLA
        swipeDelta = Vector2.zero;
        if (isDragging)
        {
            if (Input.touches.Length > 0)
                swipeDelta = Input.touches[0].position - startTouch;
            else if (Input.GetMouseButton(0))
                swipeDelta = (Vector2)Input.mousePosition - startTouch;
        }
        
        // 4. "ÖLÜ BÖLGE" KONTROLÜ (Değme kazalarını, ufak sürtmeleri reddet! > 125px)
        if (swipeDelta.sqrMagnitude > 125f * 125f)
        {
            float x = swipeDelta.x;
            float y = swipeDelta.y;
            
            // X ekseninde (Yatay) mı yoksa Y ekseninde (Dikey) mi DAHAAAA ÇOK kaydırılmış?
            if (Mathf.Abs(x) > Mathf.Abs(y))
            {
                if (x < 0) swipeLeft = true;
                else swipeRight = true;
            }
            else
            {
                if (y < 0) swipeDown = true;
                else swipeUp = true;
            }
            
            // İşi bitir ve tek kaydırmayla 5 defa atlamasın diye olayı sıfırla.
            ResetVariables();
        }
    }
    
    private void ResetVariables()
    {
        startTouch = swipeDelta = Vector2.zero;
        isDragging = false; // Birisi kaydırdı ve bıraktı!
    }
}
