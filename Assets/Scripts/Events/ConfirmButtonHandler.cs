using UnityEngine;

/// <summary>
/// 确认按钮处理器 - 触发任务验证
/// </summary>
public class ConfirmButtonHandler : MonoBehaviour
{
    [Header("引用")]
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private GameModeManager gameModeManager;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip correctSound;
    [SerializeField] private AudioClip incorrectSound;
    [SerializeField] private AudioClip resetSound;
    
    private void Awake()
    {
        // 使用 ServiceLocator 獲取服務（使用 TryGet 避免錯誤日誌）
        if (taskManager == null)
        {
            if (!ServiceLocator.Instance.TryGet(out taskManager))
            {
                taskManager = FindFirstObjectByType<TaskManager>();
            }
        }
        
        if (gameModeManager == null)
        {
            if (!ServiceLocator.Instance.TryGet(out gameModeManager))
            {
                gameModeManager = FindFirstObjectByType<GameModeManager>();
            }
        }
        
        // ✅ 移除了直接的 bucketEvent SerializeField 和初始化
        // BucketEvent 現在由 GetActiveBucketEvent() 動態獲取
    }
    
    /// <summary>
    /// ✅ 獲取當前活躍的 BucketEvent（改為使用 MultiBucketManager）
    /// </summary>
    private BucketEvent GetActiveBucketEvent()
    {
        // 優先使用 MultiBucketManager
        if (MultiBucketManager.Instance != null)
        {
            if (!MultiBucketManager.Instance.IsHardMode)
            {
                // 普通模式：從 MultiBucketManager 獲取普通水桶
                BucketEvent normalBucket = MultiBucketManager.Instance.GetNormalModeBucketEvent();
                if (normalBucket != null)
                {
                    return normalBucket;
                }
            }
            // 困難模式：使用 HardModeManager 的驗證邏輯，這裡不需要單獨的 BucketEvent
        }
        
        // 備用方案：直接查找（如果 MultiBucketManager 無法提供）
        return FindFirstObjectByType<BucketEvent>();
    }
    
    /// <summary>
    /// 确认按钮按下（由ButtonEvent的UnityEvent调用）
    /// </summary>
    public void OnConfirmButtonPressed()
    {
        if (taskManager == null)
        {
            Debug.LogWarning("[ConfirmButtonHandler] TaskManager 未设置");
            return;
        }
        
        // 困難模式平行任務：使用 MultiBucketManager 驗證
        if (MultiBucketManager.Instance != null && MultiBucketManager.Instance.IsHardMode)
        {
            Debug.Log("[ConfirmButtonHandler] 困難模式平行任務驗證");
            
            bool allValid = MultiBucketManager.Instance.ValidateAllBuckets();
            
            if (allValid)
            {
                audioSource.PlayOneShot(correctSound);
                Debug.Log("[ConfirmButtonHandler] 所有水桶任務完成！");
                // MultiBucketManager.OnAllStagesCompleted 會觸發 GameModeManager.OnAllBucketsCompleted
            }
            else
            {
                audioSource.PlayOneShot(incorrectSound);
                Debug.Log("[ConfirmButtonHandler] 尚有水桶未完成或有錯誤");
            }
            return;
        }
        
        // 普通模式或舊版困難模式：使用原有驗證邏輯
        BucketEvent activeBucket = GetActiveBucketEvent();
        
        if (activeBucket == null)
        {
            Debug.LogWarning("[ConfirmButtonHandler] 無法獲取有效的 BucketEvent");
            return;
        }
        
        Debug.Log($"[ConfirmButtonHandler] 使用水桶: {activeBucket.gameObject.name}");
        
        // 获取桶中的鱼
        var fishInBucket = activeBucket.GetFishInBucket();
        
        // 验证任务
        TaskValidationResult result = taskManager.ValidateTask(fishInBucket);
        
        Debug.Log($"[ConfirmButtonHandler] 验证结果: {result}");
        
        // 根据结果处理
        switch (result)
        {
            case TaskValidationResult.Success:
                // 任务完成，清空桶并生成新任务
                activeBucket.ClearBucket();
                audioSource.PlayOneShot(correctSound);
                Debug.Log("[ConfirmButtonHandler] 任务完成，生成新任务");
                // GameModeManager会监听TaskValidated事件并生成新任务
                break;
                
            case TaskValidationResult.Failed:
                // 任务失败，清空桶并重置任务
                activeBucket.ClearBucket();
              
                audioSource.PlayOneShot(resetSound);
                Debug.Log("[ConfirmButtonHandler] 任務失敗，重新生成任務和魚");
                break;
                
            case TaskValidationResult.Incomplete:
                // 任务不完整，提示玩家继续完成任务
                audioSource.PlayOneShot(incorrectSound);
                break;
                
            case TaskValidationResult.SubTaskComplete:
                // 子任务完成，清空桶并继续下一个子任务
                activeBucket.ClearBucket();
                audioSource.PlayOneShot(correctSound);
                break;
        }
    }
}
