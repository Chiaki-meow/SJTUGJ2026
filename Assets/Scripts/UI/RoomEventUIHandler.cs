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
        public PlayerStateManager playerStateManager;
        public PlayerInventory playerInventory;
        public InGameManager inGameManager;

        private Action onFinished;
        private RoomCard activeRoom;
        private RoomEventData activeEventData;

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
            activeRoom = room;
            activeEventData = eventData;
            ResolveReferences();

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
                AudioManager.PlaySfx(success ? SfxEnum.DiceSuccess : SfxEnum.DiceFail);
                outcome = success ? choice.successOutcome : choice.failureOutcome;
                checkSummary = $"Roll: {roll} / {FormatCheck(choice.check)} / {(success ? "Success" : "Failure")}\n\n";
            }

            ApplyOutcomeEffects(outcome);
            AudioManager.PlaySfx(SfxEnum.ButtonClick);
            SetText(resultText, checkSummary + FormatOutcome(outcome));
            SetContinueVisible(true);
        }

        private void ApplyOutcomeEffects(RoomEventOutcomeData outcome)
        {
            if (outcome == null || outcome.effects == null)
                return;

            for (int i = 0; i < outcome.effects.Length; i++)
            {
                RoomEventEffectData effect = outcome.effects[i];
                if (effect == null)
                    continue;

                if (effect.effectType == RoomEventEffectType.StatChange)
                {
                    if (playerStateManager != null)
                    {
                        playerStateManager.ApplyStatChange(effect.stat, effect.statDelta);
                    }
                }
                else if (effect.effectType == RoomEventEffectType.GainItem)
                {
                    if (playerInventory != null && effect.itemData != null)
                    {
                        playerInventory.AddItem(effect.itemData, effect.itemAmount);
                        AudioManager.PlaySfx(SfxEnum.DrawCard);
                    }
                }
            }
        }

        private void ResolveReferences()
        {
            if (inGameManager == null)
            {
                inGameManager = InGameManager.Instance != null ? InGameManager.Instance : FindObjectOfType<InGameManager>();
            }

            if (playerStateManager == null)
            {
                playerStateManager = PlayerStateManager.Instance != null ? PlayerStateManager.Instance : FindObjectOfType<PlayerStateManager>();
            }

            if (playerInventory == null)
            {
                playerInventory = inGameManager != null && inGameManager.playerInventory != null
                    ? inGameManager.playerInventory
                    : FindObjectOfType<PlayerInventory>();
            }
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
            AudioManager.PlaySfx(SfxEnum.ButtonClick);

            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            Action finishedCallback = onFinished;
            onFinished = null;
            activeRoom = null;
            activeEventData = null;
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
            {
                string itemDisplayName = effect.itemData != null ? effect.itemData.displayName : effect.itemName;
                return $"- Gain item: {itemDisplayName} x{effect.itemAmount}";
            }

            return $"- {effect.stat}: {effect.statDelta:+#;-#;0}";
        }
    }
}
