# 🎮 MetaXRRehabGame

> VR Rehabilitation Game for Physical Therapy using Meta Quest

A VR fishing game designed to help patients complete basic physical therapy exercises while in bed. Built with Unity and Meta XR SDK for Meta Quest devices.

---

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Gameplay Mechanics](#gameplay-mechanics)
- [Technical Architecture](#technical-architecture)
- [Dependencies](#dependencies)
- [Development](#development)
- [Documentation](#documentation)
- [Contributing](#contributing)
- [License](#license)

---

## 🎯 Overview

MetaXRRehabGame is a virtual reality rehabilitation application that gamifies physical therapy exercises through an engaging fishing experience. Patients can catch colorful fish using VR hand controllers while performing therapeutic movements, making rehabilitation more enjoyable and motivating.

### 🏥 Rehabilitation Goals

- **Upper Limb Movement**: Reaching and grabbing motions
- **Hand-Eye Coordination**: Tracking and catching moving fish
- **Range of Motion**: Extending arms in different directions
- **Fine Motor Skills**: Precise grabbing and releasing actions

---

## ✨ Features

### 🎣 Gameplay Features

- **VR Fishing Mechanics**: Catch fish using Meta Quest hand tracking or controllers
- **Multiple Fish Types**: Three different colored fish (Red, Blue, Green)
- **Dynamic Spawning**: Fish spawn within a tank with collision prevention
- **Statistics Tracking**: Real-time tracking of caught vs. spawned fish
- **Timer System**: Configurable time limits (3, 5, or 10 minutes)
- **Score System**: Points awarded for each fish caught

### 🔧 Technical Features

- **Meta XR SDK Integration**: Native support for Meta Quest devices
- **Grab Interaction**: Natural hand-based fish grabbing using Grabbable components
- **Physics Simulation**: Realistic underwater movement with buoyancy
- **Spawn Stabilization**: Prevents fish from colliding during initial spawn
- **UI Statistics**: Real-time display of game progress and statistics
- **Chinese Font Support**: TextMesh Pro with NotoSans Traditional Chinese

---

## 📁 Project Structure

```
MetaXRRehabGame/
├── Assets/
│   ├── Materials/              # Material assets (fish colors, etc.)
│   ├── Prefabs/                # Reusable game objects (fish prefabs)
│   ├── Scenes/                 # Unity scenes
│   │   ├── GameScene.unity     # Main game scene
│   │   ├── VRTestScene.unity   # VR testing scene
│   │   └── testScene.unity     # Development test scene
│   ├── Scripts/                # C# source code (organized by layer)
│   │   ├── Data/               # Data models
│   │   │   └── Fish.cs         # Fish data structure
│   │   ├── Gameplay/           # Game logic
│   │   │   ├── Fish/
│   │   │   │   ├── FishMovement.cs           # Fish autonomous movement
│   │   │   │   ├── GrabbableFish.cs          # VR grab interaction
│   │   │   │   └── FishSpawnStabilizer.cs    # Post-spawn stabilization
│   │   │   └── Environment/
│   │   │       └── BucketEvent.cs            # Bucket collision detection
│   │   ├── Managers/           # System managers
│   │   │   ├── GameManager.cs              # Game flow & timer
│   │   │   ├── FishSpawnManager.cs         # Fish spawning system
│   │   │   └── FishStatisticsManager.cs    # Statistics UI
│   │   ├── Events/             # Event handlers
│   │   │   └── FishEvent.cs    # Fish catch events
│   │   └── Editors/            # Editor tools
│   │       └── CenterParent.cs # Editor utility for centering objects
│   ├── Settings/               # Project settings
│   └── TextMesh Pro/           # TMP assets & fonts
├── Packages/                   # Unity packages
├── ProjectSettings/            # Unity project configuration
├── Documentation/              # Project documentation
└── README.md                   # This file
```

---

## 🚀 Getting Started

### Prerequisites

- **Unity Editor**: Version 2022.3 LTS or later
- **Meta Quest Device**: Quest 2, Quest Pro, or Quest 3
- **Meta Quest Link** (for development): Oculus PC app
- **Operating System**: Windows 10/11 (for development)

### Installation

1. **Clone the Repository**
   ```bash
   git clone https://github.com/NariakiShipo/MetaXRRehabGame.git
   cd MetaXRRehabGame
   ```

2. **Open in Unity**
   - Launch Unity Hub
   - Click "Add" → Select the `MetaXRRehabGame` folder
   - Open the project with Unity 2022.3 LTS or later

3. **Install Dependencies**
   - Unity will automatically import required packages
   - Meta XR SDK will be installed via Package Manager

4. **Configure Build Settings**
   - Go to `File → Build Settings`
   - Switch platform to `Android`
   - Select your Meta Quest device in the device list

### Quick Start

1. Open the main scene: `Assets/Scenes/GameScene.unity`
2. Press Play in Unity Editor (requires Meta Quest Link)
3. Or build and deploy to your Meta Quest device

---

## 🎮 Gameplay Mechanics

### Fish Catching

1. **Grab Fish**: Use hand controllers to reach and grab fish
2. **Place in Bucket**: Release fish over the bucket to score
3. **Track Progress**: Monitor caught vs. total spawned fish
4. **Beat the Timer**: Complete the challenge within the time limit

### Fish Types

| Color | Tag | Behavior |
|-------|-----|----------|
| 🔴 Red | `redFish` | Standard movement |
| 🔵 Blue | `blueFish` | Standard movement |
| 🟢 Green | `greenFish` | Standard movement |

### Scoring System

- **Fish Caught**: +1 point per fish
- **Time Bonus**: Remaining time converted to bonus points
- **Completion**: Bonus for catching all fish

---

## 🏗️ Technical Architecture

### Design Patterns

- **Manager Pattern**: Centralized game systems (GameManager, FishSpawnManager)
- **Component-Based**: Modular behaviors attached to GameObjects
- **Event-Driven**: Collision events trigger game logic
- **Data-Oriented**: Separate data models from behavior (Fish class)

### Key Systems

#### 1. Fish Spawning System

```csharp
// FishSpawnManager.cs
- Awake(): Initialize fish data synchronously
- OnEnable(): Start coroutine-based spawning
- SpawnFishWithDelay(): Delayed spawning to prevent collisions
- GetSafeSpawnPosition(): Calculate collision-free spawn positions
```

**Features**:
- Coroutine-based delayed spawning
- Distance checking between fish (min 0.5m)
- Wall safety margins (min 0.3m)
- Maximum spawn attempts (30 tries)

#### 2. VR Grab Interaction

```csharp
// GrabbableFish.cs
- OnFishGrabbed(): Called when fish is grabbed
- OnFishReleased(): Called when fish is released
- Integrates with Meta XR Grabbable component
```

#### 3. Physics Stabilization

```csharp
// FishSpawnStabilizer.cs
- Limits velocity during initial 1 second after spawn
- Prevents fish from being knocked out of tank
- Auto-removes after stabilization period
```

#### 4. Statistics Tracking

```csharp
// Fish.cs (Data Model)
- spawnedAmount: Total fish spawned
- caughtAmount: Total fish caught
- IncrementCaught(): Track caught fish
- DecrementCaught(): Handle fish leaving bucket
- GetProgress(): Calculate completion percentage
```

### Execution Order

```
1. Awake()      : FishSpawnManager initializes Fish data (synchronous)
2. OnEnable()   : Starts spawning coroutine (asynchronous)
3. Start()      : BucketEvent retrieves initialized Fish data
4. Update()     : GameManager updates timer
```

**Why This Order Matters**: Fish data must be initialized before other systems try to access it. Synchronous initialization in `Awake()` ensures data availability by the time `Start()` runs.

---

## 📦 Dependencies

### Unity Packages

| Package | Version | Purpose |
|---------|---------|---------|
| **Meta XR SDK All** | 78.0.0 | VR interaction & hand tracking |
| **Universal Render Pipeline** | 17.0.4 | Rendering pipeline |
| **Input System** | 1.14.2 | New input handling |
| **XR Management** | 4.5.2 | XR plugin management |
| **OpenXR** | 1.15.1 | Cross-platform VR support |
| **TextMesh Pro** | Built-in | UI text rendering |
| **AI Navigation** | 2.0.9 | Pathfinding (future use) |

### External Dependencies

- **Oculus Integration**: Included via Meta XR SDK
- **Chinese Fonts**: NotoSans Traditional Chinese for TextMesh Pro

---

## 🛠️ Development

### Architecture Guidelines

The project follows a **layered architecture**:

```
┌─────────────────────────────────────┐
│           Presentation              │  ← UI, Visual feedback
├─────────────────────────────────────┤
│            Managers                 │  ← Game flow, Systems
├─────────────────────────────────────┤
│            Gameplay                 │  ← Game logic, Behaviors
├─────────────────────────────────────┤
│         Events & Handlers           │  ← Collision, Interaction
├─────────────────────────────────────┤
│          Data Models                │  ← Pure data structures
└─────────────────────────────────────┘
```

### Coding Standards

- **Naming**: PascalCase for classes/methods, camelCase for variables
- **Comments**: XML documentation for public APIs
- **Language**: Code in English, comments can be Chinese for clarity
- **Unity Practices**: Use `[SerializeField]` instead of public fields

### Adding New Features

1. **New Fish Type**:
   - Create prefab in `Assets/Prefabs/`
   - Add tag in Project Settings → Tags
   - Update `FishSpawnManager.fishname` array
   - Update UI display names

2. **New Game Mode**:
   - Extend `GameManager.cs`
   - Add mode selection UI
   - Implement mode-specific logic

3. **New Interaction**:
   - Create script in `Assets/Scripts/Gameplay/`
   - Integrate with Meta XR Interaction SDK
   - Add event handlers in `Events/`

### Testing

1. **In-Editor Testing**:
   - Use Meta Quest Link for VR testing
   - Test with simulated hands in Unity editor

2. **Device Testing**:
   - Build APK and deploy to Meta Quest
   - Test actual hand tracking and controllers

3. **Performance Testing**:
   - Check frame rate (target: 72+ FPS)
   - Monitor physics performance with many fish

### Known Issues

- ⚠️ Fish may occasionally spawn inside each other if spawn attempts fail
- ⚠️ Hand tracking requires good lighting conditions
- ⚠️ Timer pauses when application loses focus

---

## 📚 Documentation

### Core Documentation Files

Located in `Assets/Scripts/Objects/`:

- **`REFACTORING_PLAN.md`**: Complete architecture restructuring guide
- **`BUG_FIX_BUCKET_FISH_LIST.md`**: Fish data initialization timing fix
- **`SPAWN_COLLISION_FIX.md`**: Collision prevention system setup
- **`FISH_README.md`**: Fish system overview
- **`GRAB_INTERACTION_SETUP.md`**: VR grab integration guide

### API Documentation

Key classes and their responsibilities:

#### Data Layer

**`Fish.cs`** - Fish data model
```csharp
public class Fish
{
    public string color { get; }
    public int spawnedAmount { get; }
    public int caughtAmount { get; }
    
    public bool IncrementCaught(int amount = 1)
    public void DecrementCaught(int amount = 1)
    public float GetProgress()
    public bool IsAllCaught()
}
```

#### Manager Layer

**`FishSpawnManager.cs`** - Fish spawning system
```csharp
public class FishSpawnManager : MonoBehaviour
{
    public List<Fish> GetFish()
    public Fish GetFishByColor(string color)
    public int GetTotalSpawnedCount()
    public int GetTotalCaughtCount()
    public bool IsDataInitialized()
}
```

**`GameManager.cs`** - Game flow controller
```csharp
public class GameManager : MonoBehaviour
{
    public int score { get; }
    public void SetTime(int index)
}
```

#### Gameplay Layer

**`GrabbableFish.cs`** - VR grab interaction
```csharp
public class GrabbableFish : MonoBehaviour
{
    public bool IsGrabbed { get; }
    public void OnFishGrabbed()
    public void OnFishReleased()
}
```

**`FishMovement.cs`** - Autonomous fish movement
```csharp
public class FishMovement : MonoBehaviour
{
    public void StopMovement()
    public void ResumeMovement()
    public void SetSpeed(float newSpeed)
}
```

---

## 🐛 Troubleshooting

### Common Issues

**Fish not spawning**:
- Check BoxCollider is assigned in FishSpawnManager
- Verify fishPrefab array is populated
- Check spawn parameters (min/max count)

**VR grab not working**:
- Ensure Grabbable component is attached to fish prefabs
- Verify GrabbableFish script is present
- Check Meta XR SDK is properly installed

**Statistics not updating**:
- Verify FishSpawnManager reference in BucketEvent
- Check fish tags match: `redFish`, `blueFish`, `greenFish`
- Ensure bucket has trigger collider

**Performance issues**:
- Reduce max spawn count
- Lower physics update rate
- Optimize material shaders

---

## 🤝 Contributing

### How to Contribute

1. **Fork the Repository**
2. **Create Feature Branch**
   ```bash
   git checkout -b feature/AmazingFeature
   ```
3. **Commit Changes**
   ```bash
   git commit -m 'Add some AmazingFeature'
   ```
4. **Push to Branch**
   ```bash
   git push origin feature/AmazingFeature
   ```
5. **Open Pull Request**

### Contribution Guidelines

- Follow existing code style and architecture
- Add XML documentation for public APIs
- Update README.md for new features
- Test on actual Meta Quest device before PR
- Include screenshots/videos for UI changes

---

## 📄 License

This project is part of the XRLab research initiative. Please contact the repository owner for licensing information.

---

## 👥 Team

**XRLab - Meta Quest Rehabilitation Project**

- **Developer**: NariakiShipo
- **Institution**: [Your Institution]
- **Contact**: [Your Email]

---

## 🙏 Acknowledgments

- **Meta** for the Meta XR SDK
- **Unity Technologies** for the Unity Engine
- **Oculus Developer Community** for VR best practices
- **Physical Therapy Consultants** for rehabilitation guidance

---

## 📊 Project Status

- ✅ Core fishing mechanics implemented
- ✅ VR grab interaction working
- ✅ Statistics tracking functional
- ✅ Spawn collision prevention system
- 🚧 Multi-player support (planned)
- 🚧 Additional game modes (planned)
- 🚧 Progress tracking & analytics (planned)

---

## 📞 Support

For questions, issues, or suggestions:

- **GitHub Issues**: [Create an issue](https://github.com/NariakiShipo/MetaXRRehabGame/issues)
- **Discussions**: [Join discussions](https://github.com/NariakiShipo/MetaXRRehabGame/discussions)
- **Email**: [Contact maintainer]

---

**Last Updated**: October 22, 2025  
**Unity Version**: 6000.0.58f1   
**Meta XR SDK**: 78.0.0
