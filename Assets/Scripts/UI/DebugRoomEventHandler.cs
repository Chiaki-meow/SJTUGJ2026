using System;
using System.Text;
using UnityEngine;

namespace Gameplay
{
    public class DebugRoomEventHandler : RoomEventHandler
    {
        public KeyCode finishKey = KeyCode.Space;

        private Action onFinished;
        private RoomCard activeRoom;

        public override void HandleRoomEvent(RoomCard room, RoomEventData eventData, Action finishedCallback)
        {
            activeRoom = room;
            onFinished = finishedCallback;

            string roomName = room.data != null ? room.data.roomName : room.name;
            string eventName = eventData != null ? eventData.eventName : "No Event Data";
            Debug.Log($"Started room event: {roomName} / {eventName}. Press {finishKey} to finish.", room);

            if (eventData != null)
            {
                Debug.Log(BuildEventPreview(eventData), room);
            }
        }

        private void Update()
        {
            if (onFinished == null || !Input.GetKeyDown(finishKey))
                return;

            Action finishedCallback = onFinished;
            onFinished = null;

            string roomName = activeRoom != null && activeRoom.data != null ? activeRoom.data.roomName : "Room";
            Debug.Log($"Finished room event: {roomName}.", activeRoom);

            activeRoom = null;
            finishedCallback.Invoke();
        }

        private static string BuildEventPreview(RoomEventData eventData)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"[{eventData.category}] {eventData.eventName}");
            builder.AppendLine(eventData.description);

            if (eventData.choices == null || eventData.choices.Length == 0)
            {
                builder.AppendLine("No choices configured.");
                return builder.ToString();
            }

            for (int i = 0; i < eventData.choices.Length; i++)
            {
                RoomEventChoiceData choice = eventData.choices[i];
                builder.AppendLine();
                builder.AppendLine($"{i + 1}. {choice.label}");

                if (choice.requiresCheck)
                {
                    builder.AppendLine($"   Check: {FormatCheck(choice.check)}");
                    builder.AppendLine($"   Success: {FormatOutcome(choice.successOutcome)}");
                    builder.AppendLine($"   Failure: {FormatOutcome(choice.failureOutcome)}");
                }
                else
                {
                    builder.AppendLine($"   Result: {FormatOutcome(choice.directOutcome)}");
                }
            }

            return builder.ToString();
        }

        private static string FormatCheck(RoomEventCheckData check)
        {
            if (check == null)
                return "None";

            string comparison = check.comparison == DiceComparison.GreaterThan ? ">" : ">=";
            return $"{check.stat} {comparison} {check.targetNumber}";
        }

        private static string FormatOutcome(RoomEventOutcomeData outcome)
        {
            if (outcome == null)
                return "None";

            StringBuilder builder = new StringBuilder(outcome.resultText);

            if (outcome.effects == null || outcome.effects.Length == 0)
                return builder.ToString();

            builder.Append(" Effects:");

            foreach (RoomEventEffectData effect in outcome.effects)
            {
                builder.Append(' ');
                builder.Append(FormatEffect(effect));
            }

            return builder.ToString();
        }

        private static string FormatEffect(RoomEventEffectData effect)
        {
            if (effect == null)
                return "None";

            if (effect.effectType == RoomEventEffectType.GainItem)
                return $"GainItem({effect.itemName} x{effect.itemAmount})";

            return $"StatChange({effect.stat} {effect.statDelta:+#;-#;0})";
        }
    }
}
