using Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ItemGainPopupView : MonoBehaviour
    {
        public GameObject panelRoot;
        public TMP_Text nameText;
        public TMP_Text descriptionText;
        public Image iconImage;
        public Button closeButton;

        private bool initialized;

        private void Awake()
        {
            bool wasInitialized = initialized;
            Initialize();
            if (!wasInitialized)
            {
                Hide(false);
            }
        }

        private void OnDestroy()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(Hide);
            }
        }

        public void Show(ItemData itemData, int amount)
        {
            Initialize();

            if (itemData == null)
                return;

            AudioManager.PlaySfx(SfxEnum.DrawCard);

            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }

            if (nameText != null)
            {
                nameText.text = amount > 1 ? $"{itemData.displayName} x{amount}" : itemData.displayName;
            }

            if (descriptionText != null)
            {
                descriptionText.text = itemData.description;
            }

            if (iconImage != null)
            {
                iconImage.sprite = itemData.icon;
                iconImage.enabled = itemData.icon != null;
                iconImage.preserveAspect = true;
            }
        }

        public void Hide()
        {
            Hide(true);
        }

        private void Hide(bool playSound)
        {
            if (playSound)
            {
                AudioManager.PlaySfx(SfxEnum.ButtonClick);
            }

            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        private void Initialize()
        {
            if (initialized)
                return;

            initialized = true;

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Hide);
            }
        }
    }
}
