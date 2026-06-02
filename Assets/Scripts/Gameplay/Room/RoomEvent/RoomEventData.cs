using System;
using UnityEngine;

namespace Gameplay
{
    public enum RoomEventCategory
    {
        Event,
        ItemGain,
        Omen
    }

    public enum CharacterStat
    {
        Physical,
        Mental,
        Health
    }

    public enum DiceComparison
    {
        GreaterThan,
        GreaterThanOrEqual
    }

    public enum RoomEventEffectType
    {
        StatChange,
        GainItem
    }

    [CreateAssetMenu(menuName = "Gameplay/Room Event")]
    public class RoomEventData : ScriptableObject
    {
        public string eventName;

        public RoomEventCategory category;

        [TextArea(4, 10)]
        public string description;

        public RoomEventChoiceData[] choices;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(eventName))
            {
                eventName = name;
            }
        }
    }

    [Serializable]
    public class RoomEventChoiceData
    {
        public string label;
        public bool requiresCheck;
        public RoomEventCheckData check;
        public RoomEventOutcomeData directOutcome;
        public RoomEventOutcomeData successOutcome;
        public RoomEventOutcomeData failureOutcome;
    }

    [Serializable]
    public class RoomEventCheckData
    {
        public CharacterStat stat;
        public DiceComparison comparison = DiceComparison.GreaterThanOrEqual;
        public int targetNumber = 1;
    }

    [Serializable]
    public class RoomEventOutcomeData
    {
        [TextArea(2, 8)]
        public string resultText;

        public RoomEventEffectData[] effects;
    }

    [Serializable]
    public class RoomEventEffectData
    {
        public RoomEventEffectType effectType;
        public CharacterStat stat;
        public int statDelta;
        public string itemName;
        public int itemAmount = 1;
    }
}
