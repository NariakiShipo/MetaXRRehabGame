using UnityEngine;

/// <summary>
/// 困難難度配置 - 多階段任務，要求執行順序
/// </summary>
[System.Serializable]
public class HardDifficultyConfig : DifficultyConfig
{
    [Header("困難模式特殊設定")]
    [Tooltip("生成的顏色數量")]
    public int colorCount = 4;
    
    [Tooltip("多階段任務數量")]
    public int subTaskCount = 3;
    
    [Tooltip("每個子任務的魚數量範圍")]
    public int minFishPerSubTask = 1;
    public int maxFishPerSubTask = 2;
    
    public HardDifficultyConfig()
    {
        SetDifficultyName("困難");
        difficultyIndex = 2;
        taskType = TaskType.MultiStage;
        timeLimit = 600f;  // 10分鐘
        scoreMultiplier = 2.0f;
        minFishPerColor = 5;
    }
    
    /// <summary>
    /// 配置魚生成管理器 - 困難模式產生3-4種顏色
    /// </summary>
    public override void ConfigureFishSpawnManager(FishSpawnManager fishSpawnManager)
    {
        if (fishSpawnManager == null) return;
        
        fishSpawnManager.SetSpawnMode(difficultyIndex);
        Debug.Log($"[HardDifficulty] 配置魚生成：{colorCount}種顏色混合（多階段）");
    }
    
    /// <summary>
    /// 配置任務管理器 - 困難模式要求多階段+順序
    /// </summary>
    public override void ConfigureTaskManager(TaskManager taskManager)
    {
        if (taskManager == null) return;
        
        // 可以在這裡設定困難模式特有的任務參數
        Debug.Log($"[HardDifficulty] 設定任務：多階段 ({subTaskCount}個子任務，每個{minFishPerSubTask}-{maxFishPerSubTask}條)");
    }
    
    public override string GetDescription()
    {
        return $"困難模式 - {timeLimit}秒 - 多階段任務 - {scoreMultiplier}x分數";
    }
}
