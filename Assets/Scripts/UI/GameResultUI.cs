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
        // 自动查找ScoreManager
        if (scoreManager == null)
        {
            scoreManager = Object.FindFirstObjectByType<ScoreManager>();
        }
        
        // 初始隐藏结算面板
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
    }
    
    void Start()
    {
        // 订阅游戏结束事件
        if (scoreManager != null)
        {
            scoreManager.OnGameEnd.AddListener(ShowGameResult);
            Debug.Log("[GameResultUI] 已订阅游戏结束事件");
        }
        else
        {
            Debug.LogWarning("[GameResultUI] ScoreManager 未找到！");
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
        UpdateTimeBonus(result.timeBonusScore, result.remainingTime);
        UpdateDifficulty(result.difficultyMultiplier);
        UpdateRank(result.finalScore);
        
        // 暂停游戏
        Time.timeScale = 0f;
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
    private void UpdateTimeBonus(int bonus, float remainingTime)
    {
        if (timeBonusText != null)
        {
            int minutes = Mathf.FloorToInt(remainingTime / 60f);
            int seconds = Mathf.FloorToInt(remainingTime % 60f);
            timeBonusText.text = $"時間獎勵: {bonus} 分 (剩餘 {minutes:00}:{seconds:00})";
        }
    }
    
    /// <summary>
    /// 更新难度信息
    /// </summary>
    private void UpdateDifficulty(float multiplier)
    {
        if (difficultyText != null)
        {
            string difficultyName = GetDifficultyName(multiplier);
            difficultyText.text = $"難度: {difficultyName} (x{multiplier:F1})";
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
    /// 获取难度名称
    /// </summary>
    private string GetDifficultyName(float multiplier)
    {
        if (Mathf.Approximately(multiplier, 1.0f)) return "簡單";
        if (Mathf.Approximately(multiplier, 1.5f)) return "普通";
        if (Mathf.Approximately(multiplier, 2.0f)) return "困難";
        return "未知";
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
