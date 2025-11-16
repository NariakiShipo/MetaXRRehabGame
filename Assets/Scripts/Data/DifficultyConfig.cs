using UnityEngine;

/// <summary>
/// 难度配置基类 - 定义所有难度模式的共同接口
/// </summary>
[System.Serializable]
public abstract class DifficultyConfig
{
    [Header("基础设置")]
    public string difficultyName;
    public int difficultyIndex;
    public TaskType taskType;
    
    [Header("时间设置")]
    public float timeLimit;
    
    [Header("分数设置")]
    public float scoreMultiplier;
    
    [Header("鱼生成设置")]
    public int minFishPerColor = 5;
    
    /// <summary>
    /// 获取难度索引
    /// </summary>
    public int GetDifficultyIndex() => difficultyIndex;
    
    /// <summary>
    /// 获取难度名称
    /// </summary>
    public string GetDifficultyName() => difficultyName;
    
    /// <summary>
    /// 获取任务类型
    /// </summary>
    public TaskType GetTaskType() => taskType;
    
    /// <summary>
    /// 获取时间限制
    /// </summary>
    public float GetTimeLimit() => timeLimit;
    
    /// <summary>
    /// 设置时间限制（允许玩家选择）
    /// </summary>
    public void SetTimeLimit(float newTimeLimit)
    {
        timeLimit = newTimeLimit;
    }
    
    /// <summary>
    /// 获取分数倍率
    /// </summary>
    public float GetScoreMultiplier() => scoreMultiplier;
    
    /// <summary>
    /// 配置鱼生成管理器
    /// </summary>
    public abstract void ConfigureFishSpawnManager(FishSpawnManager fishSpawnManager);
    
    /// <summary>
    /// 配置任务管理器
    /// </summary>
    public abstract void ConfigureTaskManager(TaskManager taskManager);
    
    /// <summary>
    /// 获取难度描述
    /// </summary>
    public virtual string GetDescription()
    {
        return $"{difficultyName} - {timeLimit}秒 - {scoreMultiplier}x分数";
    }
}
