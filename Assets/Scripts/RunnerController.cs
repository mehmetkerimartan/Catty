using UnityEngine;

/// <summary>
/// Subway Surfers tarzı aralıksız koşan 3-Şeritli (Lane) Kedi Kontrolcüsü.
/// Serbest dolaşan eski PlayerController'ın Endless Runner versiyonudur.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class RunnerController : MonoBehaviour
{
    public static RunnerController Instance;
    
    [Header("Sonsuz Koşu Hızı")]
    public float forwardSpeed = 15f;
    public float maxSpeed = 40f;
    public float acceleration = 0.2f;
    
    [Header("3-Şerit (Lane) Ayarları")]
    public float laneDistance = 3.5f; // Şeritler arası x mesafesi (Sol: -3.5, Orta: 0, Sağ: 3.5)
    public float laneSwitchSpeed = 12f; // Şerit değiştirme kaysma hızı
    
    // 0 = Sol, 1 = Orta, 2 = Sağ
    private int desiredLane = 1;
    
    [Header("Zıplama")]
    public float jumpForce = 8f;
    public float gravity = -25f;
    private float velocityY;
    
    private CharacterController controller;
    private Animator animator;
    
    // Animasyon Değişkenleri
    private readonly int SpeedHash = Animator.StringToHash("Speed");
    private readonly int JumpHash = Animator.StringToHash("Jump");
    private readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    
    void Awake()
    {
        Instance = this;
    }
    
    void Start()
    {
        // EKRANA MOBİL TUŞ ÇİZİCİYİ KAPAT! (Subway surfersta tuş yok kaydırma var)
        if (MobileUI.Instance != null) MobileUI.Instance.gameObject.SetActive(false);
        // Eski serbest yürüyüş kodunu pasif yap (çakışmasınlar)
        if (PlayerController.Instance != null) PlayerController.Instance.enabled = false;

        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }
    
    void Update()
    {
        // 1. Z-Ekseninde sürekli giderek hızlanma (Sonsuz zorlaşma)
        if (forwardSpeed < maxSpeed)
        {
            forwardSpeed += acceleration * Time.deltaTime;
        }
        
        if (animator != null)
        {
            animator.SetFloat(SpeedHash, forwardSpeed);
            animator.SetBool(IsGroundedHash, controller.isGrounded);
        }
        
        // 2. Mobilde Parmak Kaydırmalarını/PC Tuşlarını Oku
        HandleInputs();
        
        // 3. Hedef Şeridin Pozisyonu
        Vector3 targetPosition = transform.position;
        if (desiredLane == 0) targetPosition.x = -laneDistance; // Sol
        else if (desiredLane == 1) targetPosition.x = 0;        // Orta
        else if (desiredLane == 2) targetPosition.x = laneDistance; // Sağ
        
        // 4. Zıplama & Yerçekimi (Havada süzülme fiziği)
        if (controller.isGrounded && velocityY < 0)
        {
            velocityY = -2f; 
        }
        
        if (controller.isGrounded && (SwipeManager.swipeUp || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)))
        {
            Jump();
        }
        
        velocityY += gravity * Time.deltaTime;
        
        // 5. SON HAREKETİ UYGULA (Hepsini Bireştir)
        Vector3 moveVector = Vector3.zero;
        
        // X Ekseni: Sağ/Sol şeride zarifçe kayma pürüzsüzleştirildi (Lerp)
        moveVector.x = (targetPosition.x - transform.position.x) * laneSwitchSpeed;
        
        // Y Ekseni: Zıplama yerçekimi
        moveVector.y = velocityY;
        
        // Z Ekseni: Mükemmel ve hiç durmayan ileri atılım!
        moveVector.z = forwardSpeed;
        
        controller.Move(moveVector * Time.deltaTime);
    }
    
    private void HandleInputs()
    {
        // Sağa Kaydırma -> Sağ Şeride Geç
        if (SwipeManager.swipeRight || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            desiredLane++;
            if (desiredLane > 2) desiredLane = 2; // Duvardan dışarı taşmasın
        }
        // Sola Kaydırma -> Sol Şeride Geç
        else if (SwipeManager.swipeLeft || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            desiredLane--;
            if (desiredLane < 0) desiredLane = 0;
        }
    }
    
    private void Jump()
    {
        velocityY = Mathf.Sqrt(jumpForce * -2f * gravity);
        if (animator != null) animator.SetTrigger(JumpHash);
    }
}
