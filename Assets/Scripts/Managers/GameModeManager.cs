using UnityEngine;
using UnityEngine.Events;

public class GameModeManager : MonoBehaviour
{
    [Header("Game Mode Settings")]
    [SerializeField] private float easyModeTime = 180f;    // 3 分鐘
    [SerializeField] private float normalModeTime = 300f;  // 5 分鐘
    [SerializeField] private float hardModeTime = 600f;    // 10 分鐘
    
    [Header("References")]
    [Tooltip("GameManager 腳本引用")]
    [SerializeField] private GameManager gameManager;
    
    [Tooltip("FishSpawnManager 腳本引用")]
    [SerializeField] private FishSpawnManager fishSpawnManager;
    
    [Header("UI References")]
    [Tooltip("難度選擇按鈕的父物體（選擇後會隱藏）")]
    [SerializeField] private GameObject[] difficultySelectionUI;
    
    [Header("Events")]
    [Tooltip("遊戲開始時觸發")]
    public UnityEvent onGameStart;
    
    private bool isGameStarted = false;
    private string selectedDifficulty = "";
    
    void Start()
    {
        // 遊戲開始前先暫停其他系統
        InitializeGameSystems(false);
        
        Debug.Log("[GameModeManager] 等待玩家選擇難度...");
    }
    
    /// <summary>
    /// Easy 按鈕按下時調用（180 秒 / 3 分鐘）
    /// </summary>
    public void OnEasyButtonPressed()
    {
        StartGameWithDifficulty(0, "Easy", easyModeTime);
    }
    
    /// <summary>
    /// Normal 按鈕按下時調用（300 秒 / 5 分鐘）
    /// </summary>
    public void OnNormalButtonPressed()
    {
        StartGameWithDifficulty(1, "Normal", normalModeTime);
    }
    
    /// <summary>
    /// Hard 按鈕按下時調用（600 秒 / 10 分鐘）
    /// </summary>
    public void OnHardButtonPressed()
    {
        StartGameWithDifficulty(2, "Hard", hardModeTime);
    }
    
    /// <summary>
    /// 開始遊戲並設置難度
    /// </summary>
    private void StartGameWithDifficulty(int difficultyIndex, string difficultyName, float timeLimit)
    {
        // 防止重複啟動
        if (isGameStarted)
        {
            Debug.LogWarning("[GameModeManager] 遊戲已經開始，無法重複選擇難度");
            return;
        }
        
        selectedDifficulty = difficultyName;
        isGameStarted = true;
        
        Debug.Log($"[GameModeManager] 選擇難度：{difficultyName}，時間限制：{timeLimit} 秒");
        
        // 設置 GameManager 的倒數計時
        if (gameManager != null)
        {
            gameManager.SetTime(difficultyIndex);
            Debug.Log($"[GameModeManager] 已設置計時器：{timeLimit} 秒");
        }
        else
        {
            Debug.LogError("[GameModeManager] GameManager 引用為空！請在 Inspector 中設置");
        }
        
        // 啟動其他遊戲系統
        InitializeGameSystems(true);
        
        // 隱藏難度選擇 UI
        if (difficultySelectionUI != null)
        {
            foreach (var ui in difficultySelectionUI)
                ui.SetActive(false);
            Debug.Log("[GameModeManager] 已隱藏難度選擇 UI");
        }
        
        // 觸發遊戲開始事件
        onGameStart?.Invoke();
        
        Debug.Log($"[GameModeManager] 遊戲開始！難度：{difficultyName}");
    }
    
    /// <summary>
    /// 初始化或啟動遊戲系統
    /// </summary>
    private void InitializeGameSystems(bool enable)
    {
        // 控制 GameManager
        if (gameManager != null)
        {
            gameManager.enabled = enable;
            Debug.Log($"[GameModeManager] GameManager {(enable ? "已啟動" : "已暫停")}");
        }
        
        // 控制 FishSpawnManager
        if (fishSpawnManager != null)
        {
            fishSpawnManager.enabled = enable;
            Debug.Log($"[GameModeManager] FishSpawnManager {(enable ? "已啟動" : "已暫停")}");
        }
        
        // 可以在這裡添加更多需要控制的系統
    }
    
    /// <summary>
    /// 重新開始遊戲（用於重玩）
    /// </summary>
    public void RestartGame()
    {
        isGameStarted = false;
        selectedDifficulty = "";
        
        // 重新顯示難度選擇 UI
        if (difficultySelectionUI != null)
        {
            foreach (var ui in difficultySelectionUI)
                ui.SetActive(true);
        }
        
        // 暫停遊戲系統
        InitializeGameSystems(false);
        
        // 清除所有魚
        if (fishSpawnManager != null)
        {
            fishSpawnManager.ClearAllFish();
        }
        
        Debug.Log("[GameModeManager] 遊戲已重置，等待重新選擇難度");
    }
    
    /// <summary>
    /// 獲取當前選擇的難度
    /// </summary>
    public string GetSelectedDifficulty()
    {
        return selectedDifficulty;
    }
    
    /// <summary>
    /// 檢查遊戲是否已開始
    /// </summary>
    public bool IsGameStarted()
    {
        return isGameStarted;
    }
}
