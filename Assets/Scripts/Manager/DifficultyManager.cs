using UnityEngine;
using System;

/// <summary>
/// 难度管理器 - 中心控制器，管理所有难度配置
/// </summary>
public class DifficultyManager : MonoBehaviour
{
    [Header("难度配置")]
    [SerializeField] private EasyDifficultyConfig easyDifficulty;
    [SerializeField] private NormalDifficultyConfig normalDifficulty;
    [SerializeField] private HardDifficultyConfig hardDifficulty;
    
    [Header("依赖引用")]
    // 已改用 ServiceLocator，移除 SerializeField 依賴
    private FishSpawnManager fishSpawnManager;
    private TaskManager taskManager;
    private ScoreManager scoreManager;
    
    // 当前选择的难度配置
    private DifficultyConfig currentDifficulty;
    
    // 单例模式
    public static DifficultyManager Instance { get; private set; }
    
    // 事件
    public event Action<DifficultyConfig> OnDifficultyChanged;
    
    #region Unity生命周期
    
    private void Awake()
    {
        // 设置单例
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // 初始化配置对象
        InitializeConfigs();
    }
    
    private void Start()
    {
        // 通过 ServiceLocator 获取依赖
        fishSpawnManager = ServiceLocator.Instance.Get<FishSpawnManager>();
        taskManager = ServiceLocator.Instance.Get<TaskManager>();
        scoreManager = ServiceLocator.Instance.Get<ScoreManager>();
        
        // 验证依赖
        ValidateDependencies();
    }
    
    #endregion
    
    #region 初始化
    
    /// <summary>
    /// 初始化所有难度配置
    /// </summary>
    private void InitializeConfigs()
    {
        if (easyDifficulty == null)
            easyDifficulty = new EasyDifficultyConfig();
            
        if (normalDifficulty == null)
            normalDifficulty = new NormalDifficultyConfig();
            
        if (hardDifficulty == null)
            hardDifficulty = new HardDifficultyConfig();
            
        Debug.Log("[DifficultyManager] 难度配置初始化完成");
    }
    
    /// <summary>
    /// 验证依赖引用
    /// </summary>
    private void ValidateDependencies()
    {
        if (fishSpawnManager == null)
            Debug.LogError("[DifficultyManager] FishSpawnManager 未设置！");
            
        if (taskManager == null)
            Debug.LogError("[DifficultyManager] TaskManager 未设置！");
            
        if (scoreManager == null)
            Debug.LogError("[DifficultyManager] ScoreManager 未设置！");
    }
    
    #endregion
    
    #region 难度选择
    
    /// <summary>
    /// 设置简单难度
    /// </summary>
    public void SetEasyDifficulty()
    {
        currentDifficulty = easyDifficulty;
        ApplyDifficulty();
    }
    
    /// <summary>
    /// 设置普通难度
    /// </summary>
    public void SetNormalDifficulty()
    {
        currentDifficulty = normalDifficulty;
        ApplyDifficulty();
    }
    
    /// <summary>
    /// 设置困难难度
    /// </summary>
    public void SetHardDifficulty()
    {
        currentDifficulty = hardDifficulty;
        ApplyDifficulty();
    }
    
    private void ApplyDifficulty()
    {
        Debug.Log("===========================================");
        Debug.Log($"[DifficultyManager] 🎯 應用難度設定: {currentDifficulty?.GetDifficultyName()}");
        Debug.Log($"[DifficultyManager] 任務類型: {currentDifficulty?.GetTaskType()}");
        
        // 配置所有相關管理器
        ConfigureAllManagers();
        
        // 發布事件通知其他系統
        EventBus.Instance.Publish(new DifficultyChangedEvent 
        { 
            NewDifficulty = currentDifficulty 
        });
        
        // 觸發本地事件
        OnDifficultyChanged?.Invoke(currentDifficulty);
        
        Debug.Log($"[DifficultyManager] ✅ 難度已應用完成: {currentDifficulty?.GetDifficultyName()}");
        Debug.Log("===========================================");
    }
    /// <summary>
    /// 根据索引设置难度
    /// </summary>
    public void SetDifficultyByIndex(int index)
    {
        switch (index)
        {
            case 0:
                SetEasyDifficulty();
                break;
            case 1:
                SetNormalDifficulty();
                break;
            case 2:
                SetHardDifficulty();
                break;
            default:
                Debug.LogError($"[DifficultyManager] 无效的难度索引: {index}");
                break;
        }
    }
    
    /// <summary>
    /// 设置难度配置（核心方法）
    /// </summary>
    private void SetDifficulty(DifficultyConfig config)
    {
        if (config == null)
        {
            Debug.LogError("[DifficultyManager] 难度配置为空！");
            return;
        }
        
        currentDifficulty = config;
        
        // 配置所有相关管理器
        ConfigureAllManagers();
        
        // 触发事件
        OnDifficultyChanged?.Invoke(currentDifficulty);
        
        Debug.Log($"[DifficultyManager] 已切换到 {config.GetDifficultyName()} 难度");
    }
    
    #endregion
    
    #region 管理器配置
    
    /// <summary>
    /// 配置所有管理器
    /// </summary>
    private void ConfigureAllManagers()
    {
        if (currentDifficulty == null) return;
        
        // 获取配置数据
        FishSpawnConfig fishConfig = currentDifficulty.GetFishSpawnConfig();
        TaskConfig taskConfig = currentDifficulty.GetTaskConfig();
        
        // 配置鱼生成管理器
        if (fishSpawnManager != null)
        {
            fishSpawnManager.ApplySpawnConfig(fishConfig);
        }
        
        // 配置任务管理器
        if (taskManager != null)
        {
            taskManager.ApplyTaskConfig(taskConfig);
        }
        
        // 配置分数管理器
        if (scoreManager != null)
        {
            scoreManager.SetDifficulty(currentDifficulty.GetTaskType());
        }
        
        // 配置水桶模式（切換普通/困難模式水桶）
        if (MultiBucketManager.Instance != null)
        {
            TaskType taskType = currentDifficulty.GetTaskType();
            if (taskType == TaskType.MultiStage)
            {
                // 困難模式會在 HardModeManager.SetupMultiBuckets() 中啟用多水桶
                Debug.Log("[DifficultyManager] 困難模式 - 水桶將由 HardModeManager 設置");
            }
            else
            {
                // 簡單/普通模式使用普通水桶
                MultiBucketManager.Instance.ActivateNormalMode();
                Debug.Log("[DifficultyManager] 普通模式 - 已啟用普通水桶");
            }
        }
    }
    
    #endregion
    
    #region 获取器方法
    
    /// <summary>
    /// 获取当前难度配置
    /// </summary>
    public DifficultyConfig GetCurrentDifficulty()
    {
        return currentDifficulty;
    }
    
    /// <summary>
    /// 获取当前任务类型
    /// </summary>
    public TaskType GetCurrentTaskType()
    {
        return currentDifficulty?.GetTaskType() ?? TaskType.CountOnly;
    }
    
    /// <summary>
    /// 获取当前时间限制
    /// </summary>
    public float GetCurrentTimeLimit()
    {
        return currentDifficulty?.GetTimeLimit() ?? 180f;
    }
    
    /// <summary>
    /// 获取当前分数倍率
    /// </summary>
    public float GetCurrentScoreMultiplier()
    {
        return currentDifficulty?.GetScoreMultiplier() ?? 1.0f;
    }
    
    /// <summary>
    /// 获取当前难度索引
    /// </summary>
    public int GetCurrentDifficultyIndex()
    {
        return currentDifficulty?.GetDifficultyIndex() ?? 0;
    }
    
    /// <summary>
    /// 获取简单难度配置
    /// </summary>
    public EasyDifficultyConfig GetEasyConfig()
    {
        return easyDifficulty;
    }
    
    /// <summary>
    /// 获取普通难度配置
    /// </summary>
    public NormalDifficultyConfig GetNormalConfig()
    {
        return normalDifficulty;
    }
    
    /// <summary>
    /// 获取困难难度配置
    /// </summary>
    public HardDifficultyConfig GetHardConfig()
    {
        return hardDifficulty;
    }
    
    /// <summary>
    /// 设置当前难度的时间限制
    /// </summary>
    public void SetCustomTimeLimit(float timeLimit)
    {
        if (currentDifficulty != null)
        {
            currentDifficulty.SetTimeLimit(timeLimit);
            Debug.Log($"[DifficultyManager] 设置时间限制为 {timeLimit} 秒");
        }
        else
        {
            Debug.LogWarning("[DifficultyManager] 未选择难度，无法设置时间限制");
        }
    }
    
    #endregion
}
