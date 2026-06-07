---
id: kd_558b0bfe-db27-442d-b670-5f2ec2865051
type: memory
path: unity-project-understanding/room-event-system.md
title: room-event-system
inheritInjectMode: true
summaryEnabled: true
commandEnabled: false
readOnly: false
aiMaintained: true
explicitMaintenanceRules: true
createdAt: 1780721516242
updatedAt: 1780735356475
---

# room-event-system

## Summary
Room event system structure, content workflow, and current scene wiring status as observed in the project.

<!-- locus:maintain-rules:start -->
- Record only Unity project structure knowledge and lookup info that reduce repeated exploration
- Maintain only project-derived engineering understanding, including directory responsibilities, system entry points, asset relationships, runtime entry points, and config mappings
- Write user-supplied design goals, gameplay intent, product direction, and solution decisions into Design
- Prioritize directory responsibilities, core system entry points, key scenes, prefabs, ScriptableObjects, assemblies, and config mappings
- Record verified asset relationships, runtime entry points, key dependencies, and common lookup paths
- Remove temporary investigation traces, one-off task residue, unverified guesses, and expired cache
<!-- locus:maintain-rules:end -->

<!-- locus:body:start -->
- Room event data lives in `Assets/Scripts/Gameplay/Room/RoomEvent/RoomEventData.cs`; `RoomEventData` is a ScriptableObject created via `Create > Gameplay > Room Event` and stores `eventName`, `category`, `description`, and `choices`.
- `RoomCardData` in `Assets/Scripts/Gameplay/Room/RoomCardData.cs` links a room card to one `RoomEventData` via `eventData`.
- Flow: `PlayerGridMovement` enters a grid room, calls `GameFlowManager.EnterRoom`; if the `RoomCard` has not resolved its event, movement is blocked (`CanMove=false`) and `RoomEventHandler.HandleRoomEvent` is called. Completion marks `room.hasResolvedEvent=true` and re-enables movement.
- Current handlers: `RoomEventUIHandler` displays title/category/description, creates choice buttons, rolls 1d6 for checked choices, shows direct/success/failure outcome text and formatted effects, then Continue finishes. `DebugRoomEventHandler` logs a preview and finishes with Space.
- Effect data currently supports `StatChange` (`stat`, `statDelta`) and `GainItem` (`itemName`, `itemAmount`). Current UI/debug handlers format these effects as text only; real attribute mutation and inventory integration are not wired yet.
- Content workflow is documented in `Assets/Docs/HOW_TO_ADD_CONTENT.md`; event assets are under `Assets/Data/RoomEvent`, room cards under `Assets/Data/RoomCard`. Existing examples include `Debug Room Event.asset` and `Night Resident Event.asset`.
- As observed in 2026-06 session, active main UI scene `Assets/Scenes/Scene3Main.unity` contains UI layout canvases but no `BoardManager`, `GameFlowManager`, `PlayerGridMovement`, `RoomEventUIHandler`, `DebugRoomEventHandler`, `PlayerInventory`, `AudioManager`, `UIButtonSound`, or `AudioEventPlayer` components. Prototype gameplay wiring appears intended in `Assets/Scenes/CardPlacementScene.unity`, but scene/prefab script references did not match current script `.meta` GUIDs and `Assets/Prefabs/Card.prefab` showed a missing script; verify/repair scene references before assuming it is playable.
<!-- locus:body:end -->
