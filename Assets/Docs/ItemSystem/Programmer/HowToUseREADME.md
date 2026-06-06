# 物品系统程序使用说明

当前物品系统位于 `Assets/Scripts/Gameplay/Item`，命名空间为 `Gameplay`。它暂时独立于房间事件、宝箱、预兆和 UI 主流程。

## 核心结构

```text
ItemData : ScriptableObject
  静态配置：ID、名字、描述、图标、类别、堆叠、使用次数、唯一 ItemEffect 引用

ItemEffect : ScriptableObject, IItemEffect
  行为逻辑基类：CanUse / GetCannotUseReason / Use

ItemModel
  运行时物品：ItemData 引用、Amount、RemainingUses

PlayerInventory : MonoBehaviour
  运行时背包：AddItem / RemoveItem / UseItem / FindItem / Contains

ItemUseContext
  使用上下文：背包、玩家、棋盘、当前房间、当前事件、检定信息、结果消息
```

## 为什么 Effect 是 ScriptableObject

`ItemData` 不能稳定序列化接口字段，所以使用：

```csharp
public abstract class ItemEffect : ScriptableObject, IItemEffect
```

这样 `ItemData` 可以在 Inspector 里直接引用具体 `ItemEffect` asset，同时代码仍然通过统一接口调用。

注意：`ItemEffect` asset 不能存运行时状态。剩余次数、持有数量等状态必须放在 `ItemModel`。

## 实现一个新物品效果

每个具体物品可以新建一个 `.cs` 文件继承 `ItemEffect`。

```csharp
using UnityEngine;

namespace Gameplay
{
    [CreateAssetMenu(menuName = "Gameplay/Item Effect/Sedative")]
    public class SedativeItemEffect : ItemEffect
    {
        public int mentalDelta = 1;

        public override bool CanUse(ItemModel item, ItemUseContext context)
        {
            return base.CanUse(item, context);
        }

        public override void Use(ItemModel item, ItemUseContext context)
        {
            // TODO: 接入玩家属性系统后，在这里增加精神值。
            context?.SetMessage($"精神值 +{mentalDelta}");
        }
    }
}
```

之后在 Unity 里创建 `SedativeItemEffect.asset`，再拖到对应 `ItemData.effect`。

## 使用物品

```csharp
ItemUseContext context = new ItemUseContext
{
    Inventory = inventory,
    Player = player,
    Board = board,
    GameFlow = gameFlow,
    CurrentRoom = currentRoom
};

if (inventory.UseItem(itemModel, context, out string message))
{
    Debug.Log(message);
}
```

`PlayerInventory.UseItem` 会：

1. 检查物品是否在背包里。
2. 调用 `ItemEffect.CanUse`。
3. 调用 `ItemEffect.Use`。
4. 消耗 `ItemModel.RemainingUses`。
5. 用尽后从背包移除。

## 如何接入游戏流程

### 1. 给玩家加背包

在玩家 GameObject 上添加 `PlayerInventory`。如果后续有玩家状态系统，可以让状态系统和背包同挂在玩家身上，或由一个 PlayerRuntime 聚合。

### 2. 房间事件获得物品

当前 `RoomEventEffectData` 的 `GainItem` 只有 `itemName` 字符串。建议之后改为增加：

```csharp
public ItemData itemData;
```

接入点在 `RoomEventUIHandler.ResolveChoice` 和 `DebugRoomEventHandler` 对应的事件结算逻辑。事件结果确定后遍历 `RoomEventOutcomeData.effects`：

```csharp
if (effect.effectType == RoomEventEffectType.GainItem && effect.itemData != null)
{
    inventory.AddItem(effect.itemData, effect.itemAmount);
}
```

为了兼容现有内容，可以先保留 `itemName` 作为展示字段，等数据迁移后再删除。

### 3. 宝箱/预兆发放物品

宝箱房和预兆房之后可以在各自数据里直接引用 `ItemData`：

```text
ChestRoomData -> ItemData rewardItem
OmenRoomData -> ItemData omenItem
```

触发时调用：

```csharp
playerInventory.AddItem(rewardItem, amount);
```

### 4. 物品 UI 使用物品

背包 UI 应该读取：

```csharp
inventory.Items
```

点击使用按钮时创建 `ItemUseContext`，填入当前房间、当前事件、当前检定信息，然后调用：

```csharp
inventory.UseItem(item, context, out message);
```

### 5. 检定流程接入

如果物品要干预检定，检定系统需要给 `ItemUseContext` 填：

```text
CheckStat
CheckRoll
CheckTargetNumber
DiceDelta
CurrentEvent
CurrentChoice
```

检定前使用的物品可以修改 `DiceDelta` 或玩家临时属性；检定后使用的物品可以根据 `CheckRoll` 和目标值决定是否可用。

## 当前示例

`DebugMessageItemEffect` 是最小测试效果。它只打印文字，并把文字写入 `ItemUseContext.Message`。
