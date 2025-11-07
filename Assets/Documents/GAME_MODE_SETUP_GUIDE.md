# 🎮 游戏难度选择系统设置指南

## 📋 概述

`GameModeManager` 控制游戏的难度选择和启动流程：
- 游戏开始前暂停所有系统
- 玩家选择难度（Easy / Normal / Hard）
- 设置倒数计时时间
- 启动游戏系统开始游戏

---

## 🔧 Unity Inspector 设置

### **Step 1: GameModeManager 配置**

在 Hierarchy 中创建或选择一个 GameObject，添加 `GameModeManager` 组件：

```
Game Mode Manager:
├─ Game Mode Settings
│  ├─ Easy Mode Time: 180       (3 分钟)
│  ├─ Normal Mode Time: 300     (5 分钟)
│  └─ Hard Mode Time: 600       (10 分钟)
│
├─ References
│  ├─ Game Manager: [拖入 GameManager GameObject]
│  └─ Fish Spawn Manager: [拖入 FishSpawnManager GameObject]
│
├─ UI References
│  └─ Difficulty Selection UI: [拖入包含三个按钮的父物体]
│
└─ Events
   └─ On Game Start: [可选：添加游戏开始时的额外行为]
```

---

## 🎯 按钮设置

### **Step 2: 配置 Easy 按钮**

选择 Easy Button GameObject → Inspector：

```
Button Event (Script):
├─ Trigger Tag: "Player" 或 "Hand"
├─ Can Repeat Press: false
└─ On Button Pressed (UnityEvent)
   └─ + 添加新事件
       ├─ Object: [拖入 GameModeManager GameObject]
       ├─ Function: GameModeManager → OnEasyButtonPressed()
```

### **Step 3: 配置 Normal 按钮**

选择 Normal Button GameObject → Inspector：

```
Button Event (Script):
├─ Trigger Tag: "Player" 或 "Hand"
├─ Can Repeat Press: false
└─ On Button Pressed (UnityEvent)
   └─ + 添加新事件
       ├─ Object: [拖入 GameModeManager GameObject]
       ├─ Function: GameModeManager → OnNormalButtonPressed()
```

### **Step 4: 配置 Hard 按钮**

选择 Hard Button GameObject → Inspector：

```
Button Event (Script):
├─ Trigger Tag: "Player" 或 "Hand"
├─ Can Repeat Press: false
└─ On Button Pressed (UnityEvent)
   └─ + 添加新事件
       ├─ Object: [拖入 GameModeManager GameObject]
       ├─ Function: GameModeManager → OnHardButtonPressed()
```

---

## 📐 场景结构建议

```
Hierarchy:
├─ GameManagers
│  ├─ GameModeManager      ← 难度管理器
│  ├─ GameManager          ← 游戏计时和分数
│  └─ FishSpawnManager     ← 鱼生成管理器
│
└─ UI
   └─ DifficultySelection  ← 难度选择 UI（父物体）
      ├─ EasyButton        ← Easy 按钮（ButtonEvent 组件）
      ├─ NormalButton      ← Normal 按钮（ButtonEvent 组件）
      └─ HardButton        ← Hard 按钮（ButtonEvent 组件）
```

---

## 🔄 工作流程

### **游戏启动流程：**

```
1. 场景加载
   └─ GameModeManager.Start()
      ├─ 暂停 GameManager (enabled = false)
      ├─ 暂停 FishSpawnManager (enabled = false)
      └─ 等待玩家选择难度

2. 玩家按下按钮（例如：Normal）
   └─ ButtonEvent.OnTriggerEnter()
      └─ 触发 onButtonPressed
         └─ GameModeManager.OnNormalButtonPressed()
            ├─ SetTime(1) → GameManager.timer = 300s
            ├─ 启动 GameManager (enabled = true)
            ├─ 启动 FishSpawnManager (enabled = true)
            ├─ 隐藏 DifficultySelectionUI
            ├─ 触发 onGameStart 事件
            └─ 游戏开始！

3. 游戏进行中
   ├─ GameManager.Update() → 倒数计时
   ├─ FishSpawnManager.OnEnable() → 生成鱼
   └─ 玩家抓鱼

4. 时间到 / 游戏结束
   └─ GameManager: Time's up!
```

---

## 🎨 可选：难度选择 UI 美化

### **UI 布局示例：**

```
Canvas
└─ DifficultySelection Panel
   ├─ Title Text: "选择难度"
   ├─ EasyButton
   │  ├─ Background (绿色)
   │  └─ Text: "简单 (3分钟)"
   ├─ NormalButton
   │  ├─ Background (黄色)
   │  └─ Text: "普通 (5分钟)"
   └─ HardButton
      ├─ Background (红色)
      └─ Text: "困难 (10分钟)"
```

---

## 🛠️ 高级功能

### **1. 添加游戏开始音效**

在 `GameModeManager` 的 Inspector 中：

```
On Game Start:
└─ + 添加事件
   ├─ Object: [AudioSource]
   └─ Function: AudioSource → Play()
```

### **2. 添加开始动画**

```
On Game Start:
└─ + 添加事件
   ├─ Object: [Animator]
   └─ Function: Animator → SetTrigger("GameStart")
```

### **3. 记录选择的难度**

```csharp
// 在其他脚本中获取当前难度
GameModeManager gameModeManager = FindFirstObjectByType<GameModeManager>();
string difficulty = gameModeManager.GetSelectedDifficulty();
Debug.Log($"当前难度：{difficulty}");
```

---

## 📝 代码使用示例

### **在其他脚本中检查游戏状态：**

```csharp
public class SomeOtherScript : MonoBehaviour
{
    private GameModeManager gameModeManager;
    
    void Start()
    {
        gameModeManager = FindFirstObjectByType<GameModeManager>();
    }
    
    void Update()
    {
        // 只有游戏开始后才执行某些逻辑
        if (gameModeManager.IsGameStarted())
        {
            // 游戏进行中的逻辑
        }
    }
}
```

### **重新开始游戏：**

```csharp
// 在游戏结束后，重新选择难度
public void OnRestartButtonClicked()
{
    GameModeManager gameModeManager = FindFirstObjectByType<GameModeManager>();
    gameModeManager.RestartGame();
}
```

---

## ✅ 检查清单

在测试前，请确保：

- [ ] GameModeManager 已添加到场景中
- [ ] GameManager 引用已设置
- [ ] FishSpawnManager 引用已设置
- [ ] DifficultySelectionUI 引用已设置
- [ ] 三个按钮都有 ButtonEvent 组件
- [ ] 三个按钮的 onButtonPressed 都已连接到对应方法
- [ ] 按钮的 Collider 设置为 Trigger
- [ ] 触发对象有正确的 Tag（Player 或 Hand）

---

## 🐛 常见问题

### **问题 1：按钮按下没反应**

**解决方案：**
- 检查 ButtonEvent 的 Trigger Tag 是否正确
- 检查触发对象是否有正确的 Tag
- 检查 Collider 是否设置为 Is Trigger
- 查看 Console 是否有 Debug 信息

### **问题 2：游戏立即开始，没等待选择难度**

**解决方案：**
- 确保 GameManager 在 Start 时 enabled = false
- 确保 FishSpawnManager 在 Start 时 enabled = false
- 或者在 Inspector 中手动取消勾选这两个组件的 enabled

### **问题 3：选择难度后计时器不动**

**解决方案：**
- 检查 GameManager 的引用是否正确设置
- 确保 GameManager.SetTime() 被正确调用
- 查看 Console 的 Debug 信息

---

## 📊 难度时间对照表

| 难度 | 时间 | 索引 | 方法 |
|------|------|------|------|
| Easy | 180s (3分钟) | 0 | OnEasyButtonPressed() |
| Normal | 300s (5分钟) | 1 | OnNormalButtonPressed() |
| Hard | 600s (10分钟) | 2 | OnHardButtonPressed() |

---

## 🎯 总结

这个系统实现了：
1. ✅ 游戏开始前暂停所有系统
2. ✅ 通过按钮选择难度
3. ✅ 根据难度设置倒数计时
4. ✅ 启动游戏系统开始游戏
5. ✅ 隐藏难度选择 UI
6. ✅ 支持重新开始游戏

现在你可以在场景中放置三个按钮，玩家按下后游戏就会以选定的难度开始！
