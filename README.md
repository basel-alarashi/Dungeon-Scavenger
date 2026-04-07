# 🗡️ Dungeon Scavenger

![Unity Version](https://img.shields.io/badge/Unity-2022.3+-blue?logo=unity)
![Status](https://img.shields.io/badge/Status-In%20Development-green)
![Genre](https://img.shields.io/badge/Genre-Dungeon%20Crawler-orange)

## 📖 Overview

**Dungeon Scavenger** is a dungeon-crawling adventure game where players explore procedurally generated dungeons, scavenge for valuable loot, and survive against dangerous creatures. Every decision matters as you balance risk vs. reward in your quest to escape with your hard-earned treasures.

> *"Enter the darkness, take what you can, and pray you make it out alive."*

## 🎮 Key Features

- **Procedural Dungeons** - No two runs are the same with dynamically generated layouts
- **Risk/Reward Scavenging** - Push deeper for better loot or escape early with what you've found
- **Inventory Management** - Limited space forces tough decisions about what to keep
- **Permanent Upgrades** - Between runs, spend your scavenged gold on lasting improvements
- **Enemy Variety** - Face unique creatures with distinct behaviors and attack patterns

## 🎯 Gameplay Loop
Enter Dungeon → Explore & Scavenge → Fight or Flee → Escape → Upgrade → Repeat

## 🛠️ Tech Stack

| Category | Technology |
|----------|------------|
| Engine | Unity (2022.3 LTS) |
| Language | C# |
| Design Patterns | Component-Based, Scriptable Objects, Event System |
| Version Control | Git |
| Platform | PC (Windows/Mac/Linux) |

## 📁 Project Structure
Assets/
├── Scripts/ # Core game logic & behaviors
│ ├── Player/ # Movement, combat, inventory
│ ├── Enemies/ # AI behaviors, stats, spawning
│ ├── Dungeon/ # Generation, rooms, tiles
│ └── UI/ # Menus, HUD, tooltips
├── Prefabs/ # Reusable game objects
├── Scenes/ # Main menu, gameplay, UI scenes
├── ScriptableObjects/ # Data containers (items, enemies, upgrades)
└── Art/ # Sprites, animations, VFX


## 🚀 Getting Started

### Prerequisites
- Unity Hub & Unity 2022.3 LTS or newer
- Git (optional, for cloning)

### Installation

1. **Clone the repository**
```
bash
git clone https://github.com/yourusername/dungeon-scavenger.git
```
2. **Open in Unity Hub**

Click "Add" → Select the project folder

Choose Unity 2022.3 LTS

3. **Open the main scene**

Navigate to Assets/Scenes/MainMenu.unity

Press Play ▶️

🎮 Controls
Action	Input
Move	WASD / Arrow Keys
Interact	E
Attack	Left Mouse / Space
Use Item	1-4 Keys
Inventory	Tab
Pause	Esc

🎓 What This Project Demonstrates
Procedural Generation - Dynamic level creation using BSP and random walk algorithms

Data-Driven Design - Extensive use of ScriptableObjects for modular content creation

Event-Driven Architecture - Loose coupling via C# events and UnityEvents

State Management - Finite state machines for enemy AI and game states

Optimization - Object pooling, occlusion culling, and efficient pathfinding

🔧 Development Status
Core movement & combat

Basic dungeon generation

Inventory system

Enemy AI (basic)

Procedural loot tables

Boss encounters

Save/Load system

Sound & music integration

Full release polishing
