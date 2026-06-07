---
id: kd_34b50ebc-20e0-4559-8ea2-1fac5322e757
type: memory
path: unity-project-understanding/audio-system.md
title: audio-system
inheritInjectMode: true
summaryEnabled: true
commandEnabled: false
readOnly: false
inheritAiConfig: true
createdAt: 1780723245994
updatedAt: 1780724700684
---

# audio-system

## Summary
Project audio system uses separate SfxEnum and BgmEnum mappings in AudioLibrary, a persistent AudioManager with separate SFX/BGM sources, and button helper components.

<!-- locus:body:start -->
# audio-system

- Audio scripts live under `Assets/Scripts/Audio`: `SfxEnum`, `BgmEnum`, `AudioLibrary`, `AudioManager`, and `AudioEventPlayer`.
- `SfxEnum` and `BgmEnum` are code-maintained enums. Add new IDs at the end with explicit numeric values to avoid breaking serialized references.
- `AudioLibrary` is a `ScriptableObject` created from `Create > Gameplay > Audio > Audio Library`; it stores separate `sfxEntries` (`SfxEnum -> AudioClip + volume`) and `bgmEntries` (`BgmEnum -> AudioClip + volume`) and auto-adds missing enum entries in `OnValidate`.
- `AudioManager` is a persistent `DontDestroyOnLoad` singleton. It has separate `sfxSource` and `bgmSource`, and exposes `PlaySfx(SfxEnum)`, `PlayBgm(BgmEnum)`, `StopBgm()`, and volume setters. Scenes should contain only one configured `AudioManager` object.
- `UI.UIButtonSound` can be attached to a Unity `Button` and automatically plays its selected `SfxEnum` on click.
- `AudioEventPlayer` is a general-purpose SFX component whose `Play()` can be called by UnityEvent, animation events, or scripts.
- User-facing setup guide is at `Assets/Docs/AudioSystem/HowToUseREADME.md`.

Editor was disconnected when this system was added/modified, so no `AudioLibrary.asset`, scene object, prefab, or Unity compilation/import verification was created in those tasks.
<!-- locus:body:end -->
