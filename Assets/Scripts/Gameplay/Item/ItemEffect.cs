using UnityEngine;

namespace Gameplay
{
    public interface IItemEffect
    {
        bool CanUse(ItemModel item, ItemUseContext context);
        string GetCannotUseReason(ItemModel item, ItemUseContext context);
        void Use(ItemModel item, ItemUseContext context);
    }

    public abstract class ItemEffect : ScriptableObject, IItemEffect
    {
        public virtual bool CanUse(ItemModel item, ItemUseContext context)
        {
            return item != null && item.Amount > 0 && item.HasUsesRemaining;
        }

        public virtual string GetCannotUseReason(ItemModel item, ItemUseContext context)
        {
            if (item == null)
                return "No item selected.";

            if (item.Amount <= 0)
                return "Item stack is empty.";

            if (!item.HasUsesRemaining)
                return "Item has no remaining uses.";

            return string.Empty;
        }

        public abstract void Use(ItemModel item, ItemUseContext context);
    }
}
