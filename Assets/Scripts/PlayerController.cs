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
    private Animator animator;
    private Vector3 velocity;
    private Vector3 horizontalMomentum;
    private bool isGrounded;
    private bool wasGrounded;
    private float idleTimer;
    
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        
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
        UpdateAnimator();
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
        
        /* Get camera forward and right directions */
        Transform cam = Camera.main.transform;
        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;
        
        /* Flatten to horizontal plane */
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();
        
        /* Calculate movement direction relative to camera */
        Vector3 inputDir = new Vector3(horizontal, 0f, vertical).normalized;
        Vector3 moveDir = (camForward * vertical + camRight * horizontal).normalized;
        
        /* Determine speed (sprint or walk) */
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;
        
        /* Always allow rotation */
        if (inputDir.magnitude >= 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
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
                    Vector3 targetVelocity = moveDir * currentSpeed;
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
                    horizontalMomentum = moveDir * currentSpeed;
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
                Vector3 newDirection = Vector3.Lerp(horizontalMomentum.normalized, moveDir, airControl * Time.deltaTime * 10f);
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
            
            /* Trigger jump animation */
            if (animator != null)
            {
                animator.SetTrigger("Jump");
            }
        }
    }
    
    private void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
    
    private void UpdateAnimator()
    {
        if (animator == null) return;
        
        /* Calculate current speed for animator */
        float speed = horizontalMomentum.magnitude;
        
        /* Set animator parameters */
        animator.SetFloat("Speed", speed);
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsRunning", Input.GetKey(KeyCode.LeftShift) && speed > 0.1f);
        animator.SetBool("IsJumping", !isGrounded);
        
        /* Second idle animation trigger */
        if (speed < 0.1f && isGrounded)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer > 8f) /* 8 saniye sonra ikinci idle */
            {
                animator.SetTrigger("PlayIdle2");
                idleTimer = 0f;
            }
        }
        else
        {
            idleTimer = 0f;
        }
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
