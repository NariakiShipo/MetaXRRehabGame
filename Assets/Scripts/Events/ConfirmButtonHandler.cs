using UnityEngine;

/// <summary>
/// 确认按钮处理器 - 触发任务验证
/// </summary>
public class ConfirmButtonHandler : MonoBehaviour
{
    [Header("引用")]
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private BucketEvent bucketEvent;  // 備用：直接指定的水桶
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
        
        // bucketEvent 會在 GetActiveBucketEvent() 中動態獲取，這裡只作為備用
        if (bucketEvent == null)
        {
            if (!ServiceLocator.Instance.TryGet(out bucketEvent))
            {
                bucketEvent = FindFirstObjectByType<BucketEvent>();
            }
        }
        
        if (gameModeManager == null)
        {
            if (!ServiceLocator.Instance.TryGet(out gameModeManager))
            {
                gameModeManager = FindFirstObjectByType<GameModeManager>();
            }
        }
    }
    
    /// <summary>
    /// 獲取當前應該使用的 BucketEvent（根據難度模式）
    /// </summary>
    private BucketEvent GetActiveBucketEvent()
    {
        // 如果有 MultiBucketManager，根據當前模式獲取正確的水桶
        if (MultiBucketManager.Instance != null)
        {
            if (!MultiBucketManager.Instance.IsHardMode)
            {
                // 普通模式：使用普通水桶
                BucketEvent normalBucket = MultiBucketManager.Instance.GetNormalModeBucketEvent();
                if (normalBucket != null)
                {
                    return normalBucket;
                }
            }
            // 困難模式：使用 HardModeManager 的驗證邏輯，這裡不需要單獨的 BucketEvent
        }
        
        // 備用：使用直接設置的 bucketEvent
        return bucketEvent;
    }
    
    /// <summary>
    /// 确认按钮按下（由ButtonEvent的UnityEvent调用）
    /// </summary>
    public void OnConfirmButtonPressed()
    {
        // 獲取當前應該使用的水桶
        BucketEvent activeBucket = GetActiveBucketEvent();
        
        if (taskManager == null)
        {
            Debug.LogWarning("[ConfirmButtonHandler] TaskManager 未设置");
            return;
        }
        
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
