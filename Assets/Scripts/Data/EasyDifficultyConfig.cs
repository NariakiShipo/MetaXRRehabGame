using UnityEngine;

/// <summary>
/// 简单难度配置 - 只要求数量认知，不限颜色
/// </summary>
[System.Serializable]
public class EasyDifficultyConfig : DifficultyConfig
{
    [Header("简单模式特殊设置")]
    [Tooltip("是否只生成单一颜色的鱼")]
    public bool useSingleColor = true;
    
    [Tooltip("任务鱼数量范围")]
    public int minFishCount = 1;
    public int maxFishCount = 3;
    
    public EasyDifficultyConfig()
    {
        difficultyName = "简单";
        difficultyIndex = 0;
        taskType = TaskType.CountOnly;
        timeLimit = 180f;  // 3分钟
        scoreMultiplier = 1.0f;
        minFishPerColor = 5;
    }
    
    /// <summary>
    /// 配置鱼生成管理器 - 简单模式只生成一种颜色
    /// </summary>
    public override void ConfigureFishSpawnManager(FishSpawnManager fishSpawnManager)
    {
        if (fishSpawnManager == null) return;
        
        fishSpawnManager.SetSpawnMode(difficultyIndex);
        Debug.Log($"[EasyDifficulty] 配置鱼生成：单一颜色模式");
    }
    
    /// <summary>
    /// 配置任务管理器 - 简单模式只要求数量
    /// </summary>
    public override void ConfigureTaskManager(TaskManager taskManager)
    {
        if (taskManager == null) return;
        
        // 可以在这里设置简单模式特有的任务参数
        Debug.Log($"[EasyDifficulty] 配置任务：数量认知 ({minFishCount}-{maxFishCount}条)");
    }
    
    public override string GetDescription()
    {
        return $"簡單模式 - {timeLimit}秒 - 數量認知 - {scoreMultiplier}x分數";
    }
}
