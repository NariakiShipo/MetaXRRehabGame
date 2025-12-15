using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 多水桶管理器 - 管理困難模式中的多個水桶
/// 
/// 設計理念：
/// - 將水桶數量對應任務階段數，降低認知負荷
/// - 每個水桶對應一個任務階段，玩家只需看著對應水桶完成任務
/// - 將「序列記憶」轉化為「空間對應」
/// </summary>
public class MultiBucketManager : MonoBehaviour
{
    [Header("水桶物件池（困難模式）")]
    [Tooltip("場景中預先放置的困難模式水桶物件（最多對應最大階段數）")]
    [SerializeField] private List<GameObject> bucketPool = new List<GameObject>();
    
    [Header("簡單/普通模式水桶")]
    [Tooltip("簡單和普通模式使用的單一水桶")]
    [SerializeField] private GameObject normalModeBucket;
    
    [Header("水桶標籤（選用）")]
    [Tooltip("每個水桶上方顯示任務的文字")]
    [SerializeField] private List<TMP_Text> bucketLabels = new List<TMP_Text>();    
    [Tooltip("每個水桶上方顯示魚顏色的圖片")]
    [SerializeField] private List<Image> bucketFishImages = new List<Image>();
    
    [Header("魚顏色圖片 Sprites")]
    [Tooltip("紅色魚的圖片")]
    [SerializeField] private Sprite redFishSprite;
    [Tooltip("灰色魚的圖片")]
    [SerializeField] private Sprite grayFishSprite;
    [Tooltip("綠色魚的圖片")]
    [SerializeField] private Sprite greenFishSprite;
    [Tooltip("黃色魚的圖片")]
    [SerializeField] private Sprite yellowFishSprite;
    [Tooltip("藍色魚的圖片")]
    [SerializeField] private Sprite blueFishSprite;    
    [Header("水桶容量設定")]
    [Tooltip("是否在水桶滿時彈出多餘的魚")]
    [SerializeField] private bool ejectExcessFish = true;
    
    [Header("視覺反饋")]
    [Tooltip("階段完成時的顏色")]
    [SerializeField] private Color completedColor = new Color(0.3f, 0.8f, 0.3f, 1f);
    [Tooltip("當前階段的顏色")]
    [SerializeField] private Color activeColor = new Color(1f, 1f, 1f, 1f);
    [Tooltip("未啟用階段的顏色")]
    [SerializeField] private Color inactiveColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    
    [Header("事件")]
    public UnityEvent<int> OnBucketStageCompleted;     // 當某個水桶階段完成
    public UnityEvent OnAllStagesCompleted;             // 當所有階段都完成
    public UnityEvent<int, string> OnBucketError;       // 當水桶發生錯誤 (桶索引, 錯誤訊息)
    
    // 當前任務配置
    private List<TaskStage> currentStages = new List<TaskStage>();
    private int activeBucketCount = 0;
    
    // 每個水桶的 BucketEvent 組件快取
    private List<BucketEvent> bucketEvents = new List<BucketEvent>();
    
    // 普通模式水桶的 BucketEvent 組件
    private BucketEvent normalModeBucketEvent;
    
    // 當前是否為困難模式
    private bool isHardMode = false;
    
    // 每個水桶的完成狀態
    private List<bool> bucketCompleted = new List<bool>();
    
    // 單例
    public static MultiBucketManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // 初始化事件
        if (OnBucketStageCompleted == null) OnBucketStageCompleted = new UnityEvent<int>();
        if (OnAllStagesCompleted == null) OnAllStagesCompleted = new UnityEvent();
        if (OnBucketError == null) OnBucketError = new UnityEvent<int, string>();
        
        // 初始化水桶組件快取
        InitializeBucketComponents();
    }
    
    private void Start()
    {
        // 預設隱藏所有水桶
        HideAllBuckets();
        
        // 預設啟用普通模式水桶
        ActivateNormalMode();
    }
    
    /// <summary>
    /// 初始化水桶組件快取
    /// </summary>
    private void InitializeBucketComponents()
    {
        bucketEvents.Clear();
        
        Debug.Log("===========================================");
        Debug.Log("[MultiBucketManager] 開始初始化水桶組件...");
        
        // 初始化普通模式水桶
        if (normalModeBucket != null)
        {
            normalModeBucketEvent = normalModeBucket.GetComponent<BucketEvent>();
            if (normalModeBucketEvent != null)
            {
                Debug.Log($"[MultiBucketManager] ✅ 普通模式水桶 {normalModeBucket.name} - BucketEvent 已找到");
            }
            else
            {
                Debug.LogWarning($"[MultiBucketManager] ⚠️ 普通模式水桶 {normalModeBucket.name} 沒有 BucketEvent 組件！");
            }
        }
        else
        {
            Debug.LogWarning("[MultiBucketManager] ⚠️ 普通模式水桶未設置！");
        }
        
        // 初始化困難模式水桶池
        if (bucketPool.Count == 0)
        {
            Debug.LogWarning("[MultiBucketManager] ⚠️ 困難模式水桶池是空的！請在 Inspector 中添加水桶物件");
        }
        
        for (int i = 0; i < bucketPool.Count; i++)
        {
            var bucket = bucketPool[i];
            if (bucket != null)
            {
                BucketEvent bucketEvent = bucket.GetComponent<BucketEvent>();
                if (bucketEvent == null)
                {
                    Debug.LogWarning($"[MultiBucketManager] ⚠️ 困難模式水桶 [{i}] {bucket.name} 沒有 BucketEvent 組件！");
                }
                else
                {
                    Debug.Log($"[MultiBucketManager] ✅ 困難模式水桶 [{i}] {bucket.name} - BucketEvent 已找到");
                }
                bucketEvents.Add(bucketEvent);
            }
            else
            {
                Debug.LogWarning($"[MultiBucketManager] ⚠️ 困難模式水桶池 [{i}] 是 null！");
                bucketEvents.Add(null);
            }
        }
        
        // 檢查標籤
        if (bucketLabels.Count == 0)
        {
            Debug.Log("[MultiBucketManager] 📝 沒有設置水桶標籤（bucketLabels），任務文字將不會顯示");
        }
        else
        {
            Debug.Log($"[MultiBucketManager] 📝 已設置 {bucketLabels.Count} 個水桶標籤");
        }
        
        Debug.Log($"[MultiBucketManager] 初始化完成 - 困難模式水桶數量: {bucketPool.Count}, 有效 BucketEvent: {bucketEvents.FindAll(b => b != null).Count}");
        Debug.Log("===========================================");
    }
    
    /// <summary>
    /// 根據任務階段設置水桶
    /// </summary>
    public void SetupBucketsForTask(HardModeTask task)
    {
        if (task == null || task.stages == null)
        {
            Debug.LogError("[MultiBucketManager] 任務為空！");
            return;
        }
        
        SetupBucketsForStages(task.stages);
    }
    
    /// <summary>
    /// 根據階段列表設置水桶
    /// </summary>
    public void SetupBucketsForStages(List<TaskStage> stages)
    {
        Debug.Log("===========================================");
        Debug.Log("[MultiBucketManager] 🎯 開始設置多水桶任務...");
        
        if (stages == null || stages.Count == 0)
        {
            Debug.LogError("[MultiBucketManager] ❌ 階段列表為空！無法設置水桶");
            return;
        }
        
        // 切換到困難模式（隱藏普通模式水桶）
        ActivateHardMode();
        
        Debug.Log($"[MultiBucketManager] 📋 收到 {stages.Count} 個階段任務");
        
        currentStages = new List<TaskStage>(stages);
        activeBucketCount = Mathf.Min(stages.Count, bucketPool.Count);
        
        if (stages.Count > bucketPool.Count)
        {
            Debug.LogWarning($"[MultiBucketManager] ⚠️ 階段數 ({stages.Count}) 超過水桶數 ({bucketPool.Count})，只會使用 {activeBucketCount} 個水桶");
        }
        
        // 初始化完成狀態
        bucketCompleted.Clear();
        for (int i = 0; i < activeBucketCount; i++)
        {
            bucketCompleted.Add(false);
        }
        
        Debug.Log($"[MultiBucketManager] 🪣 設置 {activeBucketCount} 個水桶對應 {stages.Count} 個階段");
        Debug.Log("-------------------------------------------");
        
        // 遍歷水桶池
        for (int i = 0; i < bucketPool.Count; i++)
        {
            if (bucketPool[i] == null)
            {
                Debug.LogWarning($"[MultiBucketManager] ⚠️ 水桶池 [{i}] 是 null，跳過");
                continue;
            }
            
            if (i < activeBucketCount)
            {
                // 啟用需要的水桶
                bucketPool[i].SetActive(true);
                
                TaskStage stage = stages[i];
                string colorDisplayName = FishColorHelper.GetDisplayName(stage.targetColor);
                
                Debug.Log($"[MultiBucketManager] 🪣 水桶 [{i}] {bucketPool[i].name}:");
                Debug.Log($"    - 目標顏色: {stage.targetColor} ({colorDisplayName})");
                Debug.Log($"    - 目標數量: {stage.count}");
                
                // 設置水桶標籤
                UpdateBucketLabel(i, stage);
                
                // 設置 BucketEvent 的階段索引和容量
                if (bucketEvents[i] != null)
                {
                    bucketEvents[i].SetStageIndex(i);
                    bucketEvents[i].SetCapacity(stage.count);
                    bucketEvents[i].SetTargetColor(stage.targetColor);
                    bucketEvents[i].SetHardMode(true);
                    bucketEvents[i].SetMultiBucketManaged(true);  // 標記為被管理
                    bucketEvents[i].ClearBucket();
                    
                    Debug.Log($"    - ✅ BucketEvent 已配置 (stageIndex={i}, capacity={stage.count}, targetColor={stage.targetColor})");
                }
                else
                {
                    Debug.LogError($"    - ❌ BucketEvent 為 null！水桶無法正常運作");
                }
                
                // 平行任務模式：所有水桶同時啟用
                SetBucketVisualState(i, BucketState.Active);
                Debug.Log($"    - 視覺狀態: Active (平行任務模式)");
            }
            else
            {
                // 隱藏多餘的水桶
                bucketPool[i].SetActive(false);
                Debug.Log($"[MultiBucketManager] 🪣 水桶 [{i}] {bucketPool[i].name}: 已隱藏（不需要）");
            }
        }
        
        Debug.Log("-------------------------------------------");
        Debug.Log($"[MultiBucketManager] ✅ 多水桶設置完成！啟用水桶數: {activeBucketCount}");
        Debug.Log("===========================================");
    }
    
    /// <summary>
    /// 更新水桶標籤文字
    /// </summary>
    private void UpdateBucketLabel(int index, TaskStage stage)
    {
        if (index < bucketLabels.Count && bucketLabels[index] != null)
        {
            // 只顯示數量
            bucketLabels[index].text = $"撈 {stage.count} 隻";
        }
        
        // 更新魚圖片
        if (index < bucketFishImages.Count && bucketFishImages[index] != null)
        {
            Sprite fishSprite = GetFishSprite(stage.targetColor);
            if (fishSprite != null)
            {
                bucketFishImages[index].sprite = fishSprite;
                bucketFishImages[index].enabled = true;
            }
            else
            {
                bucketFishImages[index].enabled = false;
            }
        }
    }
    
    /// <summary>
    /// 根據魚顏色獲取對應的 Sprite
    /// </summary>
    private Sprite GetFishSprite(FishColor color)
    {
        switch (color)
        {
            case FishColor.Red:
                return redFishSprite;
            case FishColor.Gray:
                return grayFishSprite;
            case FishColor.Green:
                return greenFishSprite;
            case FishColor.Yellow:
                return yellowFishSprite;
            case FishColor.Blue:
                return blueFishSprite;
            default:
                Debug.LogWarning($"[MultiBucketManager] 未知的魚顏色: {color}");
                return null;
        }
    }
    
    /// <summary>
    /// 隱藏所有水桶（包括普通模式和困難模式）
    /// </summary>
    public void HideAllBuckets()
    {
        Debug.Log("[MultiBucketManager] 🙈 隱藏所有水桶");
        
        // 隱藏困難模式水桶
        foreach (var bucket in bucketPool)
        {
            if (bucket != null)
            {
                bucket.SetActive(false);
            }
        }
        
        // 隱藏普通模式水桶
        if (normalModeBucket != null)
        {
            normalModeBucket.SetActive(false);
        }
    }
    
    /// <summary>
    /// 隱藏所有水桶
    /// </summary>
    private void HideAllBuckets_Old()
    {
        for (int i = 0; i < bucketPool.Count; i++)
        {
            if (bucketPool[i] != null)
            {
                bucketPool[i].SetActive(false);
                
                // 重置 BucketEvent 的多水桶管理狀態
                if (i < bucketEvents.Count && bucketEvents[i] != null)
                {
                    bucketEvents[i].SetMultiBucketManaged(false);
                    bucketEvents[i].SetHardMode(false);
                }
            }
        }
        activeBucketCount = 0;
        currentStages.Clear();
        bucketCompleted.Clear();
        isHardMode = false;
    }
    
    /// <summary>
    /// 啟用普通模式（簡單/普通難度）
    /// </summary>
    public void ActivateNormalMode()
    {
        Debug.Log("[MultiBucketManager] 🎮 切換到普通模式");
        
        isHardMode = false;
        
        // 隱藏所有困難模式水桶
        foreach (var bucket in bucketPool)
        {
            if (bucket != null)
            {
                bucket.SetActive(false);
            }
        }
        
        // 重置困難模式水桶的狀態
        for (int i = 0; i < bucketEvents.Count; i++)
        {
            if (bucketEvents[i] != null)
            {
                bucketEvents[i].SetMultiBucketManaged(false);
                bucketEvents[i].SetHardMode(false);
            }
        }
        
        // 啟用普通模式水桶
        if (normalModeBucket != null)
        {
            normalModeBucket.SetActive(true);
            
            if (normalModeBucketEvent != null)
            {
                normalModeBucketEvent.SetHardMode(false);
                normalModeBucketEvent.SetMultiBucketManaged(false);
                normalModeBucketEvent.ClearBucket();
            }
            
            Debug.Log($"[MultiBucketManager] ✅ 普通模式水桶 {normalModeBucket.name} 已啟用");
        }
        else
        {
            Debug.LogWarning("[MultiBucketManager] ⚠️ 普通模式水桶未設置！");
        }
        
        activeBucketCount = 0;
        currentStages.Clear();
        bucketCompleted.Clear();
    }
    
    /// <summary>
    /// 啟用困難模式（使用多水桶）
    /// </summary>
    public void ActivateHardMode()
    {
        Debug.Log("[MultiBucketManager] 🎮 切換到困難模式");
        
        isHardMode = true;
        
        // 隱藏普通模式水桶
        if (normalModeBucket != null)
        {
            normalModeBucket.SetActive(false);
            
            if (normalModeBucketEvent != null)
            {
                normalModeBucketEvent.SetHardMode(false);
            }
            
            Debug.Log($"[MultiBucketManager] 普通模式水桶 {normalModeBucket.name} 已隱藏");
        }
        
        // 困難模式水桶會在 SetupBucketsForStages 中啟用
    }
    
    /// <summary>
    /// 獲取當前是否為困難模式
    /// </summary>
    public bool IsHardMode => isHardMode;
    
    /// <summary>
    /// 獲取普通模式的 BucketEvent
    /// </summary>
    public BucketEvent GetNormalModeBucketEvent()
    {
        return normalModeBucketEvent;
    }
    
    /// <summary>
    /// 驗證所有水桶是否符合任務需求
    /// </summary>
    public bool ValidateAllBuckets()
    {
        if (currentStages.Count == 0)
        {
            Debug.LogWarning("[MultiBucketManager] 沒有設置任務階段！");
            return false;
        }
        
        bool allValid = true;
        
        for (int i = 0; i < activeBucketCount; i++)
        {
            if (!ValidateBucket(i))
            {
                allValid = false;
            }
        }
        
        if (allValid)
        {
            Debug.Log("[MultiBucketManager] ✅ 所有水桶驗證通過！");
            OnAllStagesCompleted?.Invoke();
        }
        
        return allValid;
    }
    
    /// <summary>
    /// 驗證單個水桶是否符合對應階段需求
    /// </summary>
    public bool ValidateBucket(int bucketIndex)
    {
        if (bucketIndex < 0 || bucketIndex >= activeBucketCount)
        {
            Debug.LogError($"[MultiBucketManager] 無效的水桶索引: {bucketIndex}");
            return false;
        }
        
        if (bucketEvents[bucketIndex] == null)
        {
            Debug.LogError($"[MultiBucketManager] 水桶 {bucketIndex} 沒有 BucketEvent！");
            return false;
        }
        
        TaskStage stage = currentStages[bucketIndex];
        List<GameObject> fishInBucket = bucketEvents[bucketIndex].GetFishInBucket();
        
        // 檢查數量
        if (fishInBucket.Count != stage.count)
        {
            string errorMsg = $"數量不符：需要 {stage.count} 隻，實際 {fishInBucket.Count} 隻";
            Debug.Log($"[MultiBucketManager] 水桶 {bucketIndex + 1} 驗證失敗：{errorMsg}");
            OnBucketError?.Invoke(bucketIndex, errorMsg);
            return false;
        }
        
        // 檢查顏色
        string expectedTag = FishColorHelper.GetTagFromColor(stage.targetColor);
        foreach (var fish in fishInBucket)
        {
            if (fish == null) continue;
            
            if (!fish.CompareTag(expectedTag))
            {
                string actualColor = fish.tag;
                string errorMsg = $"顏色錯誤：需要 {FishColorHelper.GetDisplayName(stage.targetColor)}，但有 {actualColor}";
                Debug.Log($"[MultiBucketManager] 水桶 {bucketIndex + 1} 驗證失敗：{errorMsg}");
                OnBucketError?.Invoke(bucketIndex, errorMsg);
                return false;
            }
        }
        
        // 驗證通過
        bucketCompleted[bucketIndex] = true;
        SetBucketVisualState(bucketIndex, BucketState.Completed);
        OnBucketStageCompleted?.Invoke(bucketIndex);
        
        Debug.Log($"[MultiBucketManager] ✅ 水桶 {bucketIndex + 1} 驗證通過！");
        return true;
    }
    
    /// <summary>
    /// 當魚進入水桶時檢查（即時反饋）
    /// </summary>
    public void OnFishEnteredBucket(int bucketIndex, GameObject fish)
    {
        if (bucketIndex < 0 || bucketIndex >= activeBucketCount) return;
        
        TaskStage stage = currentStages[bucketIndex];
        BucketEvent bucket = bucketEvents[bucketIndex];
        
        if (bucket == null) return;
        
        // 檢查顏色是否正確
        string expectedTag = FishColorHelper.GetTagFromColor(stage.targetColor);
        if (!fish.CompareTag(expectedTag))
        {
            string errorMsg = $"顏色錯誤！這個水桶需要 {FishColorHelper.GetDisplayName(stage.targetColor)}";
            OnBucketError?.Invoke(bucketIndex, errorMsg);
            
            // 如果啟用了彈出機制，可以在這裡處理
            if (ejectExcessFish)
            {
                // 通知 BucketEvent 彈出這條魚
                bucket.EjectFish(fish);
            }
            return;
        }
        
        // 檢查是否超過容量
        int currentCount = bucket.GetFishInBucket().Count;
        if (currentCount > stage.count)
        {
            string errorMsg = $"水桶已滿！只需要 {stage.count} 隻魚";
            OnBucketError?.Invoke(bucketIndex, errorMsg);
            
            if (ejectExcessFish)
            {
                bucket.EjectFish(fish);
            }
            return;
        }
        
        // 檢查該水桶是否已完成
        if (currentCount == stage.count)
        {
            Debug.Log($"[MultiBucketManager] 水桶 {bucketIndex + 1} 已達到目標數量！");
            
            // 更新標籤顯示完成
            if (bucketIndex < bucketLabels.Count && bucketLabels[bucketIndex] != null)
            {
                string colorName = FishColorHelper.GetDisplayName(stage.targetColor);
                bucketLabels[bucketIndex].text = $"V 任務 {bucketIndex + 1}\n{stage.count} 隻{colorName}";
            }
            
            // 檢查是否所有水桶都完成
            CheckAllBucketsCompleted();
        }
    }
    
    /// <summary>
    /// 檢查是否所有水桶都完成
    /// </summary>
    private void CheckAllBucketsCompleted()
    {
        bool allCompleted = true;
        for (int i = 0; i < activeBucketCount; i++)
        {
            if (!bucketCompleted[i])
            {
                allCompleted = false;
                break;
            }
        }
        
        if (allCompleted)
        {
            Debug.Log("[MultiBucketManager] ✅ 所有水桶都已完成！");
            OnAllStagesCompleted?.Invoke();
        }
    }
    
    /// <summary>
    /// 重置單一水桶（獨立重試功能）
    /// </summary>
    public void ResetSingleBucket(int bucketIndex)
    {
        if (bucketIndex < 0 || bucketIndex >= activeBucketCount)
        {
            Debug.LogError($"[MultiBucketManager] 無效的水桶索引: {bucketIndex}");
            return;
        }
        
        if (bucketEvents[bucketIndex] != null)
        {
            Debug.Log($"[MultiBucketManager] 🔄 重置水桶 {bucketIndex + 1}");
            
            // 釋放水桶中的魚
            bucketEvents[bucketIndex].RetryHardModeTask();
            
            // 重置 BucketEvent 狀態
            bucketEvents[bucketIndex].ResetStatus();
            
            // 重置完成狀態
            bucketCompleted[bucketIndex] = false;
            
            // 重置視覺狀態為 Active
            SetBucketVisualState(bucketIndex, BucketState.Active);
            
            // 重置標籤
            if (bucketIndex < currentStages.Count)
            {
                UpdateBucketLabel(bucketIndex, currentStages[bucketIndex]);
            }
        }
    }
    
    /// <summary>
    /// 設置水桶的視覺狀態
    /// </summary>
    private void SetBucketVisualState(int index, BucketState state)
    {
        if (index < 0 || index >= bucketPool.Count || bucketPool[index] == null) return;
        
        // 可以在這裡更改水桶的材質、顏色等
        // 這裡僅更新標籤顏色作為示範
        if (index < bucketLabels.Count && bucketLabels[index] != null)
        {
            switch (state)
            {
                case BucketState.Active:
                    bucketLabels[index].color = activeColor;
                    break;
                case BucketState.Completed:
                    bucketLabels[index].color = completedColor;
                    break;
                case BucketState.Inactive:
                    bucketLabels[index].color = inactiveColor;
                    break;
            }
        }
    }
    
    /// <summary>
    /// 清空所有水桶
    /// </summary>
    public void ClearAllBuckets()
    {
        for (int i = 0; i < activeBucketCount; i++)
        {
            if (bucketEvents[i] != null)
            {
                bucketEvents[i].ClearBucket();
            }
        }
        
        // 重置完成狀態
        for (int i = 0; i < bucketCompleted.Count; i++)
        {
            bucketCompleted[i] = false;
        }
        
        Debug.Log("[MultiBucketManager] 已清空所有水桶");
    }
    
    /// <summary>
    /// 重試任務（釋放魚但不銷毀）
    /// </summary>
    public void RetryTask()
    {
        for (int i = 0; i < activeBucketCount; i++)
        {
            if (bucketEvents[i] != null)
            {
                bucketEvents[i].RetryHardModeTask();
            }
            
            // 平行任務模式：所有水桶重置為 Active
            SetBucketVisualState(i, BucketState.Active);
            
            // 重置標籤
            if (i < currentStages.Count)
            {
                UpdateBucketLabel(i, currentStages[i]);
            }
        }
        
        // 重置完成狀態
        for (int i = 0; i < bucketCompleted.Count; i++)
        {
            bucketCompleted[i] = false;
        }
        
        Debug.Log("[MultiBucketManager] 任務已重試，所有魚已釋放");
    }
    
    /// <summary>
    /// 獲取活動水桶數量
    /// </summary>
    public int GetActiveBucketCount() => activeBucketCount;
    
    /// <summary>
    /// 獲取指定水桶的 BucketEvent
    /// </summary>
    public BucketEvent GetBucketEvent(int index)
    {
        if (index >= 0 && index < bucketEvents.Count)
        {
            return bucketEvents[index];
        }
        return null;
    }
    
    /// <summary>
    /// 檢查指定水桶是否已完成
    /// </summary>
    public bool IsBucketCompleted(int index)
    {
        if (index >= 0 && index < bucketCompleted.Count)
        {
            return bucketCompleted[index];
        }
        return false;
    }
    
    /// <summary>
    /// 獲取已完成的水桶數量
    /// </summary>
    public int GetCompletedBucketCount()
    {
        int count = 0;
        foreach (bool completed in bucketCompleted)
        {
            if (completed) count++;
        }
        return count;
    }
}

/// <summary>
/// 水桶狀態枚舉
/// </summary>
public enum BucketState
{
    Inactive,   // 未啟用（等待前一階段完成）
    Active,     // 當前活動
    Completed   // 已完成
}
