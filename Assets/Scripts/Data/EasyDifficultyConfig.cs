using UnityEngine;

/// <summary>
/// 簡單難度配置 - 只要求數量認知，不限顏色
/// </summary>
[System.Serializable]
public class EasyDifficultyConfig : DifficultyConfig
{
    [Header("簡單模式特殊設定")]
    [Tooltip("是否只生成單一顏色的魚")]
    public bool useSingleColor = true;
    
    [Tooltip("任務魚數量範圍")]
    public int minFishCount = 1;
    public int maxFishCount = 3;
    
    public EasyDifficultyConfig()
    {
        SetDifficultyName("簡單");
        difficultyIndex = 0;
        taskType = TaskType.CountOnly;
        timeLimit = 180f;  // 3分鐘
        scoreMultiplier = 1.0f;
        minFishPerColor = 5;
    }
    
    /// <summary>
    /// 配置魚生成管理器 - 簡單模式只產生一種顏色
    /// </summary>
    public override void ConfigureFishSpawnManager(FishSpawnManager fishSpawnManager)
    {
        if (fishSpawnManager == null) return;
        
        fishSpawnManager.SetSpawnMode(difficultyIndex);
        Debug.Log($"[EasyDifficulty] 配置魚生成：單一顏色模式");
    }
    
    /// <summary>
    /// 配置任務管理器 - 簡單模式只要求數量
    /// </summary>
    public override void ConfigureTaskManager(TaskManager taskManager)
    {
        if (taskManager == null) return;
        
        // 可以在這裡設定簡單模式特有的任務參數
        Debug.Log($"[EasyDifficulty] 設定任務：數量認知 ({minFishCount}-{maxFishCount}條)");
    }
    
    public override string GetDescription()
    {
        return $"簡單模式 - {timeLimit}秒 - 數量認知 - {scoreMultiplier}x分數";
    }
}
