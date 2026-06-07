using System;
using Gameplay;
using TMPro;
using UI.Dice;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class Scene3RoomEventUIHandler : RoomEventHandler
    {
        [Serializable]
        public class ChoiceSlot
        {
            public GameObject root;
            public Button button;
            public TMP_Text labelText;
            public GameObject judgeRoot;
            public TMP_Text judgeText;
        }

        public GameObject panelRoot;
        public TMP_Text titleText;
        public TMP_Text descriptionText;
        public TMP_Text resultText;
        public ChoiceSlot[] choiceSlots;
        public Button continueButton;
        public DicePanelView dicePanelView;
        public ItemGainPopupView itemGainPopup;
        public PlayerStateManager playerStateManager;
        public PlayerInventory playerInventory;
        public InGameManager inGameManager;
        public Phase2Director phase2Director;
        public DeanBossEncounterData deanBossEncounterData;

        private Action onFinished;
        private RoomCard activeRoom;
        private RoomEventData activeEventData;
        private bool isDeanEncounter;
        private bool initialized;

        private void Awake()
        {
            bool wasInitialized = initialized;
            Initialize();
            if (!wasInitialized)
            {
                HidePanel();
            }
        }

        private void OnDestroy()
        {
            if (continueButton != null)
            {
                continueButton.onClick.RemoveListener(FinishEvent);
            }

            if (choiceSlots == null)
                return;

            for (int i = 0; i < choiceSlots.Length; i++)
            {
                if (choiceSlots[i]?.button != null)
                {
                    choiceSlots[i].button.onClick.RemoveAllListeners();
                }
            }
        }

        public override void HandleRoomEvent(RoomCard room, RoomEventData eventData, Action finishedCallback)
        {
            Initialize();
            ResolveReferences();
            onFinished = finishedCallback;
            activeRoom = room;
            activeEventData = eventData;
            isDeanEncounter = IsDeanEncounter(room);

            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }

            if (isDeanEncounter)
            {
                ShowDeanEncounterIntro();
                return;
            }

            SetText(titleText, eventData != null ? eventData.eventName : "未知事件");
            SetText(descriptionText, eventData != null ? eventData.description : "这个房间没有事件。");
            SetText(resultText, string.Empty);
            SetContinueVisible(false);
            RefreshChoiceSlots(eventData);
        }

        private void Initialize()
        {
            if (initialized)
                return;

            initialized = true;

            if (continueButton != null)
            {
                continueButton.onClick.AddListener(FinishEvent);
            }

            if (choiceSlots == null)
                return;

            for (int i = 0; i < choiceSlots.Length; i++)
            {
                ChoiceSlot slot = choiceSlots[i];
                if (slot == null)
                    continue;

                if (slot.button == null && slot.root != null)
                {
                    slot.button = slot.root.GetComponentInChildren<Button>(true);
                }

                if (slot.button == null && slot.root != null)
                {
                    Image clickableImage = slot.root.GetComponentInChildren<Image>(true);
                    if (clickableImage != null)
                    {
                        slot.button = clickableImage.gameObject.GetComponent<Button>();
                        if (slot.button == null)
                        {
                            slot.button = clickableImage.gameObject.AddComponent<Button>();
                        }
                    }
                }

                if (slot.button == null)
                    continue;

                int index = i;
                slot.button.onClick.AddListener(() => SelectChoice(index));
            }
        }

        private void RefreshChoiceSlots(RoomEventData eventData)
        {
            if (choiceSlots == null)
                return;

            int choiceCount = eventData != null && eventData.choices != null ? eventData.choices.Length : 0;

            for (int i = 0; i < choiceSlots.Length; i++)
            {
                ChoiceSlot slot = choiceSlots[i];
                if (slot == null)
                    continue;

                bool hasChoice = i < choiceCount && eventData.choices[i] != null;

                if (slot.root != null)
                {
                    slot.root.SetActive(hasChoice);
                }

                if (!hasChoice)
                    continue;

                RoomEventChoiceData choice = eventData.choices[i];
                SetText(slot.labelText, choice.label);

                if (slot.judgeRoot != null)
                {
                    slot.judgeRoot.SetActive(choice.requiresCheck);
                }

                SetText(slot.judgeText, choice.requiresCheck ? FormatCheck(choice.check) : string.Empty);

                if (slot.button != null)
                {
                    slot.button.interactable = true;
                }
            }

            if (choiceCount == 0)
            {
                SetText(resultText, "没有配置事件选项。");
                SetContinueVisible(true);
            }
        }

        private void SelectChoice(int index)
        {
            if (isDeanEncounter)
            {
                PlayDeanCheck();
                return;
            }

            if (activeEventData == null || activeEventData.choices == null || index < 0 || index >= activeEventData.choices.Length)
                return;

            RoomEventChoiceData choice = activeEventData.choices[index];
            SetChoiceSlotsVisible(false);

            if (choice.requiresCheck)
            {
                PlayCheck(choice);
                return;
            }

            ResolveOutcome(choice.directOutcome, string.Empty);
        }

        private void PlayCheck(RoomEventChoiceData choice)
        {
            if (dicePanelView == null)
            {
                ResolveOutcome(choice.failureOutcome, "检定 UI 未配置。\n\n");
                return;
            }

            RoomEventCheckData check = choice.check;
            if (check == null)
            {
                ResolveOutcome(choice.successOutcome, string.Empty);
                return;
            }

            int diceCount = Mathf.Max(0, playerStateManager != null ? playerStateManager.GetStatValue(check.stat) : 0);
            DiceCheckType checkType = ToDiceCheckType(check.stat);
            DiceCompareRule compareRule = check.comparison == DiceComparison.GreaterThan ? DiceCompareRule.GreaterOrEqual : DiceCompareRule.GreaterOrEqual;
            int difficulty = check.comparison == DiceComparison.GreaterThan ? check.targetNumber + 1 : check.targetNumber;

            dicePanelView.PlayCheck(checkType, diceCount, difficulty, compareRule, false, result =>
            {
                RoomEventOutcomeData outcome = result.isSuccess ? choice.successOutcome : choice.failureOutcome;
                string summary = $"检定结果：{result.finalTotalValue} / 目标 {difficulty} / {(result.isSuccess ? "成功" : "失败")}\n\n";
                ResolveOutcome(outcome, summary);
            });
        }

        private void ResolveOutcome(RoomEventOutcomeData outcome, string prefix)
        {
            ApplyOutcomeEffects(outcome);
            SetText(resultText, prefix + FormatOutcome(outcome));
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
                        if (itemGainPopup != null)
                        {
                            itemGainPopup.Show(effect.itemData, effect.itemAmount);
                        }
                    }
                }
                else if (effect.effectType == RoomEventEffectType.RevealTruth)
                {
                    if (inGameManager != null)
                    {
                        inGameManager.RevealTruth();
                    }
                }
                else if (effect.effectType == RoomEventEffectType.TriggerPhase2)
                {
                    if (phase2Director != null)
                    {
                        phase2Director.TriggerTruthRevealFailed();
                    }
                }
            }
        }

        private void FinishEvent()
        {
            HidePanel();
            Action finishedCallback = onFinished;
            onFinished = null;
            activeRoom = null;
            activeEventData = null;
            isDeanEncounter = false;
            finishedCallback?.Invoke();
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
            if (phase2Director == null)
            {
                phase2Director = FindObjectOfType<Phase2Director>();
            }
        }

        private bool IsDeanEncounter(RoomCard room)
        {
            return phase2Director != null && phase2Director.IsActive && room != null && room == phase2Director.DeanOfficeRoom;
        }

        private void ShowDeanEncounterIntro()
        {
            SetText(titleText, "院长室");
            SetText(descriptionText, GetDeanIntroText());
            SetText(resultText, string.Empty);
            SetContinueVisible(false);
            RefreshDeanChoiceSlot();
        }

        private void RefreshDeanChoiceSlot()
        {
            if (choiceSlots == null)
                return;

            for (int i = 0; i < choiceSlots.Length; i++)
            {
                ChoiceSlot slot = choiceSlots[i];
                if (slot == null)
                    continue;

                bool isPrimary = i == 0;
                if (slot.root != null)
                {
                    slot.root.SetActive(isPrimary);
                }

                if (!isPrimary)
                    continue;

                SetText(slot.labelText, phase2Director != null && phase2Director.CurrentRoute == Phase2Route.PatientLetter ? "说服院长" : "对抗院长");

                if (slot.judgeRoot != null)
                {
                    slot.judgeRoot.SetActive(true);
                }

                SetText(slot.judgeText, FormatDeanCheck());

                if (slot.button != null)
                {
                    slot.button.interactable = true;
                }
            }
        }

        private void PlayDeanCheck()
        {
            if (phase2Director == null || !phase2Director.IsActive)
                return;

            SetChoiceSlotsVisible(false);

            if (phase2Director.ShouldSkipFirstPatientLetterCheck() && phase2Director.SuccessfulDeanChecks == 0)
            {
                bool completed = phase2Director.RegisterDeanCheckSuccess(GetRequiredDeanSuccessCount());
                SetText(resultText, completed ? GetDeanWinText() : GetDeanSingleSuccessText());
                SetContinueVisible(completed);
                if (!completed)
                {
                    RefreshDeanChoiceSlot();
                }
                return;
            }

            if (dicePanelView == null)
            {
                ResolveDeanFailure("检定 UI 未配置。\n\n");
                return;
            }

            CharacterStat stat = phase2Director.CurrentRoute == Phase2Route.PatientLetter ? CharacterStat.Mental : CharacterStat.Physical;
            int diceCount = Mathf.Max(0, playerStateManager != null ? playerStateManager.GetStatValue(stat) : 0);
            int target = phase2Director.GetCheckTarget(GetDeanBaseTarget());

            dicePanelView.PlayCheck(ToDiceCheckType(stat), diceCount, target, DiceCompareRule.GreaterOrEqual, false, result =>
            {
                string summary = $"检定结果：{result.finalTotalValue} / 目标 {target} / {(result.isSuccess ? "成功" : "失败")}\n\n";
                if (result.isSuccess)
                {
                    ResolveDeanSuccess(summary);
                }
                else
                {
                    ResolveDeanFailure(summary);
                }
            });
        }

        private void ResolveDeanSuccess(string prefix)
        {
            bool completed = phase2Director != null && phase2Director.RegisterDeanCheckSuccess(GetRequiredDeanSuccessCount());
            SetText(resultText, prefix + (completed ? GetDeanWinText() : GetDeanSingleSuccessText()));
            SetContinueVisible(completed);
            if (!completed)
            {
                RefreshDeanChoiceSlot();
            }
        }

        private void ResolveDeanFailure(string prefix)
        {
            if (phase2Director != null)
            {
                phase2Director.RegisterDeanCheckFailure(GetDeanFailureDamage());
            }

            bool completed = phase2Director != null && phase2Director.State == Phase2State.Completed;
            SetText(resultText, prefix + (completed ? GetDeanLoseText() : GetDeanFailureText()));
            SetContinueVisible(completed);
            if (!completed)
            {
                RefreshDeanChoiceSlot();
            }
        }

        private int GetDeanBaseTarget()
        {
            if (deanBossEncounterData == null || phase2Director == null)
                return 5;

            return deanBossEncounterData.GetTarget(phase2Director.CurrentRoute, phase2Director.SuccessfulDeanChecks);
        }

        private int GetRequiredDeanSuccessCount()
        {
            return deanBossEncounterData != null ? Mathf.Max(1, deanBossEncounterData.requiredSuccessCount) : 2;
        }

        private int GetDeanFailureDamage()
        {
            return deanBossEncounterData != null ? Mathf.Max(0, deanBossEncounterData.failureDamage) : 1;
        }

        private string GetDeanIntroText()
        {
            if (deanBossEncounterData == null || phase2Director == null)
                return "院长在黑暗中等待着你。";

            return phase2Director.CurrentRoute == Phase2Route.PatientLetter
                ? deanBossEncounterData.patientLetterIntroText
                : deanBossEncounterData.bloodyKnifeIntroText;
        }

        private string GetDeanSingleSuccessText()
        {
            if (deanBossEncounterData == null || phase2Director == null)
                return "你暂时压制住了院长。";

            return phase2Director.CurrentRoute == Phase2Route.PatientLetter
                ? deanBossEncounterData.patientLetterSingleSuccessText
                : deanBossEncounterData.bloodyKnifeSingleSuccessText;
        }

        private string GetDeanWinText()
        {
            if (deanBossEncounterData == null || phase2Director == null)
                return "你战胜了院长。";

            return phase2Director.CurrentRoute == Phase2Route.PatientLetter
                ? deanBossEncounterData.patientLetterWinText
                : deanBossEncounterData.bloodyKnifeWinText;
        }

        private string GetDeanFailureText()
        {
            if (deanBossEncounterData == null || phase2Director == null)
                return "院长反击了你。";

            return phase2Director.CurrentRoute == Phase2Route.PatientLetter
                ? deanBossEncounterData.patientLetterFailureText
                : deanBossEncounterData.bloodyKnifeFailureText;
        }

        private string GetDeanLoseText()
        {
            if (deanBossEncounterData == null || phase2Director == null)
                return "你倒在了院长室里。";

            return phase2Director.CurrentRoute == Phase2Route.PatientLetter
                ? deanBossEncounterData.patientLetterLoseText
                : deanBossEncounterData.bloodyKnifeLoseText;
        }

        private string FormatDeanCheck()
        {
            if (phase2Director == null)
                return string.Empty;

            CharacterStat stat = phase2Director.CurrentRoute == Phase2Route.PatientLetter ? CharacterStat.Mental : CharacterStat.Physical;
            int target = phase2Director.GetCheckTarget(GetDeanBaseTarget());
            return $"{stat} >= {target}";
        }

        private void SetChoiceSlotsVisible(bool isVisible)
        {
            if (choiceSlots == null)
                return;

            for (int i = 0; i < choiceSlots.Length; i++)
            {
                if (choiceSlots[i]?.root != null)
                {
                    choiceSlots[i].root.SetActive(isVisible && activeEventData != null && activeEventData.choices != null && i < activeEventData.choices.Length);
                }
            }
        }

        private void SetContinueVisible(bool isVisible)
        {
            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(isVisible);
            }
        }

        private void HidePanel()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        private static void SetText(TMP_Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }

        private static DiceCheckType ToDiceCheckType(CharacterStat stat)
        {
            switch (stat)
            {
                case CharacterStat.Physical:
                    return DiceCheckType.Physical;
                case CharacterStat.Mental:
                    return DiceCheckType.Mental;
                default:
                    return DiceCheckType.Custom;
            }
        }

        private static string FormatCheck(RoomEventCheckData check)
        {
            if (check == null)
                return "无需检定";

            string comparison = check.comparison == DiceComparison.GreaterThan ? ">" : ">=";
            return $"{check.stat} {comparison} {check.targetNumber}";
        }

        private static string FormatOutcome(RoomEventOutcomeData outcome)
        {
            if (outcome == null)
                return "没有配置结果。";

            return outcome.resultText;
        }
    }
}
