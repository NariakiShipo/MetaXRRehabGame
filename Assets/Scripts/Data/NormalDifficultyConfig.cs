using UnityEngine;

/// <summary>
/// 普通難度配置 - 要求顏色+數量認知
/// </summary>
[System.Serializable]
public class NormalDifficultyConfig : DifficultyConfig
{
    [Header("普通模式特殊設定")]
    [Tooltip("產生的顏色數量")]
    public int colorCount = 3;
    
    [Tooltip("每個顏色的任務魚數量範圍")]
    public int minFishPerColorTask = 1;
    public int maxFishPerColorTask = 3;
    
    public NormalDifficultyConfig()
    {
        SetDifficultyName("普通");
        difficultyIndex = 1;
        taskType = TaskType.ColorCount;
        timeLimit = 300f;  // 5分鐘
        scoreMultiplier = 1.5f;
        minFishPerColor = 5;
    }
    
    /// <summary>
    /// 配置魚生成管理器 - 普通模式產生3-4種顏色
    /// </summary>
    public override void ConfigureFishSpawnManager(FishSpawnManager fishSpawnManager)
    {
        if (fishSpawnManager == null) return;
        
        fishSpawnManager.SetSpawnMode(difficultyIndex);
        Debug.Log($"[NormalDifficulty] 配置魚生成：{colorCount}種顏色混合");
    }
    
    /// <summary>
    /// 配置任務管理器 - 普通模式要求顏色+數量
    /// </summary>
    public override void ConfigureTaskManager(TaskManager taskManager)
    {
        if (taskManager == null) return;
        
        // 可以在這裡設定普通模式特有的任務參數
        Debug.Log($"[NormalDifficulty] 配置任務：顏色+數量 ({colorCount}種顏色，每色{minFishPerColorTask}-{maxFishPerColorTask}條)");
    }
    
    public override string GetDescription()
    {
        return $"普通模式 - {timeLimit}秒 - 顏色+數量認知 - {scoreMultiplier}x分數";
    }
}
