using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 任务显示UI - 负责显示当前任务文本、进度
/// </summary>
public class TaskDisplayUI : MonoBehaviour
{
    [Header("UI引用")]
    [SerializeField] private TextMeshProUGUI taskDescriptionText;   // 任务描述文本
    [SerializeField] private TextMeshProUGUI errorMessageText;      // 错误信息文本
    [SerializeField] private GameObject errorMessagePanel;          // 错误信息面板
    
    [Header("错误信息配置")]
    [SerializeField] private float errorMessageDuration = 2f;       // 错误信息显示时长
    
    [SerializeField]private TaskManager taskManager;
    
    // 用于跟踪当前运行的协程
    private Coroutine errorMessageCoroutine = null;
    
    private void Awake()
    {   
        if (taskManager == null)
        {
            Debug.LogError("[TaskDisplayUI] 找不到TaskManager!");
        }
        
        // 初始化UI
        if (errorMessagePanel != null)
        {
            errorMessagePanel.SetActive(false);
        }
    }
    
    private void OnEnable()
    {
        if (taskManager != null)
        {
            // 订阅事件
            taskManager.OnTaskGenerated.AddListener(OnTaskGenerated);
            taskManager.OnTaskValidated.AddListener(OnTaskValidated);
            taskManager.OnSubTaskComplete.AddListener(OnSubTaskComplete);
            taskManager.OnTaskFailed.AddListener(OnTaskFailed);
            
            Debug.Log("[TaskDisplayUI] 已订阅 TaskManager 事件");
        }
        else
        {
            Debug.LogError("[TaskDisplayUI] OnEnable: taskManager 为空，无法订阅事件！");
        }
    }
    
    private void OnDisable()
    {
        if (taskManager != null)
        {
            // 取消订阅
            taskManager.OnTaskGenerated.RemoveListener(OnTaskGenerated);
            taskManager.OnTaskValidated.RemoveListener(OnTaskValidated);
            taskManager.OnSubTaskComplete.RemoveListener(OnSubTaskComplete);
            taskManager.OnTaskFailed.RemoveListener(OnTaskFailed);
        }
        
        // 停止所有错误信息协程
        StopErrorMessageCoroutine();
    }
    
    /// <summary>
    /// 任务生成时更新UI
    /// </summary>
    private void OnTaskGenerated(TaskData task)
    {
        Debug.Log($"[TaskDisplayUI] OnTaskGenerated 被调用，任务类型: {task?.taskType}");
        UpdateTaskDescription(task);
        HideErrorMessage();
    }
    
    /// <summary>
    /// 任务验证时更新UI
    /// </summary>
    private void OnTaskValidated(TaskValidationResult result)
    {
        switch (result)
        {
            case TaskValidationResult.Success:
                // 任务完成，可以显示完成信息或清空UI
                if (taskDescriptionText != null)
                {
                    taskDescriptionText.text = "任務完成！";
                }
                break;
                
            case TaskValidationResult.Failed:
                // 任务失败，显示错误信息
                ShowErrorMessage("撈錯了！請重新開始");
                break;
                
            case TaskValidationResult.Incomplete:
                // 任务未完成，不显示任何信息
                break;
                
            case TaskValidationResult.SubTaskComplete:
                // 子任务完成，显示下一个子任务
                if (taskManager != null)
                {
                    TaskData currentTask = taskManager.GetCurrentTask();
                    UpdateTaskDescription(currentTask);
                }
                break;
        }
    }
    
    /// <summary>
    /// 子任务完成时
    /// </summary>
    private void OnSubTaskComplete(SubTask subTask)
    {
        Debug.Log($"[TaskDisplayUI] 子任务完成：{subTask.color} x {subTask.count}");
        // 直接显示下一个任务，不需要额外信息
    }
    
    /// <summary>
    /// 任务失败时
    /// </summary>
    private void OnTaskFailed()
    {
        ShowErrorMessage("撈錯了！請重新開始");
    }
    
    /// <summary>
    /// 更新任务描述
    /// </summary>
    private void UpdateTaskDescription(TaskData task)
    {
        if (taskDescriptionText == null)
        {
            Debug.LogError("[TaskDisplayUI] taskDescriptionText 引用为空！请在 Inspector 中设置");
            return;
        }
        
        if (taskManager == null)
        {
            Debug.LogError("[TaskDisplayUI] taskManager 引用为空！");
            return;
        }
        
        if (task == null)
        {
            Debug.LogError("[TaskDisplayUI] task 数据为空！");
            return;
        }
        
        string description = taskManager.GetTaskDescription(task);
        taskDescriptionText.text = description;
        Debug.Log($"[TaskDisplayUI] 更新任务描述: {description}");
        Debug.Log($"[TaskDisplayUI] TextMeshPro 组件状态: enabled={taskDescriptionText.enabled}, gameObject.activeSelf={taskDescriptionText.gameObject.activeSelf}");
    }
    
    /// <summary>
    /// 显示错误信息
    /// </summary>
    private void ShowErrorMessage(string message)
    {
        // 安全检查：确保组件存在
        if (string.IsNullOrEmpty(message))
        {
            Debug.LogWarning("[TaskDisplayUI] 尝试显示空的错误信息");
            return;
        }
        
        // 设置错误文本
        if (errorMessageText != null)
        {
            errorMessageText.text = message;
        }
        else
        {
            Debug.LogWarning("[TaskDisplayUI] errorMessageText 未设置，无法显示错误文本");
        }
        
        // 显示错误面板
        if (errorMessagePanel != null)
        {
            errorMessagePanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[TaskDisplayUI] errorMessagePanel 未设置，无法显示错误面板");
        }
        
        // 停止之前的协程（如果存在）
        StopErrorMessageCoroutine();
        
        // 启动新的协程来自动隐藏错误信息
        errorMessageCoroutine = StartCoroutine(ErrorMessageCoroutine());
        
        Debug.Log($"[TaskDisplayUI] 显示错误信息: {message}");
    }
    
    /// <summary>
    /// 错误信息协程 - 负责在指定时间后自动隐藏错误信息
    /// </summary>
    private IEnumerator ErrorMessageCoroutine()
    {
        // 安全检查：确保持续时间有效
        float duration = Mathf.Max(0.1f, errorMessageDuration);
        
        Debug.Log($"[TaskDisplayUI] 错误信息将在 {duration} 秒后隐藏");
        
        // 等待指定时间
        yield return new WaitForSeconds(duration);
        
        // 隐藏错误信息
        HideErrorMessage();
        
        // 清空协程引用
        errorMessageCoroutine = null;
    }
    
    /// <summary>
    /// 停止错误信息协程
    /// </summary>
    private void StopErrorMessageCoroutine()
    {
        if (errorMessageCoroutine != null)
        {
            StopCoroutine(errorMessageCoroutine);
            errorMessageCoroutine = null;
            Debug.Log("[TaskDisplayUI] 已停止错误信息协程");
        }
    }
    
    /// <summary>
    /// 隐藏错误信息
    /// </summary>
    private void HideErrorMessage()
    {
        // 安全检查并隐藏错误面板
        if (errorMessagePanel != null)
        {
            errorMessagePanel.SetActive(false);
           
            if (errorMessageText != null)
            {
                errorMessageText.text = "";
                Debug.Log("[TaskDisplayUI] 已隐藏错误信息");
            }
            
        }
        // if (errorMessageText != null)
        // {
        //     errorMessageText.text = "";
        // }
    }
    
    /// <summary>
    /// 清空任务显示
    /// </summary>
    public void ClearTaskDisplay()
    {
        if (taskDescriptionText != null)
        {
            taskDescriptionText.text = "";
        }
        HideErrorMessage();
    }
}
