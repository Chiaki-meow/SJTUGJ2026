# 物品系统策划使用说明

当前物品系统已经实现为独立模块，暂时还没有接入房间事件、宝箱、预兆和 UI 主流程。

## 文件夹建议

- 物品数据：`Assets/Data/Item/Data`
- 物品效果：`Assets/Data/Item/Effect`
- 物品图标：`Assets/Art/ItemIcons`

如果 Unity 里没有这些文件夹，可以自己创建。

## 创建一个物品

1. 在 Project 面板进入 `Assets/Data/Item/Data`。
2. 右键选择 `Create > Gameplay > Item > Item Data`。
3. 给 asset 起清楚的名字，比如 `Sedative`。
4. 填写字段：
   - `Item Id`：唯一 ID，建议英文，不要重复。
   - `Display Name`：游戏里显示的名字。
   - `Description`：游戏里显示的描述。
   - `Icon`：物品图标。
   - `Category`：物品类别。
   - `Max Stack`：最大堆叠数量。
   - `Max Uses`：每个物品可用次数，`0` 表示无限次。
   - `Effect`：拖入这个物品对应的效果 asset。

## 创建一个效果 asset

效果逻辑由程序写好。策划只需要创建对应的效果 asset，然后拖到 `ItemData.effect`。

1. 在 Project 面板进入 `Assets/Data/Item/Effect`。
2. 右键选择 `Create > Gameplay > Item Effect > ...`。
3. 选择程序已经提供的具体效果类型。
4. 把创建出的效果 asset 拖到对应 `ItemData` 的 `Effect` 字段。

当前示例效果：

- `Debug Message`：使用物品时在 Console 打印一段文字，用于测试物品系统。

## 把初始物品放进玩家背包

1. 打开玩法场景。
2. 选中玩家 GameObject。
3. 添加或找到 `PlayerInventory` 组件。
4. 在 `Starting Items` 里添加元素。
5. 把 `ItemData` 拖到 `Data` 字段，填写 `Amount`。

进入 Play Mode 后，`PlayerInventory` 会在 `Awake` 时把这些物品转成运行时背包物品。

## 现在还不能直接做的事

当前系统还没有接入主流程，所以这些功能之后需要程序接入：

- 房间事件奖励真正加入背包。
- 宝箱房真正发放普通物品。
- 预兆房真正发放预兆物品。
- 物品 UI 显示背包并点击使用。
- 检定前/检定中使用道具影响骰子结果。

## 接入主流程后的策划流程

之后接入完成后，策划通常只需要：

1. 创建 `ItemData`。
2. 创建或选择对应 `ItemEffect` asset。
3. 把 `ItemEffect` 拖到 `ItemData.effect`。
4. 在房间事件、宝箱或预兆配置里引用 `ItemData`。
5. 进 Play Mode 测试获得、显示、使用效果是否正确。
