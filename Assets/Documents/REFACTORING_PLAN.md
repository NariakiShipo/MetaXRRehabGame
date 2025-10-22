# 📂 Scripts 資料夾重構計畫

## 📊 當前結構分析

### 現有結構
```
Assets/Scripts/
├── Core/
│   └── GameEvent.cs                    (遊戲計時器和分數管理)
│
├── Objects/
│   ├── Fish.cs                         (資料模型 - 非 MonoBehaviour)
│   ├── FishMovement.cs                 (魚的移動行為)
│   ├── FishEvent.cs                    (魚被捕獲事件處理)
│   ├── FishSpawnStabilizer.cs          (魚生成穩定處理)
│   ├── GrabbableFish.cs                (VR 抓取互動)
│   ├── FishStatisticsManager.cs        (統計資料 UI 管理)
│   ├── Generator.cs                    (魚的生成器)
│   └── BucketEvent.cs                  (桶子碰撞事件處理)
│
└── Function/
    └── CenterParent.cs                 (編輯器工具)
```

### 🚨 當前問題

1. **Objects 資料夾職責混亂**
   - ❌ 混合了資料模型、行為、UI、管理器
   - ❌ 沒有清晰的分層架構
   - ❌ 難以快速找到相關代碼

2. **命名不清晰**
   - ❌ "Objects" 太過通用
   - ❌ "Core" 只有一個類
   - ❌ "Function" 意義不明確

3. **缺乏可擴展性**
   - ❌ 新增功能時不知道放哪裡
   - ❌ 沒有考慮未來的複雜度

---

## 💡 重構方案對比

### 方案 A：按職責分類 ⭐ **推薦**

```
Assets/Scripts/
├── Data/                               [資料模型層]
│   └── Fish.cs
│
├── Gameplay/                           [遊戲邏輯層]
│   ├── Fish/
│   │   ├── FishMovement.cs
│   │   ├── FishSpawnStabilizer.cs
│   │   └── GrabbableFish.cs
│   └── Environment/
│       └── BucketEvent.cs
│
├── Managers/                           [管理器層]
│   ├── GameManager.cs (重命名自 GameEvent)
│   ├── FishSpawnManager.cs (重命名自 Generator)
│   └── FishStatisticsManager.cs
│
├── Events/                             [事件系統層]
│   └── FishEvent.cs
│
└── Editor/                             [編輯器工具層]
    └── CenterParent.cs
```

**優點**：
- ✅ 清晰的架構分層（Data → Gameplay → Managers → Events）
- ✅ 符合 SOLID 原則和依賴反轉
- ✅ 容易擴展和維護
- ✅ 符合 Unity 社群慣例
- ✅ 職責單一，易於測試

**缺點**：
- ⚠️ 資料夾層級較多
- ⚠️ 需要明確理解各層職責

---

### 方案 B：按功能模組分類

```
Assets/Scripts/
├── Core/
│   ├── Data/
│   │   └── Fish.cs
│   └── Managers/
│       └── GameManager.cs
│
├── FishingSystem/                      [魚相關所有功能]
│   ├── FishMovement.cs
│   ├── FishSpawnStabilizer.cs
│   ├── FishSpawnManager.cs
│   ├── GrabbableFish.cs
│   ├── FishEvent.cs
│   └── FishStatisticsManager.cs
│
├── InteractionSystem/                  [互動系統]
│   └── BucketEvent.cs
│
└── EditorTools/
    └── CenterParent.cs
```

**優點**：
- ✅ 相關功能集中在一起
- ✅ 適合功能模組化開發
- ✅ 資料夾層級較少

**缺點**：
- ⚠️ 不同職責的代碼混在同一資料夾
- ⚠️ 擴展時可能產生巨大的資料夾
- ⚠️ 跨模組共用代碼不好處理

---

### 方案 C：混合式（功能 + 職責）

```
Assets/Scripts/
├── Core/
│   ├── Data/
│   │   └── Fish.cs
│   ├── Events/
│   │   └── FishEvent.cs
│   └── Managers/
│       └── GameManager.cs
│
├── Gameplay/
│   ├── Fish/
│   │   ├── Behaviors/
│   │   │   ├── FishMovement.cs
│   │   │   └── FishSpawnStabilizer.cs
│   │   ├── Interactions/
│   │   │   └── GrabbableFish.cs
│   │   └── Spawning/
│   │       └── FishSpawnManager.cs
│   └── Environment/
│       └── BucketEvent.cs
│
├── UI/
│   └── FishStatisticsManager.cs
│
└── Editor/
    └── CenterParent.cs
```

**優點**：
- ✅ 平衡了職責分離和功能聚合
- ✅ 層級清晰
- ✅ UI 獨立出來

**缺點**：
- ⚠️ 資料夾層級最深
- ⚠️ 可能過度設計

---

## 🎯 推薦方案：方案 A（按職責分類）

### 完整結構

```
Assets/Scripts/
│
├── 📁 Data/                                    資料模型（POCO，無依賴）
│   └── Fish.cs                                 魚的資料結構
│
├── 📁 Gameplay/                                遊戲玩法邏輯
│   ├── 📁 Fish/                                魚相關行為
│   │   ├── FishMovement.cs                     魚的移動控制
│   │   ├── FishSpawnStabilizer.cs              生成後穩定處理
│   │   └── GrabbableFish.cs                    VR 抓取互動
│   └── 📁 Environment/                         環境互動
│       └── BucketEvent.cs                      桶子碰撞檢測
│
├── 📁 Managers/                                系統管理器
│   ├── GameManager.cs                          遊戲流程管理（計時、分數）
│   ├── FishSpawnManager.cs                     魚生成管理
│   └── FishStatisticsManager.cs                統計資料 UI 管理
│
├── 📁 Events/                                  事件系統
│   └── FishEvent.cs                            魚相關事件處理
│
└── 📁 Editor/                                  編輯器工具
    └── CenterParent.cs                         編輯器輔助工具
```

---

## 📋 檔案重命名建議

| 舊名稱 | 新名稱 | 原因 |
|--------|--------|------|
| `GameEvent.cs` | `GameManager.cs` | 更符合職責，實際上是管理器而非事件 |
| `Generator.cs` | `FishSpawnManager.cs` | 更清楚地表達職責 |
| `FishEvent.cs` | 保持不變 | 名稱已清晰 |

---

## 🔧 重構步驟

### 階段 1：創建新資料夾結構（不移動檔案）

```powershell
# 在 Unity 內手動創建資料夾，避免破壞 .meta 檔案
Assets/Scripts/Data/
Assets/Scripts/Gameplay/
Assets/Scripts/Gameplay/Fish/
Assets/Scripts/Gameplay/Environment/
Assets/Scripts/Managers/
Assets/Scripts/Events/
Assets/Scripts/Editor/
```

### 階段 2：移動檔案（在 Unity Editor 內拖曳）

**重要：必須在 Unity Editor 內移動，保持 .meta 檔案關聯**

#### Step 1: 移動資料模型
```
Objects/Fish.cs → Data/Fish.cs
```

#### Step 2: 移動遊戲邏輯
```
Objects/FishMovement.cs → Gameplay/Fish/FishMovement.cs
Objects/FishSpawnStabilizer.cs → Gameplay/Fish/FishSpawnStabilizer.cs
Objects/GrabbableFish.cs → Gameplay/Fish/GrabbableFish.cs
Objects/BucketEvent.cs → Gameplay/Environment/BucketEvent.cs
```

#### Step 3: 移動管理器
```
Core/GameEvent.cs → Managers/GameManager.cs (重命名)
Objects/Generator.cs → Managers/FishSpawnManager.cs (重命名)
Objects/FishStatisticsManager.cs → Managers/FishStatisticsManager.cs
```

#### Step 4: 移動事件
```
Objects/FishEvent.cs → Events/FishEvent.cs
```

#### Step 5: 移動編輯器工具
```
Function/CenterParent.cs → Editor/CenterParent.cs
```

### 階段 3：重命名類別（如果重命名檔案）

如果重命名了檔案，需要同步更新類別名稱：

**GameEvent.cs → GameManager.cs**
```csharp
// 修改類別名稱
public class GameManager : MonoBehaviour  // 原本是 GameEvent
{
    // ... 內容保持不變
}
```

**Generator.cs → FishSpawnManager.cs**
```csharp
// 修改類別名稱
public class FishSpawnManager : MonoBehaviour  // 原本是 Generator
{
    // ... 內容保持不變
}
```

### 階段 4：更新引用

需要更新以下檔案的引用：

1. **BucketEvent.cs**
```csharp
// 舊引用
[SerializeField] private Generator generator;

// 新引用
[SerializeField] private FishSpawnManager fishSpawnManager;
```

2. **FishEvent.cs**
```csharp
// 舊引用
[SerializeField] private GameEvent gameEvent;

// 新引用
[SerializeField] private GameManager gameManager;
```

3. **Scene 中的 GameObject**
   - 找到掛載 Generator 的 GameObject，會自動更新為 FishSpawnManager
   - 找到掛載 GameEvent 的 GameObject，會自動更新為 GameManager

### 階段 5：刪除舊資料夾

確認所有檔案都移動完成後，刪除空資料夾：
```
Assets/Scripts/Objects/ (刪除)
Assets/Scripts/Core/ (刪除)
Assets/Scripts/Function/ (刪除)
```

---

## ⚠️ 注意事項

### 1. **必須在 Unity Editor 內操作**
- ✅ 使用 Unity Editor 拖曳移動檔案
- ❌ 不要在檔案總管直接移動
- **原因**：保持 .meta 檔案和 GUID 關聯

### 2. **移動前備份**
```
建議使用 Git commit 或 Unity Package 匯出
```

### 3. **Scene 引用檢查**
移動後檢查所有 Scene 中的引用：
- Inspector 中不應出現 "Missing Script"
- Prefab 引用應保持完整

### 4. **編譯檢查**
每移動一批檔案後，確認：
- ✅ 沒有編譯錯誤
- ✅ Console 無警告
- ✅ 所有引用正常

---

## 📊 重構前後對比

### 重構前（當前）
```
❌ 職責不清晰
❌ 難以擴展
❌ 查找代碼困難
```

### 重構後
```
✅ 清晰的分層架構：Data → Gameplay → Managers → Events
✅ 容易找到相關代碼
✅ 易於擴展新功能
✅ 符合單一職責原則
✅ 便於團隊協作
```

---

## 🚀 未來擴展建議

重構後的結構可以輕鬆加入：

### 新增 UI 系統
```
Assets/Scripts/UI/
├── HUD/
├── Menus/
└── Panels/
```

### 新增音效系統
```
Assets/Scripts/Audio/
├── SoundManager.cs
└── MusicManager.cs
```

### 新增 VR 互動系統
```
Assets/Scripts/VR/
├── Interactions/
├── Controllers/
└── Teleportation/
```

### 新增工具類
```
Assets/Scripts/Utils/
├── Extensions/
├── Helpers/
└── Constants/
```

---

## ✅ 檢查清單

重構完成後確認：

- [ ] 所有檔案都移動到正確位置
- [ ] 沒有編譯錯誤或警告
- [ ] Scene 中的引用都正常
- [ ] Prefab 引用完整
- [ ] 測試所有功能正常運作
- [ ] Git commit 記錄變更
- [ ] 更新專案文件（如果有的話）

---

## 📚 參考資源

- [Unity 專案結構最佳實踐](https://unity.com/how-to/organizing-your-project)
- [SOLID 原則](https://en.wikipedia.org/wiki/SOLID)
- [Clean Architecture in Unity](https://www.youtube.com/watch?v=tE1qH8OWQvg)

---

**準備好重構了嗎？建議先提交 Git commit，然後逐步進行！** 🎯
