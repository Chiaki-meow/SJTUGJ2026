---
id: kd_bf166940-8abf-417e-9646-3d523ca09ebf
type: memory
path: unity-project-understanding/item-system.md
title: item-system
inheritInjectMode: true
summaryEnabled: true
commandEnabled: false
readOnly: false
inheritAiConfig: true
createdAt: 1780695231861
updatedAt: 1780695231862
---

# item-system

## Summary
Item system script locations, responsibilities, and planned gameplay integration points.

<!-- locus:body:start -->
- Item system scripts live under `Assets/Scripts/Gameplay/Item` in namespace `Gameplay`.
- `ItemData` is a `ScriptableObject` created from `Create > Gameplay > Item > Item Data`; it stores static item configuration: `itemId`, `displayName`, `description`, `icon`, `category`, `maxStack`, `maxUses`, and one `ItemEffect` asset reference.
- `IItemEffect` defines `CanUse`, `GetCannotUseReason`, and `Use`; `ItemEffect : ScriptableObject, IItemEffect` is the abstract Inspector-serializable base for concrete item behavior assets.
- `ItemModel` is the runtime inventory item model and owns mutable state: `Amount` and `RemainingUses`. It delegates behavior to `Data.effect` and consumes uses after successful use.
- `PlayerInventory : MonoBehaviour` owns runtime items and supports `AddItem`, `RemoveItem`, `UseItem`, `FindItem`, and `Contains`; `startingItems` are converted to runtime `ItemModel`s in `Awake`.
- `ItemUseContext` carries optional runtime context for effects: inventory, player movement, board, game flow, current room/event/choice, and check-related fields.
- `DebugMessageItemEffect` under `Assets/Scripts/Gameplay/Item/Effects` is the minimal test effect asset type (`Create > Gameplay > Item Effect > Debug Message`).
- Designer docs are at `Assets/Docs/ItemSystem/Designer/HowToUseREADME.md`; programmer docs and planned integration steps are at `Assets/Docs/ItemSystem/Programmer/HowToUseREADME.md`.
- Gameplay flow is not wired yet. Planned integration: add `PlayerInventory` to player; replace/extend `RoomEventEffectData.GainItem` string flow with `ItemData` references; event handlers call `inventory.AddItem`; item UI calls `inventory.UseItem` with an `ItemUseContext`; dice/check integration fills check fields in `ItemUseContext`.
<!-- locus:body:end -->
