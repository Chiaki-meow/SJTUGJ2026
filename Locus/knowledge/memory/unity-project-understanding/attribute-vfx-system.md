---
id: kd_de7d65c2-4ffe-4198-8c80-384527e077cd
type: memory
path: unity-project-understanding/attribute-vfx-system.md
title: attribute-vfx-system
inheritInjectMode: true
summaryEnabled: true
commandEnabled: false
readOnly: false
inheritAiConfig: true
createdAt: 1780754312259
updatedAt: 1780755879174
---

# attribute-vfx-system

## Summary
Attribute-change visual feedback module integration points and test scene.

<!-- locus:body:start -->
- Attribute-change visual feedback lives under `Assets/Scripts/UI/AttributeVFX/`.
- `AttributeChangeVFXController` listens to `Gameplay.PlayerStateManager.OnStateChanged`, compares cached `Physical`, `Mental`, and `Health`, and triggers VFX only when values decrease.
- `AttributeChangeVFXView` controls animation intensity and range parameters; `Mental Edge Width` drives the camera post-process edge range, while `Physical Edge Width` and `Health Edge Width` drive the blood overlay material `_EdgeWidth` per effect.
- `AttributeChangeVFXView` also exposes `Physical Edges` and `Health Edges` using the `BloodOverlayEdges` flags enum. Current defaults: `Physical Edges = All`, `Health Edges = Top`, so health damage appears only from the top edge.
- `Mental` drops call the camera post-process; `Physical` and `Health` drops drive the blood overlay Image material.
- `AttributePostProcessController` is a Built-in Render Pipeline `OnRenderImage` component for `Assets/Shaders/PostProcess/EdgeDizzinessBlur.shader`; it should be attached to a scene camera using `Assets/Materials/PostProcess/M_DizzinessBlur.mat`. Its edge width can be set by `AttributeChangeVFXView` at playback time.
- Blood overlay uses `Assets/Shaders/UI/EdgeBloodOverlay.shader`, `Assets/Materials/UI/M_BloodOverlay.mat`, and generated texture `Assets/Art/UI/Generated/T_BloodNoise.asset`; material `Edge Width` is only the fallback/default because View overrides `_EdgeWidth` during playback, and material `_EdgeMask` is set from View-selected edges.
- Reusable UI prefab is `Assets/Prefabs/UI/AttributeVFXRig.prefab`; place it under a Canvas and ensure a camera has `AttributePostProcessController`. The controller auto-finds `PlayerStateManager.Instance`; the view auto-finds `Camera.main` post-process when not manually wired.
- Dedicated test scene is `Assets/Scenes/Debug/AttributeVFXDebugScene.unity`; it contains `PlayerStateManager`, `Main Camera`, `AttributeVFXRig`, debug buttons for health/mental/physical decreases, and a reset button.
- Developer documentation is at `Assets/Docs/AttributeVFX/HowToDevREADME.md`.
<!-- locus:body:end -->
