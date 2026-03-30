using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Modüler tasarladığın yol parçası (Prefab) modellerini arka arkaya dizerek 
/// oyunun hiç bitmemesini ve rastgelelik içeren sonsuz koşuyu sağlayan yönetici.
/// </summary>
public class TrackManager : MonoBehaviour
{
    [Header("Tasarım Ayarları")]
    [Tooltip("Hazırladığın 30 metrelik sokak/yol Prefablerini buraya ekle. (Normal Engeller, Reality Tear Bulmacalı Engeller vs.)")]
    public GameObject[] trackPrefabs;
    
    [Tooltip("Her bir yol parçasının Z eksenindeki net uzunluğu! Standart genelde 30'dur.")]
    public float trackLength = 30f;
    
    [Tooltip("Aynı anda ekranda kaç tane render edilmiş (aktif) yol parçası bulunsun? Kamera görüş mesafene göre ayarla.")]
    public int numberOfTracksVisible = 4;
    
    private float spawnZ = 0f; // Sıradaki parça haritada Z ekseninde nereye konacak?
    private Transform playerTransform;
    
    // Geçilen ve arkada çöp olan yolları silmek (bellek yönetimi) için listeliyoruz.
    private List<GameObject> activeTracks = new List<GameObject>();
    
    void Start()
    {
        playerTransform = FindObjectOfType<RunnerController>().transform;
        
        // Oyun başladığında ekranın dolması için baştan yolları diz.
        for (int i = 0; i < numberOfTracksVisible; i++)
        {
            if (i == 0) 
            {
                // İLK YOL GÜVENLİ OLMALIDIR (Engelsiz bir ilk prefab varsa Array'de 0. sıraya koyun).
                SpawnTrack(0);
            }
            else 
            {
                // Diğerleri rastgele engel çıksın
                SpawnTrack(Random.Range(0, trackPrefabs.Length));
            }
        }
    }
    
    void Update()
    {
        if (playerTransform == null) return;

        /* MAJÖR MANTIK: Kedi koştukça ve ilk yol parçasını geçtiğinde, 
         * en öne yepyeni rastgele bir yol parçası ser, arkada kalmış en eski yola elveda de (Destroy).
         */
        if (playerTransform.position.z - trackLength > spawnZ - (numberOfTracksVisible * trackLength))
        {
            SpawnTrack(Random.Range(0, trackPrefabs.Length));
            DeleteTrack();
        }
    }
    
    public void SpawnTrack(int trackIndex)
    {
        // Gelen Prefab'i sahneye sıradaki Z kordinatına bas.
        GameObject go = Instantiate(trackPrefabs[trackIndex], transform.forward * spawnZ, transform.rotation);
        activeTracks.Add(go);
        
        // Z Kırmızı ibreyi (Birleşme noktasını) yol mesafesi kadar ileri atarak bir sonraki sırayı beklet.
        spawnZ += trackLength;
    }
    
    private void DeleteTrack()
    {
        // Araba (Kedi) tamamen geçtiğinde listedeki (ve hafızadaki) en eski index[0] yolunu yak yık.
        Destroy(activeTracks[0]);
        activeTracks.RemoveAt(0);
    }
}
