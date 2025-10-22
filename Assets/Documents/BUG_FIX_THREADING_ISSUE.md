# Unity 多執行緒錯誤修復報告

## 🐛 錯誤描述

```
UnityException: GetName can only be called from the main thread.
Constructors and field initializers will be executed from the loading thread when loading a scene.
```

---

## 🔍 根本原因分析

### 問題本質
這是一個 **Unity 執行緒安全問題**，發生在場景載入期間。

### 技術細節

1. **Unity 的執行緒模型**
   - 主執行緒：處理大部分 Unity API 呼叫
   - 背景執行緒：處理場景載入、資源載入、UI 佈局計算

2. **TextMeshPro 的限制**
   - TextMeshPro 的字體系統（FontAsset）必須在主執行緒上存取
   - 在 `Awake()` 階段，場景可能還在背景執行緒載入
   - 此時存取 TMP_Text 會觸發字體載入，導致崩潰

3. **觸發時機**
   ```
   場景載入 (背景執行緒)
      ↓
   BucketEvent.Awake() 執行
      ↓
   generator.GetFish() 被呼叫
      ↓
   可能觸發 UpdateUI()
      ↓
   TextMeshPro 在背景執行緒被存取
      ↓
   💥 崩潰
   ```

---

## ⚠️ 問題程式碼

### 原始程式碼（有問題）

```csharp
// BucketEvent.cs - 原始版本
private void Awake()
{
    // ❌ 問題 1：在 Awake 中取得 Generator 資料
    // 此時 Generator 可能還未初始化完成
    fishes = generator != null ? generator.GetFish() : new List<Fish>();
    
    // ❌ 問題 2：字典初始化位置錯誤
    fishInBucket["redFish"] = 0;
    fishInBucket["blueFish"] = 0;
    fishInBucket["greenFish"] = 0;
}

private void OnTriggerEnter(Collider other)
{
    // ❌ 問題 3：沒有檢查初始化狀態
    // 如果 fishes 還是 null，會出錯
    Fish fishData = fishes.Find(f => f.color == fishTag);
    
    // ❌ 問題 4：可能在背景執行緒更新 UI
    UpdateUI();
}

private void UpdateUI()
{
    // ❌ 問題 5：沒有檢查是否可以安全存取
    if (bucketText != null)
    {
        bucketText.text = $"桶內魚數: {fishCount}";
    }
}
```

---

## ✅ 修復方案

### 修復重點

1. **分離初始化階段**
   - `Awake()`：只初始化不依賴其他物件的資料
   - `Start()`：初始化需要其他物件的資料

2. **新增初始化標記**
   - 使用 `isInitialized` 標記確保資料已準備好

3. **防禦性檢查**
   - 在所有可能存取未初始化資料的地方加入檢查

### 修復後的程式碼

```csharp
// BucketEvent.cs - 修復版本
private bool isInitialized = false; // ✅ 新增初始化標記

private void Awake()
{
    // ✅ 只初始化字典，不依賴外部物件
    fishInBucket["redFish"] = 0;
    fishInBucket["blueFish"] = 0;
    fishInBucket["greenFish"] = 0;
}

private void Start()
{
    // ✅ 在 Start 中初始化，確保 Generator 已完成初始化
    fishes = generator != null ? generator.GetFish() : new List<Fish>();
    isInitialized = true;
    
    // ✅ 安全地初始化 UI
    UpdateUI();
}

private void OnTriggerEnter(Collider other)
{
    // ✅ 確保已初始化
    if (!isInitialized) return;
    
    string fishTag = GetFishTag(other.gameObject);
    
    if (!string.IsNullOrEmpty(fishTag))
    {
        // ... 處理邏輯
        UpdateUI();
    }
}

private void UpdateUI()
{
    // ✅ 防禦性檢查
    if (!isInitialized || fishes == null) return;
    
    if (bucketText != null)
    {
        bucketText.text = $"桶內魚數: {fishCount}";
    }
    // ...
}

public bool IsAllFishCaught()
{
    // ✅ 確保資料可用
    if (!isInitialized || fishes == null) return false;
    
    foreach (Fish f in fishes)
    {
        if (!f.IsAllCaught())
            return false;
    }
    return true;
}

public float GetOverallProgress()
{
    // ✅ 確保資料可用
    if (!isInitialized || generator == null) return 0f;
    
    // ...
}
```

---

## 🎯 Unity 生命週期最佳實踐

### 正確的初始化順序

```
Awake()     → 初始化自身資料（不依賴其他物件）
  ↓
OnEnable()  → 註冊事件監聽器
  ↓
Start()     → 初始化需要其他物件的資料
  ↓
Update()    → 遊戲邏輯
```

### 何時使用哪個方法

| 方法 | 用途 | 注意事項 |
|------|------|----------|
| **Awake()** | 初始化自身資料 | ❌ 不要存取其他物件<br>❌ 不要存取 UI |
| **OnEnable()** | 啟用時的設定 | ⚠️ 可能在 Awake 之前執行 |
| **Start()** | 初始化外部引用 | ✅ 可以安全存取其他物件<br>✅ 可以存取 UI |
| **Update()** | 每幀更新 | ✅ 確保所有物件已初始化 |

---

## 📋 檢查清單

在編寫 Unity 腳本時，請確認：

- [ ] `Awake()` 中只初始化自身資料
- [ ] `Start()` 中初始化需要其他物件的資料
- [ ] 所有公開方法都檢查初始化狀態
- [ ] UI 更新只在主執行緒（MonoBehaviour 方法）中進行
- [ ] 使用 `isInitialized` 標記追蹤初始化狀態
- [ ] 在存取集合前檢查 null

---

## 🔧 相關修復

### 修改的檔案

1. **BucketEvent.cs**
   - ✅ 新增 `isInitialized` 標記
   - ✅ 移動 Fish 資料初始化到 `Start()`
   - ✅ 在 `Awake()` 只初始化字典
   - ✅ 所有方法加入初始化檢查

2. **FishStatisticsManager.cs**
   - ✅ 新增 `isInitialized` 標記
   - ✅ 在 `Update()` 加入初始化檢查
   - ✅ 在 `Start()` 初始化 UI

---

## 🚀 測試建議

### 測試場景

1. **正常啟動**
   - 啟動遊戲，檢查無錯誤訊息
   - 確認 UI 正常顯示

2. **場景重新載入**
   - 重新載入場景多次
   - 確認不會崩潰

3. **快速互動**
   - 在場景載入完成前立即移動魚
   - 確認不會出錯（會被 `isInitialized` 保護）

### 預期結果

- ✅ 無執行緒錯誤
- ✅ UI 正常更新
- ✅ 魚的捕獲統計正確
- ✅ Console 無警告訊息

---

## 📚 參考資料

### Unity 文檔
- [Order of Execution for Event Functions](https://docs.unity3d.com/Manual/ExecutionOrder.html)
- [Threading in Unity](https://docs.unity3d.com/Manual/JobSystem.html)

### 常見錯誤
- `GetName can only be called from the main thread`
- `GetTransform can only be called from the main thread`
- `Find can only be called from the main thread`

### 解決原則
> **任何 Unity API 呼叫都必須在主執行緒上進行**
> 
> 在 `Awake()` 和 `OnEnable()` 中要特別小心，
> 因為這些方法可能在場景載入的早期階段執行。

---

## ✨ 總結

這次修復的關鍵是：

1. **理解 Unity 的執行緒模型**
2. **正確使用生命週期方法**
3. **新增防禦性檢查**
4. **使用初始化標記確保安全**

透過這些改進，程式碼現在更加穩定和可靠！

---

**修復日期**: 2025-10-21  
**影響範圍**: BucketEvent.cs, FishStatisticsManager.cs  
**嚴重程度**: 高（會導致遊戲崩潰）  
**狀態**: ✅ 已修復
