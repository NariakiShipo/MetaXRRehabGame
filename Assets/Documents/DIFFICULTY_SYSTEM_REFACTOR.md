# 難度系統重構說明

## 概述
將原本分散在各個管理器中的難度相關代碼重構為類結構，使用面向對象的方式管理三種難度模式。

## 新增文件

### 1. 核心配置文件
- `Assets/Scripts/Data/DifficultyConfig.cs` - 抽象基類
- `Assets/Scripts/Data/EasyDifficultyConfig.cs` - 簡單模式配置
- `Assets/Scripts/Data/NormalDifficultyConfig.cs` - 普通模式配置
- `Assets/Scripts/Data/HardDifficultyConfig.cs` - 困難模式配置

### 2. 管理器
- `Assets/Scripts/Manager/DifficultyManager.cs` - 中心控制器

## 架構設計

```
DifficultyConfig (抽象基類)
├── EasyDifficultyConfig (簡單模式)
│   ├── TaskType.CountOnly
│   ├── 180秒時間限制
│   └── 1.0x分數倍率
├── NormalDifficultyConfig (普通模式)
│   ├── TaskType.ColorCount
│   ├── 300秒時間限制
│   └── 1.5x分數倍率
└── HardDifficultyConfig (困難模式)
    ├── TaskType.MultiStage
    ├── 600秒時間限制
    └── 2.0x分數倍率

DifficultyManager (中心控制器)
├── 管理所有難度配置
├── 協調 FishSpawnManager
├── 協調 TaskManager
└── 協調 ScoreManager
```

## 使用方法

### 在 Unity Editor 中設置

1. **創建 DifficultyManager GameObject**
   - 在場景中創建空物體，命名為 "DifficultyManager"
   - 添加 `DifficultyManager` 組件

2. **設置引用**
   - 在 `DifficultyManager` Inspector 中設置：
     - `FishSpawnManager` - 拖入魚生成管理器
     - `TaskManager` - 拖入任務管理器
     - `ScoreManager` - 拖入分數管理器

3. **在 GameModeManager 中設置**
   - 在 `GameModeManager` Inspector 中設置：
     - `DifficultyManager` - 拖入難度管理器

### 程式碼調用範例

```csharp
// 獲取 DifficultyManager 實例
DifficultyManager difficultyManager = DifficultyManager.Instance;

// 設置簡單難度
difficultyManager.SetEasyDifficulty();

// 設置普通難度
difficultyManager.SetNormalDifficulty();

// 設置困難難度
difficultyManager.SetHardDifficulty();

// 根據索引設置難度
difficultyManager.SetDifficultyByIndex(0); // 0=簡單, 1=普通, 2=困難

// 獲取當前難度信息
TaskType currentTaskType = difficultyManager.GetCurrentTaskType();
float currentTimeLimit = difficultyManager.GetCurrentTimeLimit();
float currentMultiplier = difficultyManager.GetCurrentScoreMultiplier();
int currentDifficultyIndex = difficultyManager.GetCurrentDifficultyIndex();

// 訂閱難度改變事件
difficultyManager.OnDifficultyChanged += (config) => {
    Debug.Log($"難度已改變為：{config.GetDifficultyName()}");
};
```

## 各難度配置詳細

### 簡單模式 (EasyDifficultyConfig)
- **任務類型**: CountOnly - 只要求數量認知
- **時間限制**: 180秒 (3分鐘)
- **分數倍率**: 1.0x
- **魚生成**: 只生成單一顏色的魚
- **任務範圍**: 1-3條魚

### 普通模式 (NormalDifficultyConfig)
- **任務類型**: ColorCount - 要求顏色+數量認知
- **時間限制**: 300秒 (5分鐘)
- **分數倍率**: 1.5x
- **魚生成**: 3-4種顏色混合
- **任務範圍**: 每種顏色1-3條

### 困難模式 (HardDifficultyConfig)
- **任務類型**: MultiStage - 多階段任務
- **時間限制**: 600秒 (10分鐘)
- **分數倍率**: 2.0x
- **魚生成**: 3-4種顏色混合
- **任務範圍**: 3個子任務，每個1-2條魚

## 改動的現有文件

### GameModeManager.cs
**移除的成員變量**:
- `easyModeTime`
- `normalModeTime`
- `hardModeTime`
- `currentTaskType`

**新增的引用**:
- `DifficultyManager difficultyManager`

**修改的方法**:
- `OnEasyButtonPressed()` - 現在調用 `difficultyManager.SetEasyDifficulty()`
- `OnNormalButtonPressed()` - 現在調用 `difficultyManager.SetNormalDifficulty()`
- `OnHardButtonPressed()` - 現在調用 `difficultyManager.SetHardDifficulty()`
- `StartGameWithDifficulty()` - 不再直接設置 ScoreManager，由 DifficultyManager 管理
- `GenerateNewTask()` - 從 DifficultyManager 獲取 TaskType
- `RegenerateFish()` - 不再調用 `SetSpawnMode()`，由 DifficultyManager 配置
- `GetDifficultyIndex()` - 直接從 DifficultyManager 獲取

## 優點

1. **代碼組織更清晰**
   - 難度相關邏輯集中在各自的配置類中
   - 易於維護和擴展

2. **擴展性更好**
   - 新增難度只需繼承 `DifficultyConfig` 並實現抽象方法
   - 不需要修改多個管理器的 switch 語句

3. **降低耦合**
   - 各管理器不需要知道具體難度邏輯
   - 統一通過 DifficultyManager 協調

4. **易於測試**
   - 每個難度配置可以獨立測試
   - 中心控制器職責單一

## 未來擴展

如需新增難度，只需：

1. 創建新的配置類繼承 `DifficultyConfig`
```csharp
public class ExpertDifficultyConfig : DifficultyConfig
{
    public ExpertDifficultyConfig()
    {
        difficultyName = "專家";
        difficultyIndex = 3;
        taskType = TaskType.Custom; // 新任務類型
        timeLimit = 480f;
        scoreMultiplier = 2.5f;
    }
    
    public override void ConfigureFishSpawnManager(FishSpawnManager fishSpawnManager)
    {
        // 實現專家模式的魚生成邏輯
    }
    
    public override void ConfigureTaskManager(TaskManager taskManager)
    {
        // 實現專家模式的任務生成邏輯
    }
}
```

2. 在 `DifficultyManager` 中添加對應的設置方法
```csharp
[SerializeField] private ExpertDifficultyConfig expertConfig;

public void SetExpertDifficulty()
{
    SetDifficulty(expertConfig);
}
```

3. 在 `GameModeManager` 中添加按鈕響應
```csharp
public void OnExpertButtonPressed()
{
    if (difficultyManager != null)
    {
        difficultyManager.SetExpertDifficulty();
        StartGameWithDifficulty(3, "Expert", difficultyManager.GetCurrentTimeLimit());
    }
}
```

## 注意事項

- 確保在場景中只有一個 `DifficultyManager` 實例（使用 Singleton 模式）
- 遊戲開始前必須先調用難度設置方法
- 各管理器的引用必須正確設置，否則配置會失效
