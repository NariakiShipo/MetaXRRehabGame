using UnityEngine;

/// <summary>
/// 重试按钮处理器 - 重试当前子任务（高级模式专用）
/// </summary>
public class RetryButtonHandler : MonoBehaviour
{
    [Header("引用")]
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private BucketEvent bucketEvent;
    
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
    }
    
    /// <summary>
    /// 重试按钮按下（由ButtonEvent的UnityEvent调用）
    /// </summary>
    public void OnRetryButtonPressed()
    {
        if (taskManager == null || bucketEvent == null)
        {
            Debug.LogWarning("[RetryButtonHandler] TaskManager或BucketEvent未设置");
            return;
        }
        
        Debug.Log("[RetryButtonHandler] 重试当前子任务");
        
        // 清空桶
        bucketEvent.ClearBucket();
        
        // 重置当前子任务进度
        taskManager.RetryCurrentSubTask();
    }
}
