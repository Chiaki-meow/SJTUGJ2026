using Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ItemInventoryView : MonoBehaviour
    {
        public PlayerInventory inventory;
        public PlayerStateManager playerStateManager;
        public PlayerGridMovement player;
        public BoardManager board;
        public InGameManager inGame;

        public Transform slotRoot;
        public Image[] slotImages;
        public Button[] slotButtons;
        public Sprite emptySlotSprite;
        public Color emptyColor = new Color(1f, 1f, 1f, 0.2f);
        public Color filledColor = Color.white;

        public GameObject usePanel;
        public TMP_Text useNameText;
        public TMP_Text useDescriptionText;
        public Image useIconImage;
        public Button useButton;
        public Button closeButton;

        private ItemModel selectedItem;
        private bool subscribed;

        private void Awake()
        {
            if (slotRoot == null)
            {
                slotRoot = transform;
            }

            CollectSlots();
            RegisterSlotButtons();

            if (useButton != null)
            {
                useButton.onClick.AddListener(UseSelectedItem);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(HideUsePanel);
            }

            HideUsePanel(false);
        }

        private void OnEnable()
        {
            ResolveReferences();
            SubscribeInventory();
            Refresh();
        }

        private void Start()
        {
            ResolveReferences();
            SubscribeInventory();
            Refresh();
        }

        private void OnDisable()
        {
            UnsubscribeInventory();
        }

        private void OnDestroy()
        {
            if (useButton != null)
            {
                useButton.onClick.RemoveListener(UseSelectedItem);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(HideUsePanel);
            }
        }

        public void Refresh()
        {
            if (slotImages == null)
                return;

            int itemCount = inventory != null ? inventory.Items.Count : 0;

            for (int i = 0; i < slotImages.Length; i++)
            {
                Image slotImage = slotImages[i];
                if (slotImage == null)
                    continue;

                bool hasItem = i < itemCount;
                ItemModel item = hasItem ? inventory.Items[i] : null;
                ItemData data = item != null ? item.Data : null;

                slotImage.sprite = data != null && data.icon != null ? data.icon : emptySlotSprite;
                slotImage.color = data != null ? filledColor : emptyColor;
                slotImage.preserveAspect = true;

                if (slotButtons != null && i < slotButtons.Length && slotButtons[i] != null)
                {
                    slotButtons[i].interactable = data != null;
                }
            }

            if (selectedItem != null && (inventory == null || !ContainsItem(selectedItem)))
            {
                HideUsePanel();
            }
        }

        public void SelectSlot(int index)
        {
            AudioManager.PlaySfx(SfxEnum.ButtonClick);

            if (inventory == null || index < 0 || index >= inventory.Items.Count)
                return;

            selectedItem = inventory.Items[index];
            ShowSelectedItem();
        }

        private void UseSelectedItem()
        {
            if (selectedItem == null || inventory == null)
                return;

            ItemUseContext context = CreateUseContext();
            bool used = inventory.UseItem(selectedItem, context, out string message);
            if (used)
            {
                AudioManager.PlaySfx(SfxEnum.ButtonClick);
            }

            if (useDescriptionText != null)
            {
                useDescriptionText.text = string.IsNullOrWhiteSpace(message)
                    ? (used ? "已使用。" : FormatCannotUseReason(selectedItem, context))
                    : message;
            }

            Refresh();
            UpdateUseButtonState(context);
        }

        private void ShowSelectedItem()
        {
            if (selectedItem == null || selectedItem.Data == null)
                return;

            ItemData data = selectedItem.Data;

            if (usePanel != null)
            {
                usePanel.SetActive(true);
            }

            if (useNameText != null)
            {
                useNameText.text = data.displayName;
            }

            if (useDescriptionText != null)
            {
                useDescriptionText.text = FormatDescription(selectedItem);
            }

            if (useIconImage != null)
            {
                useIconImage.sprite = data.icon;
                useIconImage.enabled = data.icon != null;
                useIconImage.preserveAspect = true;
            }

            UpdateUseButtonState(CreateUseContext());
        }

        private void HideUsePanel()
        {
            HideUsePanel(true);
        }

        private void HideUsePanel(bool playSound)
        {
            if (playSound)
            {
                AudioManager.PlaySfx(SfxEnum.ButtonClick);
            }

            selectedItem = null;

            if (usePanel != null)
            {
                usePanel.SetActive(false);
            }
        }

        private void UpdateUseButtonState(ItemUseContext context)
        {
            if (useButton == null)
                return;

            useButton.interactable = selectedItem != null;
        }

        private ItemUseContext CreateUseContext()
        {
            ResolveReferences();

            return new ItemUseContext
            {
                Inventory = inventory,
                PlayerState = playerStateManager,
                Player = player,
                Board = board,
                InGame = inGame,
                CurrentRoom = inGame != null ? inGame.CurrentRoom : null,
                CurrentEvent = inGame != null ? inGame.CurrentEventData : null
            };
        }

        private string FormatDescription(ItemModel item)
        {
            ItemData data = item.Data;
            string description = data.description;

            if (item.Amount > 1)
            {
                description += $"\n持有数量：{item.Amount}";
            }

            if (data.maxUses > 0)
            {
                description += $"\n剩余次数：{item.RemainingUses}";
            }

            if (!item.CanUse(CreateUseContext()))
            {
                description += $"\n{FormatCannotUseReason(item, CreateUseContext())}";
            }

            return description;
        }

        private static string FormatCannotUseReason(ItemModel item, ItemUseContext context)
        {
            ItemData data = item != null ? item.Data : null;
            return data != null && data.effect != null ? data.effect.GetCannotUseReason(item, context) : "该物品不能主动使用。";
        }

        private bool ContainsItem(ItemModel item)
        {
            if (inventory == null || item == null)
                return false;

            for (int i = 0; i < inventory.Items.Count; i++)
            {
                if (inventory.Items[i] == item)
                    return true;
            }

            return false;
        }

        private void ResolveReferences()
        {
            if (inventory == null)
            {
                inventory = FindObjectOfType<PlayerInventory>();
            }

            if (playerStateManager == null)
            {
                playerStateManager = PlayerStateManager.Instance != null ? PlayerStateManager.Instance : FindObjectOfType<PlayerStateManager>();
            }

            if (player == null)
            {
                player = FindObjectOfType<PlayerGridMovement>();
            }

            if (board == null)
            {
                board = FindObjectOfType<BoardManager>();
            }

            if (inGame == null)
            {
                inGame = InGameManager.Instance != null ? InGameManager.Instance : FindObjectOfType<InGameManager>();
            }
        }

        private void SubscribeInventory()
        {
            if (subscribed || inventory == null)
                return;

            inventory.OnInventoryChanged += Refresh;
            subscribed = true;
        }

        private void UnsubscribeInventory()
        {
            if (!subscribed || inventory == null)
                return;

            inventory.OnInventoryChanged -= Refresh;
            subscribed = false;
        }

        private void CollectSlots()
        {
            if (slotRoot == null)
                return;

            int childCount = slotRoot.childCount;

            if (slotImages == null || slotImages.Length == 0)
            {
                slotImages = new Image[childCount];
            }

            if (slotButtons == null || slotButtons.Length == 0)
            {
                slotButtons = new Button[childCount];
            }

            for (int i = 0; i < childCount; i++)
            {
                Transform child = slotRoot.GetChild(i);

                if (i < slotImages.Length && slotImages[i] == null)
                {
                    slotImages[i] = child.GetComponent<Image>();
                }

                if (i < slotButtons.Length && slotButtons[i] == null)
                {
                    Button button = child.GetComponent<Button>();
                    if (button == null)
                    {
                        button = child.gameObject.AddComponent<Button>();
                    }

                    slotButtons[i] = button;
                }
            }
        }

        private void RegisterSlotButtons()
        {
            if (slotButtons == null)
                return;

            for (int i = 0; i < slotButtons.Length; i++)
            {
                Button button = slotButtons[i];
                if (button == null)
                    continue;

                int slotIndex = i;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => SelectSlot(slotIndex));
            }
        }
    }
}
