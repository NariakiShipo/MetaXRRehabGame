using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject scoopNet;
    [SerializeField] TMP_Text timerText;
    [SerializeField] private ScoreManager scoreManager;
    

    private float timer = 0f;
    private bool isEnd = false;
    public int score = 0;
    
    void Start()
    {
        scoopNet.SetActive(true);
        
        // 自動查找 ScoreManager
        if (scoreManager == null)
        {
            scoreManager = Object.FindFirstObjectByType<ScoreManager>();
        }
    }
    
    void Update()
    {
        if (timer >= 0f && !isEnd)
        {
            timer -= Time.deltaTime;
            float minutes = Mathf.Floor(timer / 60f);
            float seconds = Mathf.Floor(timer % 60f);
            timerText.text = "Time: " + minutes.ToString("00") + ":" + seconds.ToString("00");
        }
        else if (!isEnd)
        {
            isEnd = true;
            Debug.LogWarning("Time's up!");
            
            // 觸發遊戲結束並計算分數
            if (scoreManager != null)
            {
                scoreManager.EndGame(0f); // 時間用完，沒有剩餘時間獎勵
            }
            else
            {
                // 如果沒有 ScoreManager，保持原有行為
                Time.timeScale = 0f;
            }
        }
    }

    public void SetTime(int index, float timeLimit)
    {
        timer = timeLimit;
        isEnd = false; // 重置結束標記
    }
    
    /// <summary>
    /// 獲取剩餘時間
    /// </summary>
    public float GetRemainingTime()
    {
        return Mathf.Max(0f, timer);
    }
    
    /// <summary>
    /// 手動結束遊戲（例如提前完成所有任務）
    /// </summary>
    public void EndGameEarly()
    {
        if (!isEnd && scoreManager != null)
        {
            isEnd = true;
            float remainingTime = GetRemainingTime();
            Debug.Log($"[GameManager] 提前結束遊戲！剩餘時間：{remainingTime:F1} 秒");
            scoreManager.EndGame(remainingTime);
        }
    }
}
