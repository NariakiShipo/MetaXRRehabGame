using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// 游戏结算UI - 显示最终得分和统计信息
/// </summary>
public class GameResultUI : MonoBehaviour
{
    [Header("UI引用")]
    [Tooltip("结算面板GameObject")]
    [SerializeField] private GameObject resultPanel;
    
    [Tooltip("最终得分文本")]
    [SerializeField] private TextMeshProUGUI finalScoreText;
    
    [Tooltip("完成任务数文本")]
    [SerializeField] private TextMeshProUGUI completedTasksText;
    
    [Tooltip("时间奖励文本")]
    [SerializeField] private TextMeshProUGUI timeBonusText;
    
    [Tooltip("难度倍率文本")]
    [SerializeField] private TextMeshProUGUI difficultyText;
    
    [Tooltip("评价文本")]
    [SerializeField] private TextMeshProUGUI rankText;
    
    [Header("引用")]
    [Tooltip("ScoreManager引用")]
    [SerializeField] private ScoreManager scoreManager;
    
    [SerializeField]DifficultyManager difficultyManager;
    //DifficultyConfig currentDifficulty;

    [Header("评价设置")]
    [Tooltip("S级评价分数线")]
    [SerializeField] private int sRankThreshold = 1000;
    
    [Tooltip("A级评价分数线")]
    [SerializeField] private int aRankThreshold = 750;
    
    [Tooltip("B级评价分数线")]
    [SerializeField] private int bRankThreshold = 500;
    
    [Tooltip("C级评价分数线")]
    [SerializeField] private int cRankThreshold = 250;

    
    void Awake()
    {
        // 初始隐藏结算面板
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
    }
    
    void Start()
    {
        // 獲取依賴
        TryGetDependencies();
        
        // 订阅游戏结束事件
        SubscribeToEvents();
    }
    
    /// <summary>
    /// 嘗試獲取依賴
    /// </summary>
    private void TryGetDependencies()
    {
        // 獲取 ScoreManager - 使用 TryGet 避免錯誤日誌
        if (scoreManager == null)
        {
            if (!ServiceLocator.Instance.TryGet(out scoreManager))
            {
                scoreManager = FindFirstObjectByType<ScoreManager>();
                if (scoreManager != null)
                {
                    Debug.Log("[GameResultUI] 從場景中找到 ScoreManager");
                }
            }
        }
        
        // 獲取 DifficultyManager - 使用 TryGet 避免錯誤日誌
        if (difficultyManager == null)
        {
            if (!ServiceLocator.Instance.TryGet(out difficultyManager))
            {
                // 嘗試使用單例
                difficultyManager = DifficultyManager.Instance;
                
                // 如果單例也為空，嘗試在場景中查找
                if (difficultyManager == null)
                {
                    difficultyManager = FindFirstObjectByType<DifficultyManager>();
                }
                
                if (difficultyManager != null)
                {
                    Debug.Log("[GameResultUI] 從場景中找到 DifficultyManager");
                }
            }
        }
    }
    
    /// <summary>
    /// 訂閱事件
    /// </summary>
    private void SubscribeToEvents()
    {
        if (scoreManager != null)
        {
            scoreManager.OnGameEnd.AddListener(ShowGameResult);
            Debug.Log("[GameResultUI] 已订阅游戏结束事件");
        }
        else
        {
            Debug.LogWarning("[GameResultUI] ScoreManager 未找到！請確保場景中有 ScoreManager");
        }
    }
    
    void OnDestroy()
    {
        // 取消订阅
        if (scoreManager != null)
        {
            scoreManager.OnGameEnd.RemoveListener(ShowGameResult);
        }
    }
    
    /// <summary>
    /// 显示游戏结算
    /// </summary>
    public void ShowGameResult(GameResult result)
    {
        Debug.Log($"[GameResultUI] 显示游戏结算 - 最终得分: {result.finalScore}");
        
        // 显示结算面板
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }
        
        // 更新各项统计
        UpdateFinalScore(result.finalScore);
        UpdateCompletedTasks(result.completedTasks);
        UpdateTimeBonus(result.totalTimeSpent);
        UpdateDifficulty(result.difficultyMultiplier);
        UpdateRank(result.finalScore);
        
    }
    
    /// <summary>
    /// 更新最终得分
    /// </summary>
    private void UpdateFinalScore(int score)
    {
        if (finalScoreText != null)
        {
            finalScoreText.text = $"最終得分: {score}";
        }
    }
    
    /// <summary>
    /// 更新完成任务数
    /// </summary>
    private void UpdateCompletedTasks(int tasks)
    {
        if (completedTasksText != null)
        {
            completedTasksText.text = $"完成任務: {tasks}";
        }
    }
    
    /// <summary>
    /// 更新时间奖励
    /// </summary>
        private void UpdateTimeBonus(float totalTime)
    {
        if (timeBonusText != null)
        {
                int minutes = Mathf.FloorToInt(totalTime / 60f);
                int seconds = Mathf.FloorToInt(totalTime % 60f);
                timeBonusText.text = $"本次遊玩時間: {minutes:00}:{seconds:00}";
        }
    }
    
    /// <summary>
    /// 更新难度信息
    /// </summary>
    private void UpdateDifficulty(float multiplier)
    {
        if (difficultyText != null)
        {
            if (difficultyManager != null)
            {
                var currentDifficulty = difficultyManager.GetCurrentDifficulty();
                if (currentDifficulty != null)
                {
                    string difficultyName = currentDifficulty.GetDifficultyName();
                    difficultyText.text = $"難度: {difficultyName}";
                }
                else
                {
                    // 如果難度配置為空，使用倍率顯示
                    difficultyText.text = $"難度倍率: x{multiplier:F1}";
                    Debug.LogWarning("[GameResultUI] GetCurrentDifficulty() 返回 null，使用倍率顯示");
                }
            }
            else
            {
                // 如果沒有 DifficultyManager，使用倍率顯示
                difficultyText.text = $"難度倍率: x{multiplier:F1}";
                Debug.LogWarning("[GameResultUI] DifficultyManager 為空，使用倍率顯示");
            }
        }
    }
    
    /// <summary>
    /// 更新评价等级
    /// </summary>
    private void UpdateRank(int score)
    {
        if (rankText != null)
        {
            string rank = GetRank(score);
            string color = GetRankColor(rank);
            rankText.text = $"<color={color}>評價: {rank}</color>";
        }
    }
    
    /// <summary>
    /// 获取评价等级
    /// </summary>
    private string GetRank(int score)
    {
        if (score >= sRankThreshold) return "S";
        if (score >= aRankThreshold) return "A";
        if (score >= bRankThreshold) return "B";
        if (score >= cRankThreshold) return "C";
        return "D";
    }
    
    /// <summary>
    /// 获取评价等级颜色
    /// </summary>
    private string GetRankColor(string rank)
    {
        switch (rank)
        {
            case "S": return "#FFD700"; // 金色
            case "A": return "#00FF00"; // 绿色
            case "B": return "#00BFFF"; // 蓝色
            case "C": return "#FFA500"; // 橙色
            case "D": return "#808080"; // 灰色
            default: return "#FFFFFF";  // 白色
        }
    }
    
    /// <summary>
    /// 重新开始游戏按钮
    /// </summary>
    public void OnRestartButtonPressed()
    {
        Debug.Log("[GameResultUI] 重新开始游戏");
        
        // 恢复时间流速
        Time.timeScale = 1f;
        
        // 重新加载场景
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    /// <summary>
    /// 返回主菜单按钮
    /// </summary>
    public void OnMainMenuButtonPressed()
    {
        Debug.Log("[GameResultUI] 返回主菜单");
        
        // 恢复时间流速
        Time.timeScale = 1f;
        
        // 隐藏结算面板
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
        
        // 这里可以添加返回主菜单的逻辑
        // 例如：SceneManager.LoadScene("MainMenu");
    }
    
    /// <summary>
    /// 退出游戏按钮
    /// </summary>
    public void OnQuitButtonPressed()
    {
        Debug.Log("[GameResultUI] 退出游戏");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
