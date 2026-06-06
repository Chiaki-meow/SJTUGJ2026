using UnityEngine;
using UnityEngine.Serialization;

namespace Gameplay
{
    public class AudioEventPlayer : MonoBehaviour
    {
        [FormerlySerializedAs("audioId")]
        public SfxEnum sfxId = SfxEnum.ButtonClick;

        public void Play()
        {
            AudioManager.PlaySfx(sfxId);
        }
    }
}
