using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BucketEvent : MonoBehaviour
{
    [SerializeField] private TMP_Text bucketText;
    [SerializeField] private TMP_Text statisticsText;
    private FishSpawnManager fishSpawnManager;
    
    private int fishCount = 0; 
    private List<Fish> fishes;
    private Dictionary<string, int> fishInBucket = new Dictionary<string, int>(); 
    private bool isInitialized = false;
    
    // 任务系统：追踪桶中的鱼GameObject列表
    private List<GameObject> fishGameObjectsInBucket = new List<GameObject>();
    
    // 困難模式：追蹤魚的進入順序
    private List<GameObject> fishEntryOrder = new List<GameObject>();
    
    // 困難模式：鎖定的魚（不可取出）
    private HashSet<GameObject> lockedFish = new HashSet<GameObject>();
    
    // 當前是否為困難模式
    private bool isHardMode = false; 

    private void Awake()
    {
        // initialize dictionary (not relying on Generator)
        fishInBucket["redFish"] = 0;
        fishInBucket["blueFish"] = 0;
        fishInBucket["greenFish"] = 0;
        fishInBucket["grayFish"] = 0;
    }

    private void Start()
    {
        // 通过 ServiceLocator 获取 FishSpawnManager
        fishSpawnManager = ServiceLocator.Instance.Get<FishSpawnManager>();
        
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
            
            // 设置鱼的 isInBucket 状态
            FishForwardMovement fishMovement = other.GetComponent<FishForwardMovement>();
            if (fishMovement != null)
            {
                fishMovement.isInBucket = true;
                Debug.Log($"[BucketEvent] 设置 {fishTag} isInBucket = true");
            }
            
            // 添加到鱼GameObject列表（任务系统需要）
            if (!fishGameObjectsInBucket.Contains(other.gameObject))
            {
                fishGameObjectsInBucket.Add(other.gameObject);
                
                // 困難模式：記錄進入順序並鎖定
                if (isHardMode)
                {
                    fishEntryOrder.Add(other.gameObject);
                    lockedFish.Add(other.gameObject);
                    
                    // 通知 HardModeManager
                    if (HardModeManager.Instance != null)
                    {
                        HardModeManager.Instance.OnFishEnteredBucket(other.gameObject);
                    }
                    
                    Debug.Log($"[BucketEvent] 困難模式：{fishTag} 已鎖定 (順序: {fishEntryOrder.Count})");
                }
            }
            
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
            // 困難模式：如果魚已鎖定，阻止離開（強制放回）
            if (isHardMode && lockedFish.Contains(other.gameObject))
            {
                Debug.LogWarning($"[BucketEvent] 困難模式：{fishTag} 已鎖定，無法取出！");
                
                // 可選：這裡可以觸發視覺/聽覺反饋
                // 注意：物理上阻止需要在 GrabbableFish 中處理
                return;
            }
            
            Debug.Log($"[BucketEvent] {fishTag} 離開桶子");
            
            // 重置鱼的 isInBucket 状态
            FishForwardMovement fishMovement = other.GetComponent<FishForwardMovement>();
            if (fishMovement != null)
            {
                fishMovement.isInBucket = false;
                Debug.Log($"[BucketEvent] 设置 {fishTag} isInBucket = false");
            }
            
            // 从鱼GameObject列表中移除（任务系统需要）
            fishGameObjectsInBucket.Remove(other.gameObject);
            
            // 非困難模式：也從順序列表中移除
            if (!isHardMode)
            {
                fishEntryOrder.Remove(other.gameObject);
            }
            
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
        if (obj.CompareTag("grayFish")) return "grayFish";
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
    
    // ========== 任务系统接口 ==========
    
    /// <summary>
    /// 获取桶中的鱼GameObject列表（任务系统使用）
    /// </summary>
    public List<GameObject> GetFishInBucket()
    {
        return new List<GameObject>(fishGameObjectsInBucket);
    }
    
    /// <summary>
    /// 清空桶中的所有鱼（任务系统使用）
    /// </summary>
    public void ClearBucket()
    {
        Debug.Log($"[BucketEvent] 清空桶，销毁 {fishGameObjectsInBucket.Count} 条鱼");
        
        // 销毁所有桶中的鱼
        foreach (GameObject fish in fishGameObjectsInBucket)
        {
            if (fish != null)
            {
                Destroy(fish);
            }
        }
        
        // 清空列表
        fishGameObjectsInBucket.Clear();
        
        // 清空困難模式數據
        ClearHardModeData();
        
        // 重置计数
        fishCount = 0;
        foreach (string key in new List<string>(fishInBucket.Keys))
        {
            fishInBucket[key] = 0;
        }
        
        // 更新UI
        UpdateUI();
    }
    
    // ========== 困難模式接口 ==========
    
    /// <summary>
    /// 設置困難模式狀態
    /// </summary>
    public void SetHardMode(bool enabled)
    {
        isHardMode = enabled;
        Debug.Log($"[BucketEvent] 困難模式: {(enabled ? "啟用" : "停用")}");
        
        if (!enabled)
        {
            ClearHardModeData();
        }
    }
    
    /// <summary>
    /// 檢查是否為困難模式
    /// </summary>
    public bool IsHardMode()
    {
        return isHardMode;
    }
    
    /// <summary>
    /// 檢查魚是否已被鎖定（不可取出）
    /// </summary>
    public bool IsFishLocked(GameObject fish)
    {
        return isHardMode && lockedFish.Contains(fish);
    }
    
    /// <summary>
    /// 獲取魚的進入順序列表
    /// </summary>
    public List<GameObject> GetFishEntryOrder()
    {
        return new List<GameObject>(fishEntryOrder);
    }
    
    /// <summary>
    /// 獲取鎖定魚的數量
    /// </summary>
    public int GetLockedFishCount()
    {
        return lockedFish.Count;
    }
    
    /// <summary>
    /// 清空困難模式數據（用於重試）
    /// </summary>
    public void ClearHardModeData()
    {
        fishEntryOrder.Clear();
        lockedFish.Clear();
        Debug.Log("[BucketEvent] 困難模式數據已清空");
    }
    
    /// <summary>
    /// 重試困難模式任務：清空桶並重置狀態
    /// </summary>
    public void RetryHardModeTask()
    {
        if (!isHardMode)
        {
            Debug.LogWarning("[BucketEvent] RetryHardModeTask 只能在困難模式下使用");
            return;
        }
        
        Debug.Log("[BucketEvent] 重試困難模式任務");
        
        // 釋放所有桶中的魚到場景中（不銷毀）
        foreach (GameObject fish in fishGameObjectsInBucket)
        {
            if (fish != null)
            {
                // 解除鎖定
                lockedFish.Remove(fish);
                
                // 重置魚的狀態
                FishForwardMovement fishMovement = fish.GetComponent<FishForwardMovement>();
                if (fishMovement != null)
                {
                    fishMovement.isInBucket = false;
                }
                
                // 將魚移到桶外的隨機位置
                Vector3 releasePosition = transform.position + 
                    new Vector3(
                        UnityEngine.Random.Range(-2f, 2f),
                        UnityEngine.Random.Range(0.5f, 1.5f),
                        UnityEngine.Random.Range(-2f, 2f)
                    );
                fish.transform.position = releasePosition;
                
                string fishTag = GetFishTag(fish);
                if (!string.IsNullOrEmpty(fishTag))
                {
                    fishInBucket[fishTag] -= 1;
                    
                    Fish fishData = fishes.Find(f => f.color == fishTag);
                    if (fishData != null)
                    {
                        fishData.DecrementCaught();
                    }
                }
            }
        }
        
        // 清空列表
        fishGameObjectsInBucket.Clear();
        fishEntryOrder.Clear();
        lockedFish.Clear();
        fishCount = 0;
        
        UpdateUI();
        
        Debug.Log("[BucketEvent] 困難模式任務已重置，魚已釋放");
    }
}
