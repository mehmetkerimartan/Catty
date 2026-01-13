using UnityEngine;

/// <summary>
/// Isometric character controller for the cat.
/// Handles WASD movement (character relative), jump, and sprint.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 12f;
    [SerializeField] private float sprintSpeed = 20f;
    [SerializeField] private float rotationSpeed = 20f;
    
    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 1f;
    [SerializeField] private float gravity = -40f;
    [SerializeField] private float airControl = 0.3f;
    
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.2f;
    [SerializeField] private LayerMask groundMask;
    
    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 horizontalMomentum;
    private bool isGrounded;
    private bool wasGrounded;
    
    /* Isometric camera angle (45 degrees rotated) */
    private readonly float isoAngle = 45f;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        if (groundCheck == null)
        {
            /* Create ground check point if not assigned */
            GameObject gc = new GameObject("GroundCheck");
            gc.transform.SetParent(transform);
            gc.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = gc.transform;
        }
    }
    
    void Update()
    {
        HandleGroundCheck();
        HandleMovement();
        HandleJump();
        ApplyGravity();
    }
    
    private void HandleGroundCheck()
    {
        /* Use CharacterController's built-in ground check */
        wasGrounded = isGrounded;
        isGrounded = controller.isGrounded;
        
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }
    
    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        Vector3 inputDir = new Vector3(horizontal, 0f, vertical).normalized;
        Vector3 isoDir = Quaternion.Euler(0, isoAngle, 0) * inputDir;
        
        /* Determine speed (sprint or walk) */
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;
        
        /* Always allow rotation */
        if (inputDir.magnitude >= 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(isoDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        if (isGrounded)
        {
            /* Ice friction handling */
            float friction = IceGround.IsOnIce ? IceGround.CurrentFriction : 1f;
            
            if (inputDir.magnitude >= 0.1f)
            {
                if (IceGround.IsOnIce)
                {
                    /* On ice: accelerate slowly, maintain momentum */
                    Vector3 targetVelocity = isoDir * currentSpeed;
                    horizontalMomentum = Vector3.Lerp(horizontalMomentum, targetVelocity, friction * Time.deltaTime * 5f);
                    
                    /* Clamp to max slide speed */
                    if (horizontalMomentum.magnitude > IceGround.CurrentMaxSlide)
                    {
                        horizontalMomentum = horizontalMomentum.normalized * IceGround.CurrentMaxSlide;
                    }
                }
                else
                {
                    /* Normal ground: full control */
                    horizontalMomentum = isoDir * currentSpeed;
                }
            }
            else
            {
                if (IceGround.IsOnIce)
                {
                    /* On ice: slow deceleration */
                    horizontalMomentum = Vector3.Lerp(horizontalMomentum, Vector3.zero, friction * Time.deltaTime * 2f);
                }
                else
                {
                    horizontalMomentum = Vector3.zero;
                }
            }
        }
        else
        {
            /* In air: can steer but keep same speed */
            if (inputDir.magnitude >= 0.1f)
            {
                float currentMomentumSpeed = horizontalMomentum.magnitude;
                Vector3 newDirection = Vector3.Lerp(horizontalMomentum.normalized, isoDir, airControl * Time.deltaTime * 10f);
                horizontalMomentum = newDirection.normalized * currentMomentumSpeed;
            }
        }
        
        /* Apply wind force */
        if (WindZone.IsInWind)
        {
            Vector3 windForce = WindZone.CurrentWindForce;
            
            /* Stronger effect in air */
            if (!isGrounded && WindZone.AffectsAir)
            {
                windForce *= WindZone.AirborneMultiplier;
            }
            
            horizontalMomentum += windForce * Time.deltaTime;
        }
        
        controller.Move(horizontalMomentum * Time.deltaTime);
    }
    
    private void HandleJump()
    {
        /* Use KeyCode.Space directly for reliability */
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            Debug.Log("Ziplama yapildi!");
        }
    }
    
    private void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
    
    /* Called when player falls off the map */
    public void Die()
    {
        GameManager.Instance.PlayerDied();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        /* Fall zone detection */
        if (other.CompareTag("DeathZone"))
        {
            Die();
        }
        
        /* Checkpoint detection */
        if (other.CompareTag("Checkpoint"))
        {
            Checkpoint cp = other.GetComponent<Checkpoint>();
            if (cp != null)
            {
                cp.Activate();
            }
        }
        
        /* Heart pickup detection */
        if (other.CompareTag("Pickup"))
        {
            HeartPickup heart = other.GetComponent<HeartPickup>();
            if (heart != null)
            {
                heart.Collect();
            }
        }
    }
}
