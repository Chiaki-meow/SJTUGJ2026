using Gameplay;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class UIButtonSound : MonoBehaviour
    {
        public Button button;
        [FormerlySerializedAs("audioId")]
        public SfxEnum sfxId = SfxEnum.ButtonClick;

        private void Awake()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }

            if (button != null)
            {
                button.onClick.AddListener(Play);
            }
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(Play);
            }
        }

        public void Play()
        {
            AudioManager.PlaySfx(sfxId);
        }
    }
}
