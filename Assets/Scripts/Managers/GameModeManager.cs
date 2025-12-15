using UnityEngine;
using UnityEngine.Events;

public class GameModeManager : MonoBehaviour
{
    [Header("References")]
    // 已改用 ServiceLocator，移除 SerializeField 依賴
    private GameManager gameManager;
    private FishSpawnManager fishSpawnManager;
    private TaskManager taskManager;
    private ScoreManager scoreManager;
    private DifficultyManager difficultyManager;
    private HardModeManager hardModeManager;
    
    [Header("UI References")]
    [Tooltip("難度選擇按鈕的父物體（選擇後會隱藏）")]
    [SerializeField] private GameObject[] difficultySelectionUI;
    
    [Tooltip("時間選擇按鈕的父物體（難度選擇後顯示）")]
    [SerializeField] private GameObject[] timeSelectionUI;
    
    [Tooltip("遊戲進行中的 UI（遊戲開始前隱藏，開始後顯示）")]
    [SerializeField] private GameObject[] gameplayUI;
    
    [Header("Events")]
    [Tooltip("遊戲開始時觸發")]
    public UnityEvent onGameStart;
    
    private bool isGameStarted = false;
    private string selectedDifficulty = "";
    private int selectedDifficultyIndex = -1;
    private float selectedTimeLimit = 0f;
    
    void Start()
    {
        // 使用 ServiceLocator 獲取服務（帶備用方案）
        gameManager = GetServiceOrFind<GameManager>();
        difficultyManager = GetServiceOrFind<DifficultyManager>();
        taskManager = GetServiceOrFind<TaskManager>();
        scoreManager = GetServiceOrFind<ScoreManager>();
        fishSpawnManager = GetServiceOrFind<FishSpawnManager>();
        hardModeManager = GetServiceOrFind<HardModeManager>();
        
        // 如果 ServiceLocator 沒有 HardModeManager，嘗試使用單例
        if (hardModeManager == null)
        {
            hardModeManager = HardModeManager.Instance;
        }
        
        // 禁用 FishSpawnManager 的自動生成，由我們手動控制
        if (fishSpawnManager != null)
        {
            fishSpawnManager.SetAutoSpawnOnEnable(false);
        }
        
        // 遊戲開始前先暫停其他系統
        InitializeGameSystems(false);
        
        // 验证关键依赖
        if (scoreManager == null)
        {
            Debug.LogError("[GameModeManager] ScoreManager 未找到！");
        }
        
        // 订阅任务验证事件
        if (taskManager != null)
        {
            taskManager.OnTaskValidated.AddListener(OnTaskValidated);
            taskManager.OnSubTaskComplete.AddListener(OnSubTaskComplete);
        }
        
        // 訂閱 MultiBucketManager 的完成事件（困難模式專用）
        if (MultiBucketManager.Instance != null)
        {
            MultiBucketManager.Instance.OnAllStagesCompleted.AddListener(OnAllBucketsCompleted);
            Debug.Log("[GameModeManager] 已訂閱 MultiBucketManager.OnAllStagesCompleted 事件");
        }
        
        // 初始化時隱藏時間選擇UI和遊戲UI
        HideTimeSelectionUI();
        HideGameplayUI();
        
        Debug.Log("[GameModeManager] 等待玩家選擇難度...");
    }
    
    /// <summary>
    /// 從 ServiceLocator 獲取服務，如果失敗則在場景中查找
    /// </summary>
    private T GetServiceOrFind<T>() where T : UnityEngine.Object
    {
        // 使用 TryGet 避免錯誤日誌
        if (ServiceLocator.Instance.TryGet(out T service))
        {
            return service;
        }
        
        // 如果 ServiceLocator 沒有，嘗試在場景中查找
        service = FindFirstObjectByType<T>();
        if (service != null)
        {
            Debug.LogWarning($"[GameModeManager] {typeof(T).Name} 未在 ServiceLocator 中找到，使用場景中的實例");
        }
        return service;
    }
    
    void OnDestroy()
    {
        // 取消订阅
        if (taskManager != null)
        {
            taskManager.OnTaskValidated.RemoveListener(OnTaskValidated);
            taskManager.OnSubTaskComplete.RemoveListener(OnSubTaskComplete);
           // taskManager.OnTaskFailed.RemoveListener(OnTaskValidated);
        }
        
        // 取消訂閱 MultiBucketManager 事件
        if (MultiBucketManager.Instance != null)
        {
            MultiBucketManager.Instance.OnAllStagesCompleted.RemoveListener(OnAllBucketsCompleted);
        }
    }
    
    /// <summary>
    /// Easy 按鈕按下時調用
    /// </summary>
    public void OnEasyButtonPressed()
    {
        if (difficultyManager != null)
        {
            difficultyManager.SetEasyDifficulty();
            selectedDifficultyIndex = 0;
            selectedDifficulty = "Easy";
            
            // 隱藏難度選擇UI，顯示時間選擇UI
            HideDifficultySelectionUI();
            ShowTimeSelectionUI();
            
            Debug.Log("[GameModeManager] 選擇簡單難度，請選擇時間");
        }
        else
        {
            Debug.LogError("[GameModeManager] DifficultyManager 未設置！");
        }
    }
    
    /// <summary>
    /// Normal 按鈕按下時調用
    /// </summary>
    public void OnNormalButtonPressed()
    {
        if (difficultyManager != null)
        {
            difficultyManager.SetNormalDifficulty();
            selectedDifficultyIndex = 1;
            selectedDifficulty = "Normal";
            
            // 隱藏難度選擇UI，顯示時間選擇UI
            HideDifficultySelectionUI();
            ShowTimeSelectionUI();
            
            Debug.Log("[GameModeManager] 選擇普通難度，請選擇時間");
        }
        else
        {
            Debug.LogError("[GameModeManager] DifficultyManager 未設置！");
        }
    }
    
    /// <summary>
    /// Hard 按鈕按下時調用
    /// </summary>
    public void OnHardButtonPressed()
    {
        if (difficultyManager != null)
        {
            difficultyManager.SetHardDifficulty();
            selectedDifficultyIndex = 2;
            selectedDifficulty = "Hard";
            
            // 隱藏難度選擇UI，顯示時間選擇UI
            HideDifficultySelectionUI();
            ShowTimeSelectionUI();
            
            Debug.Log("[GameModeManager] 選擇困難難度，請選擇時間");
        }
        else
        {
            Debug.LogError("[GameModeManager] DifficultyManager 未設置！");
        }
    }
    
    /// <summary>
    /// 3分鐘按鈕按下時調用（180秒）
    /// </summary>
    public void OnTimeOneMinButtonPressed()
    {
        selectedTimeLimit = 60f;
        StartGameWithSelectedSettings();
    }

    /// <summary>
    /// 3分鐘按鈕按下時調用（180秒）
    /// </summary>
    public void OnTimeThreeMinButtonPressed()
    {
        selectedTimeLimit = 180f;
        StartGameWithSelectedSettings();
    }
    
    /// <summary>
    /// 5分鐘按鈕按下時調用（300秒）
    /// </summary>
    public void OnTimeFiveMinButtonPressed()
    {
        selectedTimeLimit = 300f;
        StartGameWithSelectedSettings();
    }
    
    /// <summary>
    /// 10分鐘按鈕按下時調用（600秒）
    /// </summary>
    public void OnTimeTenMinButtonPressed()
    {
        selectedTimeLimit = 600f;
        StartGameWithSelectedSettings();
    }
    
    /// <summary>
    /// 使用選擇的設定開始遊戲
    /// </summary>
    private void StartGameWithSelectedSettings()
    {
        if (selectedDifficultyIndex < 0 || selectedTimeLimit <= 0)
        {
            Debug.LogError("[GameModeManager] 未正確選擇難度或時間！");
            return;
        }
        
        // 設置自定義時間限制
        if (difficultyManager != null)
        {
            difficultyManager.SetCustomTimeLimit(selectedTimeLimit);
        }
        
        // 隱藏時間選擇UI
        HideTimeSelectionUI();
        
        // 顯示遊戲UI
        ShowGameplayUI();
        
        // 開始遊戲
        StartGameWithDifficulty(selectedDifficultyIndex, selectedDifficulty, selectedTimeLimit);
    }
    
    /// <summary>
    /// 隱藏難度選擇UI
    /// </summary>
    private void HideDifficultySelectionUI()
    {
        if (difficultySelectionUI != null)
        {
            foreach (var ui in difficultySelectionUI)
                ui.SetActive(false);
            Debug.Log("[GameModeManager] 已隱藏難度選擇 UI");
        }
    }
    
    /// <summary>
    /// 顯示時間選擇UI
    /// </summary>
    private void ShowTimeSelectionUI()
    {
        if (timeSelectionUI != null)
        {
            foreach (var ui in timeSelectionUI)
                ui.SetActive(true);
            Debug.Log("[GameModeManager] 已顯示時間選擇 UI");
        }
    }
    
    /// <summary>
    /// 隱藏時間選擇UI
    /// </summary>
    private void HideTimeSelectionUI()
    {
        if (timeSelectionUI != null)
        {
            foreach (var ui in timeSelectionUI)
                ui.SetActive(false);
        }
    }
    
    /// <summary>
    /// 顯示遊戲UI
    /// </summary>
    private void ShowGameplayUI()
    {
        if (gameplayUI != null)
        {
            foreach (var ui in gameplayUI)
            {
                if (ui != null)
                {
                    ui.SetActive(true);
                }
            }
            Debug.Log("[GameModeManager] 已顯示遊戲 UI");
        }
    }
    
    /// <summary>
    /// 隱藏遊戲UI
    /// </summary>
    private void HideGameplayUI()
    {
        if (gameplayUI != null)
        {
            foreach (var ui in gameplayUI)
            {
                if (ui != null)
                {
                    ui.SetActive(false);
                }
            }
            Debug.Log("[GameModeManager] 已隱藏遊戲 UI");
        }
    }
    
    /// <summary>
    /// 開始遊戲並設置難度
    /// </summary>
    private void StartGameWithDifficulty(int difficultyIndex, string difficultyName, float timeLimit)
    {
        // 防止重複啟動
        if (isGameStarted)
        {
            Debug.LogWarning("[GameModeManager] 遊戲已經開始，無法重複選擇難度");
            return;
        }
        
        selectedDifficulty = difficultyName;
        isGameStarted = true;
        
        Debug.Log($"[GameModeManager] 選擇難度：{difficultyName}，時間限制：{timeLimit} 秒");
        
        // 設置 GameManager 的倒數計時
        if (gameManager != null)
        {
            gameManager.SetTime(difficultyIndex, timeLimit);
            Debug.Log($"[GameModeManager] 已設置計時器：{timeLimit} 秒");
        }
        else
        {
            Debug.LogError("[GameModeManager] GameManager 引用為空！請在 Inspector 中設置");
        }
        
        // 注意：分数系统已由DifficultyManager配置，不需要在这里再设置
        
        // 啟動其他遊戲系統
        InitializeGameSystems(true);
        
        // 触发游戏开始事件
        onGameStart?.Invoke();
        
        // 生成第一个任务（会自动设置生成模式和生成鱼）
        GenerateNewTask();
        
        Debug.Log($"[GameModeManager] 遊戲開始！難度：{difficultyName}");
    }
    
    /// <summary>
    /// 初始化或啟動遊戲系統
    /// </summary>
    private void InitializeGameSystems(bool enable)
    {
        // 控制 GameManager
        if (gameManager != null)
        {
            gameManager.enabled = enable;
            Debug.Log($"[GameModeManager] GameManager {(enable ? "已啟動" : "已暫停")}");
        }
        
        // 控制 FishSpawnManager
        if (fishSpawnManager != null)
        {
            fishSpawnManager.enabled = enable;
            Debug.Log($"[GameModeManager] FishSpawnManager {(enable ? "已啟動" : "已暫停")}");
        }
        
        // 可以在這裡添加更多需要控制的系統
    }
    
    /// <summary>
    /// 重新開始遊戲（用於重玩）
    /// </summary>
    public void RestartGame()
    {
        isGameStarted = false;
        selectedDifficulty = "";
        selectedDifficultyIndex = -1;
        selectedTimeLimit = 0f;
        
        // 重新顯示難度選擇 UI，隱藏時間選擇UI和遊戲UI
        if (difficultySelectionUI != null)
        {
            foreach (var ui in difficultySelectionUI)
                ui.SetActive(true);
        }
        HideTimeSelectionUI();
        HideGameplayUI();
        
        // 暫停遊戲系統
        InitializeGameSystems(false);
        
        // 清除所有魚
        if (fishSpawnManager != null)
        {
            fishSpawnManager.ClearAllFish();
        }
        
        Debug.Log("[GameModeManager] 遊戲已重置，等待重新選擇難度");
    }
    
    /// <summary>
    /// 獲取當前選擇的難度
    /// </summary>
    public string GetSelectedDifficulty()
    {
        return selectedDifficulty;
    }
    
    /// <summary>
    /// 檢查遊戲是否已開始
    /// </summary>
    public bool IsGameStarted()
    {
        return isGameStarted;
    }
    
    // ========== 任务系统集成 ==========
    
    /// <summary>
    /// 生成新任务
    /// </summary>
    private void GenerateNewTask()
    {
        if (taskManager != null && difficultyManager != null)
        {
            // 重新生成鱼（在生成任务前）
            RegenerateFish();
            
            // 验证鱼数量是否足够
            ValidateFishCount();
            
            // 从DifficultyManager获取任务类型
            TaskType taskType = difficultyManager.GetCurrentTaskType();
            
            Debug.Log("===========================================");
            Debug.Log($"[GameModeManager] 🎮 生成新任務 - 任務類型: {taskType}");
            
            // 困難模式使用 HardModeManager 生成任務（支援多水桶）
            if (taskType == TaskType.MultiStage && hardModeManager != null)
            {
                Debug.Log($"[GameModeManager] ✅ 使用 HardModeManager 生成困難模式任務（多水桶模式）");
                hardModeManager.GenerateHardTask();
            }
            else if (taskType == TaskType.MultiStage && hardModeManager == null)
            {
                Debug.LogWarning($"[GameModeManager] ⚠️ 任務類型是 MultiStage 但 HardModeManager 為 null！");
                Debug.LogWarning($"[GameModeManager] 回退使用 TaskManager 生成任務");
                taskManager.GenerateRandomTask(taskType);
            }
            else
            {
                // 簡單/普通模式使用 TaskManager
                Debug.Log($"[GameModeManager] 使用 TaskManager 生成任務: {taskType}");
                taskManager.GenerateRandomTask(taskType);
            }
            Debug.Log("===========================================");
        }
        else
        {
            if (taskManager == null)
                Debug.LogError("[GameModeManager] ❌ TaskManager 引用为空！");
            if (difficultyManager == null)
                Debug.LogError("[GameModeManager] ❌ DifficultyManager 引用为空！");
        }
    }
    
    /// <summary>
    /// 重新生成鱼
    /// </summary>
    private void RegenerateFish()
    {
        // 根據當前模式獲取正確的水桶並清空
        ClearActiveBucket();
        
        if (fishSpawnManager != null && difficultyManager != null)
        {
            // 清除所有场景中的鱼
            fishSpawnManager.ClearAllFish();
            
            // 注意：生成模式已由DifficultyManager在难度选择时配置，不需要再设置
            
            // 重新生成鱼
            fishSpawnManager.RegenerateAllFish();
            
            int difficultyIndex = difficultyManager.GetCurrentDifficultyIndex();
            Debug.Log($"[GameModeManager] 重新生成鱼，难度：{difficultyIndex}");
        }
        else
        {
            if (fishSpawnManager == null)
                Debug.LogError("[GameModeManager] FishSpawnManager 引用为空！");
            if (difficultyManager == null)
                Debug.LogError("[GameModeManager] DifficultyManager 引用为空！");
        }
    }
    
    /// <summary>
    /// 清空當前活動的水桶
    /// </summary>
    private void ClearActiveBucket()
    {
        // 如果有 MultiBucketManager，根據當前模式清空正確的水桶
        if (MultiBucketManager.Instance != null)
        {
            if (!MultiBucketManager.Instance.IsHardMode)
            {
                // 普通模式：清空普通水桶
                BucketEvent normalBucket = MultiBucketManager.Instance.GetNormalModeBucketEvent();
                if (normalBucket != null)
                {
                    normalBucket.ClearBucket();
                    Debug.Log($"[GameModeManager] 已清空普通模式水桶: {normalBucket.gameObject.name}");
                    return;
                }
            }
            else
            {
                // 困難模式：由 MultiBucketManager 清空所有水桶
                MultiBucketManager.Instance.ClearAllBuckets();
                Debug.Log("[GameModeManager] 已清空所有困難模式水桶");
                return;
            }
        }
        
        // 備用：使用 ServiceLocator 獲取 BucketEvent
        BucketEvent bucketEvent = ServiceLocator.Instance.Get<BucketEvent>();
        if (bucketEvent != null)
        {
            bucketEvent.ClearBucket();
            Debug.Log("[GameModeManager] 已清空桶中的鱼");
        }
    }
    
    /// <summary>
    /// 验证鱼数量是否足够完成任务
    /// </summary>
    private void ValidateFishCount()
    {
        if (fishSpawnManager == null || taskManager == null) return;
        
        // 获取当前任务
        TaskData currentTask = taskManager.GetCurrentTask();
        if (currentTask == null) return;
        
        // 等待一帧，确保鱼已经生成完毕
        StartCoroutine(ValidateFishCountCoroutine(currentTask));
    }
    
    /// <summary>
    /// 延迟验证鱼数量（等待生成完成）
    /// </summary>
    private System.Collections.IEnumerator ValidateFishCountCoroutine(TaskData currentTask)
    {
        // 等待 0.5 秒，确保所有鱼都已生成
        yield return new WaitForSeconds(0.5f);
        
        // 根据任务类型验证
        switch (currentTask.taskType)
        {
            case TaskType.CountOnly:
                // 简单模式：只需要足够的鱼即可
                int totalFish = fishSpawnManager.GetActualTotalFishCount();
                if (totalFish < currentTask.targetCount)
                {
                    Debug.LogError($"[GameModeManager] ❌ 鱼数量不足！当前 {totalFish} 条，任务需要 {currentTask.targetCount} 条");
                    ShowSpawnPointWarning(currentTask.targetCount);
                }
                else
                {
                    Debug.Log($"[GameModeManager] ✅ 鱼数量充足：{totalFish} 条（需要 {currentTask.targetCount} 条）");
                }
                break;
                
            case TaskType.ColorCount:
                // 中级模式：验证特定颜色的鱼数量
                int colorFishCount = fishSpawnManager.GetActualFishCountByColor(currentTask.targetColor);
                if (colorFishCount < currentTask.targetCount)
                {
                    Debug.LogError($"[GameModeManager] ❌ {currentTask.targetColor} 数量不足！");
                    Debug.LogError($"[GameModeManager] 当前场景中有 {colorFishCount} 条，任务需要 {currentTask.targetCount} 条");
                    ShowSpawnPointWarning(currentTask.targetCount);
                }
                else
                {
                    Debug.Log($"[GameModeManager] ✅ {currentTask.targetColor} 数量充足：{colorFishCount} 条（需要 {currentTask.targetCount} 条）");
                }
                break;
                
            case TaskType.MultiStage:
                // 高级模式：验证所有子任务的鱼数量
                bool allSubTasksValid = true;
                foreach (var subTask in currentTask.subTasks)
                {
                    int subTaskFishCount = fishSpawnManager.GetActualFishCountByColor(subTask.color);
                    if (subTaskFishCount < subTask.count)
                    {
                        Debug.LogError($"[GameModeManager] ❌ {subTask.color} 数量不足！当前 {subTaskFishCount} 条，需要 {subTask.count} 条");
                        allSubTasksValid = false;
                    }
                    else
                    {
                        Debug.Log($"[GameModeManager] ✅ {subTask.color} 数量充足：{subTaskFishCount} 条（需要 {subTask.count} 条）");
                    }
                }
                
                if (!allSubTasksValid)
                {
                    ShowSpawnPointWarning(5); // 高级模式通常需要更多鱼
                }
                break;
        }
    }
    
    /// <summary>
    /// 显示生成点不足的警告信息
    /// </summary>
    private void ShowSpawnPointWarning(int requiredCount)
    {
        Debug.LogWarning($"[GameModeManager] 📋 解決方案：");
        Debug.LogWarning($"[GameModeManager] 1. 在 FishSpawnManager Inspector 中增加 Spawn Points 数量");
        Debug.LogWarning($"[GameModeManager] 2. 或启用 'Allow Reuse Spawn Points'（自动启用中...）");
        Debug.LogWarning($"[GameModeManager] 3. 或调整 'Min Fish Per Color' 值（当前默认: 5）");
    }
    
    /// <summary>
    /// 获取当前难度索引
    /// </summary>
    private int GetDifficultyIndex()
    {
        if (difficultyManager != null)
        {
            return difficultyManager.GetCurrentDifficultyIndex();
        }
        
        Debug.LogWarning("[GameModeManager] DifficultyManager 未设置，返回默认难度0");
        return 0;
    }
    
    /// <summary>
    /// 任务验证回调
    /// </summary>
    private void OnTaskValidated(TaskValidationResult result)
    {
        switch (result)
        {
            case TaskValidationResult.Success:
                // 任务完成，加分并生成新任务
                if (scoreManager != null)
                {
                    scoreManager.AddTaskScore();
                }
                GenerateNewTask();
                break;
                
            case TaskValidationResult.Failed:
                // 任务失败（所有模式都重新生成任务）
                Debug.Log("[GameModeManager] 任务失败，将重新生成任务");
                OnTaskFailed();
                break;
                
            case TaskValidationResult.SubTaskComplete:
                // 子任务完成，继续当前任务（不重新生成鱼）
                Debug.Log("[GameModeManager] 子任务完成，继续下一阶段");
                
                // 添加子任务分数（已移到OnSubTaskComplete中处理）
                break;
                
            case TaskValidationResult.Incomplete:
                // 任务未完成，继续
                Debug.Log("[GameModeManager] 任务未完成，继续收集");
                break;
        }
    }
    
    /// <summary>
    /// 子任务完成回调
    /// </summary>
    private void OnSubTaskComplete(SubTask subTask)
    {
        Debug.Log($"[GameModeManager] 子任务完成：{subTask.color} x {subTask.count}");
        
        // 添加子任务分数
        if (scoreManager != null)
        {
            scoreManager.AddSubTaskScore();
        }
    }
    
    /// <summary>
    /// 困難模式：所有水桶完成回調（平行任務模式專用）
    /// </summary>
    private void OnAllBucketsCompleted()
    {
        Debug.Log("[GameModeManager] 🎉 困難模式：所有水桶任務完成！");
        
        // 加分
        if (scoreManager != null)
        {
            scoreManager.AddTaskScore();
            Debug.Log("[GameModeManager] ✅ 已添加任務分數");
        }
        
        // 刷新任務：清空所有水桶 + 重新生成魚 + 生成新任務
        Debug.Log("[GameModeManager] 🔄 準備生成新的困難模式任務...");
        GenerateNewTask();
    }
    
    /// <summary>
    /// 任务失败处理（由ConfirmButtonHandler调用）
    /// </summary>
    public void OnTaskFailed()
    {
        Debug.Log("[GameModeManager] 处理任务失败");
        
        // 所有难度模式在任务失败时都重新生成任务
        if (difficultyManager != null)
        {
            TaskType taskType = difficultyManager.GetCurrentTaskType();
            Debug.Log($"[GameModeManager] 任务失败，重新生成任务（难度：{taskType}）");
        }
        GenerateNewTask();
    }
}
