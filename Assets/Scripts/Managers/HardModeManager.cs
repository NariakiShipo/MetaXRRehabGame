using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 困難模式管理器 - 專門處理困難模式的多階段任務邏輯
/// 
/// 核心功能：
/// 1. 多階段任務生成 (Multi-stage Tasks)
/// 2. 順序驗證 (Sequence Validation) - 嚴格檢查水桶內金魚的「進入順序」
/// 3. 魚鎖定機制 (Locking Mechanism) - 魚進入水桶後不可取出
/// 4. 重來機制 (Retry Mechanism)
/// </summary>
public class HardModeManager : MonoBehaviour
{
    [Header("任務配置 (從 HardDifficultyConfig 讀取，以下為預設值)")]
    [SerializeField] private int minStages = 2;
    [SerializeField] private int maxStages = 3;
    [SerializeField] private int minFishPerStage = 1;
    [SerializeField] private int maxFishPerStage = 2;
    
    [Header("可用顏色 (從 HardDifficultyConfig 讀取)")]
    [SerializeField] private FishColor[] availableColors = { FishColor.Red, FishColor.Gray, FishColor.Yellow, FishColor.Green };
    
    [Header("事件")]
    public UnityEvent<HardModeTask> OnTaskGenerated;                    // 任務生成
    public UnityEvent<TaskStage> OnStageComplete;                       // 階段完成
    public UnityEvent<HardModeTask> OnTaskComplete;                     // 任務完成
    public UnityEvent<HardModeValidationResult> OnValidationFailed;     // 驗證失敗
    public UnityEvent OnTaskReset;                                       // 任務重置
    
    // UI 相關事件
    public UnityEvent<int, int> OnStageAdvanced;                         // (當前階段, 總階段數)
    public UnityEvent OnTaskCompleted;                                   // 任務完成 (無參數版本)
    public UnityEvent<string> OnSequenceError;                           // 順序錯誤訊息
    
    // 當前任務
    private HardModeTask currentTask;
    private int taskIdCounter = 0;
    private int currentStageIndex = 0;                                   // 當前階段索引
    
    // 追蹤水桶中魚的進入順序
    private List<FishColor> fishEntrySequence = new List<FishColor>();
    
    // 配置是否已初始化
    private bool isConfigInitialized = false;
    
    // 單例
    public static HardModeManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // 初始化事件
        if (OnStageAdvanced == null) OnStageAdvanced = new UnityEvent<int, int>();
        if (OnTaskCompleted == null) OnTaskCompleted = new UnityEvent();
        if (OnSequenceError == null) OnSequenceError = new UnityEvent<string>();
    }
    
    private void Start()
    {
        // 嘗試從 DifficultyManager 獲取配置
        TryLoadConfigFromDifficultyManager();
    }
    
    /// <summary>
    /// 從 DifficultyManager 獲取困難模式配置
    /// </summary>
    private void TryLoadConfigFromDifficultyManager()
    {
        if (isConfigInitialized) return;
        
        DifficultyManager difficultyManager = DifficultyManager.Instance;
        if (difficultyManager != null)
        {
            HardDifficultyConfig hardConfig = difficultyManager.GetHardConfig();
            if (hardConfig != null)
            {
                ApplyConfig(hardConfig.GetHardModeConfig());
                Debug.Log("[HardModeManager] 已從 HardDifficultyConfig 載入配置");
            }
        }
    }
    
    /// <summary>
    /// 應用困難模式配置
    /// </summary>
    public void ApplyConfig(HardModeConfig config)
    {
        minStages = config.MinStages;
        maxStages = config.MaxStages;
        minFishPerStage = config.MinFishPerStage;
        maxFishPerStage = config.MaxFishPerStage;
        
        if (config.AvailableColors != null && config.AvailableColors.Length > 0)
        {
            availableColors = config.AvailableColors;
        }
        
        isConfigInitialized = true;
        
        Debug.Log($"[HardModeManager] 配置已更新: 階段數 {minStages}-{maxStages}, 每階段魚數 {minFishPerStage}-{maxFishPerStage}, 顏色數 {availableColors.Length}");
    }
    
    /// <summary>
    /// 獲取當前任務
    /// </summary>
    public HardModeTask GetCurrentTask() => currentTask;
    
    /// <summary>
    /// 檢查是否有活動的困難模式任務
    /// </summary>
    public bool HasActiveTask => currentTask != null;
    
    #region 任務生成
    
    /// <summary>
    /// 生成新的困難模式任務
    /// </summary>
    public HardModeTask GenerateHardTask()
    {
        currentTask = new HardModeTask(++taskIdCounter);
        
        // 決定階段數量
        int stageCount = Random.Range(minStages, maxStages + 1);
        
        // 使用一個列表來追蹤已使用的顏色，避免連續相同
        FishColor lastColor = (FishColor)(-1);
        
        for (int i = 0; i < stageCount; i++)
        {
            // 選擇一個與上一個不同的顏色
            FishColor selectedColor;
            int attempts = 0;
            do
            {
                selectedColor = availableColors[Random.Range(0, availableColors.Length)];
                attempts++;
            } while (selectedColor == lastColor && attempts < 10 && availableColors.Length > 1);
            
            lastColor = selectedColor;
            
            // 決定該階段需要的魚數量
            int fishCount = Random.Range(minFishPerStage, maxFishPerStage + 1);
            
            // 創建階段
            TaskStage stage = new TaskStage(selectedColor, fishCount);
            currentTask.stages.Add(stage);
        }
        
        // 生成指示文字
        currentTask.GenerateInstructionText();
        
        // 清空進入順序追蹤
        fishEntrySequence.Clear();
        
        Debug.Log($"[HardModeManager] 生成困難模式任務 ID:{currentTask.taskID}");
        Debug.Log($"[HardModeManager] 任務內容: {currentTask.instructionText}");
        Debug.Log($"[HardModeManager] 總共 {currentTask.TotalStages} 個階段，需要 {currentTask.GetTotalFishRequired()} 隻魚");
        
        // 設置多水桶（如果可用）
        SetupMultiBuckets();
        
        OnTaskGenerated?.Invoke(currentTask);
        
        // 通知 UI 初始階段進度
        NotifyStageProgress();
        
        return currentTask;
    }
    
    /// <summary>
    /// 使用指定的階段配置生成任務
    /// </summary>
    public HardModeTask GenerateHardTask(List<TaskStage> stages)
    {
        currentTask = new HardModeTask(++taskIdCounter);
        currentTask.stages = new List<TaskStage>(stages);
        currentTask.GenerateInstructionText();
        
        fishEntrySequence.Clear();
        
        Debug.Log($"[HardModeManager] 生成自定義困難模式任務: {currentTask.instructionText}");
        
        // 設置多水桶（如果可用）
        SetupMultiBuckets();
        
        OnTaskGenerated?.Invoke(currentTask);
        
        // 通知 UI 初始階段進度
        NotifyStageProgress();
        
        return currentTask;
    }
    
    /// <summary>
    /// 設置多水桶模式
    /// </summary>
    private void SetupMultiBuckets()
    {
        if (currentTask == null)
        {
            Debug.LogWarning("[HardModeManager] 無法設置多水桶：當前任務為 null");
            return;
        }
        
        Debug.Log("===========================================");
        Debug.Log("[HardModeManager] 🎯 準備設置多水桶模式...");
        Debug.Log($"[HardModeManager] 當前任務 ID: {currentTask.taskID}, 階段數: {currentTask.TotalStages}");
        
        // 如果有 MultiBucketManager，使用多水桶模式
        if (MultiBucketManager.Instance != null)
        {
            Debug.Log("[HardModeManager] ✅ 找到 MultiBucketManager，開始設置水桶...");
            MultiBucketManager.Instance.SetupBucketsForTask(currentTask);
            Debug.Log($"[HardModeManager] ✅ 已設置 {currentTask.TotalStages} 個水桶對應任務階段");
        }
        else
        {
            Debug.LogWarning("[HardModeManager] ⚠️ MultiBucketManager.Instance 為 null！無法使用多水桶模式");
            Debug.LogWarning("[HardModeManager] 請確認場景中有 MultiBucketManager 物件且已正確設置");
        }
        Debug.Log("===========================================");
    }
    
    #endregion
    
    #region 魚進入追蹤
    
    /// <summary>
    /// 當魚進入水桶時呼叫（由 BucketEvent 調用）
    /// </summary>
    public void OnFishEnteredBucket(GameObject fishObject)
    {
        if (currentTask == null) return;
        
        string fishTag = fishObject.tag;
        FishColor fishColor = FishColorHelper.GetColorFromTag(fishTag);
        
        // 記錄進入順序
        fishEntrySequence.Add(fishColor);
        
        Debug.Log($"[HardModeManager] 魚進入水桶: {fishColor} (順序位置: {fishEntrySequence.Count})");
    }
    
    /// <summary>
    /// 當魚離開水桶時呼叫（困難模式下應該被阻止）
    /// </summary>
    public bool CanFishLeaveBucket(GameObject fishObject)
    {
        // 困難模式下，魚不能離開水桶
        Debug.Log("[HardModeManager] 困難模式：魚不能從水桶取出！");
        return false;
    }
    
    /// <summary>
    /// 強制移除魚記錄（僅用於重置）
    /// </summary>
    public void RemoveFishFromSequence(GameObject fishObject)
    {
        if (fishEntrySequence.Count > 0)
        {
            string fishTag = fishObject.tag;
            FishColor fishColor = FishColorHelper.GetColorFromTag(fishTag);
            
            // 從末尾開始移除
            for (int i = fishEntrySequence.Count - 1; i >= 0; i--)
            {
                if (fishEntrySequence[i] == fishColor)
                {
                    fishEntrySequence.RemoveAt(i);
                    break;
                }
            }
        }
    }
    
    #endregion
    
    #region 順序驗證
    
    /// <summary>
    /// 驗證水桶中的魚是否符合任務要求（核心驗證邏輯）
    /// 
    /// 驗證規則：
    /// 1. 多水桶模式：檢查每個水桶是否符合對應階段需求
    /// 2. 單水桶模式：按照 fishEntrySequence 的順序驗證
    /// 3. 數量必須精確匹配
    /// </summary>
    public HardModeValidationResult ValidateHardMode(List<GameObject> bucketFish)
    {
        if (currentTask == null || currentTask.stages.Count == 0)
        {
            Debug.LogWarning("[HardModeManager] 沒有活動的任務");
            return HardModeValidationResult.Incomplete;
        }
        
        // 優先使用多水桶模式驗證
        if (MultiBucketManager.Instance != null && MultiBucketManager.Instance.GetActiveBucketCount() > 0)
        {
            return ValidateMultiBucketMode();
        }
        
        // 單水桶模式：使用進入順序來驗證
        return ValidateSequence(fishEntrySequence);
    }
    
    /// <summary>
    /// 多水桶模式驗證 - 檢查每個水桶是否符合對應階段需求
    /// </summary>
    private HardModeValidationResult ValidateMultiBucketMode()
    {
        if (MultiBucketManager.Instance == null) 
            return HardModeValidationResult.Incomplete;
        
        bool allValid = MultiBucketManager.Instance.ValidateAllBuckets();
        
        if (allValid)
        {
            Debug.Log("[HardModeManager] 多水桶模式：所有階段驗證通過！");
            OnTaskCompleted?.Invoke();
            OnTaskComplete?.Invoke(currentTask);
            return HardModeValidationResult.Success;
        }
        
        // 檢查是否有部分完成
        int completed = MultiBucketManager.Instance.GetCompletedBucketCount();
        int total = MultiBucketManager.Instance.GetActiveBucketCount();
        
        if (completed > 0)
        {
            Debug.Log($"[HardModeManager] 多水桶模式：已完成 {completed}/{total} 個階段");
            return HardModeValidationResult.Incomplete;
        }
        
        return HardModeValidationResult.Incomplete;
    }
    
    /// <summary>
    /// 驗證魚的進入順序是否符合任務階段
    /// </summary>
    private HardModeValidationResult ValidateSequence(List<FishColor> sequence)
    {
        int fishIndex = 0;
        
        // 遍歷每個階段
        for (int stageIndex = 0; stageIndex < currentTask.stages.Count; stageIndex++)
        {
            TaskStage stage = currentTask.stages[stageIndex];
            
            // 檢查該階段所需的魚
            for (int i = 0; i < stage.count; i++)
            {
                // 檢查是否還有魚
                if (fishIndex >= sequence.Count)
                {
                    // 未完成 - 魚不夠
                    if (stageIndex < currentTask.currentStageIndex || 
                        (stageIndex == currentTask.currentStageIndex && i > 0))
                    {
                        Debug.Log($"[HardModeManager] 驗證：階段 {stageIndex + 1} 未完成，需要 {stage.count} 隻，有 {i} 隻");
                        return HardModeValidationResult.Incomplete;
                    }
                    return HardModeValidationResult.Incomplete;
                }
                
                // 檢查顏色是否匹配
                FishColor actualColor = sequence[fishIndex];
                if (actualColor != stage.targetColor)
                {
                    string errorMsg = $"順序錯誤！第 {fishIndex + 1} 隻魚應該是 {FishColorHelper.GetColorName(stage.targetColor)}，但放入了 {FishColorHelper.GetColorName(actualColor)}";
                    Debug.Log($"[HardModeManager] 驗證失敗：位置 {fishIndex + 1}，期望 {stage.targetColor}，實際 {actualColor}");
                    OnSequenceError?.Invoke(errorMsg);
                    OnValidationFailed?.Invoke(HardModeValidationResult.WrongSequence);
                    return HardModeValidationResult.WrongSequence;
                }
                
                fishIndex++;
            }
            
            // 該階段完成
            Debug.Log($"[HardModeManager] 階段 {stageIndex + 1} 驗證通過");
        }
        
        // 檢查是否有多餘的魚
        if (fishIndex < sequence.Count)
        {
            string errorMsg = $"魚太多了！有 {sequence.Count - fishIndex} 隻多餘的魚";
            Debug.Log($"[HardModeManager] 驗證失敗：有多餘的魚 ({sequence.Count - fishIndex} 隻)");
            OnSequenceError?.Invoke(errorMsg);
            OnValidationFailed?.Invoke(HardModeValidationResult.ExcessFish);
            return HardModeValidationResult.ExcessFish;
        }
        
        // 所有階段都完成
        Debug.Log("[HardModeManager] 任務完全驗證通過！");
        OnTaskCompleted?.Invoke();
        OnTaskComplete?.Invoke(currentTask);
        return HardModeValidationResult.Success;
    }
    
    /// <summary>
    /// 驗證當前階段是否完成（用於即時反饋）
    /// </summary>
    public HardModeValidationResult ValidateCurrentStage()
    {
        if (currentTask == null) return HardModeValidationResult.Incomplete;
        
        TaskStage? currentStage = currentTask.GetCurrentStage();
        if (!currentStage.HasValue) return HardModeValidationResult.Incomplete;
        
        int startIndex = 0;
        // 計算當前階段在序列中的起始位置
        for (int i = 0; i < currentTask.currentStageIndex; i++)
        {
            startIndex += currentTask.stages[i].count;
        }
        
        int endIndex = startIndex + currentStage.Value.count;
        int currentCount = 0;
        
        // 檢查已經進入的魚
        for (int i = startIndex; i < fishEntrySequence.Count && i < endIndex; i++)
        {
            if (fishEntrySequence[i] == currentStage.Value.targetColor)
            {
                currentCount++;
            }
            else
            {
                // 顏色錯誤
                string errorMsg = $"順序錯誤！期望 {FishColorHelper.GetColorName(currentStage.Value.targetColor)}，但放入了 {FishColorHelper.GetColorName(fishEntrySequence[i])}";
                OnSequenceError?.Invoke(errorMsg);
                return HardModeValidationResult.WrongColor;
            }
        }
        
        // 檢查是否完成當前階段
        if (currentCount >= currentStage.Value.count)
        {
            // 更新階段計數
            TaskStage updatedStage = currentTask.stages[currentTask.currentStageIndex];
            updatedStage.currentCount = currentCount;
            currentTask.stages[currentTask.currentStageIndex] = updatedStage;
            
            OnStageComplete?.Invoke(updatedStage);
            
            // 移動到下一階段
            if (currentTask.MoveToNextStage())
            {
                Debug.Log($"[HardModeManager] 進入下一階段: {currentTask.GetCurrentStageDisplayText()}");
                
                // 通知 UI 階段進度更新
                NotifyStageProgress();
                
                return HardModeValidationResult.StageComplete;
            }
            else
            {
                // 任務完全完成
                OnTaskCompleted?.Invoke();
                return HardModeValidationResult.Success;
            }
        }
        
        return HardModeValidationResult.Incomplete;
    }
    
    #endregion
    
    #region 任務重置
    
    /// <summary>
    /// 重置當前任務（重來機制）
    /// </summary>
    public void ResetCurrentTask()
    {
        if (currentTask != null)
        {
            currentTask.Reset();
            fishEntrySequence.Clear();
            
            // 重置多水桶（如果使用）
            if (MultiBucketManager.Instance != null)
            {
                MultiBucketManager.Instance.RetryTask();
            }
            
            Debug.Log("[HardModeManager] 任務已重置");
            OnTaskReset?.Invoke();
        }
    }
    
    /// <summary>
    /// 清除當前任務
    /// </summary>
    public void ClearTask()
    {
        currentTask = null;
        fishEntrySequence.Clear();
        
        // 隱藏多水桶（如果使用）
        if (MultiBucketManager.Instance != null)
        {
            MultiBucketManager.Instance.HideAllBuckets();
        }
    }
    
    #endregion
    
    #region 輔助方法
    
    /// <summary>
    /// 獲取當前階段的顯示文字
    /// </summary>
    public string GetCurrentStageDisplayText()
    {
        return currentTask?.GetCurrentStageDisplayText() ?? "";
    }
    
    /// <summary>
    /// 獲取完整任務描述
    /// </summary>
    public string GetFullTaskDescription()
    {
        return currentTask?.instructionText ?? "";
    }
    
    /// <summary>
    /// 獲取當前進度 (0-1)
    /// </summary>
    public float GetProgress()
    {
        if (currentTask == null || currentTask.GetTotalFishRequired() == 0)
            return 0f;
        
        return (float)fishEntrySequence.Count / currentTask.GetTotalFishRequired();
    }
    
    /// <summary>
    /// 獲取水桶中的魚數量
    /// </summary>
    public int GetFishInBucketCount() => fishEntrySequence.Count;
    
    /// <summary>
    /// 獲取當前階段需要的魚數量
    /// </summary>
    public int GetCurrentStageRequiredCount()
    {
        TaskStage? stage = currentTask?.GetCurrentStage();
        return stage?.count ?? 0;
    }
    
    /// <summary>
    /// 獲取當前階段已完成的魚數量
    /// </summary>
    public int GetCurrentStageCompletedCount()
    {
        if (currentTask == null) return 0;
        
        int startIndex = 0;
        for (int i = 0; i < currentTask.currentStageIndex; i++)
        {
            startIndex += currentTask.stages[i].count;
        }
        
        TaskStage? currentStage = currentTask.GetCurrentStage();
        if (!currentStage.HasValue) return 0;
        
        int count = 0;
        for (int i = startIndex; i < fishEntrySequence.Count; i++)
        {
            if (fishEntrySequence[i] == currentStage.Value.targetColor)
            {
                count++;
            }
            else
            {
                break; // 如果顏色不對，停止計數
            }
        }
        
        return count;
    }
    
    /// <summary>
    /// 獲取當前階段的指示文字
    /// </summary>
    public string GetCurrentStageInstruction()
    {
        if (currentTask == null) return "";
        
        TaskStage? stage = currentTask.GetCurrentStage();
        if (!stage.HasValue) return "任務完成！";
        
        string colorName = FishColorHelper.GetColorName(stage.Value.targetColor);
        int completed = GetCurrentStageCompletedCount();
        int required = stage.Value.count;
        
        return $"撈 {required} 隻{colorName}金魚 ({completed}/{required})";
    }
    
    /// <summary>
    /// 獲取當前階段索引（從 0 開始）
    /// </summary>
    public int GetCurrentStageIndex()
    {
        return currentTask?.currentStageIndex ?? 0;
    }
    
    /// <summary>
    /// 通知 UI 階段進度更新
    /// </summary>
    private void NotifyStageProgress()
    {
        if (currentTask == null) return;
        
        int currentStage = currentTask.currentStageIndex + 1; // 1-based for UI
        int totalStages = currentTask.TotalStages;
        
        OnStageAdvanced?.Invoke(currentStage, totalStages);
    }
    
    #endregion
}
