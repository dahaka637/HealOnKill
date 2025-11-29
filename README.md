# Heal On Kill — Technical Overview (v1.3.0)

A lightweight, event-driven life-steal system for Mount & Blade II: Bannerlord, built for simplicity, stability, and maintainability.
This document provides a clear, technical description of the mod’s architecture to assist future contributors or maintainers.

## Project Structure
```
Source/
 └── HealOnKill/
      ├── HealOnKillBehavior.cs
      ├── HealOnKillSettings.cs
      └── HealOnKillSubModule.cs
```

No Harmony patches, no overrides, no XML configuration.
All configuration is handled through MCM v5 Global Settings.

## Core Architecture

### HealOnKillSubModule.cs
- Registers the mod when loading.
- Retrieves HealOnKillSettings.Instance from MCM.
- Injects HealOnKillBehavior into each mission.

### HealOnKillSettings.cs
Defines options using MCM v5 AttributeGlobalSettings.

Options:
- EnableMod
- AllowNpcHeal
- PlayerLifeLeech
- TroopLifeLeech
- LogPlayer
- LogTroops

Automatically saved in JSON by MCM.

### HealOnKillBehavior.cs
Implements all gameplay mechanics using official Mission events.

Core formula:
```
heal = inflicted_damage * life_leech_percent
```

Handles:
- Player vs. Troop leech values
- Friendly-team detection
- Logging (optional)
- Safety checks

## Design Goals
- Stability: no patches or overrides
- Performance: minimal overhead
- Maintainability: small, clean codebase
- Modding-friendly: easy to extend

## Extending the Mod
| Feature | File | Notes |
|--------|------|-------|
| Kill-based healing | HealOnKillBehavior | Add OnAgentRemoved logic |
| Skill scaling | HealOnKillBehavior | Modify formula |
| New MCM options | HealOnKillSettings | Add attributes |
| Custom colors | HealOnKillBehavior | Modify UI calls |

## Source Code
https://github.com/dahaka637/HealOnKill
