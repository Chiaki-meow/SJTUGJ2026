# Attribute VFX 开发接入说明

本文档说明属性变化演出模块的结构、测试方式和主流程接入方式。

## 核心结论

属性变化演出不直接修改玩家状态，只监听 `PlayerStateManager.OnStateChanged`。

当前触发规则：

```text
Mental 降低   -> 屏幕边缘头晕模糊
Physical 降低 -> 屏幕边缘轻度血迹
Health 降低   -> 屏幕边缘较强血迹
```

## 文件位置

```text
Assets/Scripts/UI/AttributeVFX/AttributeChangeVFXController.cs
Assets/Scripts/UI/AttributeVFX/AttributeChangeVFXView.cs
Assets/Scripts/UI/AttributeVFX/AttributePostProcessController.cs
Assets/Scripts/UI/AttributeVFX/AttributeVFXDebugPanel.cs
Assets/Shaders/PostProcess/EdgeDizzinessBlur.shader
Assets/Shaders/UI/EdgeBloodOverlay.shader
Assets/Materials/PostProcess/M_DizzinessBlur.mat
Assets/Materials/UI/M_BloodOverlay.mat
Assets/Art/UI/Generated/T_BloodNoise.asset
Assets/Prefabs/UI/AttributeVFXRig.prefab
Assets/Scenes/Debug/AttributeVFXDebugScene.unity
Assets/Docs/AttributeVFX/HowToDevREADME.md
```

## 运行测试 Scene

打开：

```text
Assets/Scenes/Debug/AttributeVFXDebugScene.unity
```

进入 Play Mode 后使用右侧 Debug 按钮：

```text
扣生命值 -> 测试 Health 下降血迹效果
扣精神   -> 测试 Mental 下降头晕模糊效果
扣物理   -> 测试 Physical 下降轻度血迹效果
重置状态 -> 恢复 Physical/Mental/Health 为 3/3/3
```

Scene 中有彩色测试背景，用于观察边缘模糊；右侧 UI 面板用于观察状态数值。

## 模块职责

### AttributeChangeVFXController

职责：

```text
- 订阅 PlayerStateManager.OnStateChanged
- 缓存上一次 Physical / Mental / Health
- 判断哪些属性发生下降
- 调用 AttributeChangeVFXView 播放对应演出
```

不负责：

```text
- 修改玩家属性
- 管理 UI 材质参数细节
- 管理相机后处理参数细节
```

### AttributeChangeVFXView

职责：

```text
- 播放精神下降、物理下降、生命下降的视觉动画
- 控制 Shader 强度淡入淡出
- 持有 BloodOverlayImage 引用
- 自动查找 Main Camera 上的 AttributePostProcessController
```

### AttributePostProcessController

职责：

```text
- 挂在 Camera 上
- 使用 EdgeDizzinessBlur.shader 做 Built-in Render Pipeline 下的 OnRenderImage 后处理
- 接收 View 传入的精神头晕强度
```

注意：

```text
Screen Space Overlay UI 不会被这个后处理模糊。
这符合当前需求：模糊背景与场景，不影响 Debug 面板可读性。
```

### AttributeVFXDebugPanel

仅用于测试 Scene：

```text
- 扣 Health
- 扣 Mental
- 扣 Physical
- ResetState
- 刷新状态文本
```

不要在正式玩法 UI 中依赖这个脚本。

## 主场景接入方式

后续接入主流程时，建议按以下步骤：

1. 在主场景 Camera 上添加：

```text
AttributePostProcessController
```

2. 给 `AttributePostProcessController` 设置材质：

```text
Assets/Materials/PostProcess/M_DizzinessBlur.mat
```

3. 在主 Canvas 下放入 prefab：

```text
Assets/Prefabs/UI/AttributeVFXRig.prefab
```

4. 确保场景中存在：

```text
PlayerStateManager
```

`AttributeChangeVFXController` 会自动查找 `PlayerStateManager.Instance`。如果主场景初始化较复杂，也可以在 Inspector 手动拖引用。

## 参数调整建议

在 `AttributeChangeVFXView` 上调：

```text
Mental Base Intensity      精神下降基础强度
Mental Edge Width          精神模糊影响边缘范围
Mental Fade In Duration    精神下降淡入时间
Mental Fade Out Duration   精神下降淡出时间
Physical Base Intensity    物理下降血迹基础强度
Physical Edge Width        物理下降血迹从边缘侵入范围
Physical Edges             物理下降血迹出现在哪些边，默认 All
Health Base Intensity      生命下降血迹基础强度
Health Edge Width          生命下降血迹从边缘侵入范围
Health Edges               生命下降血迹出现在哪些边，默认 Top
```

在 `AttributePostProcessController` 上调：

```text
Blur Radius     模糊半径
Distort Amount  扭曲强度
Tint Color      眩晕偏色
Time Scale      波动速度
```

在 `M_BloodOverlay.mat` 上调：

```text
Blood Color   血色
Noise Scale   血迹噪声密度
Drip Amount   向下渗出的程度
```

血迹范围由 `AttributeChangeVFXView` 的 `Physical Edge Width` / `Health Edge Width` 驱动，材质上的 `Edge Width` 只作为材质默认值保留。

## 常见调参

### 调整扣血 / 扣物理时的红色范围

选中场景内或 prefab 内的：

```text
AttributeVFXRig
```

在 `AttributeChangeVFXView` 上调整：

```text
Physical Edge Width    扣物理时的血迹范围
Health Edge Width      扣生命值时的血迹范围
```

效果：

```text
数值越大，红色从屏幕边缘往中间扩得越多。
数值越小，红色越贴近屏幕边缘。
```

### 调整扣血 / 扣物理时红色出现在哪些边

选中场景内或 prefab 内的：

```text
AttributeVFXRig
```

在 `AttributeChangeVFXView` 上调整：

```text
Physical Edges    扣物理时出现血迹的边，默认 All
Health Edges      扣生命值时出现血迹的边，默认 Top
```

可选边：

```text
Left
Right
Top
Bottom
All
None
```

当前默认：

```text
扣物理：All，四周都会有轻度血迹。
扣血：Top，只有屏幕上方有血迹。
```

### 调整扣精神时的模糊范围

选中场景内或 prefab 内的：

```text
AttributeVFXRig
```

在 `AttributeChangeVFXView` 上调整：

```text
Mental Edge Width
```

效果：

```text
数值越大，精神下降时的边缘模糊覆盖范围越大。
数值越小，模糊越集中在屏幕边缘。
```

### 调整扣血 / 扣物理时的红色浓淡

选中场景内或 prefab 内的：

```text
AttributeVFXRig
```

在 `AttributeChangeVFXView` 上调整：

```text
Physical Base Intensity    扣物理时的血迹强度
Health Base Intensity      扣生命值时的血迹强度
```

效果：

```text
Physical Base Intensity 只影响 Physical 下降。
Health Base Intensity 只影响 Health 下降。
```

## 事件/道具接入

只要通过 `PlayerStateManager` 修改属性，就会自动触发演出：

```csharp
PlayerStateManager.Instance.ApplyStatChange(CharacterStat.Mental, -1);
PlayerStateManager.Instance.ApplyStatChange(CharacterStat.Physical, -1);
PlayerStateManager.Instance.Damage(1);
```

如果直接改字段或绕过 `PlayerStateManager`，不会触发演出。

## 注意事项

```text
- 当前模块只处理属性下降，属性上升不会播放演出。
- 血迹使用 UI Image，显示在 Canvas 上。
- 扣血默认只显示屏幕上方血迹；扣物理默认四周显示，可在 `AttributeChangeVFXView` 的 Edges 字段调整。
- 精神模糊使用 Camera 后处理，只适用于 Built-in Render Pipeline 当前方案。
- AttributeVFXDebugPanel 只用于 Debug Scene，不要接入正式流程。
```
