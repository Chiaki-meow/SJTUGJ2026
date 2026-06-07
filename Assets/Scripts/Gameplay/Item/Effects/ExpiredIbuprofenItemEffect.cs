using UnityEngine;

namespace Gameplay
{
    [CreateAssetMenu(menuName = "Gameplay/Item Effect/Expired Ibuprofen")]
    public class ExpiredIbuprofenItemEffect : ItemEffect
    {
        [TextArea(2, 6)]
        public string fullHealMessage = "药效意外地稳定了下来。生命值已回满。";
        [TextArea(2, 6)]
        public string smallHealMessage = "你又撑住了一会儿。生命值 +1。";
        [TextArea(2, 6)]
        public string damageMessage = "过期药物带来剧烈反噬。生命值 -3。";

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
            int roll = Random.Range(0, 3);
            if (roll == 2)
            {
                context.PlayerState.Heal(context.PlayerState.MaxHealth);
                context.SetMessage(fullHealMessage);
            }
            else if (roll == 1)
            {
                context.PlayerState.Heal(1);
                context.SetMessage(smallHealMessage);
            }
            else
            {
                context.PlayerState.Damage(3);
                context.SetMessage(damageMessage);
            }
        }
    }
}
