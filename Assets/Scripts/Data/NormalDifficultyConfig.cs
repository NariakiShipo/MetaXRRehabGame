using UnityEngine;

/// <summary>
/// 普通难度配置 - 要求颜色+数量认知
/// </summary>
[System.Serializable]
public class NormalDifficultyConfig : DifficultyConfig
{
    [Header("普通模式特殊设置")]
    [Tooltip("生成的颜色数量")]
    public int colorCount = 3;
    
    [Tooltip("每个颜色的任务鱼数量范围")]
    public int minFishPerColorTask = 1;
    public int maxFishPerColorTask = 3;
    
    public NormalDifficultyConfig()
    {
        difficultyName = "普通";
        difficultyIndex = 1;
        taskType = TaskType.ColorCount;
        timeLimit = 300f;  // 5分钟
        scoreMultiplier = 1.5f;
        minFishPerColor = 5;
    }
    
    /// <summary>
    /// 配置鱼生成管理器 - 普通模式生成3-4种颜色
    /// </summary>
    public override void ConfigureFishSpawnManager(FishSpawnManager fishSpawnManager)
    {
        if (fishSpawnManager == null) return;
        
        fishSpawnManager.SetSpawnMode(difficultyIndex);
        Debug.Log($"[NormalDifficulty] 配置鱼生成：{colorCount}种颜色混合");
    }
    
    /// <summary>
    /// 配置任务管理器 - 普通模式要求颜色+数量
    /// </summary>
    public override void ConfigureTaskManager(TaskManager taskManager)
    {
        if (taskManager == null) return;
        
        // 可以在这里设置普通模式特有的任务参数
        Debug.Log($"[NormalDifficulty] 配置任务：颜色+数量 ({colorCount}种颜色，每色{minFishPerColorTask}-{maxFishPerColorTask}条)");
    }
    
    public override string GetDescription()
    {
        return $"普通模式 - {timeLimit}秒 - 顏色+數量認知 - {scoreMultiplier}x分數";
    }
}
