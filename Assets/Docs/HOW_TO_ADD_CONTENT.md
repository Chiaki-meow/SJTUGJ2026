# 内容添加说明

这个文档给策划和美术使用，用来添加房间卡、房间事件和图片资源，不需要改代码。

## 文件夹位置

- 房间卡数据：`Assets/Data/RoomCard`
- 房间事件数据：`Assets/Data/RoomEvent`
- 房间图片：`Assets/Art/RoomSprites`
- UI 图片：`Assets/Art/UI`

如果 Unity 里还没有对应文件夹，可以自己创建。

## 添加一个房间事件

1. 在 Unity 的 Project 面板进入 `Assets/Data/RoomEvent`。
2. 右键选择 `Create > Gameplay > Room Event`。
3. 给 asset 起一个清楚的名字，比如 `Muttering Patient Omen`。
4. 填这些字段：
   - `Event Name`：游戏里显示的事件标题。
   - `Category`：事件类别，可选 `Event`、`ItemGain`、`Omen`。
   - `Description`：事件正文。
   - `Choices`：玩家可以选择的选项。

每个 `Choice` 的字段：

- `Label`：按钮上显示的选项文字。
- `Requires Check`：如果这个选项需要检定，就勾选。
- `Check`：只有勾选 `Requires Check` 时才会用。
- `Direct Outcome`：不需要检定时，点击选项后直接进入这个结果。
- `Success Outcome`：检定成功时进入这个结果。
- `Failure Outcome`：检定失败时进入这个结果。

每个 `Outcome` 的字段：

- `Result Text`：玩家选择后显示的结果文本。
- `Effects`：临时效果列表。目前主要用于展示文字，还没有完整接入属性和背包系统。

目前支持的效果类型：

- `StatChange`：属性变化，比如生命值、物理、精神变化。
- `GainItem`：获得道具，用道具名字填写。

## 添加一张房间卡

1. 在 Unity 的 Project 面板进入 `Assets/Data/RoomCard`。
2. 右键选择 `Create > Gameplay > Room Card`。
3. 给 asset 起一个清楚的名字，比如 `Ward_2`。
4. 填这些字段：
   - `Room Name`：房间显示名。
   - `Sprite`：房间卡图片。
   - `Event Data`：把对应的 `RoomEventData` 拖到这里。
   - `Door Count`：这个房间还能向外生成几张新房间卡。

现在 `Door Count` 的意思是：

```text
玩家站在这个房间里，朝空格方向按 WASD 时，这个房间最多能生成几张相邻新房间卡。
```

## 添加房间图片

1. 把房间图片放进 `Assets/Art/RoomSprites`。
2. 在 Unity 里选中图片。
3. 在 Inspector 里确认图片可以作为 Sprite 使用。
4. 把 Sprite 拖到对应房间卡的 `Sprite` 字段里。

推荐命名规则：

```text
RoomCardData asset 名字 == 房间 Sprite 名字
```

例子：

```text
Assets/Data/RoomCard/Ward_2.asset
Assets/Art/RoomSprites/Ward_2.png
```

## 自动按名字填写房间卡

项目里有一个编辑器工具，可以根据房间卡 asset 的名字自动填写部分字段。

使用方式：

1. 在 Project 面板里选中一个或多个 `RoomCardData` asset。
2. 点击 Unity 顶部菜单：`Tools > Room Cards > Fill Selected From Asset Names`。
3. 工具会自动处理：
   - 把 asset 名字填到 `Room Name`。
   - 从名字里的最后一个数字推断 `Door Count`。
   - 在 `Assets/Art/RoomSprites` 里查找同名 Sprite，并填到 `Sprite`。

例子：

```text
Assets/Data/RoomCard/Ward_2.asset
Assets/Art/RoomSprites/Ward_2.png
```

执行工具后：

```text
Room Name = Ward_2
Door Count = 2
Sprite = Ward_2.png
```

注意：

- 图片必须放在 `Assets/Art/RoomSprites`。
- 房间卡 asset 名字和 Sprite 名字要一致。
- 如果没有找到同名 Sprite，Unity Console 会出现 warning。
- `Event Data` 仍然需要手动拖对应的 `RoomEventData`。

## 把内容接进游戏

可抽取的房间卡列表在 `BoardManager` 上配置。

1. 打开玩法场景。
2. 选中场景里的 `BoardManager` 物体。
3. 设置：
   - `Start Room`：起始房间卡。
   - `Deck`：游戏过程中可以抽到的房间卡列表。

当玩家朝空格方向按 WASD 时，游戏会从 `Deck` 里随机抽一张房间卡并放到那个方向。

## 当前玩法流程

```text
开局只有一个起始房间
-> 玩家按 WASD
-> 目标位置已有房间：移动过去
-> 目标位置是空格：抽一张房间卡并放到那里
-> 玩家进入新放置的房间
-> 触发这个房间的事件
-> 事件结束
-> 继续移动
```

## 现在不用担心的内容

这些系统目前还没有完整实现：

- 真实背包存储
- 真实属性修改
- 最终骰子 UI
- 抽卡动画
- 完整美术版事件 UI

所以现在事件结果可以先用文字描述效果，即使游戏还没有真的扣属性或添加道具。

## 快速检查清单

如果内容没有正常出现，先检查这些：

- 房间卡是否放进了 `BoardManager.deck`。
- 房间卡是否设置了 `Event Data`。
- 事件里是否至少有一个 `Choice`。
- 场景里是否有 `GameFlowManager`。
- `GameFlowManager.roomEventHandler` 是否拖了 `DebugRoomEventHandler` 或 `RoomEventUIHandler`。
- `PlayerGridMovement.enterStartingRoomOnStart` 当前通常应该不勾选。
