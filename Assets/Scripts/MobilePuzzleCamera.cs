using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MobilePuzzleCamera : MonoBehaviour
{
    [Header("Takip Edilecek Obje")]
    [Tooltip("Karakterini (Player) buraya sürükle bırak.")]
    public Transform target;

    [Header("Perspektif Kamera Ayarları")]
    [Tooltip("Kameranın görüş açısı (Küçülttükçe yakınlaşır, büyüttükçe genişler).")]
    public float fieldOfView = 60f; 
    
    [Tooltip("Kameranın karaktere olan uzaklığı (Kedinin arkasından +X yönüne bakarken).")]
    public Vector3 offset = new Vector3(-8f, 5f, 0f); 
    
    [Tooltip("Sırtının arkasından hafif aşağıya bakış açısı.")]
    public Vector3 rotation = new Vector3(25f, 90f, 0f); 

    [Tooltip("Kameranın karakteri takip etme yumuşaklığı.")]
    public float smoothSpeed = 8f; 

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        
        // 1. Kamera modunu Perspektif yap (Derinlik hissi geri gelsin)
        cam.orthographic = false;
        cam.fieldOfView = fieldOfView;
        
        // 2. Kameranın dönüşünü belirle
        transform.rotation = Quaternion.Euler(rotation);

        // 3. Hedef yoksa otomatik bul
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }

        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        cam.fieldOfView = fieldOfView; // Oyundayken canlı canlı ayarı değiştirebilmen için

        Vector3 desiredPosition = target.position + offset;
        
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        
        transform.position = smoothedPosition;
    }
}
