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
    
    [Header("Movement Polish")]
    [SerializeField] private float groundDeceleration = 80f;
    
    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 1f;
    [SerializeField] private float gravity = -40f;
    [SerializeField] [Range(0f, 1f)] private float airControl = 0.5f;
    [SerializeField] private float airAcceleration = 8f;
    
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundMask;
    
    [Header("Squash & Stretch")]
    [SerializeField] private Transform visualGroup;
    [SerializeField] private float squashStrength = 0.15f;
    [SerializeField] private float squashSmoothSpeed = 8f;
    [SerializeField] private float startRunStretch = 0.25f;

    [Header("Lean (Visual Polish)")]
    [SerializeField] private float sideLeanStrength = 15f;
    [SerializeField] private float forwardLeanStrength = 10f;
    [SerializeField] private float leanSmoothSpeed = 10f;
    
    [Header("Landing Impact")]
    [SerializeField] private float minImpactVelocity = 5f;
    [SerializeField] private float maxImpactVelocity = 20f;
    [SerializeField] private float landingSquashMultiplier = 2f;
    [SerializeField] private ParticleSystem landingDustPrefab;
    
    
    [Header("Advanced Jump")]
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.2f;
    
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private bool sprintToggled = false;       /* For controller toggle sprint */
    private bool isSprinting = false;         /* Current sprint state for animator */
    private bool wasSprintingBeforeJump = false; /* Remember sprint state through jump */
    
    private CharacterController controller;
    private Animator animator;
    private Vector3 velocity;
    private Vector3 horizontalMomentum;
    private bool isGrounded;
    private bool wasGrounded;
    private float idleTimer;
    private float targetTurnDirection;
    private float smoothTurnDirection;
    private Vector3 targetScale = Vector3.one;
    private Vector3 currentScale = Vector3.one;
    private bool wasMoving = false;
    private float runStartTimer = 0f;
    
    
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
        
        if (visualGroup == null)
        {
            /* If not assigned, assume it's the first child (rig/model) */
            if (transform.childCount > 0) visualGroup = transform.GetChild(0);
        }
    }
    
    void Update()
    {
        HandleGroundCheck();
        HandleMovement();
        HandleJump();
        ApplyGravity();
        
        /* SINGLE MOVE CALL: All movement combined to prevent jitter */
        Vector3 finalMove = horizontalMomentum + (Vector3.up * velocity.y);
        controller.Move(finalMove * Time.deltaTime);

        UpdateAnimator();
        HandleSquashStretch();
        HandleLean();
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
        
        /* Detect landing for squash effect */
        if (isGrounded && !wasGrounded)
        {
            HandleLandingImpact();
            
            /* Restore sprint state if we were sprinting before jump */
            if (wasSprintingBeforeJump)
            {
                sprintToggled = true;
                wasSprintingBeforeJump = false;
            }
        }
    }
    
    private void HandleLandingImpact()
    {
        /* Calculate impact intensity based on fall velocity */
        float impactVelocity = Mathf.Abs(velocity.y);
        float impactRatio = Mathf.InverseLerp(minImpactVelocity, maxImpactVelocity, impactVelocity);
        
        if (impactRatio > 0.1f)
        {
            /* Stronger squash for harder landings */
            float actualSquash = squashStrength * (1f + impactRatio * landingSquashMultiplier);
            currentScale = new Vector3(1f + actualSquash, 1f - actualSquash, 1f + actualSquash);
            
            /* Camera shake */
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.Shake(impactRatio * 0.1f, 0.15f);
            }
            
            /* Landing dust particles */
            if (landingDustPrefab != null)
            {
                ParticleSystem dust = Instantiate(landingDustPrefab, transform.position, Quaternion.identity);
                var main = dust.main;
                main.startSizeMultiplier = 0.5f + impactRatio;
                Destroy(dust.gameObject, 2f);
            }
        }
        else
        {
            /* Normal squash for small drops */
            currentScale = new Vector3(1f + squashStrength, 1f - squashStrength, 1f + squashStrength);
        }
    }
    
    private void HandleMovement()
    {
        /* Use GetAxisRaw for instant response - still works with controllers */
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
        
        /* Toggle sprint with LB (controller) - keyboard shift still works as hold */
        if (Input.GetKeyDown(KeyCode.JoystickButton4)) /* LB pressed */
        {
            sprintToggled = !sprintToggled;
        }
        
        /* Reset sprint toggle when player stops moving - ONLY ON GROUND */
        if (isGrounded && inputDir.magnitude < 0.1f)
        {
            sprintToggled = false;
        }
        
        /* Determine speed - Keyboard: hold Shift, Controller: toggle LB */
        isSprinting = Input.GetKey(KeyCode.LeftShift) || sprintToggled;
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;
        
        /* Detect movement start for stretch effect */
        bool isMoving = inputDir.magnitude >= 0.1f;
        if (isMoving && !wasMoving && isGrounded)
        {
            /* Start running stretch: thin XZ, stretch Y (anticipation) */
            currentScale = new Vector3(1f - startRunStretch * 0.5f, 1f + startRunStretch, 1f - startRunStretch * 0.5f);
        }
        wasMoving = isMoving;
        
        /* Always allow rotation */
        if (isMoving)
        {
            /* Calculate target turn direction before rotating */
            float angle = Vector3.SignedAngle(transform.forward, moveDir, Vector3.up);
            
            /* Proportional target for smoother blending. 
               45 degrees difference = full lean. Adjust this value to change sensitivity. */
            targetTurnDirection = Mathf.Clamp(angle / 45f, -1f, 1f);
            
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            targetTurnDirection = 0f;
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
                    /* Instant acceleration - immediate response */
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
                    /* ZELDA-STYLE: Smooth deceleration when stopping */
                    horizontalMomentum = Vector3.MoveTowards(
                        horizontalMomentum,
                        Vector3.zero,
                        groundDeceleration * Time.deltaTime
                    );
                }
            }
        }
        else
        {
            /* In air: ALWAYS preserve momentum, only allow steering */
            float currentAirSpeed = horizontalMomentum.magnitude;
            
            if (inputDir.magnitude >= 0.1f)
            {
                if (currentAirSpeed > 0.1f)
                {
                    /* Has momentum: only allow steering, preserve speed */
                    Vector3 newDirection = Vector3.Lerp(
                        horizontalMomentum.normalized, 
                        moveDir, 
                        airControl * Time.deltaTime * 8f
                    );
                    horizontalMomentum = newDirection.normalized * currentAirSpeed;
                }
                else
                {
                    /* No momentum (jumped from standstill): allow building up air speed */
                    float maxAirSpeed = currentSpeed * airControl;
                    Vector3 targetAirVelocity = moveDir * maxAirSpeed;
                    horizontalMomentum = Vector3.MoveTowards(
                        horizontalMomentum, 
                        targetAirVelocity, 
                        airAcceleration * Time.deltaTime
                    );
                }
            }
            /* No input in air: maintain momentum completely (no air resistance) */
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
    }
    
    private void HandleJump()
    {
        /* Coyote Time Management: allows jumping shortly after leaving ground */
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        /* Jump Buffer Management: remembers jump button press shortly before landing */
        /* Jump input: Keyboard Space OR Controller A button */
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        /* Execute Jump if both conditions are met */
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            /* Save toggle state before jump (controller only) */
            wasSprintingBeforeJump = sprintToggled;
            
            /* Calculate jump velocity */
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            
            /* Reset counters to prevent double-jumping or stale buffers */
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
            
            /* Trigger jump animation */
            if (animator != null)
            {
                animator.SetTrigger("Jump");
            }
            
            /* Stretch on jump: thin XZ, grow Y */
            currentScale = new Vector3(1f - squashStrength, 1f + squashStrength * 1.5f, 1f - squashStrength);
        }
    }
    
    private void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
    }
    
    private void UpdateAnimator()
    {
        if (animator == null) return;
        
        /* Calculate current speed for animator */
        float speed = horizontalMomentum.magnitude;
        
        /* Smooth the turn direction for better animation blending */
        smoothTurnDirection = Mathf.Lerp(smoothTurnDirection, targetTurnDirection, Time.deltaTime * 5f);
        
        /* Set animator parameters */
        animator.SetFloat("Speed", speed);
        animator.SetFloat("TurnDirection", smoothTurnDirection);
        
        animator.SetBool("IsGrounded", isGrounded);
        
        /* IsRunning based on INPUT, not actual speed - this prevents animation lag after landing */
        float inputMag = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).magnitude;
        bool isRunning = isSprinting && inputMag > 0.1f;
        
        animator.SetBool("IsRunning", isRunning);
        
        /* Sync animation speed with movement speed to prevent foot sliding */
        /* Walk animation base speed: 12, Run animation base speed: 20 */
        if (speed > 0.1f)
        {
            float baseSpeed = isSprinting ? sprintSpeed : walkSpeed;
            float targetAnimSpeed = speed / baseSpeed;
            /* Smooth the animation speed change to prevent micro-stutters */
            animator.speed = Mathf.Lerp(animator.speed, Mathf.Clamp(targetAnimSpeed, 0.5f, 1.5f), Time.deltaTime * 10f);
        }
        else
        {
            animator.speed = Mathf.Lerp(animator.speed, 1f, Time.deltaTime * 10f);
        }
        
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
    
    private void HandleSquashStretch()
    {
        if (visualGroup == null) return;
        
        /* Smoothly return to normal scale */
        currentScale = Vector3.Lerp(currentScale, Vector3.one, squashSmoothSpeed * Time.deltaTime);
        visualGroup.localScale = currentScale;
    }
    
    public float GetCurrentSpeed()
    {
        return horizontalMomentum.magnitude;
    }

    private void HandleLean()
    {
        if (visualGroup == null) return;

        /* Side Lean: based on turn direction (smoothTurnDirection) */
        float targetSideLean = -smoothTurnDirection * sideLeanStrength;

        /* Forward Lean: based on sprinting state and speed */
        float speedPercent = horizontalMomentum.magnitude / sprintSpeed;
        float targetForwardLean = (isSprinting && speedPercent > 0.5f) ? forwardLeanStrength : 0f;

        /* Apply rotations smoothly */
        Quaternion targetLeanRotation = Quaternion.Euler(targetForwardLean, 0f, targetSideLean);
        visualGroup.localRotation = Quaternion.Lerp(
            visualGroup.localRotation, 
            targetLeanRotation, 
            leanSmoothSpeed * Time.deltaTime
        );
    }
}
