using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BucketEvent : MonoBehaviour
{
    [SerializeField] private TMP_Text bucketText;
    [SerializeField] private TMP_Text statisticsText;
    [SerializeField] private FishSpawnManager fishSpawnManager;
    
    private int fishCount = 0; 
    private List<Fish> fishes;
    private Dictionary<string, int> fishInBucket = new Dictionary<string, int>(); 
    private bool isInitialized = false; 

    private void Awake()
    {
        // initialize dictionary (not relying on Generator)
        fishInBucket["redFish"] = 0;
        fishInBucket["blueFish"] = 0;
        fishInBucket["greenFish"] = 0;
    }

    private void Start()
    {
        // initialize Fish data in Start to ensure Generator is ready
        fishes = fishSpawnManager != null ? fishSpawnManager.GetFish() : new List<Fish>();
        isInitialized = true;
        
        // 初始化 UI 顯示
        UpdateUI();
    }

    private void OnTriggerEnter(Collider other)
    {
        // make sure initialized.
        if (!isInitialized) return;
        
        string fishTag = GetFishTag(other.gameObject);
        
        if (!string.IsNullOrEmpty(fishTag))
        {
            Debug.Log($"[BucketEvent] {fishTag} 進入桶子");
            
            fishCount += 1;
            fishInBucket[fishTag] += 1;
            
            // update corresponding Fish object's caught amount
            Fish fishData = fishes.Find(f => f.color == fishTag);
            if (fishData != null)
            {
                fishData.IncrementCaught();
            }
            
            UpdateUI();
            PrintStatistics();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // can add logic for fish staying in the bucket here
    }

    private void OnTriggerExit(Collider other)
    {
        // make sure initialized.
        if (!isInitialized) return;
        
        string fishTag = GetFishTag(other.gameObject);
        
        if (!string.IsNullOrEmpty(fishTag))
        {
            Debug.Log($"[BucketEvent] {fishTag} 離開桶子");
            
            fishCount -= 1;
            fishInBucket[fishTag] -= 1;
            
            // update corresponding Fish object's caught amount
            Fish fishData = fishes.Find(f => f.color == fishTag);
            if (fishData != null)
            {
                fishData.DecrementCaught();
            }
            
            UpdateUI();
            PrintStatistics();
        }
    }

    /// <summary>
    /// get fish tag from GameObject
    /// </summary>
    private string GetFishTag(GameObject obj)
    {
        if (obj.CompareTag("redFish")) return "redFish";
        if (obj.CompareTag("blueFish")) return "blueFish";
        if (obj.CompareTag("greenFish")) return "greenFish";
        return null;
    }

    /// <summary>
    /// UPdate UI display
    /// </summary>
    private void UpdateUI()
    {
        // make sure initialized
        if (!isInitialized || fishes == null) return;
        
        //update bucket fish count display
        if (bucketText != null)
        {
            bucketText.text = $"Fish in bucket: {fishCount}";
        }

        // update detailed statistics display
        if (statisticsText != null)
        {
            string stats = "=== Caught Information ===\n";
            
            foreach (Fish f in fishes)
            {
                stats += $"{GetFishDisplayName(f.color)}: {f.caughtAmount}/{f.spawnedAmount} ";
                stats += $"({f.GetProgress():P0})\n";
            }
            
            int totalCaught = fishSpawnManager != null ? fishSpawnManager.GetTotalCaughtCount() : 0;
            int totalSpawned = fishSpawnManager != null ? fishSpawnManager.GetTotalSpawnedCount() : 0;
            stats += $"\nTotal: {totalCaught}/{totalSpawned}";
            
            statisticsText.text = stats;
        }
    }

    /// <summary>
    /// Print detailed fish statistics to Console
    /// </summary>
    private void PrintStatistics()
    {
        // make sure initialized
        if (!isInitialized || fishes == null) return;
        
        Debug.Log("==================== 魚類統計 ====================");
        foreach (Fish f in fishes)
        {
            Debug.Log(f.ToString());
        }
        
        Debug.Log($"桶內魚數: RedFish {fishInBucket["redFish"]} | BlueFish {fishInBucket["blueFish"]} | GreenFish {fishInBucket["greenFish"]}");
        
        if (fishSpawnManager != null)
        {
            Debug.Log($"總捕獲進度: {fishSpawnManager.GetTotalCaughtCount()}/{fishSpawnManager.GetTotalSpawnedCount()}");
        }
        Debug.Log("=================================================");
    }

    /// <summary>
    /// get fish display name
    /// </summary>
    private string GetFishDisplayName(string tag)
    {
        switch (tag)
        {
            case "redFish": return "redFish";
            case "blueFish": return "blueFish";
            case "greenFish": return "greenFish";
            default: return tag;
        }
    }

    /// <summary>
    /// check if all fish are caught
    /// </summary>
    public bool IsAllFishCaught()
    {
        if (!isInitialized || fishes == null) return false;
        
        foreach (Fish f in fishes)
        {
            if (!f.IsAllCaught())
                return false;
        }
        return true;
    }

    /// <summary>
    ///get overall progress
    /// </summary>
    public float GetOverallProgress()
    {
        if (!isInitialized || fishSpawnManager == null) return 0f;
        
        int totalSpawned = fishSpawnManager.GetTotalSpawnedCount();
        if (totalSpawned == 0) return 0f;
        
        int totalCaught = fishSpawnManager.GetTotalCaughtCount();
        return (float)totalCaught / totalSpawned;
    }
}
