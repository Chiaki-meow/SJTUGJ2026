# DicePanelPrefab 使用说明

本文档说明骰盅检定 UI 的接入方式、Prefab 结构、可调参数和 Debug 场景测试方式。

## 核心结论

当前正式使用的骰盅面板 Prefab 是：

```text
Assets/Prefabs/UI/popDice.prefab
```

运行时骰子 Prefab 是：

```text
Assets/Prefabs/UI/DiceItemView.prefab
```

`popDice` 负责显示整套检定界面，`DiceItemView` 只负责单颗骰子的点数显示。

## 文件位置

```text
Assets/Scripts/UI/Dice/DicePanelView.cs
Assets/Scripts/UI/Dice/DiceAnimatorController.cs
Assets/Scripts/UI/Dice/DiceItemView.cs
Assets/Scripts/UI/Dice/DiceCheckDebugPanel.cs
Assets/Prefabs/UI/popDice.prefab
Assets/Prefabs/UI/DiceItemView.prefab
Assets/Scenes/Debug/AttributeVFXDebugScene.unity
Assets/Docs/DicePanelPrefab/HowToUseREADME.md
```

相关 DiceSystem 逻辑位于：

```text
Assets/Scripts/Dice/DiceManager.cs
Assets/Scripts/Dice/DiceModel.cs
Assets/Scripts/Dice/DiceCheckResultModel.cs
Assets/Scripts/Dice/DiceCheckTypeEnum.cs
Assets/Scripts/Dice/DiceCompareRuleEnum.cs
```

## Prefab 结构

`popDice.prefab` 的关键结构：

```text
popDice
├─ Panel
├─ dice
│  ├─ DiceSpawnRoot
│  └─ CupLid
├─ judge
│  ├─ judgeIcon
│  ├─ judgeText
│  └─ judgeNum
├─ InstructionText
└─ ResultText
```

关键组件：

```text
popDice / DicePanelView
popDice / DiceAnimatorController
popDice / CanvasGroup
```

## 基础接入方式

1. 把 `Assets/Prefabs/UI/popDice.prefab` 放到场景中。
2. 获取 `DicePanelView` 组件引用。
3. 在需要触发检定时调用 `PlayCheck(...)`。
4. 在回调里接收玩家确认后的检定结果。

示例：

```csharp
using UI.Dice;
using UnityEngine;

public class DiceCheckExample : MonoBehaviour
{
    [SerializeField] private DicePanelView dicePanelView;

    public void StartPhysicalCheck()
    {
        dicePanelView.PlayCheck(
            DiceCheckType.Physical,
            diceCount: 3,
            difficulty: 4,
            compareRule: DiceCompareRule.GreaterOrEqual,
            allowModifier: false,
            resultConfirmed: HandleResultConfirmed);
    }

    private void HandleResultConfirmed(DiceCheckResultModel result)
    {
        Debug.Log(result.isSuccess ? "检定成功" : "检定失败");
    }
}
```

## PlayCheck 参数

```text
checkType        检定类型，影响顶部图标和文字
                  Physical -> 物理检定
                  Mental   -> 精神检定
                  Omen     -> 预兆检定

diceCount        骰子数量

difficulty       检定要求数值

compareRule      比较规则
                  GreaterOrEqual -> 总值 >= 要求值
                  LessThan       -> 总值 < 要求值
                  Equal          -> 总值 == 要求值

allowModifier    是否允许 DiceSystem 修正器逻辑

resultConfirmed  玩家看到 Success / Failure 后点击任意位置确认时触发
```

注意：`resultConfirmed` 不是摇动结束立刻触发，而是玩家点击结果界面后触发。

## 运行表现流程

```text
1. 显示透明面板和打开的骰盅
2. 展示初始骰子
3. 骰盅自动合上
4. 提示玩家按住骰盅并用力拖动
5. 第一次开始拖动时重掷一次骰子
6. 拖动过程中骰盅和骰子产生摇动位移
7. 松手且摇动距离足够后自动打开骰盅
8. 播放 Success / Failure 大字落下动画
9. 玩家点击任意位置关闭面板并确认结果
```

## DicePanelView 参数

在 `DicePanelView` 上调整：

```text
Initial Open Hold Seconds   初始打开展示多久后合上
Required Shake Distance     玩家需要累计拖动多远才算摇动成功
Dice Shake Move Scale       骰子随拖动移动的强度
Max Dice Shake Offset       单颗骰子相对初始位置的最大偏移
Dice Shake Rotation Per Pixel 骰子旋转强度
Dice Movement Padding       骰子移动区域内边距，避免落到骰盅边框上
Shake Prompt Text           合上后提示文字
Shake Again Text            摇动不足时提示文字
Click To Close Text         结果出现后的点击提示文字
```

如果正式骰盅贴图边框较厚，优先调大：

```text
Dice Movement Padding
```

这样骰子的生成位置和摇动后位置都会向内收，不会落在边框上。

## DiceAnimatorController 参数

在 `DiceAnimatorController` 上调整：

```text
Lid Closed Position   盖子闭合位置
Lid Open Position     盖子打开位置
Open Duration         打开动画时间
Close Duration        合上动画时间
Cup Curve             开合缓动曲线
Drag Move Scale       玩家拖动骰盅时的位移倍率
Max Shake Rotation    拖动时骰盅最大倾斜角
Rotation Per Pixel    拖动时骰盅旋转强度
Result Start Position Success / Failure 起始位置
Result Impact Position 落下冲击位置
Result End Position   回弹结束位置
Result Drop Duration  下落时间
Result Rebound Duration 回弹时间
Result Start Scale    起始缩放
Result Impact Scale   冲击缩放
Result End Scale      结束缩放
Result Rotation       结果文字旋转角度
Fade Duration         面板淡入淡出时间
```

当前 `popDice` 使用正交 UI 表现：`CupLid` 合上时与 `dice` 重合，盖子会正好遮住骰盅内部。

## DiceItemView 参数

`DiceItemView.prefab` 用于每颗骰子。

```text
Dice Image       骰子 Image
Value Text       可选文字显示
Value Sprites    各点数贴图
Fallback Sprite  找不到贴图时使用的默认贴图
```

当前 DiceSystem 默认骰子值为 `0 / 1 / 2`，所以 `Value Sprites` 需要至少配置 3 张图。

## Debug 场景测试

打开：

```text
Assets/Scenes/Debug/AttributeVFXDebugScene.unity
```

进入 Play Mode 后可以测试：

```text
扣生命值 -> Health 降低，触发较强血迹效果
扣精神   -> Mental 降低，触发边缘头晕模糊效果
扣物理   -> Physical 降低，触发轻度血迹效果
物理检定 -> 打开骰盅并进行物理检定
精神检定 -> 打开骰盅并进行精神检定
重置状态 -> 恢复 Physical / Mental / Health
```

骰盅测试流程：

```text
1. 点击物理检定或精神检定
2. 等待骰盅自动合上
3. 按住骰盅并拖动
4. 松手后等待打开和结果文字落下
5. 点击任意位置关闭结果界面
```

## 常见问题

### 骰子落到骰盅边框上

调大 `DicePanelView.Dice Movement Padding`。

### 骰子数量多时挤在一起

`DicePanelView` 会根据 `DiceSpawnRoot` 尺寸自动网格排布并缩小骰子。若仍然太挤，优先：

```text
1. 放大 DiceSpawnRoot
2. 减小 Dice Movement Padding
3. 减小 DiceItemView.prefab 的默认尺寸
```

### 点击后没有打开骰盅

检查：

```text
popDice 是否在场景中
DicePanelView 引用是否正确
CanvasGroup 是否存在
DiceAnimatorController 引用是否正确
GraphicRaycaster 是否存在
```

### 拖动后没有结算

检查：

```text
是否拖动的是 dice / CupRoot 区域
Required Shake Distance 是否过大
CanvasGroup.blocksRaycasts 是否在面板显示时为 true
```

### 结果没有回调

`resultConfirmed` 只有在玩家点击结果界面后触发。仅摇动完成不会立即回调。
