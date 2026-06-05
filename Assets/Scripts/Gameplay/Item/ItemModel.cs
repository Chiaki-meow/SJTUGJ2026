using System;

namespace Gameplay
{
    public class ItemModel
    {
        public ItemData Data { get; }
        public int Amount { get; private set; }
        public int RemainingUses { get; private set; }

        public bool HasLimitedUses => Data.maxUses > 0;
        public bool HasUsesRemaining => !HasLimitedUses || RemainingUses > 0;
        public bool IsEmpty => Amount <= 0;
        public ItemEffect Effect => Data.effect;

        public ItemModel(ItemData data, int amount = 1)
        {
            Data = data != null ? data : throw new ArgumentNullException(nameof(data));
            Amount = Math.Min(Math.Max(1, amount), GetMaxStack(data));
            RemainingUses = data.maxUses;
        }

        public int AddAmount(int amount)
        {
            if (amount <= 0)
                return 0;

            int accepted = Math.Min(amount, GetAvailableStackSpace());
            Amount += accepted;

            if (HasLimitedUses && RemainingUses <= 0 && Amount > 0)
            {
                RemainingUses = Data.maxUses;
            }

            return accepted;
        }

        public int RemoveAmount(int amount)
        {
            if (amount <= 0)
                return 0;

            int removed = Math.Min(amount, Amount);
            Amount -= removed;

            if (Amount <= 0)
            {
                RemainingUses = 0;
            }

            return removed;
        }

        public bool CanStackWith(ItemData data)
        {
            return Data == data && GetAvailableStackSpace() > 0;
        }

        public bool CanUse(ItemUseContext context)
        {
            ItemEffect effect = Effect;
            return effect != null && effect.CanUse(this, context);
        }

        public bool TryUse(ItemUseContext context, out string message)
        {
            ItemEffect effect = Effect;

            if (effect == null)
            {
                message = "Item has no effect.";
                return false;
            }

            if (!effect.CanUse(this, context))
            {
                message = effect.GetCannotUseReason(this, context);
                return false;
            }

            effect.Use(this, context);
            ConsumeUse();
            message = context != null ? context.Message : string.Empty;
            return true;
        }

        private void ConsumeUse()
        {
            if (!HasLimitedUses)
                return;

            RemainingUses--;

            if (RemainingUses > 0)
                return;

            Amount--;
            RemainingUses = Amount > 0 ? Data.maxUses : 0;
        }

        private int GetAvailableStackSpace()
        {
            return GetMaxStack(Data) - Amount;
        }

        private static int GetMaxStack(ItemData data)
        {
            return Math.Max(1, data.maxStack);
        }
    }
}
