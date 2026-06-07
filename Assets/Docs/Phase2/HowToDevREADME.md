# 二阶段流程开发接入说明

本文档给程序、UI 和内容同学使用，说明二阶段如何接入当前局内流程。目标是多人并行开发时只通过清晰接口对接，避免把院长战、地图、背包、房间事件写死在同一个脚本里。

## 核心结论

二阶段由独立组件 `Phase2Director` 管理。

```text
Phase2Director = 二阶段流程状态、院长室生成、耐心、院长战结果结算
InGameManager = 通用局内阶段和房间进入/结算事件
BoardManager = 地图房间放置
PlayerInventory = 判断关键道具
PlayerStateManager = 扣血和死亡
```

不要把二阶段逻辑写进 `RoomEventUIHandler` 或 `BoardManager`。

## 文件位置

```text
Assets/Scripts/Gameplay/Phase2/Phase2Director.cs
Assets/Scripts/Gameplay/Phase2/DeanBossEncounterData.cs
Assets/Docs/Phase2/HowToDevREADME.md
```

相关改动：

```text
Assets/Scripts/Gameplay/BoardManager.cs
Assets/Scripts/Gameplay/InGameManager.cs
```

## 新增公共接口

### BoardManager.TryPlaceFixedRoom

用于把院长室直接生成到地图固定坐标。

```csharp
public bool TryPlaceFixedRoom(RoomCardData card, Vector2Int gridPosition, out RoomCard placedRoom)
```

特点：

```text
- 不消耗 deck
- 不消耗来源房间门数
- 如果目标坐标已有房间，会返回 false
```

### InGameManager 房间事件

`InGameManager` 新增两个通用事件：

```csharp
public event Action<RoomCard> OnRoomEntered;
public event Action<RoomCard> OnRoomResolved;
```

用途：

```text
OnRoomEntered = 进入未结算房间时触发
OnRoomResolved = 房间事件 Continue 后触发
```

`Phase2Director` 使用 `OnRoomResolved` 统计二阶段后玩家继续探索的新房间，用来扣院长耐心或增加难度。

## Phase2Director 场景接入

推荐在玩法场景中建立：

```text
GameRuntime
├─ InGameManager
└─ Phase2Director
```

`Phase2Director` Inspector 字段：

```text
Board Manager：场景里的 BoardManager
In Game Manager：场景里的 InGameManager
Player Inventory：玩家身上的 PlayerInventory
Player State Manager：玩家身上的 PlayerStateManager
Dean Office Room Card：院长室 RoomCardData
Dean Office Grid Position：院长室生成坐标，默认右上角暂定为 (3, 3)
Bloody Knife Item：染血的手术刀 ItemData
Patient Letter Item：患者的遗书 ItemData
Patient Diary Item：患者的日记 ItemData
Bloody Knife Initial Patience：默认 3
Patient Letter Initial Patience：默认 7
```

如果没有手动拖引用，`Phase2Director.Start()` 会尝试用 `FindObjectOfType` 和单例查找，但正式场景建议手动拖好。

## 触发二阶段

真相揭露失败时调用：

```csharp
phase2Director.TriggerTruthRevealFailed();
```

该方法会：

```text
1. 根据背包关键道具选择路线
2. 初始化院长耐心
3. 直接在 Dean Office Grid Position 生成院长室
4. 进入 Phase2State.Active
```

路线判断：

```text
持有染血的手术刀 -> Phase2Route.BloodyKnife
持有患者的遗书 -> Phase2Route.PatientLetter
都没有 -> 不开启二阶段
```

如果测试或特殊流程要指定路线，可以直接调用：

```csharp
phase2Director.BeginPhase2(Phase2Route.BloodyKnife);
phase2Director.BeginPhase2(Phase2Route.PatientLetter);
```

## 院长室生成规则

院长室不是加入下一次抽卡队列，而是直接生成到地图固定坐标：

```csharp
boardManager.TryPlaceFixedRoom(deanOfficeRoomCard, deanOfficeGridPosition, out room);
```

默认坐标暂定：

```text
(3, 3)
```

坐标暴露在 `Phase2Director.deanOfficeGridPosition`，后续可在 Inspector 中调整。

注意：如果该坐标已有房间，生成会失败并输出 warning。需要测试地图布局时，优先调整 `Dean Office Grid Position`。

## 院长耐心机制

二阶段开启后：

```text
武力路线初始耐心 = 3
共情路线初始耐心 = 7
```

规则：

```text
每结算一个新的非院长室房间：
- 当前耐心 > 0：耐心 -1
- 当前耐心 == 0：DifficultyBonus +1
```

`Phase2Director` 会忽略触发二阶段的当前房间，避免“刚触发二阶段就扣一次耐心”。

获取当前修正后的检定目标：

```csharp
int target = phase2Director.GetCheckTarget(baseTarget);
```

例如武力路线第二次检定基础目标是 9，当前 `DifficultyBonus = 2` 时实际目标是 11。

## 院长战数据

院长战文本和基础数值放在 `DeanBossEncounterData`：

```text
Create > Gameplay > Phase 2 > Dean Boss Encounter
```

默认字段：

```text
Bloody Knife Check Targets = [8, 9]
Patient Letter Check Targets = [10, 5]
Failure Damage = 1
Required Success Count = 2
```

读取当前第几次检定的基础目标：

```csharp
int baseTarget = encounterData.GetTarget(phase2Director.CurrentRoute, phase2Director.SuccessfulDeanChecks);
int target = phase2Director.GetCheckTarget(baseTarget);
```

## 院长战 UI 接入建议

UI 不要自己维护二阶段状态，只读取 `Phase2Director`。

伪代码：

```csharp
Phase2Route route = phase2Director.CurrentRoute;
int successIndex = phase2Director.SuccessfulDeanChecks;
int baseTarget = encounterData.GetTarget(route, successIndex);
int target = phase2Director.GetCheckTarget(baseTarget);

// route == BloodyKnife -> Physical 检定
// route == PatientLetter -> Mental 检定
```

共情路线持有患者日记时，第一次精神检定可跳过：

```csharp
if (successIndex == 0 && phase2Director.ShouldSkipFirstPatientLetterCheck())
{
    phase2Director.RegisterDeanCheckSuccess(encounterData.requiredSuccessCount);
}
```

检定成功：

```csharp
bool isFinished = phase2Director.RegisterDeanCheckSuccess(encounterData.requiredSuccessCount);
```

检定失败：

```csharp
phase2Director.RegisterDeanCheckFailure(encounterData.failureDamage);
```

当成功次数达到 `requiredSuccessCount`：

```text
Phase2Director 会调用 InGameManager.EndGame(true)
```

当失败扣血导致玩家死亡：

```text
Phase2Director / PlayerStateManager / InGameManager 会进入失败结局
```

## 和骰子系统的关系

骰子系统本体已经使用 0/1/2 骰面。

```text
DiceManager 默认检定骰面 = 0, 1, 2
```

但普通房间事件当前仍在 `RoomEventUIHandler` 内用旧的 `Random.Range(1, 7)`。后续接入院长战或普通事件检定时，应走统一检定入口，不要继续新增 1d6 逻辑。

建议后续新增 `CheckResolver`，统一处理：

```text
Physical / Mental 数值 = 骰子数量
DicePanelView 播放骰盅 UI
DiceManager 返回最终结果
道具加骰或加结果
```

## 内容同学接入清单

需要准备这些资产：

```text
ItemData：染血的手术刀
ItemData：患者的遗书
ItemData：患者的日记
RoomCardData：院长室
DeanBossEncounterData：院长战文本和数值
```

现有 `Muttering Patient Omen.asset` 里奖励仍可能是 `itemName` 字符串。正式接入时应改为 `RoomEventEffectData.itemData` 引用真实 `ItemData`，否则 `PlayerInventory.Contains(...)` 无法判断路线。

## 程序分工建议

### 真相系统

负责在真相揭露失败时调用：

```csharp
phase2Director.TriggerTruthRevealFailed();
```

### 地图系统

只需要维护 `BoardManager.TryPlaceFixedRoom`，不要知道院长室剧情。

### 院长战 UI

负责：

```text
- 展示 DeanBossEncounterData 文本
- 根据 Phase2Director.CurrentRoute 选择物理/精神检定
- 调用 Phase2Director.RegisterDeanCheckSuccess / RegisterDeanCheckFailure
```

### 道具/背包

负责确保关键道具进入 `PlayerInventory`，并使用真实 `ItemData`。

## 快速测试方式

1. 在场景中添加并配置 `Phase2Director`。
2. 给玩家背包添加 `bloodyKnifeItem` 或 `patientLetterItem`。
3. 运行时调用：

```csharp
FindObjectOfType<Phase2Director>().TriggerTruthRevealFailed();
```

4. 检查地图固定坐标是否生成院长室。
5. 继续结算非院长室房间，检查 `CurrentPatience` 和 `DifficultyBonus`。
6. 用院长战 UI 或调试代码调用成功/失败接口，检查胜负流程。

## 注意事项

- `Phase2Director` 只管理流程，不直接显示 UI。
- `DeanBossEncounterData` 只存配置，不存运行时状态。
- 院长室固定坐标被占用时不会覆盖旧房间。
- 如果玩家同时持有手术刀和遗书，`TriggerTruthRevealFailed()` 当前优先走手术刀路线；特殊测试可用 `BeginPhase2(route)` 指定路线。
