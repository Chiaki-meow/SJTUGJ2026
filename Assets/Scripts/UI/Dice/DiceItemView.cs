using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Dice
{
    public class DiceItemView : MonoBehaviour
    {
        [SerializeField] private Image diceImage;
        [SerializeField] private TMP_Text valueText;
        [SerializeField] private Sprite[] valueSprites;
        [SerializeField] private Sprite fallbackSprite;

        private void Awake()
        {
            ResolveReferences();
        }

        public void SetValue(int value)
        {
            ResolveReferences();

            if (diceImage != null)
            {
                Sprite sprite = value >= 0 && valueSprites != null && value < valueSprites.Length ? valueSprites[value] : fallbackSprite;
                diceImage.sprite = sprite;
                diceImage.enabled = sprite != null;
            }

            if (valueText != null)
            {
                valueText.text = value.ToString();
            }
        }

        private void ResolveReferences()
        {
            if (diceImage == null)
            {
                diceImage = GetComponent<Image>();
            }

            if (valueText == null)
            {
                valueText = GetComponentInChildren<TMP_Text>(true);
            }
        }
    }
}
