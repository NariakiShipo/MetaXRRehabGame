# 🎣 Meta XR Grab Interaction 設置指南 - 抓取魚

## 📋 概述

這份指南將教你如何在 Meta XR 專案中設置 Grab Interaction 來抓取魚。

---

## 🛠️ 設置步驟

### **步驟 1: 準備魚的 Prefab**

#### 在 Unity Editor 中：

1. **選擇魚的 Prefab**（例如：`Assets/Prefabs/red_Fish.prefab`）

2. **添加必要的組件**：
   
   a. **Rigidbody**（如果還沒有）
   - `Use Gravity`: ❌ 取消勾選（魚在水中）
   - `Linear Damping`: 1.0（模擬水的阻力）
   - `Angular Damping`: 0.5
   - `Constraints`: 根據需要凍結旋轉

   b. **Collider**（如果還沒有）
   - 推薦使用 `SphereCollider` 或 `CapsuleCollider`
   - 調整大小以符合魚的形狀
   - ✅ 取消勾選 `Is Trigger`（改為實體碰撞）

   c. **FishMovement.cs**（已經存在）
   - 此腳本已更新以支援 Grab 互動

   d. **GrabbableFish.cs**（新增的腳本）
   - 拖放到魚的 Prefab 上

3. **添加 Meta XR SDK 組件**：

   a. **Grabbable** 組件
   - 在 Inspector 中點擊 `Add Component`
   - 搜尋並添加 `Grabbable`（來自 Oculus.Interaction 命名空間）
   
   b. **配置 Grabbable**：
   ```
   Grabbable 設置：
   ├─ Transform
   │  └─ Physics Transformer: OneGrabFreeTransformer
   │
   ├─ Events
   │  ├─ When Select: 連結 GrabbableFish.OnFishGrabbed()
   │  ├─ When Unselect: 連結 GrabbableFish.OnFishReleased()
   │  ├─ When Hover: 連結 GrabbableFish.OnFishHoverEnter()
   │  └─ When Unhover: 連結 GrabbableFish.OnFishHoverExit()
   │
   └─ Optional Components
      └─ Transfer On Second Selection: 允許手之間傳遞
   ```

   c. **添加 Transformer**（選擇一種）：
   - `OneGrabFreeTransformer` - 自由移動和旋轉（推薦）
   - `OneGrabTranslateTransformer` - 只允許移動
   - `OneGrabRotateTransformer` - 只允許旋轉

---

### **步驟 2: 設置 XR 手部/控制器**

確保場景中有以下設置：

1. **OVRCameraRig** 或 **XR Origin**
2. **HandGrabInteractor**（手部抓取）或 **GrabInteractor**（控制器抓取）

#### 如果使用手部抓取：

```
LeftHand GameObject
├─ HandGrabInteractor (Component)
├─ HandVisual (Component)
└─ 其他手部組件...

RightHand GameObject
├─ HandGrabInteractor (Component)
├─ HandVisual (Component)
└─ 其他手部組件...
```

#### 如果使用控制器抓取：

```
LeftController GameObject
├─ GrabInteractor (Component)
├─ Controller (Component)
└─ 其他控制器組件...

RightController GameObject
├─ GrabInteractor (Component)
├─ Controller (Component)
└─ 其他控制器組件...
```

---

### **步驟 3: 配置 GrabbableFish 組件**

在 Inspector 中配置 `GrabbableFish` 組件：

```
GrabbableFish 設置：
│
├─ Fish Settings
│  ├─ Disable Movement When Grabbed: ✅（抓住時停止移動）
│  ├─ Destroy On Release: ❌（正常模式）或 ✅（收集模式）
│  └─ Release Velocity Multiplier: 1.0
│
└─ References
   └─ Generator: 拖放 Generator GameObject
```

---

### **步驟 4: 更新 Generator.cs**

確保 Generator 腳本已經更新為支援 Fish 資料追蹤（已完成）。

---

## 🎮 使用模式

### **模式 A: 正常抓取模式**
```
設置：
- Destroy On Release: ❌

行為：
- 抓取魚 → 魚停止移動
- 放開魚 → 魚恢復移動
```

### **模式 B: 收集模式**
```
設置：
- Destroy On Release: ✅

行為：
- 抓取魚 → 魚停止移動
- 放開魚 → 魚被銷毀（計入捕獲統計）
```

### **模式 C: 投入桶子模式**（推薦）
```
設置：
- Destroy On Release: ❌
- 使用 BucketEvent 偵測進入桶子

行為：
- 抓取魚 → 魚停止移動
- 將魚投入桶子 → BucketEvent 觸發
- 魚在桶內 → 計入統計
```

---

## 🔧 完整組件清單

### **魚 Prefab 必須有的組件**：

1. ✅ **Transform**
2. ✅ **Rigidbody**
3. ✅ **Collider** (SphereCollider/CapsuleCollider)
4. ✅ **FishMovement.cs**
5. ✅ **GrabbableFish.cs**
6. ✅ **Grabbable** (Meta XR SDK)
7. ✅ **OneGrabFreeTransformer** (或其他 Transformer)

### **可選組件**：

- **MeshRenderer** - 視覺外觀
- **AudioSource** - 音效
- **ParticleSystem** - 特效

---

## 🎯 事件連結指南

### 在 Grabbable 組件中設置 UnityEvents：

#### **When Select (當被選擇/抓取)**
```
1. 點擊 "+" 按鈕
2. 拖放魚的 GameObject 到 Object 欄位
3. 選擇 GrabbableFish → OnFishGrabbed()
```

#### **When Unselect (當被取消選擇/放開)**
```
1. 點擊 "+" 按鈕
2. 拖放魚的 GameObject 到 Object 欄位
3. 選擇 GrabbableFish → OnFishReleased()
```

#### **When Hover (當被指向)**
```
1. 點擊 "+" 按鈕
2. 拖放魚的 GameObject 到 Object 欄位
3. 選擇 GrabbableFish → OnFishHoverEnter()
```

#### **When Unhover (當不再被指向)**
```
1. 點擊 "+" 按鈕
2. 拖放魚的 GameObject 到 Object 欄位
3. 選擇 GrabbableFish → OnFishHoverExit()
```

---

## 🐛 常見問題排解

### **問題 1: 無法抓取魚**

**可能原因**：
- ❌ 沒有 Grabbable 組件
- ❌ Collider 設為 Trigger
- ❌ 沒有 Interactor（手部/控制器）

**解決方案**：
- ✅ 確認魚有 Grabbable 組件
- ✅ Collider 的 `Is Trigger` 取消勾選
- ✅ 確認手部/控制器有 GrabInteractor

---

### **問題 2: 抓取後魚還在移動**

**可能原因**：
- ❌ 沒有連結 OnFishGrabbed 事件
- ❌ FishMovement 沒有被停用

**解決方案**：
- ✅ 在 Grabbable → When Select 中連結 OnFishGrabbed
- ✅ 確認 `Disable Movement When Grabbed` 已勾選

---

### **問題 3: 魚穿過牆壁**

**可能原因**：
- ❌ Rigidbody 的 Collision Detection 設為 Discrete
- ❌ 移動速度太快

**解決方案**：
- ✅ Rigidbody → Collision Detection 改為 `Continuous`
- ✅ 降低 FishMovement 的速度

---

### **問題 4: 抓取時魚抖動**

**可能原因**：
- ❌ Rigidbody 的 Interpolate 未設置
- ❌ 有多個物理系統衝突

**解決方案**：
- ✅ Rigidbody → Interpolate 設為 `Interpolate`
- ✅ 抓取時停用 FishMovement

---

## 📊 進階功能

### **添加視覺反饋**

修改 `GrabbableFish.cs` 的 Hover 方法：

```csharp
public void OnFishHoverEnter()
{
    // 添加高亮效果
    var renderer = GetComponent<Renderer>();
    if (renderer != null)
    {
        renderer.material.color = Color.yellow;
    }
}

public void OnFishHoverExit()
{
    // 移除高亮效果
    var renderer = GetComponent<Renderer>();
    if (renderer != null)
    {
        renderer.material.color = Color.white;
    }
}
```

---

### **添加音效**

```csharp
[SerializeField] private AudioClip grabSound;
[SerializeField] private AudioClip releaseSound;

private AudioSource audioSource;

public void OnFishGrabbed()
{
    // 播放抓取音效
    if (audioSource != null && grabSound != null)
    {
        audioSource.PlayOneShot(grabSound);
    }
    // ... 其他邏輯
}
```

---

### **整合 BucketEvent**

在 BucketEvent.cs 中檢測被抓取的魚：

```csharp
private void OnTriggerEnter(Collider other)
{
    GrabbableFish grabbableFish = other.GetComponent<GrabbableFish>();
    
    if (grabbableFish != null)
    {
        string fishTag = GetFishTag(other.gameObject);
        
        if (!string.IsNullOrEmpty(fishTag))
        {
            // 如果魚正在被抓住，計為有效捕獲
            if (grabbableFish.IsGrabbed)
            {
                Debug.Log($"魚被正確投入桶中！");
                // 更新統計...
            }
        }
    }
}
```

---

## ✅ 檢查清單

完成設置後，檢查以下項目：

- [ ] 魚的 Prefab 有 Rigidbody
- [ ] 魚的 Prefab 有 Collider（非 Trigger）
- [ ] 魚的 Prefab 有 FishMovement.cs
- [ ] 魚的 Prefab 有 GrabbableFish.cs
- [ ] 魚的 Prefab 有 Grabbable 組件
- [ ] 魚的 Prefab 有 Transformer 組件
- [ ] Grabbable 事件已連結
- [ ] 場景中有 HandGrabInteractor 或 GrabInteractor
- [ ] Generator 引用已設置
- [ ] 測試抓取功能正常

---

## 🎮 測試流程

1. **啟動遊戲**
2. **伸手接近魚** → 應該看到 Hover 反饋
3. **執行抓取手勢**（捏合或按下抓取按鈕）→ 魚應該停止移動
4. **移動手/控制器** → 魚應該跟隨
5. **放開魚** → 魚應該恢復移動（或被銷毀，根據設置）
6. **檢查 Console** → 應該看到相關的 Debug 訊息

---

## 🚀 下一步

- 添加粒子特效
- 實現計分系統
- 添加音效和觸覺反饋
- 創建不同類型的魚（稀有度系統）
- 實現成就系統

---

**設置完成後，你就可以在 VR 中抓魚了！** 🎣✨
