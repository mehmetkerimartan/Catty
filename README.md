# 🐱 Catty - Reality Tear Platformer

A unique 3D isometric platformer where a cat navigates through dual realities.

## 🎮 Gameplay

- **WASD** - Movement (isometric perspective)
- **Space** - Jump
- **Shift** - Sprint
- **Right Click** - Reality Tear (reveals the hidden "Hell" layer)

## ✨ Core Mechanics

### Reality Tear
Hold right-click to reveal an alternate reality layer. This mechanic:
- Creates a radar-like visual effect expanding from the player
- Reveals hidden platforms and objects in the "Hell" dimension
- Consumes charges (limited uses per level)
- Slows down time while active

### Dual World Objects
Objects can exist in two states:
- **Normal World** - The visible, beautiful reality
- **Hell World** - The hidden truth revealed by Reality Tear

### Environmental Hazards
- **Ice Ground** - Slippery surfaces with momentum-based movement
- **Wind Zones** - Push the player in specified directions
- **Sinking Platforms** - Platforms that sink when stepped on
- **Death Zones** - Instant death areas

### Collectibles & Progression
- **Hearts** - Restore health
- **Coins** - Currency for the shop
- **Checkpoints** - Save progress
- **Level Portals** - Travel between levels

## 🏪 Shop System
Visit the ShopKeeper to purchase perks using collected coins.

## 🎨 Visual Effects
- Reality tear with shader-based edge effects
- Particle systems for tear boundaries
- Camera shake on impacts
- Trail effects on player movement
- Pixelation and post-processing effects

## 📁 Project Structure

```
Assets/
└── Scripts/
    ├── Player/
    │   └── PlayerController.cs    # Character movement & input
    ├── Core/
    │   ├── GameManager.cs         # Lives, health, respawn
    │   ├── RealityManager.cs      # Reality Tear mechanic
    │   └── HUDManager.cs          # UI updates
    ├── Mechanics/
    │   ├── IceGround.cs           # Ice friction
    │   ├── WindZone.cs            # Wind push effect
    │   ├── SinkingPlatform.cs     # Sinking platforms
    │   └── DualWorldObject.cs     # Dual reality objects
    ├── Pickups/
    │   ├── HeartPickup.cs         # Health restore
    │   ├── CoinPickup.cs          # Currency
    │   └── Checkpoint.cs          # Save points
    ├── UI/
    │   ├── FairyUI.cs             # Dialogue system
    │   └── ShopUI.cs              # Shop interface
    └── Effects/
        ├── RealityParticles.cs    # Tear particles
        ├── CameraShake.cs         # Screen shake
        └── PlayerTrail.cs         # Movement trail
```

## 🛠️ Technical Details

- **Engine**: Unity (C#)
- **Perspective**: Isometric 3D (45° rotation)
- **Input**: CharacterController-based movement
- **Design Patterns**: Singleton (GameManager, RealityManager)

## 📝 Development Notes

This project features a unique "Reality Tear" mechanic inspired by games that play with perception and hidden layers. The dual-world system allows for creative level design where players must use their limited tear charges strategically.

---

*Made with Unity* 🎮
