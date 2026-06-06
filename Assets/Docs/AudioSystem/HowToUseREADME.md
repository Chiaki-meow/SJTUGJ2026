# Audio System 使用说明

本系统用于统一管理游戏音频，SFX 和 BGM 分开配置，适合按钮、房间事件、道具事件等交互快速添加音效。

## 文件位置

- SFX 枚举：`Assets/Scripts/Audio/SfxEnum.cs`
- BGM 枚举：`Assets/Scripts/Audio/BgmEnum.cs`
- 音频表：`Assets/Scripts/Audio/AudioLibrary.cs`
- 音频管理器：`Assets/Scripts/Audio/AudioManager.cs`
- 通用音效播放组件：`Assets/Scripts/Audio/AudioEventPlayer.cs`
- UI 按钮音效组件：`Assets/Scripts/UI/UIButtonSound.cs`

建议音频资源放在：

```text
Assets/Audio/Clips
Assets/Data/Audio
```

如果 Unity 里还没有对应文件夹，可以自己创建。

## 第一次配置 AudioManager

1. 在 Unity 里创建一个空物体，命名为 `AudioManager`。
2. 给它添加 `AudioManager` 组件。
3. 在 Project 面板中右键选择：`Create > Gameplay > Audio > Audio Library`。
4. 建议把资产保存为：`Assets/Data/Audio/AudioLibrary.asset`。
5. 把 `AudioLibrary.asset` 拖到 `AudioManager.audioLibrary` 字段。
6. 可以不手动创建 `AudioSource`，`AudioManager` 运行时会自动补上 `sfxSource` 和 `bgmSource`。

`AudioManager` 会自动 `DontDestroyOnLoad`，所以不要在多个场景里重复放多个 `AudioManager`。

## 添加一个新 SFX

1. 打开 `Assets/Scripts/Audio/SfxEnum.cs`。
2. 在 enum 末尾新增一个名字，例如：

```csharp
public enum SfxEnum
{
    None = 0,
    ButtonClick = 1,
    ItemPickup = 2,
    RoomEnter = 3
}
```

3. 回到 Unity 等待脚本编译完成。
4. 选中 `AudioLibrary.asset`。
5. 新增的 enum 条目会自动出现在 `sfxEntries` 列表里。
6. 把对应的 `AudioClip` 拖到 `clip` 字段。
7. 按需要调整 `volume`。

## 添加一个新 BGM

1. 打开 `Assets/Scripts/Audio/BgmEnum.cs`。
2. 在 enum 末尾新增一个名字，例如：

```csharp
public enum BgmEnum
{
    None = 0,
    MainTheme = 1,
    Battle = 2
}
```

3. 回到 Unity 等待脚本编译完成。
4. 选中 `AudioLibrary.asset`。
5. 新增的 enum 条目会自动出现在 `bgmEntries` 列表里。
6. 把对应的 `AudioClip` 拖到 `clip` 字段。
7. 按需要调整 `volume`。

注意：不要删除或重排已经在场景、Prefab、事件里使用的 enum 值。推荐只在末尾追加新值，并显式写数字。

## 给按钮添加点击音效

1. 选中 Button 物体。
2. 添加 `UIButtonSound` 组件。
3. 如果 `button` 字段为空，运行时会自动获取同物体上的 `Button`。
4. 在 `sfxId` 里选择要播放的音效，例如 `ButtonClick`。

运行时点击按钮时会自动播放 SFX。

## 给普通交互添加音效

如果是没有 Button 的物体，可以添加 `AudioEventPlayer` 组件：

1. 给目标物体添加 `AudioEventPlayer`。
2. 在 `sfxId` 里选择音效。
3. 在 UnityEvent、Timeline、Animation Event 或其他脚本里调用它的 `Play()`。

## 在代码里播放 SFX

```csharp
AudioManager.PlaySfx(SfxEnum.ItemPickup);
```

适合道具拾取、房间进入、事件结算等逻辑。

## 在代码里播放 BGM

```csharp
AudioManager.PlayBgm(BgmEnum.MainTheme);
```

停止 BGM：

```csharp
AudioManager.StopBgm();
```

SFX 使用 `sfxSource` 播放，BGM 使用 `bgmSource` 播放。

## 音量控制

`AudioManager` 上有三个音量：

- `masterVolume`：总音量
- `sfxVolume`：音效音量
- `bgmVolume`：背景音乐音量

运行时也可以调用：

```csharp
AudioManager.Instance.SetMasterVolume(0.8f);
AudioManager.Instance.SetSfxVolume(0.8f);
AudioManager.Instance.SetBgmVolume(0.6f);
```

## 快速检查清单

如果没有声音，先检查：

- 场景里是否有 `AudioManager`。
- `AudioManager.audioLibrary` 是否拖了 `AudioLibrary.asset`。
- `AudioLibrary.sfxEntries` 或 `AudioLibrary.bgmEntries` 里对应 enum 是否填了 `AudioClip`。
- 音频文件是否能正常播放。
- `masterVolume`、对应类型音量、条目 `volume` 是否大于 0。
- Button 物体上是否有 `UIButtonSound`。
- `UIButtonSound.sfxId` 是否不是 `None`。
