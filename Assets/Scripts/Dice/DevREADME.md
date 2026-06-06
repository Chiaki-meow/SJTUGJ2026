# DevREADME

## 项目基本信息

- Unity：2022.3.62f2c1；Built-in Render Pipeline；Legacy Input Manager。
- 当前主要开发分支：`dice_basic`。

## 快速启动

1. 用 Unity Hub 打开项目根目录，打开 `Assets/Scenes/SampleScene.unity`。
2. 等待脚本编译完成，确认 Console 没有编译错误。

## 目录结构

```text
Assets/
  Scenes/
    SampleScene.unity
  Scripts/
    Dice/
      DiceManager.cs
      DiceModel.cs
      DiceModifierModel.cs
      DiceCheckResultModel.cs
      DiceCheckTypeEnum.cs
      DiceCompareRuleEnum.cs
      DiceModifierTypeEnum.cs
      DiceModifierTagEnum.cs
      DiceModifierCostTypeEnum.cs
Packages/
ProjectSettings/
```

## Dice 系统概览

Dice 系统目前是纯 C# 逻辑层，不继承 `MonoBehaviour`，也不依赖场景对象。默认检定骰符合策划案：每颗骰子的面值为 `0 / 1 / 2`，骰子数量由调用方通过 `diceCount` 传入。

### `DiceManager`

位置：`Assets/Scripts/Dice/DiceManager.cs`

负责一次骰子检定流程：创建骰池、发起检定、投掷与重掷、修改点数、应用 Modifier、统计点数、判断成败、生成结果、通知外部系统。

常用入口：

```csharp
DiceManager diceManager = new DiceManager();
diceManager.StartCheck(DiceCheckType.Physical, 3, 4, DiceCompareRule.GreaterOrEqual, true);
DiceCheckResultModel result = diceManager.ConfirmCheckResult();
```

常用函数：

- `StartCheck(DiceCheckType checkType, int diceCount, int difficulty, DiceCompareRule compareRule, bool allowModifier)`：发起普通检定。
- `StartOmenCheck(int omenCount, int diceCount)`：发起预兆检定。
- `ConfirmCheckResult()`：确认并结算检定。
- `CancelCurrentCheck()`：取消当前检定。
- `CreateDicePool(int diceCount, DiceCheckType checkType)`：创建默认 `0 / 1 / 2` 面值骰池。
- `CreateDicePool(int diceCount, int minValue, int maxValue, DiceCheckType checkType)`：创建范围骰池。
- `CreateDicePool(int diceCount, List<int> faces, DiceCheckType checkType)`：创建指定面值骰池。
- `CreateDice(int minValue, int maxValue, string sourceId)`：创建范围骰子。
- `CreateDice(List<int> faces, string sourceId)`：创建指定面值骰子。
- `AddDiceToCurrentPool(DiceModel dice)`：向当前骰池加骰。
- `RemoveDiceFromCurrentPool(DiceModel dice)`：从当前骰池移除骰子。
- `GetDiceById(string diceId)`：按 ID 获取骰子。
- `RollAllDices()`：投掷全部骰子。
- `RollDice(DiceModel dice)`：投掷指定骰子。
- `RollDiceById(string diceId)`：按 ID 投掷骰子。
- `RerollDice(DiceModel dice)`：重掷指定骰子。
- `RerollDiceById(string diceId)`：按 ID 重掷骰子。
- `RerollAllUnlockedDices()`：重掷所有未锁定骰子。
- `CanRerollDice(DiceModel dice)`：判断骰子能否重掷。
- `SetDiceValue(DiceModel dice, int value)`：设置骰子点数。
- `SetDiceValueById(string diceId, int value)`：按 ID 设置点数。
- `AddDiceValue(DiceModel dice, int value)`：增加骰子点数。
- `AddDiceValueById(string diceId, int value)`：按 ID 增加点数。
- `LockDice(DiceModel dice)`：锁定指定骰子。
- `UnlockDice(DiceModel dice)`：解锁指定骰子。
- `LockAllDices()`：锁定全部骰子。
- `UnlockAllDices()`：解锁全部骰子。
- `GetAvailableModifiers()`：获取可用 Modifier。
- `CanUseModifier(DiceModifierModel modifier)`：判断 Modifier 可用性。
- `ApplyModifier(DiceModifierModel modifier, DiceModel targetDice)`：应用 Modifier。
- `GetUsedModifierCount(string modifierId)`：获取 Modifier 使用次数。
- `GetTotalValue()`：获取当前总点数。
- `GetRawTotalValue()`：获取原始总点数。
- `GetFinalTotalValue()`：获取最终总点数。
- `RefreshFinalTotalValue()`：刷新最终总点数。
- `CheckSuccess()`：判断检定是否成功。
- `CalculateMargin()`：计算成功或失败差值。
- `GenerateCheckResult()`：生成检定结果。
- `GetCurrentDiceList()`：获取当前骰子列表。

### `DiceModel`

位置：`Assets/Scripts/Dice/DiceModel.cs`

表示单颗骰子，只负责自身数据与随机投掷：唯一 ID、范围骰或指定面值骰、当前/上一点数、投掷状态、锁定状态、重掷权限、来源 ID 与备注。

常用函数：

- `Init()`：初始化默认骰子。
- `Init(int minValue, int maxValue, string sourceId)`：按范围初始化。
- `Init(List<int> faces, string sourceId)`：按面值初始化。
- `Reset()`：重置骰子状态。
- `Roll(Random random)`：投掷骰子。
- `RollByRange(Random random)`：按范围随机点数。
- `RollByFaces(Random random)`：按面值随机点数。
- `SetValue(int value)`：设置当前点数。
- `AddValue(int value)`：增加当前点数。
- `SetPreviousValue(int value)`：设置上一点数。
- `RestorePreviousValue()`：恢复上一点数。
- `Lock()`：锁定骰子。
- `Unlock()`：解锁骰子。
- `SetCanReroll(bool value)`：设置能否重掷。
- `MarkRolled()`：标记已投掷。
- `MarkUnrolled()`：标记未投掷。
- `CanRoll()`：判断能否投掷。
- `CanReroll()`：判断能否重掷。
- `IsLocked()`：判断是否锁定。
- `HasRolled()`：判断是否已投掷。
- `GetCurrentValue()`：获取当前点数。
- `GetPreviousValue()`：获取上一点数。
- `GetDiceId()`：获取骰子 ID。
- `GetSourceId()`：获取来源 ID。

### `DiceModifierModel`

位置：`Assets/Scripts/Dice/DiceModifierModel.cs`

描述一次骰子修正效果，例如：

- 重掷
- 加点
- 改点
- 增加骰子
- 降低难度
- 消耗属性或道具

当前成本支付逻辑还未接入角色属性、物品或货币系统。

### `DiceCheckResultModel`

位置：`Assets/Scripts/Dice/DiceCheckResultModel.cs`

保存一次检定的最终结果：

- 检定类型
- 是否成功
- 原始总点数
- 最终总点数
- 难度
- 差值
- 最终骰子列表
- 已使用 Modifier 列表

## 枚举说明

- `DiceCheckType`：检定类型，包含 Physical、Mental、Omen、Custom。
- `DiceCompareRule`：比较规则，包含 GreaterOrEqual、LessThan、Equal。
- `DiceModifierType`：Modifier 效果类型。
- `DiceModifierTag`：Modifier 标签，用于筛选当前检定可用项。
- `DiceModifierCostType`：Modifier 成本类型。

## 当前开发约定

- Dice 脚本暂时不使用 namespace。
- Dice 逻辑层保持纯 C#，不要直接依赖 UI、场景对象或 `MonoBehaviour`。
- 如需接入 UI，请在外部 Controller 或 View 中持有 `DiceManager`。
- 如需接入角色属性、道具或资源系统，应优先通过外部服务或回调完成成本判断和支付。
- 不要把场景表现、动画、音效逻辑写进 `DiceModel` 或 `DiceManager`。

## 建议测试点

1. 创建 `DiceManager`，发起 3 个 `0 / 1 / 2` 骰大于等于 4 的检定。
2. 确认 `StartCheck` 后骰子数量正确，所有骰子已投掷。
3. 调用 `SetDiceValue`、`AddDiceValue` 后确认总点数刷新。
4. 锁定骰子后确认不能重掷。
5. 调用 `ConfirmCheckResult` 后确认成功失败和 margin 符合预期。

## Git 注意事项

- 不提交 `Library/`、`Temp/`、`Obj/`、`Logs/`、构建产物。
- 不手动删除或重建 `.meta` 文件，避免 Unity 资源 GUID 变化。
- 修改脚本后在 Unity 中等待编译完成，再进行运行测试。
