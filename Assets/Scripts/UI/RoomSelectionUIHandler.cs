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

        private Action<RoomPlacementOption> selectedCallback;
        private Action placementFailedDismissedCallback;
        private readonly List<RoomPlacementOption> currentChoices = new();
        private bool initialized;
        private bool isRotationMode;

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

        public override void ShowRoomSelection(IReadOnlyList<RoomPlacementOption> choices, Action<RoomPlacementOption> callback)
        {
            Initialize();
            selectedCallback = callback;
            placementFailedDismissedCallback = null;
            isRotationMode = false;
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
            AudioManager.PlaySfx(SfxEnum.DrawCard);

            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }
        }

        public override void ShowRoomRotation(RoomPlacementOption selectedOption, Vector2Int requiredDoor, Action<RoomPlacementOption> callback)
        {
            Initialize();
            selectedCallback = callback;
            placementFailedDismissedCallback = null;
            isRotationMode = true;
            currentChoices.Clear();

            if (selectedOption != null && selectedOption.data != null)
            {
                RoomDoorLayout layout = selectedOption.doorLayout;
                for (int i = 0; i < 4; i++)
                {
                    if (layout.HasDoor(requiredDoor) && !ContainsLayout(layout))
                    {
                        currentChoices.Add(selectedOption.WithDoorLayout(layout));
                    }

                    layout = layout.RotatedClockwise();
                }
            }

            RefreshSlots();
            AudioManager.PlaySfx(SfxEnum.SpinRoom);

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
            isRotationMode = false;
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
                RoomPlacementOption option = hasChoice ? currentChoices[i] : null;
                RoomCardData data = option != null ? option.data : null;

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
                    slot.roomNameText.text = isRotationMode ? $"{data.roomName} 朝向{i + 1}" : data.roomName;
                }

                if (slot.roomInfoText != null)
                {
                    slot.roomInfoText.text = isRotationMode ? "点击确认放置" : string.Format(roomInfoFormat, option.DoorCount);
                }

                RefreshDoorImages(slot.root, option);

                if (slot.button != null)
                {
                    slot.button.interactable = true;
                }
            }
        }

        private void RefreshDoorImages(GameObject root, RoomPlacementOption option)
        {
            if (root == null || option == null)
                return;

            SetDoorImage(root, "link1", option.HasDoor(Vector2Int.up));
            SetDoorImage(root, "link1-red", false);
            SetDoorImage(root, "link2", option.HasDoor(Vector2Int.left));
            SetDoorImage(root, "link2-red", false);
            SetDoorImage(root, "link3", option.HasDoor(Vector2Int.down));
            SetDoorImage(root, "link3-red", false);
            SetDoorImage(root, "link4", option.HasDoor(Vector2Int.right));
            SetDoorImage(root, "link4-red", false);
        }

        private void SetDoorImage(GameObject root, string childName, bool isVisible)
        {
            Transform child = root.transform.Find(childName);
            if (child != null)
            {
                child.gameObject.SetActive(isVisible);
            }
        }

        private bool ContainsLayout(RoomDoorLayout layout)
        {
            for (int i = 0; i < currentChoices.Count; i++)
            {
                if (currentChoices[i] != null && currentChoices[i].doorLayout.HasSameDoors(layout))
                    return true;
            }

            return false;
        }

        private void Select(int index)
        {
            if (index < 0 || index >= currentChoices.Count)
                return;

            RoomPlacementOption selected = currentChoices[index];
            AudioManager.PlaySfx(isRotationMode ? SfxEnum.SpinRoom : SfxEnum.ButtonClick);
            Hide();
            Action<RoomPlacementOption> callback = selectedCallback;
            selectedCallback = null;
            callback?.Invoke(selected);
        }

        private void CancelSelection()
        {
            if (placementFailedDismissedCallback != null)
            {
                Action dismissedCallback = placementFailedDismissedCallback;
                placementFailedDismissedCallback = null;
                AudioManager.PlaySfx(SfxEnum.ButtonClick);
                Hide();
                dismissedCallback?.Invoke();
                return;
            }

            AudioManager.PlaySfx(SfxEnum.ButtonClick);
            Hide();
            Action<RoomPlacementOption> callback = selectedCallback;
            selectedCallback = null;
            callback?.Invoke(null);
        }

        private void Hide()
        {
            placementFailedDismissedCallback = null;
            isRotationMode = false;
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }
    }
}
