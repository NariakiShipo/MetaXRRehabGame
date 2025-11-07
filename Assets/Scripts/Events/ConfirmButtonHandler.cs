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
    
    private void Awake()
    {
        // 自动查找引用
        if (taskManager == null)
        {
            taskManager = Object.FindFirstObjectByType<TaskManager>();
        }
        
        if (bucketEvent == null)
        {
            bucketEvent = Object.FindFirstObjectByType<BucketEvent>();
        }
        
        if (gameModeManager == null)
        {
            gameModeManager = Object.FindFirstObjectByType<GameModeManager>();
        }
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
                // GameModeManager会监听TaskValidated事件并生成新任务
                break;
                
            case TaskValidationResult.Failed:
                // 任务失败，清空桶并重置任务（中高级模式）
                bucketEvent.ClearBucket();
                if (gameModeManager != null)
                {
                    gameModeManager.OnTaskFailed();
                }
                break;
                
            case TaskValidationResult.Incomplete:
                // 任务未完成，不做任何操作
                Debug.Log("[ConfirmButtonHandler] 任务未完成，继续收集鱼");
                break;
                
            case TaskValidationResult.SubTaskComplete:
                // 子任务完成，清空桶并继续下一个子任务
                bucketEvent.ClearBucket();
                Debug.Log("[ConfirmButtonHandler] 子任务完成，继续下一个");
                break;
        }
    }
}
