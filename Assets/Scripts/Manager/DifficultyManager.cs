using UnityEngine;
using System;

/// <summary>
/// 难度管理器 - 中心控制器，管理所有难度配置
/// </summary>
public class DifficultyManager : MonoBehaviour
{
    [Header("难度配置")]
    [SerializeField] private EasyDifficultyConfig easyConfig;
    [SerializeField] private NormalDifficultyConfig normalConfig;
    [SerializeField] private HardDifficultyConfig hardConfig;
    
    [Header("依赖引用")]
    [SerializeField] private FishSpawnManager fishSpawnManager;
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private ScoreManager scoreManager;
    
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
        if (easyConfig == null)
            easyConfig = new EasyDifficultyConfig();
            
        if (normalConfig == null)
            normalConfig = new NormalDifficultyConfig();
            
        if (hardConfig == null)
            hardConfig = new HardDifficultyConfig();
            
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
        SetDifficulty(easyConfig);
    }
    
    /// <summary>
    /// 设置普通难度
    /// </summary>
    public void SetNormalDifficulty()
    {
        SetDifficulty(normalConfig);
    }
    
    /// <summary>
    /// 设置困难难度
    /// </summary>
    public void SetHardDifficulty()
    {
        SetDifficulty(hardConfig);
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
        
        // 配置鱼生成管理器
        if (fishSpawnManager != null)
        {
            currentDifficulty.ConfigureFishSpawnManager(fishSpawnManager);
        }
        
        // 配置任务管理器
        if (taskManager != null)
        {
            currentDifficulty.ConfigureTaskManager(taskManager);
        }
        
        // 配置分数管理器
        if (scoreManager != null)
        {
            scoreManager.SetDifficulty(currentDifficulty.GetTaskType());
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
        return easyConfig;
    }
    
    /// <summary>
    /// 获取普通难度配置
    /// </summary>
    public NormalDifficultyConfig GetNormalConfig()
    {
        return normalConfig;
    }
    
    /// <summary>
    /// 获取困难难度配置
    /// </summary>
    public HardDifficultyConfig GetHardConfig()
    {
        return hardConfig;
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
