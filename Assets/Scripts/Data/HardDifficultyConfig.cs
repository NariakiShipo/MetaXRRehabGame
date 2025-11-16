using UnityEngine;

/// <summary>
/// 困难难度配置 - 多阶段任务，要求执行顺序
/// </summary>
[System.Serializable]
public class HardDifficultyConfig : DifficultyConfig
{
    [Header("困难模式特殊设置")]
    [Tooltip("生成的颜色数量")]
    public int colorCount = 4;
    
    [Tooltip("多阶段任务数量")]
    public int subTaskCount = 3;
    
    [Tooltip("每个子任务的鱼数量范围")]
    public int minFishPerSubTask = 1;
    public int maxFishPerSubTask = 2;
    
    public HardDifficultyConfig()
    {
        difficultyName = "困难";
        difficultyIndex = 2;
        taskType = TaskType.MultiStage;
        timeLimit = 600f;  // 10分钟
        scoreMultiplier = 2.0f;
        minFishPerColor = 5;
    }
    
    /// <summary>
    /// 配置鱼生成管理器 - 困难模式生成3-4种颜色
    /// </summary>
    public override void ConfigureFishSpawnManager(FishSpawnManager fishSpawnManager)
    {
        if (fishSpawnManager == null) return;
        
        fishSpawnManager.SetSpawnMode(difficultyIndex);
        Debug.Log($"[HardDifficulty] 配置鱼生成：{colorCount}种颜色混合（多阶段）");
    }
    
    /// <summary>
    /// 配置任务管理器 - 困难模式要求多阶段+顺序
    /// </summary>
    public override void ConfigureTaskManager(TaskManager taskManager)
    {
        if (taskManager == null) return;
        
        // 可以在这里设置困难模式特有的任务参数
        Debug.Log($"[HardDifficulty] 配置任务：多阶段 ({subTaskCount}个子任务，每个{minFishPerSubTask}-{maxFishPerSubTask}条)");
    }
    
    public override string GetDescription()
    {
        return $"困難模式 - {timeLimit}秒 - 多階段任務 - {scoreMultiplier}x分數";
    }
}
