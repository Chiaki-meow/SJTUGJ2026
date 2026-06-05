using UnityEngine;

namespace Gameplay
{
    [CreateAssetMenu(menuName = "Gameplay/Item Effect/Debug Message")]
    public class DebugMessageItemEffect : ItemEffect
    {
        [TextArea(2, 6)]
        public string message = "Used item.";

        public override void Use(ItemModel item, ItemUseContext context)
        {
            string itemName = item != null && item.Data != null ? item.Data.displayName : "Item";
            string finalMessage = string.IsNullOrWhiteSpace(message) ? $"Used {itemName}." : message;

            if (context != null)
            {
                context.SetMessage(finalMessage);
            }

            Debug.Log(finalMessage);
        }
    }
}
