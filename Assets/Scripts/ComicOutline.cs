using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ComicOutline : MonoBehaviour
{
    [Header("Çizgi Ayarları")]
    [Tooltip("Çizginin kalınlığı")]
    public float outlineThickness = 0.03f;
    public Color outlineColor = Color.black;

    void Start()
    {
        CreateOutline();
    }

    void CreateOutline()
    {
        MeshFilter originalFilter = GetComponent<MeshFilter>();
        if (originalFilter.mesh == null) return;

        // Çizgiyi oluşturacak yeni bir alt obje yarat
        GameObject outlineObj = new GameObject("ComicOutlineMesh");
        outlineObj.transform.SetParent(this.transform, false);
        
        MeshFilter mf = outlineObj.AddComponent<MeshFilter>();
        MeshRenderer mr = outlineObj.AddComponent<MeshRenderer>();

        // Orijinal objenin mesh'ini kopyala
        Mesh outlineMesh = Instantiate(originalFilter.mesh);

        // 1. Tersyüz Etme (Inverted Hull): Sadece en arkada kalan kısmın görülmesi için 
        int[] triangles = outlineMesh.triangles;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int temp = triangles[i];
            triangles[i] = triangles[i + 1];
            triangles[i + 1] = temp;
        }
        outlineMesh.triangles = triangles;
        mf.mesh = outlineMesh;

        // 2. Skalayı Büyüterek Kalınlık Verme (Objenin kendi büyüklüğüyle çakışmaması için matematiksel oranlama)
        // Böylece zemin ne kadar geniş/uzun olursa olsun, çizgi her tarafta "eşit" kalınlıkta kalır.
        Vector3 pScale = transform.lossyScale;
        float t = outlineThickness * 2f; 
        outlineObj.transform.localScale = new Vector3(
            1f + (t / Mathf.Max(pScale.x, 0.001f)),
            1f + (t / Mathf.Max(pScale.y, 0.001f)),
            1f + (t / Mathf.Max(pScale.z, 0.001f))
        );

        // 3. Materyal (Siyah Boya) Ayarı
        Shader outlineShader = Shader.Find("Universal Render Pipeline/Unlit");
        if (outlineShader == null) outlineShader = Shader.Find("Unlit/Color");
        if (outlineShader == null) outlineShader = GetComponent<MeshRenderer>().material.shader;

        Material outlineMat = new Material(outlineShader);
        if (outlineMat.HasProperty("_BaseColor")) outlineMat.SetColor("_BaseColor", outlineColor);
        if (outlineMat.HasProperty("_Color")) outlineMat.SetColor("_Color", outlineColor);
        
        mr.material = outlineMat;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;
    }
}
