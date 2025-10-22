# Fish.cs 重構說明文件

## 📋 重構概述

重構後的 `Fish.cs` 系統提供完整的魚類生成與捕獲追蹤功能，包含以下檔案：

### 修改的檔案
1. **Fish.cs** - 核心資料類別（重構）
2. **Generator.cs** - 魚類生成器（更新）
3. **BucketEvent.cs** - 桶子事件處理（重構）

### 新增的檔案
4. **FishStatisticsManager.cs** - 統計管理器（新增，可選）

---

## 🎯 Fish.cs 核心功能

### 屬性

| 屬性 | 類型 | 說明 |
|------|------|------|
| `color` | string | 魚的顏色（Tag 名稱） |
| `spawnedAmount` | int | 已生成的魚數量 |
| `caughtAmount` | int | 已捕獲的魚數量 |
| `targetAmount` | int | 目標捕獲數量（可選） |
| `order` | int | 順序編號 |

### 主要方法

#### ✅ 增加/減少捕獲數量
```csharp
// 增加捕獲數量
bool IncrementCaught(int amount = 1)

// 減少捕獲數量（魚離開桶子）
void DecrementCaught(int amount = 1)
```

#### 📊 進度查詢
```csharp
// 取得捕獲進度（0.0 ~ 1.0）
float GetProgress()

// 取得目標進度（0.0 ~ 1.0）
float GetTargetProgress()

// 取得剩餘未捕獲數量
int GetRemainingAmount()
```

#### ✔️ 狀態檢查
```csharp
// 檢查是否達成目標
bool IsTargetComplete()

// 檢查是否全部捕獲
bool IsAllCaught()
```

---

## 🔧 Generator.cs 更新

### 新增功能

```csharp
// 根據顏色取得特定 Fish 物件
Fish GetFishByColor(string color)

// 取得總生成數量
int GetTotalSpawnedCount()

// 取得總捕獲數量
int GetTotalCaughtCount()
```

### 改進點
- 生成數量改為 1-3 隻（避免生成 0 隻）
- 添加生成日誌輸出
- 使用正確的參數名稱 `spawnedAmount`

---

## 🪣 BucketEvent.cs 重構

### 核心改進

1. **實際追蹤捕獲數量**
   - 魚進入桶子時：`fish.IncrementCaught()`
   - 魚離開桶子時：`fish.DecrementCaught()`

2. **詳細統計資訊**
   - 每種顏色魚的捕獲數量
   - 桶內當前魚數量
   - 整體捕獲進度

3. **改進的 UI 顯示**
   - 基本顯示：桶內魚數
   - 詳細統計：各顏色魚的進度

### 新增方法

```csharp
// 檢查是否全部捕獲完成
bool IsAllFishCaught()

// 取得整體捕獲進度
float GetOverallProgress()
```

---

## 🎮 使用範例

### 在 Unity Editor 中設置

1. **Generator 物件**
   - 添加 `Generator.cs` 腳本
   - 設置 `fishPrefab` 陣列（紅、藍、綠魚的 Prefab）
   - 設置 `boxCollider`（生成範圍）

2. **Bucket 物件**
   - 添加 `BucketEvent.cs` 腳本
   - 設置 `bucketText`（顯示桶內魚數）
   - 設置 `statisticsText`（顯示詳細統計，可選）
   - 引用 `generator` 物件

3. **統計管理器（可選）**
   - 創建空 GameObject
   - 添加 `FishStatisticsManager.cs`
   - 引用 Generator 和 BucketEvent
   - 設置各個統計文字元件

### 程式碼範例

```csharp
// 取得特定顏色的魚資訊
Fish redFish = generator.GetFishByColor("redFish");
Debug.Log($"紅魚進度: {redFish.GetProgress():P0}");

// 檢查是否完成
if (bucketEvent.IsAllFishCaught())
{
    Debug.Log("所有魚都已捕獲！");
}

// 取得整體進度
float progress = bucketEvent.GetOverallProgress();
Debug.Log($"整體進度: {progress:P0}");
```

---

## 📊 輸出範例

### Console 輸出
```
[Generator] 生成 redFish: 2 隻
[Generator] 生成 blueFish: 3 隻
[Generator] 生成 greenFish: 1 隻
[Generator] 總共生成 6 隻魚

[BucketEvent] redFish 進入桶子
==================== 魚類統計 ====================
[redFish] 生成: 2 | 捕獲: 1 | 剩餘: 1 | 進度: 50%
[blueFish] 生成: 3 | 捕獲: 0 | 剩餘: 3 | 進度: 0%
[greenFish] 生成: 1 | 捕獲: 0 | 剩餘: 1 | 進度: 0%
桶內魚數: 紅魚 1 | 藍魚 0 | 綠魚 0
總捕獲進度: 1/6
=================================================
```

### UI 顯示
```
桶內魚數: 1

=== 捕獲統計 ===
紅魚: 1/2 (50%)
藍魚: 0/3 (0%)
綠魚: 0/1 (0%)

總計: 1/6
```

---

## 🚀 進階應用

### 設置目標捕獲數量

如果想要設定每種魚的目標數量（例如任務系統）：

```csharp
// 在 Generator.cs 中修改
fish.Add(new Fish(fishname[i], spawnCount, i + 1, targetAmount: 5));
```

然後可以使用：
```csharp
if (redFish.IsTargetComplete())
{
    Debug.Log("紅魚任務完成！");
}
```

### 遊戲結束檢測

```csharp
void Update()
{
    if (bucketEvent.IsAllFishCaught() && !gameEnded)
    {
        gameEnded = true;
        OnGameComplete();
    }
}

void OnGameComplete()
{
    Debug.Log("遊戲完成！");
    Time.timeScale = 0; // 暫停遊戲
    // 顯示完成畫面...
}
```

---

## ⚠️ 注意事項

1. **Tag 設置**：確保魚 GameObject 有正確的 Tag（redFish, blueFish, greenFish）

2. **生成順序**：Generator 的 `fishPrefab` 陣列順序要與 `fishname` 陣列對應

3. **效能考量**：如果魚數量很多，考慮降低 `FishStatisticsManager` 的 `updateInterval`

4. **資料一致性**：`caughtAmount` 會自動檢查不會超過 `spawnedAmount`

---

## 🔍 除錯功能

### 在 Inspector 中查看統計
- 在 `FishStatisticsManager` 組件上右鍵選擇 `Show Full Report`

### Console 輸出
- 每次魚進出桶子都會輸出詳細統計
- 可以在 `BucketEvent` 中調整輸出頻率

---

## 📝 版本歷史

### v2.0 (2025-10-20) - 重構版本
- ✅ 重新命名 `caughtAmount` → `spawnedAmount`
- ✅ 新增實際的 `caughtAmount` 追蹤
- ✅ 新增進度計算方法
- ✅ 新增目標系統
- ✅ 改進 UI 顯示
- ✅ 新增統計管理器

### v1.0 - 原始版本
- 基本的 Fish 資料類別
- 只記錄生成數量

---

## 🤝 貢獻

如有問題或建議，請聯繫開發團隊。
