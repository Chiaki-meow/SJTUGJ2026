using UnityEngine;

namespace Gameplay
{
    [CreateAssetMenu(menuName = "Gameplay/Item Effect/Passive Item")]
    public class PassiveItemEffect : ItemEffect
    {
        [TextArea(2, 6)]
        public string cannotUseReason = "这是剧情关键道具，无法主动使用。";

        public override bool CanUse(ItemModel item, ItemUseContext context)
        {
            return false;
        }

        public override string GetCannotUseReason(ItemModel item, ItemUseContext context)
        {
            return string.IsNullOrWhiteSpace(cannotUseReason) ? base.GetCannotUseReason(item, context) : cannotUseReason;
        }

        public override void Use(ItemModel item, ItemUseContext context)
        {
            if (context != null)
            {
                context.SetMessage(GetCannotUseReason(item, context));
            }
        }
    }
}
