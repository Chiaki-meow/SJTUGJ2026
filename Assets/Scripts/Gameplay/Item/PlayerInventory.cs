using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay
{
    public class PlayerInventory : MonoBehaviour
    {
        public List<StartingItemStack> startingItems = new();

        private readonly List<ItemModel> items = new();

        public IReadOnlyList<ItemModel> Items => items;

        private void Awake()
        {
            for (int i = 0; i < startingItems.Count; i++)
            {
                StartingItemStack stack = startingItems[i];
                if (stack != null && stack.data != null && stack.amount > 0)
                {
                    AddItem(stack.data, stack.amount);
                }
            }
        }

        public ItemModel AddItem(ItemData data, int amount = 1)
        {
            if (data == null || amount <= 0)
                return null;

            ItemModel firstChangedItem = null;
            int remaining = amount;

            for (int i = 0; i < items.Count && remaining > 0; i++)
            {
                ItemModel item = items[i];
                if (!item.CanStackWith(data))
                    continue;

                int accepted = item.AddAmount(remaining);
                remaining -= accepted;

                if (firstChangedItem == null)
                {
                    firstChangedItem = item;
                }
            }

            int maxStack = Math.Max(1, data.maxStack);

            while (remaining > 0)
            {
                int stackAmount = Math.Min(maxStack, remaining);
                ItemModel item = new ItemModel(data, stackAmount);
                items.Add(item);
                remaining -= stackAmount;

                if (firstChangedItem == null)
                {
                    firstChangedItem = item;
                }
            }

            return firstChangedItem;
        }

        public bool RemoveItem(ItemData data, int amount = 1)
        {
            if (data == null || amount <= 0)
                return false;

            if (!Contains(data, amount))
                return false;

            int remaining = amount;

            for (int i = items.Count - 1; i >= 0 && remaining > 0; i--)
            {
                ItemModel item = items[i];
                if (item.Data != data)
                    continue;

                int removed = item.RemoveAmount(remaining);
                remaining -= removed;

                if (item.IsEmpty)
                {
                    items.RemoveAt(i);
                }
            }

            return true;
        }

        public bool UseItem(ItemModel item, ItemUseContext context, out string message)
        {
            if (item == null || !items.Contains(item))
            {
                message = "Item is not in this inventory.";
                return false;
            }

            if (context == null)
            {
                context = new ItemUseContext();
            }

            context.Inventory = this;

            bool used = item.TryUse(context, out message);

            if (item.IsEmpty)
            {
                items.Remove(item);
            }

            return used;
        }

        public ItemModel FindItem(ItemData data)
        {
            if (data == null)
                return null;

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Data == data)
                    return items[i];
            }

            return null;
        }

        public bool Contains(ItemData data, int amount = 1)
        {
            if (data == null || amount <= 0)
                return false;

            int count = 0;

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Data != data)
                    continue;

                count += items[i].Amount;

                if (count >= amount)
                    return true;
            }

            return false;
        }
    }

    [Serializable]
    public class StartingItemStack
    {
        public ItemData data;
        public int amount = 1;
    }
}
