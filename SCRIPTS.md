# 📜 Script Documentation - Catty

Quick reference for all game scripts and their responsibilities.

---

## 🎮 Core Systems

| Script | Lines | Description |
|--------|-------|-------------|
| **GameManager.cs** | 204 | Singleton. Handles lives, health, coins, respawning, and game state |
| **RealityManager.cs** | 189 | Singleton. Controls Reality Tear mechanic with charge system and slow-motion |
| **HUDManager.cs** | ~100 | Updates UI elements (health, lives, coins, tear charges) |
| **PerkManager.cs** | ~100 | Manages purchased perks and their effects |

---

## 🐱 Player

| Script | Lines | Description |
|--------|-------|-------------|
| **PlayerController.cs** | 204 | WASD movement, jump, sprint. Isometric 45° input handling. Ice/wind integration |
| **PlayerTrail.cs** | ~60 | Visual trail effect following player movement |

---

## 🌍 Mechanics

| Script | Lines | Description |
|--------|-------|-------------|
| **DualWorldObject.cs** | ~120 | Objects with two states (Normal/Hell). Switches based on tear radius |
| **IceGround.cs** | ~50 | Static class. Makes surfaces slippery with momentum |
| **WindZone.cs** | ~90 | Pushes player in specified direction. Affects air movement |
| **SinkingPlatform.cs** | ~130 | Platforms that sink when player stands on them |
| **DeathZone.cs** | ~35 | Trigger that kills player on contact |
| **SecretWall.cs** | ~120 | Walls revealed only during Reality Tear |

---

## 🎁 Pickups & Progression

| Script | Lines | Description |
|--------|-------|-------------|
| **HeartPickup.cs** | ~40 | Restores player health |
| **CoinPickup.cs** | ~70 | Currency with spin animation |
| **Checkpoint.cs** | ~45 | Saves respawn position when activated |
| **LevelPortal.cs** | ~55 | Transitions to next level |

---

## 🏪 Shop System

| Script | Lines | Description |
|--------|-------|-------------|
| **ShopKeeper.cs** | ~110 | NPC that opens shop on interaction |
| **ShopUI.cs** | ~170 | Shop interface for buying perks |

---

## 🎨 Visual Effects

| Script | Lines | Description |
|--------|-------|-------------|
| **RealityParticles.cs** | 157 | Particles on tear edge. Horizontal XZ spread |
| **RealityOverlay.cs** | ~115 | Screen overlay during tear |
| **RealityPostProcessing.cs** | ~85 | Post-processing effects for tear |
| **RealitySkybox.cs** | ~90 | Skybox changes during tear |
| **CameraShake.cs** | ~65 | Screen shake utility |
| **CameraFollow.cs** | ~45 | Smooth camera follow with offset |
| **PixelationEffect.cs** | ~40 | Retro pixelation shader control |
| **HaloEffect.cs** | ~110 | Glowing halo around objects |
| **DustParticles.cs** | ~95 | Ambient dust particles |
| **TearSparkles.cs** | ~100 | Sparkle effects |
| **BackgroundTransition.cs** | ~55 | Background color transitions |

---

## 🧚 UI & Dialogue

| Script | Lines | Description |
|--------|-------|-------------|
| **FairyUI.cs** | 235 | Hades-style dialogue with portrait. Typewriter effect |
| **CatReaction.cs** | ~140 | Cat facial expressions and reactions |

---

## 🔧 Design Patterns Used

1. **Singleton** - GameManager, RealityManager, FairyUI (Instance property)
2. **Observer** - Reality tear state updates all DualWorldObject instances
3. **Component** - Unity MonoBehaviour-based modular design

---

## 📊 Statistics

- **Total Scripts**: 31
- **Total Lines (estimated)**: ~3,500
- **Average Script Size**: ~110 lines
- **Largest Script**: FairyUI.cs (235 lines)

---

*Generated for Catty project documentation*
