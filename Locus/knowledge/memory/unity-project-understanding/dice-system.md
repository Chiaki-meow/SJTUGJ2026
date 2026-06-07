---
id: kd_24560eef-15dc-4304-a030-6bd8adf405d8
type: memory
path: unity-project-understanding/dice-system.md
title: dice-system
inheritInjectMode: true
summaryEnabled: true
commandEnabled: false
readOnly: false
aiMaintained: true
explicitMaintenanceRules: true
createdAt: 1779967131405
updatedAt: 1780760014428
---

# dice-system

## Summary
Dice system script locations, responsibilities, default 0/1/2 check dice behavior, and popDice dice cup UI integration points.

<!-- locus:maintain-rules:start -->
- Record only Unity project structure knowledge and lookup info that reduce repeated exploration
- Maintain only project-derived engineering understanding, including directory responsibilities, system entry points, asset relationships, runtime entry points, and config mappings
- Write user-supplied design goals, gameplay intent, product direction, and solution decisions into Design
- Prioritize directory responsibilities, core system entry points, key scenes, prefabs, ScriptableObjects, assemblies, and config mappings
- Record verified asset relationships, runtime entry points, key dependencies, and common lookup paths
- Remove temporary investigation traces, one-off task residue, unverified guesses, and expired cache
<!-- locus:maintain-rules:end -->

<!-- locus:body:start -->
- `Assets/Scripts/Dice/DevREADME.md` is the quick onboarding document for Dice developers and summarizes project setup, Dice script layout, conventions, suggested tests, and Git notes. It must stay under 200 lines per user request.
- `Assets/Scripts/Dice/DiceModel.cs` contains the plain C# single-die model for dice IDs, faces/ranges, roll state, value edits, locking, and reroll eligibility.
- `Assets/Scripts/Dice/DiceManager.cs` contains the plain C# dice check flow and references dice models/enums from separate scripts.
- `DiceManager.StartCheck(DiceCheckType checkType, int diceCount, int difficulty, DiceCompareRule compareRule, bool allowModifier)` keeps dice quantity parameterized through `diceCount`.
- The default `DiceManager.CreateDicePool(int diceCount, DiceCheckType checkType)` now creates check dice with faces `0 / 1 / 2`, matching the current design plan. Custom range dice and custom face dice overloads still exist.
- `DiceManager.StartOmenCheck(int omenCount, int diceCount)` uses the same default `0 / 1 / 2` dice through `StartCheck`, with `diceCount` supplied by the caller.
- `DiceManager.ApplyAddDiceModifier` defaults newly added modifier dice to range `0..2` unless modifier params override `minValue` / `maxValue`.
- `Assets/Scripts/Dice/DiceModifierModel.cs` contains modifier data and usage-count/parameter helpers.
- `Assets/Scripts/Dice/DiceCheckResultModel.cs` contains the final check result data model.
- Dice-related enum scripts use an `Enum.cs` filename suffix: `DiceCheckTypeEnum.cs`, `DiceCompareRuleEnum.cs`, `DiceModifierTypeEnum.cs`, `DiceModifierTagEnum.cs`, and `DiceModifierCostTypeEnum.cs` under `Assets/Scripts/Dice`.
- The dice scripts currently do not use a namespace and are not `MonoBehaviour` components.
- Dice cup UI presentation lives under `Assets/Scripts/UI/Dice/`: `DicePanelView` owns one check presentation and confirmation callback, `DiceAnimatorController` exposes cup open/close, drag, shake rotation, fade, and result-drop animation parameters, `DiceItemView` displays each die value, and `DiceCheckDebugPanel` wires debug buttons.
- The active dice cup UI prefab is now `Assets/Prefabs/UI/popDice.prefab`; it has root `popDice` with `CanvasGroup`, `DiceAnimatorController`, and `DicePanelView`. Its `dice` child is both the cup root/container, with `dice/CupLid` as the lid and `dice/DiceSpawnRoot` as the runtime dice spawn parent.
- `Assets/Scenes/Scene3Main.unity` contains a top-level `popDice` object adapted with the same Dice cup components and child structure as `Assets/Prefabs/UI/popDice.prefab`.
- Legacy generated prefabs `Assets/Prefabs/UI/DicePanelView.prefab` and `Assets/Prefabs/UI/DiceItemView.prefab` still exist; `DiceItemView.prefab` is used as the runtime die item prefab, while `popDice.prefab` is the intended panel layout.
- Default placeholder sprites are `Assets/Art/UI/Generated/S_DefaultCircle.png` and `Assets/Art/UI/Generated/S_DefaultSquare.png`.
- `Assets/Scenes/Debug/AttributeVFXDebugScene.unity` contains a `Canvas/DicePanelView` prefab instance and added `Canvas/DebugPanel/Button_DicePhysical`, `Button_DiceMental`, and `DiceResultText` for dice cup smoke testing.
- `DicePanelView.PlayCheck(...)` creates and owns a `DiceManager`, subscribes to `onDiceRolled` for display, previews the result after shake via `GenerateCheckResult`, and calls `ConfirmCheckResult` only when the player clicks to dismiss the result screen.
- Current dice cup behavior: after initial roll display, closing the cup enables dragging; drag movement permanently changes `CupRoot.anchoredPosition` until the panel is closed or a new check begins; the first drag after a check starts calls `DiceManager.RollAllDices()` once, so visible dice and final result change once before confirmation.
- During drag, `DicePanelView.MoveDiceDuringShake` applies lightweight RectTransform offsets and rotation to each spawned `DiceItemView`, clamped by `maxDiceShakeOffset`; this is intentionally not physics collision.
- `DicePanelView` lays out spawned dice from the actual `DiceSpawnRoot.rect`, auto-scales dice down when count is high, clamps them inside an inner movement area controlled by serialized `diceMovementPadding`, and runs a small pairwise separation pass during shake so dice do not visually overlap or land on the cup frame.
- Current `popDice` visual direction: `dice` and `dice/CupLid` are circular `Image`s with matching size. Lid closed position is `(0, 0)` relative to `dice`, so it fully covers the cup in orthographic UI; `ResultText` is a large black rotated text with start/impact/end positions and rebound timing exposed on `DiceAnimatorController`.
<!-- locus:body:end -->
