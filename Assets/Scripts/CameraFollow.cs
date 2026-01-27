using UnityEngine;

/// <summary>
/// Camera that follows the player smoothly.
/// Supports Isometric, Rotating Follow, and Stray-style Free Look.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    public enum CameraMode { Isometric, RotatingFollow, StrayStyle }

    [Header("Target")]
    [SerializeField] private Transform target;
    
    [Header("Mode Selection")]
    [SerializeField] private CameraMode mode = CameraMode.StrayStyle;
    
    [Header("General Settings")]
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private Vector3 strayOffset = new Vector3(0f, 3.5f, -9f);
    [SerializeField] private Vector3 isoOffset = new Vector3(0f, 8f, -8f);
    
    [Header("Stray Style Settings")]
    [SerializeField] private float mouseSensitivity = 3f;
    [SerializeField] private float autoAlignSpeed = 1.5f;
    [SerializeField] private float minPitch = -10f;
    [SerializeField] private float maxPitch = 60f;
    
    [Header("Dynamic FOV")]
    [SerializeField] private float baseFOV = 70f;
    [SerializeField] private float maxFOV = 85f;
    [SerializeField] private float fovSmoothSpeed = 4f;

    private Camera cam;
    private PlayerController playerController;
    private float currentYaw;
    private float currentPitch = 30f;
    private Vector3 lastPlayerPosition;
    private float idleCameraTimer;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null) cam = GetComponentInChildren<Camera>();
        
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
        }
        
        if (target != null)
        {
            playerController = target.GetComponent<PlayerController>();
            lastPlayerPosition = target.position;
            currentYaw = target.eulerAngles.y;
            
            /* Trap cursor for Stray feel */
            if (mode == CameraMode.StrayStyle)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        float hInput = Input.GetAxisRaw("Horizontal");
        float vInput = Input.GetAxisRaw("Vertical");
        bool isMoving = (new Vector3(hInput, 0, vInput)).magnitude > 0.1f;

        float mouseX = Input.GetAxis("Mouse X") + SafeGetAxis("Joystick X");
        float mouseY = Input.GetAxis("Mouse Y") + SafeGetAxis("Joystick Y");
        bool hasCameraInput = Mathf.Abs(mouseX) > 0.01f || Mathf.Abs(mouseY) > 0.01f;

        if (mode == CameraMode.StrayStyle)
        {
            HandleStrayCamera(mouseX, mouseY, isMoving, hasCameraInput);
        }
        else if (mode == CameraMode.RotatingFollow)
        {
            currentYaw = Mathf.LerpAngle(currentYaw, target.eulerAngles.y, 3f * Time.deltaTime);
            currentPitch = 40f;
            ApplyStandardFollow(isoOffset);
        }
        else // Isometric
        {
            currentYaw = 0f;
            currentPitch = 40f;
            ApplyStandardFollow(isoOffset);
        }

        UpdateFOV();
    }

    private float SafeGetAxis(string axisName)
    {
        try
        {
            return Input.GetAxis(axisName);
        }
        catch
        {
            return 0f;
        }
    }

    private void HandleStrayCamera(float mouseX, float mouseY, bool isMoving, bool hasCameraInput)
    {
        /* Manual control */
        currentYaw += mouseX * mouseSensitivity;
        currentPitch -= mouseY * mouseSensitivity;
        currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);

        /* Auto-alignment: if moving and no manual camera input, align behind cat */
        if (hasCameraInput)
        {
            idleCameraTimer = 0f;
        }
        else if (isMoving)
        {
            idleCameraTimer += Time.deltaTime;
            if (idleCameraTimer > 1f) /* 1 second of movement without cam input = start aligning */
            {
                float targetYaw = target.eulerAngles.y;
                currentYaw = Mathf.LerpAngle(currentYaw, targetYaw, autoAlignSpeed * Time.deltaTime);
            }
        }

        /* Calculate position */
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
        Vector3 desiredPosition = target.position + (rotation * strayOffset);

        /* Collision Check (Subtle) */
        if (Physics.SphereCast(target.position + Vector3.up, 0.2f, (desiredPosition - target.position).normalized, out RaycastHit hit, strayOffset.magnitude))
        {
            desiredPosition = hit.point + hit.normal * 0.2f;
        }

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.LookAt(target.position + Vector3.up * 0.5f);
    }

    private void ApplyStandardFollow(Vector3 currentOffset)
    {
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
        Vector3 desiredPosition = target.position + (rotation * currentOffset);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
    }

    private void UpdateFOV()
    {
        if (cam == null || playerController == null) return;
        
        float currentSpeed = playerController.GetCurrentSpeed();
        float speedPercent = Mathf.InverseLerp(12f, 20f, currentSpeed);
        float targetFOV = Mathf.Lerp(baseFOV, maxFOV, speedPercent);
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, fovSmoothSpeed * Time.deltaTime);
    }
}
