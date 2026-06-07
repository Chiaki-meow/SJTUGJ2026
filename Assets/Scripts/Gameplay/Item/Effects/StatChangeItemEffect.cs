using UnityEngine;

namespace Gameplay
{
    [CreateAssetMenu(menuName = "Gameplay/Item Effect/Stat Change")]
    public class StatChangeItemEffect : ItemEffect
    {
        public CharacterStat stat;
        public int delta = 1;

        [TextArea(2, 6)]
        public string successMessage;

        public override bool CanUse(ItemModel item, ItemUseContext context)
        {
            return base.CanUse(item, context) && context != null && context.PlayerState != null;
        }

        public override string GetCannotUseReason(ItemModel item, ItemUseContext context)
        {
            if (!base.CanUse(item, context))
                return base.GetCannotUseReason(item, context);

            if (context == null || context.PlayerState == null)
                return "缺少玩家状态，无法使用。";

            return string.Empty;
        }

        public override void Use(ItemModel item, ItemUseContext context)
        {
            context.PlayerState.ApplyStatChange(stat, delta);

            if (context != null)
            {
                context.SetMessage(string.IsNullOrWhiteSpace(successMessage) ? FormatDefaultMessage() : successMessage);
            }
        }

        private string FormatDefaultMessage()
        {
            string sign = delta > 0 ? "+" : string.Empty;
            return $"{stat} {sign}{delta}";
        }
    }
}
