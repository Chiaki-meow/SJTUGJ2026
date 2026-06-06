# PlayerState / InGameManager 开发接入说明

本文档给程序和后续 agent 使用，说明局内玩家状态与局内流程管理的职责划分、接入方式和注意事项。

## 核心结论

当前项目使用两个单例管理局内运行状态：

```text
PlayerStateManager = 玩家资源状态
InGameManager = 局内流程阶段
```

不要把玩家属性、回合、预兆、事件流程、胜负都塞进同一个管理器。

## 文件位置

```text
Assets/Scripts/Gameplay/Player/PlayerStateManager.cs
Assets/Scripts/Gameplay/InGameManager.cs
Assets/Docs/InGameState/HowToDevREADME.md
```

相关已有系统：

```text
Assets/Scripts/Gameplay/PlayerGridMovement.cs
Assets/Scripts/Gameplay/Item/PlayerInventory.cs
Assets/Scripts/Gameplay/Item/ItemUseContext.cs
Assets/Scripts/Gameplay/Room/RoomEvent/RoomEventData.cs
Assets/Scripts/UI/RoomEventUIHandler.cs
```

## PlayerStateManager

`PlayerStateManager` 管玩家局内数值资源。

### 当前职责

```text
- Physical
- Mental
- Health
- MaxHealth
- IsDead
- ApplyStatChange(CharacterStat, int)
- Heal(int)
- Damage(int)
- ResetState(int, int, int)
- OnStateChanged
- OnDied
```

### 不负责

```text
- 回合数
- 预兆数量
- 真相揭露
- 当前房间
- 当前事件
- 胜负流程
- 背包列表
```

背包由 `PlayerInventory` 负责。

### 使用方式

```csharp
PlayerStateManager.Instance.ApplyStatChange(CharacterStat.Physical, -1);
PlayerStateManager.Instance.Heal(1);
PlayerStateManager.Instance.Damage(1);
```

监听 UI 刷新：

```csharp
private void OnEnable()
{
    PlayerStateManager.Instance.OnStateChanged += Refresh;
}

private void OnDisable()
{
    if (PlayerStateManager.Instance != null)
    {
        PlayerStateManager.Instance.OnStateChanged -= Refresh;
    }
}
```

死亡时会触发 `OnDied`。`InGameManager` 会监听它并进入失败流程。

## InGameManager

`InGameManager` 管局内阶段与流程。

### 当前阶段枚举

```csharp
public enum InGamePhase
{
    Preparation,
    Exploring,
    ResolvingRoomEvent,
    TruthRevealed,
    GameOver
}
```

### 当前职责

```text
- 当前阶段 Phase
- 是否允许玩家行动 CanPlayerAct
- 当前回合 TurnCount
- 预兆数量 OmenCount
- 是否真相揭露 TruthRevealed
- 当前房间 CurrentRoom
- 当前事件 CurrentEventData
- EnterRoom(Vector2Int)
- AddOmen(int)
- RevealTruth()
- EndGame(bool)
```

### 不负责

```text
- 具体属性如何变化
- 背包堆叠与使用次数
- UI 文本显示
- 房间卡抽取细节
```

房间卡抽取仍由 `BoardManager` 负责。

## 单例初始化规则

两个管理器都使用 `Instance`。

```text
Awake: 只设置 Instance
Start: 查找其他组件、订阅事件
OnDestroy: 取消订阅、清空 Instance
```

不要在 `Awake` 里写互相强依赖逻辑，避免 Unity 脚本初始化顺序导致空引用。

## 场景接入方式

推荐在玩法场景中建立：

```text
GameRuntime
├─ InGameManager

Player
├─ PlayerGridMovement
├─ PlayerStateManager
└─ PlayerInventory
```

`InGameManager` Inspector 建议拖入：

```text
BoardManager
RoomEventHandler，例如 RoomEventUIHandler
PlayerGridMovement
PlayerStateManager
PlayerInventory
```

如果没有手动拖引用，`InGameManager.Start()` 会用 `FindObjectOfType` 做一次兜底查找。

## 移动接入

`PlayerGridMovement` 已新增 `inGameManager` 引用。

移动前会优先检查：

```csharp
if (inGameManager != null && !inGameManager.CanPlayerAct)
    return;
```

进入房间时优先调用：

```csharp
inGameManager.EnterRoom(gridPosition);
```

`GameFlowManager` 暂时保留为旧流程兜底。后续确认场景全部迁移到 `InGameManager` 后，可以删除或废弃 `GameFlowManager`。

## 房间事件接入

当前事件流程：

```text
PlayerGridMovement
-> InGameManager.EnterRoom(gridPosition)
-> phase = ResolvingRoomEvent
-> RoomEventHandler.HandleRoomEvent(...)
-> RoomEventUIHandler 选择 outcome
-> 应用 outcome.effects
-> Continue
-> InGameManager ResolveCurrentRoom
-> phase = Exploring 或 TruthRevealed
```

## 事件效果接入

`RoomEventEffectData` 当前支持：

```text
StatChange
GainItem
```

### StatChange

`RoomEventUIHandler` 会调用：

```csharp
playerStateManager.ApplyStatChange(effect.stat, effect.statDelta);
```

支持：

```text
Physical
Mental
Health
```

### GainItem

`RoomEventEffectData` 新增：

```csharp
public ItemData itemData;
```

`RoomEventUIHandler` 会调用：

```csharp
playerInventory.AddItem(effect.itemData, effect.itemAmount);
```

`itemName` 暂时保留，主要用于旧数据展示。新内容应优先填写 `itemData`。

## 道具接入

`ItemUseContext` 已新增：

```csharp
public PlayerStateManager PlayerState;
public InGameManager InGame;
```

后续物品效果中可以这样修改属性：

```csharp
context.PlayerState.ApplyStatChange(CharacterStat.Mental, 1);
```

创建道具使用上下文时建议填：

```csharp
ItemUseContext context = new ItemUseContext
{
    Inventory = playerInventory,
    PlayerState = playerStateManager,
    Player = playerMovement,
    Board = boardManager,
    InGame = inGameManager,
    CurrentRoom = inGameManager.CurrentRoom,
    CurrentEvent = inGameManager.CurrentEventData
};
```

## 预兆接入

当前 `InGameManager.ResolveCurrentRoom()` 中，如果当前事件类别是 `Omen`，会调用：

```csharp
AddOmen();
```

真相揭露检定还没有完整接入。之后应在预兆事件结算后执行：

```text
按策划规则掷预兆骰
如果触发，则 InGameManager.RevealTruth()
```

## 检定接入注意

当前 `RoomEventUIHandler` 仍使用旧的 `1d6` 检定逻辑。后续接入策划案规则时，应改为：

```text
Physical / Mental 数值 = 骰子数量
骰子面 = 0, 1, 2
总和与 targetNumber 比较
```

建议通过现有 `DiceManager` 实现，不要把掷骰逻辑写进 `PlayerStateManager`。

## UI 接入建议

后续建议新增：

```text
Assets/Scripts/UI/PlayerStateUI.cs
Assets/Scripts/UI/InGameStateUI.cs
```

UI 只监听状态变化并刷新显示，不直接推进流程。

推荐显示：

```text
PlayerStateUI:
- Health / MaxHealth
- Physical
- Mental

InGameStateUI:
- Phase
- TurnCount
- OmenCount
- TruthRevealed
```

## 后续清理建议

迁移稳定后可以处理：

```text
1. 废弃或删除 GameFlowManager
2. 把事件检定从 1d6 改为 PlayerState + DiceManager
3. 给 Scene3Main 接入 InGameManager / PlayerStateManager / PlayerInventory
4. 修复 Card prefab missing script 与旧场景脚本 GUID 问题
5. 为 PlayerStateManager 和 InGameManager 增加 UI 绑定脚本
```
