using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class PlatformEdges : MonoBehaviour
{
    [Header("Kenar Çizgisi Ayarları")]
    public float lineThickness = 0.03f;
    [Tooltip("Normal dünyadaki (Mavi) çizgi rengi")]
    public Color normalColor = Color.black;
    [Tooltip("Kırmızı dünyadaki (Cehennem) çizgi rengi")]
    public Color hellColor = Color.white;

    private LineRenderer[] lines;
    private BoxCollider box;
    private Material lineMat; // Rengi sonradan da değiştirebilmek için saklıyoruz

    private Vector3[] localCorners = new Vector3[8];
    private int[,] edgePairs = new int[,]
    {
        {0,1}, {1,2}, {2,3}, {3,0},
        {4,5}, {5,6}, {6,7}, {7,4},
        {0,4}, {1,5}, {2,6}, {3,7}
    };
    
    private bool isValid = false;

    void Start()
    {
        box = GetComponent<BoxCollider>();
        
        // GİZLİ DUVAR ÇÖZÜMÜ
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr == null || !mr.enabled) return;

        isValid = true;

        Vector3 size = box.size;
        Vector3 center = box.center;

        localCorners[0] = center + new Vector3(-size.x, -size.y, -size.z) * 0.5f;
        localCorners[1] = center + new Vector3( size.x, -size.y, -size.z) * 0.5f;
        localCorners[2] = center + new Vector3( size.x, -size.y,  size.z) * 0.5f;
        localCorners[3] = center + new Vector3(-size.x, -size.y,  size.z) * 0.5f;
        
        localCorners[4] = center + new Vector3(-size.x,  size.y, -size.z) * 0.5f;
        localCorners[5] = center + new Vector3( size.x,  size.y, -size.z) * 0.5f;
        localCorners[6] = center + new Vector3( size.x,  size.y,  size.z) * 0.5f;
        localCorners[7] = center + new Vector3(-size.x,  size.y,  size.z) * 0.5f;

        lines = new LineRenderer[12];
        
        Shader lineShader = Shader.Find("Universal Render Pipeline/Unlit");
        if(lineShader == null) lineShader = Shader.Find("Unlit/Color");
        if(lineShader == null) lineShader = Shader.Find("Sprites/Default");
        
        lineMat = new Material(lineShader);

        // Başlangıç rengini ver
        if (lineMat.HasProperty("_BaseColor")) lineMat.SetColor("_BaseColor", normalColor);
        else if (lineMat.HasProperty("_Color")) lineMat.SetColor("_Color", normalColor);

        for (int i = 0; i < 12; i++)
        {
            GameObject lineObj = new GameObject("EdgeLine_" + i);
            lineObj.transform.SetParent(this.transform, false);

            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.material = lineMat;
            lr.startWidth = lineThickness;
            lr.endWidth = lineThickness;
            lr.positionCount = 2;
            lr.useWorldSpace = true; 
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;

            // Bazı materyaller renk ayarını Vertex'ten kabul eder, o yüzden bunu da verelim
            lr.startColor = normalColor;
            lr.endColor = normalColor;

            lines[i] = lr;
        }

        UpdateLines();
    }

    void LateUpdate()
    {
        if (isValid)
        {
            UpdateLines();
        }
    }

    void UpdateLines()
    {
        if (lines == null || lineMat == null) return;

        // 2. KUSURSUZ CEHENNEM DÜNYASI RADAR KONTROLÜ
        Color targetColor = normalColor;
        
        // Sadece fareye (sağ tık) basıldı mı diye değil, kırmızı alan bu platforma ulaştı mı diye kontrol et!
        if (RealityManager.Instance != null)
        {
            float radius = RealityManager.Instance.CurrentRadius;
            if (radius > 0f)
            {
                // Platformun merkezi ile Reality Tear (Kırmızı çember) arasındaki mesafeyi ölçüyoruz.
                float distance = Vector3.Distance(transform.position, RealityManager.Instance.transform.position);
                
                // Eğer platform o kırmızı alanın (balonun) içine girdiyse çizgileri beyaz (HellColor) yap!
                if (distance <= radius)
                {
                    targetColor = hellColor;
                }
            }
        }

        // Materyal rengini doğrudan değiştir (Böylece materyal her zaman doğru rengi gösterir)
        if (lineMat.HasProperty("_BaseColor")) lineMat.SetColor("_BaseColor", targetColor);
        else if (lineMat.HasProperty("_Color")) lineMat.SetColor("_Color", targetColor);

        for (int i = 0; i < 12; i++)
        {
            if (lines[i] == null) continue;
            
            // Hem materyalden hem çizgiden güncelleyelim (tam güvenlik için)
            lines[i].startColor = targetColor;
            lines[i].endColor = targetColor;

            // Pozisyonları dünyadaki yerlerine çek
            Vector3 worldPointA = transform.TransformPoint(localCorners[edgePairs[i, 0]]);
            Vector3 worldPointB = transform.TransformPoint(localCorners[edgePairs[i, 1]]);

            lines[i].SetPosition(0, worldPointA);
            lines[i].SetPosition(1, worldPointB);
        }
    }
}
