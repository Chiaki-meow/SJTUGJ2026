---
id: kd_0adcbcda-4b10-48de-b29d-e5ca4ec98967
type: memory
path: unity-project-understanding/in-game-state-system.md
title: in-game-state-system
inheritInjectMode: true
summaryEnabled: true
commandEnabled: false
readOnly: false
inheritAiConfig: true
createdAt: 1780752439089
updatedAt: 1780752439090
---

# in-game-state-system

## Summary
PlayerStateManager and InGameManager runtime state architecture and integration points.

<!-- locus:body:start -->
- Runtime state management is split into two singleton MonoBehaviours: `PlayerStateManager` for player resources and `InGameManager` for in-run phase/progress.
- `PlayerStateManager` lives at `Assets/Scripts/Gameplay/Player/PlayerStateManager.cs`; it stores `Physical`, `Mental`, `Health`, `MaxHealth`, exposes `IsDead`, and provides `ApplyStatChange`, `Heal`, `Damage`, `ResetState`, `OnStateChanged`, and `OnDied`.
- `InGameManager` lives at `Assets/Scripts/Gameplay/InGameManager.cs`; it stores `Phase`, `TurnCount`, `OmenCount`, `TruthRevealed`, `CurrentRoom`, `CurrentEventData`, `GameResult`, exposes `CanPlayerAct`, and provides `EnterRoom`, `StartNextTurn`, `AddOmen`, `RevealTruth`, and `EndGame`.
- `InGamePhase` currently includes `Preparation`, `Exploring`, `ResolvingRoomEvent`, `TruthRevealed`, and `GameOver`.
- `PlayerGridMovement` now has an `InGameManager` reference and prefers `InGameManager.CanPlayerAct` / `InGameManager.EnterRoom`; the old `GameFlowManager` path remains as fallback for legacy scene wiring.
- `ItemUseContext` now includes `PlayerStateManager PlayerState` and `InGameManager InGame` fields for item effect integration.
- `RoomEventEffectData` now includes `ItemData itemData` while keeping legacy `itemName`; new event content should use `itemData` for real inventory rewards.
- `RoomEventUIHandler` now resolves `InGameManager`, `PlayerStateManager`, and `PlayerInventory`; it applies `StatChange` effects to `PlayerStateManager` and `GainItem` effects with `itemData` to `PlayerInventory` after choice resolution. Event check logic is still the old 1d6 logic and has not yet been migrated to PlayerState + DiceManager.
- Developer integration documentation is at `Assets/Docs/InGameState/HowToDevREADME.md`.
<!-- locus:body:end -->
