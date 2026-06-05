using UnityEngine;

namespace Gameplay
{
    public enum ItemCategory
    {
        Normal,
        Omen,
        Ending,
        Key
    }

    [CreateAssetMenu(menuName = "Gameplay/Item/Item Data")]
    public class ItemData : ScriptableObject
    {
        public string itemId;
        public string displayName;

        [TextArea(2, 8)]
        public string description;

        public Sprite icon;
        public ItemCategory category;

        [Min(1)]
        public int maxStack = 1;

        [Tooltip("0 means unlimited uses.")]
        [Min(0)]
        public int maxUses = 1;

        public ItemEffect effect;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                itemId = name;
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = name;
            }

            maxStack = Mathf.Max(1, maxStack);
            maxUses = Mathf.Max(0, maxUses);
        }
    }
}
