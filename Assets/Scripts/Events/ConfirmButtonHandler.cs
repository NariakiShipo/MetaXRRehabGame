using UnityEngine;

/// <summary>
/// 确认按钮处理器 - 触发任务验证
/// </summary>
public class ConfirmButtonHandler : MonoBehaviour
{
    [Header("引用")]
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private BucketEvent bucketEvent;
    [SerializeField] private GameModeManager gameModeManager;
    [SerializeField]private AudioSource audioSource;
    [SerializeField]private AudioClip correctSound;
    [SerializeField]private AudioClip incorrectSound;
    [SerializeField]private AudioClip resetSound;
    
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
        
        // 診斷日誌
        Debug.Log($"[ConfirmButtonHandler] 初始化完成 - TaskManager: {(taskManager != null ? "✓" : "✗")}, BucketEvent: {(bucketEvent != null ? "✓" : "✗")}");
    }
    
    /// <summary>
    /// 确认按钮按下（由ButtonEvent的UnityEvent调用）
    /// </summary>
    public void OnConfirmButtonPressed()
    {
        if (taskManager == null || bucketEvent == null)
        {
            Debug.LogWarning("[ConfirmButtonHandler] TaskManager或BucketEvent未设置");
            return;
        }
        
        // 获取桶中的鱼
        var fishInBucket = bucketEvent.GetFishInBucket();
        
        // 验证任务
        TaskValidationResult result = taskManager.ValidateTask(fishInBucket);
        
        Debug.Log($"[ConfirmButtonHandler] 验证结果: {result}");
        
        // 根据结果处理
        switch (result)
        {
            case TaskValidationResult.Success:
                // 任务完成，清空桶并生成新任务
                bucketEvent.ClearBucket();
                audioSource.PlayOneShot(correctSound);
                Debug.Log("[ConfirmButtonHandler] 任务完成，生成新任务");
                // GameModeManager会监听TaskValidated事件并生成新任务
                break;
                
            case TaskValidationResult.Failed:
                // 任务失败，清空桶并重置任务
                bucketEvent.ClearBucket();
              
                audioSource.PlayOneShot(resetSound);
                Debug.Log("[ConfirmButtonHandler] 任務失敗，重新生成任務和魚");
                break;
                
            case TaskValidationResult.Incomplete:
                // 任务不完整，提示玩家继续完成任务
                audioSource.PlayOneShot(incorrectSound);
                break;
                
            case TaskValidationResult.SubTaskComplete:
                // 子任务完成，清空桶并继续下一个子任务
                bucketEvent.ClearBucket();
                audioSource.PlayOneShot(correctSound);
                break;
        }
    }
}
