# 任务系统设置指南 (Task System Setup Guide)

## 📋 概述

本指南详细说明如何设置三阶段任务系统：
- **初级 (Easy)**: 数量认知 - "撈 2 隻魚"
- **中级 (Normal)**: 颜色+数量 - "撈 2 隻紅色的魚"（撈错重新开始）
- **高级 (Hard)**: 多阶段任务 - "先撈 2 隻紅色，再 1 隻藍色"（撈错销毁错误的鱼）

---

## 📦 新创建的文件

### 核心系统
1. **TaskData.cs** - 任务数据结构
   - `TaskType` 枚举（CountOnly, ColorCount, MultiStage）
   - `TaskValidationResult` 枚举（Success, Failed, Incomplete, SubTaskComplete）
   - `SubTask` 类（子任务数据）
   - `TaskData` 类（主任务数据）

2. **TaskManager.cs** - 任务管理核心
   - 随机生成任务
   - 验证任务完成度
   - 处理多阶段任务逻辑
   - 颜色显示名称映射（redFish → 紅色）

3. **FishData.cs** - 鱼数据组件
   - 附加到每条鱼GameObject上
   - 存储 `prefabName` 用于任务验证

### UI 组件
4. **TaskDisplayUI.cs** - 任务显示UI
   - 显示当前任务文本
   - 显示错误信息（"撈錯了！請重新開始"）
   - 自动隐藏错误信息（2秒后）

### 事件处理器
5. **ConfirmButtonHandler.cs** - 确认按钮处理器
   - 触发任务验证
   - 根据验证结果处理后续逻辑

6. **RetryButtonHandler.cs** - 重试按钮处理器（高级模式专用）
   - 清空桶
   - 重置当前子任务进度

### 修改的文件
7. **BucketEvent.cs** - 新增功能
   - `GetFishInBucket()` - 获取桶中鱼的GameObject列表
   - `ClearBucket()` - 清空桶并销毁所有鱼
   - 追踪鱼GameObject（进入/离开桶时）

8. **GameModeManager.cs** - 集成任务系统
   - 订阅任务验证事件
   - 根据难度设置任务类型
   - 任务完成后生成新任务
   - 任务失败时重新生成任务

9. **FishSpawnManager.cs** - 颜色控制
   - `SetSpawnMode(int difficulty)` - 根据难度控制生成颜色
   - 初级：单色
   - 中高级：3-4种颜色
   - 自动添加 FishData 组件到生成的鱼

---

## 🔧 Scene 设置步骤

### 1. 创建 TaskManager GameObject

```
Hierarchy 右键 -> Create Empty
命名为: TaskManager
添加组件: TaskManager
```

**Inspector 配置:**
- **Available Colors**: `redFish`, `blueFish`, `yellowFish`, `greenFish`
- **Color Name Mappings** (4个元素):
  - Element 0: colorKey=`redFish`, displayName=`紅色`
  - Element 1: colorKey=`blueFish`, displayName=`藍色`
  - Element 2: colorKey=`yellowFish`, displayName=`黃色`
  - Element 3: colorKey=`greenFish`, displayName=`綠色`
- **Min Fish Count**: `1`
- **Max Fish Count**: `5`
- **Min Sub Tasks**: `2`
- **Max Sub Tasks**: `3`

### 2. 创建任务显示 UI

```
Hierarchy 右键 -> UI -> Canvas (如果没有)
Canvas 右键 -> UI -> Panel
命名为: TaskPanel
```

**TaskPanel 配置:**
- Position: 根据需要调整（例如屏幕上方）
- 添加组件: `TaskDisplayUI`

**添加子对象 - 任务描述文本:**
```
TaskPanel 右键 -> UI -> Text - TextMeshPro
命名为: TaskDescriptionText
```
- Font: NotoSansTC（中文字体）
- Font Size: `36`
- Alignment: Center
- Color: White

**添加子对象 - 错误信息面板:**
```
TaskPanel 右键 -> UI -> Panel
命名为: ErrorMessagePanel
```
- Background Color: Red (半透明)
- 默认设置为: **Inactive**

**ErrorMessagePanel 添加子对象 - 错误文本:**
```
ErrorMessagePanel 右键 -> UI -> Text - TextMeshPro
命名为: ErrorMessageText
```
- Font: NotoSansTC
- Font Size: `48`
- Alignment: Center
- Color: White

**TaskDisplayUI Inspector 配置:**
- **Task Description Text**: 拖入 `TaskDescriptionText`
- **Error Message Text**: 拖入 `ErrorMessageText`
- **Error Message Panel**: 拖入 `ErrorMessagePanel`
- **Error Message Duration**: `2` (秒)

### 3. 创建确认按钮 GameObject

**方案A: 使用现有的 Button Prefab**
```
将 Assets/Prefabs/Button.prefab 拖入场景
重命名为: ConfirmButton
```

**方案B: 创建新的 VR 按钮**
```
Hierarchy 右键 -> 3D Object -> Cube
命名为: ConfirmButton
```
- Scale: `(0.2, 0.1, 0.2)`
- Position: 放在玩家容易触碰的位置
- Tag: `Button`
- 添加组件: `Rigidbody` (Is Kinematic = true)
- 添加组件: `Box Collider` (Is Trigger = true)
- 添加组件: `ButtonEvent`

**添加 ConfirmButtonHandler:**
```
ConfirmButton -> Add Component -> ConfirmButtonHandler
```

**ButtonEvent Inspector 配置:**
- **Trigger Tag**: `Player`
- **Can Repeat Press**: `true`
- **On Button Pressed**: 
  - 点击 `+` 添加事件
  - 拖入 `ConfirmButton`
  - 选择函数: `ConfirmButtonHandler.OnConfirmButtonPressed()`

**ConfirmButtonHandler Inspector 配置:**
- **Task Manager**: 拖入 `TaskManager`
- **Bucket Event**: 拖入 `Bucket`
- **Game Mode Manager**: 拖入 `GameModeManager`

### 4. 创建重试按钮（高级模式专用，可选）

```
复制 ConfirmButton
重命名为: RetryButton
```

**添加 RetryButtonHandler:**
```
RetryButton -> Add Component -> RetryButtonHandler
```

**ButtonEvent Inspector 配置:**
- **On Button Pressed**: 
  - 点击 `+` 添加事件
  - 拖入 `RetryButton`
  - 选择函数: `RetryButtonHandler.OnRetryButtonPressed()`

**RetryButtonHandler Inspector 配置:**
- **Task Manager**: 拖入 `TaskManager`
- **Bucket Event**: 拖入 `Bucket`

### 5. 更新 GameModeManager

找到场景中的 `GameModeManager` GameObject:

**Inspector 配置:**
- **Task Manager**: 拖入 `TaskManager`

### 6. 配置难度选择按钮（如果已存在）

找到三个难度按钮（EasyButton, NormalButton, HardButton）:

**每个按钮的 ButtonEvent 组件:**
- **Easy Button** -> On Button Pressed -> `GameModeManager.OnEasyButtonPressed()`
- **Normal Button** -> On Button Pressed -> `GameModeManager.OnNormalButtonPressed()`
- **Hard Button** -> On Button Pressed -> `GameModeManager.OnHardButtonPressed()`

### 7. 添加诊断工具（强烈推荐）⭐

创建一个空 GameObject 用于系统诊断：

```
Hierarchy 右键 -> Create Empty
命名为: TaskSystemDiagnostic
添加组件: TaskSystemDiagnostic
```

**使用方法：**
1. **自动诊断**: 运行游戏后会自动检查，查看 Console 输出
2. **手动诊断**: 在 Inspector 中右键点击组件 -> 选择 "Run Diagnostic"

**诊断输出示例：**
```
========== 任务系统诊断开始 ==========
--- 检查 TaskManager ---
✅ TaskManager 存在: TaskManager
   - GameObject 激活: True
   - 组件启用: True
--- 检查 TaskDisplayUI ---
✅ TaskDisplayUI 存在: TaskPanel
   - GameObject 激活: True
   - 组件启用: True
✅ Task Description Text 已设置
========== 任务系统诊断完成 ==========
```

如果有任何 ❌ 或 ⚠️ 标记，按照提示修复。

---

## 🎮 游戏流程

### 初级模式 (Easy)
1. 玩家按下 Easy 按钮
2. 系统生成单一颜色的鱼
3. TaskDisplayUI 显示："撈 3 隻魚"
4. 玩家收集鱼到桶中
5. 玩家按下确认按钮
6. 系统验证数量：
   - **成功**: 生成新任务
   - **未完成**: 继续收集

### 中级模式 (Normal)
1. 玩家按下 Normal 按钮
2. 系统生成 3-4 种颜色的鱼
3. TaskDisplayUI 显示："撈 2 隻紅色的魚"
4. 玩家收集鱼到桶中
5. 玩家按下确认按钮
6. 系统验证颜色和数量：
   - **成功**: 清空桶，生成新任务
   - **撈错**: 显示"撈錯了！請重新開始"，清空桶，生成新任务
   - **未完成**: 继续收集

### 高级模式 (Hard)
1. 玩家按下 Hard 按钮
2. 系统生成 3-4 种颜色的鱼
3. TaskDisplayUI 显示第一个子任务："撈 2 隻紅色的魚"
4. 玩家收集鱼到桶中
5. 玩家按下确认按钮
6. 系统验证：
   - **撈错**: 显示"撈錯了！請重新開始"，**销毁错误的鱼**，清空桶，重新生成任务
   - **子任务完成**: 清空桶，显示下一个子任务："撈 1 隻藍色的魚"
   - **所有子任务完成**: 清空桶，生成新的多阶段任务
   - **未完成**: 继续收集

---

## 🐛 测试检查清单

### 基础功能
- [ ] TaskManager 在场景中已创建
- [ ] TaskDisplayUI 正确显示任务文本
- [ ] 确认按钮可以触发验证
- [ ] 错误信息会显示并自动隐藏

### 初级模式测试
- [ ] 按下 Easy 按钮后只生成一种颜色的鱼
- [ ] 任务显示格式正确："撈 X 隻魚"
- [ ] 收集正确数量后确认可以完成任务
- [ ] 完成任务后生成新任务

### 中级模式测试
- [ ] 按下 Normal 按钮后生成 3-4 种颜色
- [ ] 任务显示格式正确："撈 X 隻紅色的魚"
- [ ] 收集正确颜色和数量可以完成任务
- [ ] 撈错鱼后显示错误信息并重新生成任务
- [ ] 桶会被正确清空

### 高级模式测试
- [ ] 按下 Hard 按钮后生成 3-4 种颜色
- [ ] 第一个子任务显示正确
- [ ] 撈错鱼会被销毁
- [ ] 完成子任务后显示下一个子任务
- [ ] 所有子任务完成后生成新任务
- [ ] 重试按钮可以重置当前子任务（可选）

### UI 测试
- [ ] 中文字体正确显示
- [ ] 颜色映射正确（redFish → 紅色）
- [ ] 错误信息在 2 秒后自动隐藏
- [ ] UI 位置不遮挡游戏视野

---

## ⚠️ 常见问题

### 0. 任务文本不显示（最常见）⭐
**症状**: 按下难度按钮后，任务文本没有显示

**诊断步骤（按顺序检查）：**

#### 步骤 1: 检查 Console 日志
运行游戏并按下难度按钮，查看 Console 应该出现的日志：

✅ **正常日志顺序：**
```
[GameModeManager] 選擇難度：Easy，時間限制：180 秒
[TaskDisplayUI] 已订阅 TaskManager 事件
[FishSpawnManager] 初級模式：只生成 redFish
[TaskManager] 生成任务: 撈 3 隻魚
[TaskManager] 触发 OnTaskGenerated 事件
[TaskDisplayUI] OnTaskGenerated 被调用，任务类型: CountOnly
[TaskDisplayUI] 更新任务描述: 撈 3 隻魚
```

❌ **如果看到这个错误：**
```
[TaskDisplayUI] 找不到TaskManager!
```
→ **解决**: TaskManager GameObject 不存在或未激活，创建它（见设置步骤 1）

❌ **如果看到这个错误：**
```
[TaskDisplayUI] taskDescriptionText 引用为空！请在 Inspector 中设置
```
→ **解决**: TaskDisplayUI 组件的 Task Description Text 引用未设置（见设置步骤 2）

❌ **如果看到这个警告：**
```
[TaskManager] OnTaskGenerated 事件没有订阅者！
```
→ **解决**: TaskDisplayUI 组件未正确启用或未订阅事件

#### 步骤 2: 检查 TaskPanel 的激活状态
在 Hierarchy 中选择 `TaskPanel`：
- [ ] TaskPanel GameObject 是否激活（左侧勾选框✓）
- [ ] TaskPanel 的父对象（Canvas）是否激活
- [ ] TaskDescriptionText 是否激活
- [ ] TaskDisplayUI 组件是否启用（Inspector 中勾选✓）

#### 步骤 3: 检查 Inspector 引用
选择 `TaskPanel`，查看 `TaskDisplayUI` 组件：
- [ ] **Task Description Text**: 是否拖入了 TaskDescriptionText
- [ ] **Error Message Text**: 是否拖入了 ErrorMessageText
- [ ] **Error Message Panel**: 是否拖入了 ErrorMessagePanel

#### 步骤 4: 检查 TextMeshPro 组件
选择 `TaskDescriptionText` GameObject：
- [ ] 是否有 `TextMeshProUGUI` 组件
- [ ] Font Asset 是否设置（推荐 NotoSansTC SDF）
- [ ] Font Size 是否合理（推荐 36）
- [ ] Color 是否可见（推荐 White）
- [ ] GameObject 是否激活

#### 步骤 5: 检查 Canvas 设置
选择 `Canvas` GameObject：
- [ ] Canvas 组件存在
- [ ] Render Mode 设置（推荐 Screen Space - Overlay 或 World Space）
- [ ] 如果是 World Space，Camera 是否设置

#### 步骤 6: 检查游戏开始顺序
确保按照这个顺序：
1. 启动游戏（Play）
2. 等待加载完成
3. 按下难度按钮（Easy/Normal/Hard）
4. 查看任务文本

**如果还是不显示**，在 Console 中搜索以下关键字：
- `[TaskDisplayUI]`
- `[TaskManager]`
- `[GameModeManager]`

将所有相关日志发送给开发者以便诊断。

---

### 1. 任务不生成
**原因**: TaskManager 未正确配置或未订阅事件
**解决**: 检查 GameModeManager Inspector 中是否拖入了 TaskManager

### 2. 验证不工作
**原因**: ConfirmButtonHandler 引用缺失
**解决**: 检查 ConfirmButtonHandler 的三个引用是否都已设置

### 3. 中文显示为方块
**原因**: TextMeshPro 字体未设置
**解决**: 
- 选择 TaskDescriptionText
- Font Asset 设置为 `NotoSansTC SDF`
- 如果没有，需要创建 TextMeshPro 字体资源

### 4. 鱼颜色识别失败
**原因**: FishData 组件未添加
**解决**: FishSpawnManager 会自动添加，检查生成的鱼是否有 FishData 组件

### 5. 高级模式销毁鱼无效
**原因**: 桶中鱼列表未正确追踪
**解决**: 检查 BucketEvent 的 OnTriggerEnter/Exit 是否正常工作

### 6. 错误信息不显示
**原因**: ErrorMessagePanel 未设置为 Inactive
**解决**: 在 Inspector 中取消勾选 ErrorMessagePanel 的 Active

---

## 🎯 扩展功能建议

### 1. 音效系统
- 任务完成音效
- 撈错音效
- 子任务完成音效

### 2. 动画效果
- 任务文本淡入淡出
- 错误信息抖动动画
- 进度条显示

### 3. 提示系统
- 超过时间未确认给予提示
- 显示当前桶中鱼的数量和颜色

### 4. 统计系统
- 记录每个难度完成的任务数
- 显示正确率
- 最快完成时间

---

## 📝 代码架构说明

### 数据流
```
GameModeManager (选择难度)
    ↓
TaskManager (生成任务)
    ↓
FishSpawnManager (根据难度生成鱼)
    ↓
BucketEvent (追踪桶中的鱼)
    ↓
ConfirmButtonHandler (触发验证)
    ↓
TaskManager (验证任务)
    ↓
GameModeManager (处理结果，生成新任务)
```

### 事件系统
- `TaskManager.OnTaskGenerated` - 任务生成时触发
- `TaskManager.OnTaskValidated` - 任务验证时触发
- `TaskManager.OnSubTaskComplete` - 子任务完成时触发
- `TaskManager.OnTaskFailed` - 任务失败时触发

### 验证逻辑
1. **初级**: 只检查数量
2. **中级**: 检查颜色和数量，有错误立即失败
3. **高级**: 检查当前子任务，错误则销毁错误的鱼

---

## 🔄 重要更新：鱼重新生成逻辑

### 问题
任务完成或失败后，桶中的鱼被清空，但场景中没有新的鱼生成，导致无法继续下一个任务。

### 解决方案
在 `GameModeManager.cs` 中添加了 `RegenerateFish()` 方法：

**触发时机：**
1. ✅ **任务完成** (Success) - 清空桶 → 重新生成鱼 → 生成新任务
2. ✅ **任务失败** (Failed) - 清空桶 → 重新生成鱼 → 重新生成任务
3. ❌ **子任务完成** (SubTaskComplete) - 清空桶 → **不重新生成鱼** → 继续下一个子任务
4. ❌ **任务未完成** (Incomplete) - 不做任何操作

**代码流程：**
```
任务完成/失败
    ↓
GameModeManager.GenerateNewTask()
    ↓
GameModeManager.RegenerateFish()
    ↓
FishSpawnManager.ClearAllFish()  (清除旧鱼)
    ↓
FishSpawnManager.SetSpawnMode()  (设置颜色模式)
    ↓
FishSpawnManager.RegenerateAllFish()  (生成新鱼)
```

### 高级模式特殊处理
在高级模式的多阶段任务中：
- 完成**子任务**时：只清空桶，**不重新生成鱼**，玩家继续用现有的鱼完成下一阶段
- 完成**整个任务**时：清空桶，**重新生成鱼**，开始新的多阶段任务

---

## 🐟 重要更新：鱼数量保证机制

### 问题
任务要求抓 5 条鱼，但场景中只生成了 1-3 条鱼，导致任务无法完成。

### 原因
FishSpawnManager 根据 spawn points 数量平均分配鱼，如果 spawn points 太少，生成的鱼就不够。

### 解决方案
在 `FishSpawnManager.cs` 中添加了 **`minFishPerColor`** 配置：

**FishSpawnManager Inspector 必须配置：**
1. **Min Fish Per Color**: 设置为 `5` 或更高
   - 确保每种颜色至少生成 5 条鱼
   - 应该 ≥ TaskManager 的 Max Fish Count

2. **Allow Reuse Spawn Points**: 建议启用
   - 如果 spawn points 数量 < minFishPerColor × 颜色数，必须启用
   - 允许在同一个位置生成多条鱼

3. **Spawn Points 数量建议：**
   - 初级模式：至少 5 个 spawn points
   - 中高级模式：至少 15-20 个 spawn points（3-4 种颜色 × 5）

### 自动验证
GameModeManager 会在生成任务后自动检查鱼数量：
- 如果鱼数量不足，会在 Console 输出警告
- 警告信息会提示如何修复配置

**Console 警告示例：**
```
[GameModeManager] 生成的鱼数量 (3) 小于任务要求 (5)
[GameModeManager] 请在 FishSpawnManager Inspector 中：
[GameModeManager] 1. 增加 Spawn Points 数量
[GameModeManager] 2. 或启用 'Allow Reuse Spawn Points'
[GameModeManager] 3. 或增加 'Min Fish Per Color' 值
```

### 推荐配置
```
FishSpawnManager Inspector:
├─ Spawn Points: 20+ Transform objects
├─ Min Spawn Count: 0
├─ Max Spawn Count: 0 (使用默认逻辑)
├─ Allow Reuse Spawn Points: ✓ (启用)
└─ Min Fish Per Color: 5 (或更高)

TaskManager Inspector:
├─ Min Fish Count: 1
└─ Max Fish Count: 5 (必须 ≤ minFishPerColor)
```

---

## ✅ 完成标志

所有文件已创建，无编译错误。按照本指南设置场景后，任务系统将完全运行！

如需帮助，请检查 Console 日志，所有关键操作都有详细的 Debug.Log 输出。
