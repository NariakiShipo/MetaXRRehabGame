# 分數系統設置指南

## 概述
已為遊戲添加完整的分數系統，包括：
- ✅ 任務完成加分（可調整基礎分數）
- ✅ 難度倍率系統（簡單 1.0x、普通 1.5x、困難 2.0x）
- ✅ 子任務加分（高級模式）
- ✅ 時間獎勵（剩餘時間加分）
- ✅ 實時分數顯示（帶動畫效果）
- ✅ 遊戲結算界面（顯示最終得分和評級）

---

## 📁 新增文件

### 1. 核心邏輯
- `Assets/Scripts/Managers/ScoreManager.cs` - 分數管理器
- `Assets/Scripts/UI/ScoreDisplayUI.cs` - 分數顯示UI
- `Assets/Scripts/UI/GameResultUI.cs` - 遊戲結算UI

### 2. 修改的文件
- `Assets/Scripts/Managers/GameModeManager.cs` - 集成分數系統
- `Assets/Scripts/Managers/GameManager.cs` - 添加遊戲結束時的分數結算

---

## 🎮 Unity 場景設置步驟

### 步驟 1：添加 ScoreManager
1. 在 Hierarchy 中創建空物體，命名為 `ScoreManager`
2. 添加 `ScoreManager.cs` 腳本
3. 在 Inspector 中調整分數設置：
   - **Base Task Score**: 完成任務的基礎分數（默認：100）
   - **Easy Mode Multiplier**: 簡單模式倍率（默認：1.0）
   - **Normal Mode Multiplier**: 普通模式倍率（默認：1.5）
   - **Hard Mode Multiplier**: 困難模式倍率（默認：2.0）
   - **Sub Task Score**: 子任務分數（默認：50）
   - **Time Bonus**: 剩餘每秒獲得的分數（默認：1）

### 步驟 2：創建分數顯示 UI

#### 2.1 在 Canvas 中創建分數顯示
```
Canvas
└── ScoreDisplay (Panel)
    ├── ScoreText (TextMeshPro - Text)
    └── TasksCompletedText (TextMeshPro - Text) [可選]
```

#### 2.2 設置文本樣式
- **ScoreText**:
  - Text: "分數: 0"
  - Font Size: 36
  - Alignment: Center
  - Color: Yellow (#FFFF00)

- **TasksCompletedText**:
  - Text: "完成任務: 0"
  - Font Size: 24
  - Alignment: Center
  - Color: White

#### 2.3 添加 ScoreDisplayUI 腳本
1. 在 `ScoreDisplay` Panel 上添加 `ScoreDisplayUI.cs`
2. 在 Inspector 中連接引用：
   - **Score Text**: 拖入 ScoreText
   - **Tasks Completed Text**: 拖入 TasksCompletedText
   - **Score Manager**: 拖入 ScoreManager 物體（或留空自動查找）

3. 調整顯示設置：
   - **Score Format**: "分數: {0}"
   - **Tasks Format**: "完成任務: {0}"
   - **Enable Animation**: ✓（啟用數字跳動動畫）
   - **Animation Duration**: 0.5 秒

### 步驟 3：創建遊戲結算 UI

#### 3.1 創建結算面板
```
Canvas
└── GameResultPanel (Panel)
    ├── Background (Image) - 半透明黑色背景
    ├── ResultContainer (Vertical Layout Group)
    │   ├── TitleText: "遊戲結束"
    │   ├── FinalScoreText: "最終得分: 0"
    │   ├── CompletedTasksText: "完成任務: 0"
    │   ├── TimeBonusText: "時間獎勵: 0 分"
    │   ├── DifficultyText: "難度: 簡單 (x1.0)"
    │   ├── RankText: "評價: C"
    │   └── ButtonContainer (Horizontal Layout Group)
    │       ├── RestartButton: "重新開始"
    │       ├── MainMenuButton: "主菜單"
    │       └── QuitButton: "退出"
```

#### 3.2 設置結算 UI 樣式
- **Background**: 
  - Color: Black, Alpha: 180/255
  - 填滿整個螢幕

- **TitleText**:
  - Font Size: 48
  - Color: White
  - Alignment: Center

- **各項統計文本**:
  - Font Size: 28-32
  - Color: White
  - Alignment: Left

- **RankText**:
  - Font Size: 42
  - 顏色會根據評級自動改變

#### 3.3 添加 GameResultUI 腳本
1. 在 `GameResultPanel` 上添加 `GameResultUI.cs`
2. 在 Inspector 中連接所有引用：
   - **Result Panel**: 拖入 GameResultPanel 自己
   - **Final Score Text**: 拖入對應文本
   - **Completed Tasks Text**: 拖入對應文本
   - **Time Bonus Text**: 拖入對應文本
   - **Difficulty Text**: 拖入對應文本
   - **Rank Text**: 拖入對應文本
   - **Score Manager**: 拖入 ScoreManager（或留空自動查找）

3. 調整評級分數線：
   - **S Rank Threshold**: 100 分
   - **A Rank Threshold**: 75 分
   - **B Rank Threshold**: 50 分
   - **C Rank Threshold**: 25 分

4. 設置按鈕事件：
   - **RestartButton**: OnClick() → GameResultUI.OnRestartButtonPressed()
   - **MainMenuButton**: OnClick() → GameResultUI.OnMainMenuButtonPressed()
   - **QuitButton**: OnClick() → GameResultUI.OnQuitButtonPressed()

5. **重要**：確保 `GameResultPanel` 在開始時是**隱藏**的（取消勾選）

### 步驟 4：連接到 GameModeManager

1. 找到場景中的 `GameModeManager` 物體
2. 在 Inspector 中找到 **References** 區域
3. 拖入 `ScoreManager` 物體到 **Score Manager** 欄位

### 步驟 5：連接到 GameManager

1. 找到場景中的 `GameManager` 物體
2. 在 Inspector 中添加對 `ScoreManager` 的引用（或留空自動查找）

---

## 🎯 分數計算規則

### 基礎分數
- **完成任務**: `基礎分數 × 難度倍率`
  - 簡單模式：100 × 1.0 = 100 分
  - 普通模式：100 × 1.5 = 150 分
  - 困難模式：100 × 2.0 = 200 分

- **完成子任務**（高級模式）: `子任務分數 × 難度倍率`
  - 困難模式：50 × 2.0 = 100 分/子任務

### 時間獎勵
- 遊戲結束時計算：`剩餘秒數 × 時間獎勵`
- 例如：剩餘 30 秒，每秒 1 分 = 30 分

### 評級系統
- **S 級**: ≥ 1000 分（金色）
- **A 級**: ≥ 750 分（綠色）
- **B 級**: ≥ 500 分（藍色）
- **C 級**: ≥ 250 分（橙色）
- **D 級**: < 250 分（灰色）

---

## 🔧 自定義分數設置

### 調整任務完成分數
在 `ScoreManager` Inspector 中：
- **Base Task Score**: 改變任務完成的基礎分數
- 例如：設為 200 可獲得更高分數

### 調整難度倍率
在 `ScoreManager` Inspector 中：
- **Easy Mode Multiplier**: 簡單模式倍率
- **Normal Mode Multiplier**: 普通模式倍率
- **Hard Mode Multiplier**: 困難模式倍率
- 例如：設困難模式為 3.0x 可增加挑戰獎勵

### 調整時間獎勵
在 `ScoreManager` Inspector 中：
- **Time Bonus**: 每秒獲得的分數
- 例如：設為 2 可讓快速完成更有價值

### 調整評級分數線
在 `GameResultUI` Inspector 中：
- **S/A/B/C Rank Threshold**: 調整各等級所需分數

---

## 📊 事件系統

ScoreManager 提供以下事件供其他系統訂閱：

### OnScoreChanged
- **類型**: `UnityEvent<int>`
- **觸發時機**: 分數變化時
- **參數**: 當前總分
- **用途**: 更新 UI、觸發音效等

### OnGameEnd
- **類型**: `UnityEvent<GameResult>`
- **觸發時機**: 遊戲結束時
- **參數**: 遊戲結果數據
- **用途**: 顯示結算畫面、保存記錄等

---

## 🎨 UI 布局建議

### 分數顯示位置
推薦放在螢幕**右上角**：
- Anchor: Top-Right
- Position: (-50, -50) 從右上角偏移
- 與計時器一起顯示

### 結算面板位置
- Anchor: Stretch (填滿螢幕)
- 居中顯示
- 使用半透明背景遮罩遊戲畫面

---

## 🐛 故障排除

### 分數不顯示
1. 確認 `ScoreDisplayUI` 已添加到場景
2. 檢查是否正確連接 `scoreText` 引用
3. 查看 Console 是否有錯誤訊息

### 分數不增加
1. 確認 `GameModeManager` 已連接 `ScoreManager`
2. 檢查 `ScoreManager.OnScoreChanged` 事件是否有訂閱者
3. 查看 Console 日誌確認任務完成事件是否觸發

### 結算畫面不顯示
1. 確認 `GameResultPanel` 初始是隱藏的
2. 檢查 `GameResultUI` 是否連接到 `ScoreManager.OnGameEnd` 事件
3. 確認 `GameManager` 在時間結束時調用 `scoreManager.EndGame()`

### 動畫不流暢
1. 調整 `ScoreDisplayUI` 的 `Animation Duration`
2. 確認 `Enable Animation` 已勾選
3. 檢查遊戲幀率是否穩定

---

## 📝 擴展功能建議

### 1. 音效系統
訂閱 `OnScoreChanged` 事件播放加分音效：
```csharp
scoreManager.OnScoreChanged.AddListener((score) => {
    audioSource.PlayOneShot(scoreSound);
});
```

### 2. 連擊系統
快速完成多個任務時增加額外獎勵

### 3. 排行榜
保存最高分數並顯示

### 4. 成就系統
達成特定分數或條件解鎖成就

---

## ✅ 測試清單

- [ ] 分數在任務完成時正確增加
- [ ] 不同難度的倍率正確應用
- [ ] 子任務完成時加分（困難模式）
- [ ] 分數顯示實時更新
- [ ] 數字動畫流暢播放
- [ ] 時間結束時顯示結算畫面
- [ ] 結算畫面顯示所有統計信息
- [ ] 評級正確顯示並變色
- [ ] 時間獎勵正確計算
- [ ] 重新開始按鈕功能正常
- [ ] 在不同難度下測試完整流程

---

## 🎓 使用範例

### 在自定義腳本中添加分數
```csharp
public class MyCustomScript : MonoBehaviour
{
    private ScoreManager scoreManager;
    
    void Start()
    {
        scoreManager = FindObjectOfType<ScoreManager>();
    }
    
    void OnSpecialEvent()
    {
        // 添加自定義分數
        scoreManager.AddCustomScore(50);
    }
}
```

---

完成以上設置後，您的遊戲就擁有完整的分數系統了！🎉
