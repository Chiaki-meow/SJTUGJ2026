using System;
using System.Text;
using Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class RoomEventUIHandler : RoomEventHandler
    {
        public GameObject panelRoot;
        public TMP_Text titleText;
        public TMP_Text categoryText;
        public TMP_Text descriptionText;
        public TMP_Text resultText;
        public Transform choicesParent;
        public Button choiceButtonPrefab;
        public Button continueButton;

        private Action onFinished;

        private void Awake()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            if (continueButton != null)
            {
                continueButton.onClick.AddListener(FinishEvent);
            }
        }

        private void OnDestroy()
        {
            if (continueButton != null)
            {
                continueButton.onClick.RemoveListener(FinishEvent);
            }
        }

        public override void HandleRoomEvent(RoomCard room, RoomEventData eventData, Action finishedCallback)
        {
            onFinished = finishedCallback;

            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }

            SetText(titleText, eventData != null ? eventData.eventName : "Unknown Event");
            SetText(categoryText, eventData != null ? eventData.category.ToString() : "None");
            SetText(descriptionText, eventData != null ? eventData.description : "This room has no event data.");
            SetText(resultText, string.Empty);

            SetContinueVisible(false);
            ClearChoices();

            if (eventData == null || eventData.choices == null || eventData.choices.Length == 0)
            {
                SetText(resultText, "No choices configured.");
                SetContinueVisible(true);
                return;
            }

            if (choiceButtonPrefab == null || choicesParent == null)
            {
                Debug.LogWarning("RoomEventUIHandler needs a choice button prefab and choices parent.", this);
                SetText(resultText, "Choice UI is not configured.");
                SetContinueVisible(true);
                return;
            }

            for (int i = 0; i < eventData.choices.Length; i++)
            {
                RoomEventChoiceData choice = eventData.choices[i];
                CreateChoiceButton(choice);
            }
        }

        private void CreateChoiceButton(RoomEventChoiceData choice)
        {
            if (choiceButtonPrefab == null || choicesParent == null)
                return;

            Button button = Instantiate(choiceButtonPrefab, choicesParent);
            TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
            SetText(buttonText, choice.label);
            button.onClick.AddListener(() => ResolveChoice(choice));
        }

        private void ResolveChoice(RoomEventChoiceData choice)
        {
            ClearChoices();

            RoomEventOutcomeData outcome = choice.directOutcome;
            string checkSummary = string.Empty;

            if (choice.requiresCheck)
            {
                int roll = UnityEngine.Random.Range(1, 7);
                bool success = IsCheckSuccessful(roll, choice.check);
                outcome = success ? choice.successOutcome : choice.failureOutcome;
                checkSummary = $"Roll: {roll} / {FormatCheck(choice.check)} / {(success ? "Success" : "Failure")}\n\n";
            }

            SetText(resultText, checkSummary + FormatOutcome(outcome));
            SetContinueVisible(true);
        }

        private static bool IsCheckSuccessful(int roll, RoomEventCheckData check)
        {
            if (check == null)
                return true;

            if (check.comparison == DiceComparison.GreaterThan)
                return roll > check.targetNumber;

            return roll >= check.targetNumber;
        }

        private void FinishEvent()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            Action finishedCallback = onFinished;
            onFinished = null;
            finishedCallback?.Invoke();
        }

        private void ClearChoices()
        {
            if (choicesParent == null)
                return;

            for (int i = choicesParent.childCount - 1; i >= 0; i--)
            {
                Destroy(choicesParent.GetChild(i).gameObject);
            }
        }

        private void SetContinueVisible(bool isVisible)
        {
            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(isVisible);
            }
        }

        private static void SetText(TMP_Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }

        private static string FormatCheck(RoomEventCheckData check)
        {
            if (check == null)
                return "No check";

            string comparison = check.comparison == DiceComparison.GreaterThan ? ">" : ">=";
            return $"{check.stat} {comparison} {check.targetNumber}";
        }

        private static string FormatOutcome(RoomEventOutcomeData outcome)
        {
            if (outcome == null)
                return "No outcome configured.";

            StringBuilder builder = new StringBuilder(outcome.resultText);

            if (outcome.effects == null || outcome.effects.Length == 0)
                return builder.ToString();

            builder.AppendLine();
            builder.AppendLine();
            builder.AppendLine("Effects:");

            foreach (RoomEventEffectData effect in outcome.effects)
            {
                builder.AppendLine(FormatEffect(effect));
            }

            return builder.ToString();
        }

        private static string FormatEffect(RoomEventEffectData effect)
        {
            if (effect == null)
                return "- None";

            if (effect.effectType == RoomEventEffectType.GainItem)
                return $"- Gain item: {effect.itemName} x{effect.itemAmount}";

            return $"- {effect.stat}: {effect.statDelta:+#;-#;0}";
        }
    }
}
