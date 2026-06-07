using System;
using System.Collections.Generic;
using Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class RoomSelectionUIHandler : RoomSelectionHandler
    {
        [Serializable]
        public class RoomChoiceSlot
        {
            public GameObject root;
            public Button button;
            public Image roomImage;
            public TMP_Text roomNameText;
            public TMP_Text roomInfoText;
        }

        public GameObject panelRoot;
        public RoomChoiceSlot[] slots;
        public Button closeButton;
        public string roomInfoFormat = "门：{0}";

        private Action<RoomCardData> selectedCallback;
        private Action placementFailedDismissedCallback;
        private readonly List<RoomCardData> currentChoices = new();
        private bool initialized;

        public override bool IsSelecting => panelRoot != null && panelRoot.activeSelf;

        private void Awake()
        {
            bool wasInitialized = initialized;
            Initialize();
            if (!wasInitialized)
            {
                Hide();
            }
        }

        private void OnDestroy()
        {
            RemoveListeners();
        }

        public override void ShowRoomSelection(IReadOnlyList<RoomCardData> choices, Action<RoomCardData> callback)
        {
            Initialize();
            selectedCallback = callback;
            placementFailedDismissedCallback = null;
            currentChoices.Clear();

            if (choices != null)
            {
                for (int i = 0; i < choices.Count; i++)
                {
                    if (choices[i] != null)
                    {
                        currentChoices.Add(choices[i]);
                    }
                }
            }

            RefreshSlots();

            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }
        }

        public override void ShowPlacementFailed(string message, Action dismissedCallback)
        {
            Initialize();
            selectedCallback = null;
            placementFailedDismissedCallback = dismissedCallback;
            currentChoices.Clear();

            if (slots != null)
            {
                for (int i = 0; i < slots.Length; i++)
                {
                    RoomChoiceSlot slot = slots[i];
                    if (slot == null)
                        continue;

                    bool isMessageSlot = i == 0;
                    if (slot.root != null)
                    {
                        slot.root.SetActive(isMessageSlot);
                    }

                    if (!isMessageSlot)
                        continue;

                    if (slot.roomImage != null)
                    {
                        slot.roomImage.enabled = false;
                    }

                    if (slot.roomNameText != null)
                    {
                        slot.roomNameText.text = "无法放置房间";
                    }

                    if (slot.roomInfoText != null)
                    {
                        slot.roomInfoText.text = message;
                    }

                    if (slot.button != null)
                    {
                        slot.button.interactable = false;
                    }
                }
            }

            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }
        }

        private void Initialize()
        {
            if (initialized)
                return;

            initialized = true;

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CancelSelection);
            }

            if (slots == null)
                return;

            for (int i = 0; i < slots.Length; i++)
            {
                RoomChoiceSlot slot = slots[i];
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
                slot.button.onClick.AddListener(() => Select(index));
            }
        }

        private void RemoveListeners()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(CancelSelection);
            }

            if (slots == null)
                return;

            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i]?.button != null)
                {
                    slots[i].button.onClick.RemoveAllListeners();
                }
            }
        }

        private void RefreshSlots()
        {
            if (slots == null)
                return;

            for (int i = 0; i < slots.Length; i++)
            {
                RoomChoiceSlot slot = slots[i];
                if (slot == null)
                    continue;

                bool hasChoice = i < currentChoices.Count && currentChoices[i] != null;
                RoomCardData data = hasChoice ? currentChoices[i] : null;

                if (slot.root != null)
                {
                    slot.root.SetActive(hasChoice);
                }

                if (!hasChoice)
                    continue;

                if (slot.roomImage != null)
                {
                    slot.roomImage.sprite = data.sprite;
                    slot.roomImage.enabled = data.sprite != null;
                    slot.roomImage.preserveAspect = true;
                }

                if (slot.roomNameText != null)
                {
                    slot.roomNameText.text = data.roomName;
                }

                if (slot.roomInfoText != null)
                {
                    slot.roomInfoText.text = string.Format(roomInfoFormat, data.doorCount);
                }

                if (slot.button != null)
                {
                    slot.button.interactable = true;
                }
            }
        }

        private void Select(int index)
        {
            if (index < 0 || index >= currentChoices.Count)
                return;

            RoomCardData selected = currentChoices[index];
            Hide();
            Action<RoomCardData> callback = selectedCallback;
            selectedCallback = null;
            callback?.Invoke(selected);
        }

        private void CancelSelection()
        {
            if (placementFailedDismissedCallback != null)
            {
                Action dismissedCallback = placementFailedDismissedCallback;
                placementFailedDismissedCallback = null;
                Hide();
                dismissedCallback?.Invoke();
                return;
            }

            Hide();
            Action<RoomCardData> callback = selectedCallback;
            selectedCallback = null;
            callback?.Invoke(null);
        }

        private void Hide()
        {
            placementFailedDismissedCallback = null;
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }
    }
}
