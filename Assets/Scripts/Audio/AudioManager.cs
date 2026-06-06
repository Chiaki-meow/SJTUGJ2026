using UnityEngine;
using UnityEngine.Serialization;

namespace Gameplay
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        public AudioLibrary audioLibrary;
        public AudioSource sfxSource;

        [FormerlySerializedAs("musicSource")]
        public AudioSource bgmSource;

        [Range(0f, 1f)]
        public float masterVolume = 1f;

        [Range(0f, 1f)]
        public float sfxVolume = 1f;

        [FormerlySerializedAs("musicVolume")]
        [Range(0f, 1f)]
        public float bgmVolume = 1f;

        private float currentBgmEntryVolume = 1f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
            }

            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
            }

            sfxSource.playOnAwake = false;
            bgmSource.playOnAwake = false;
            bgmSource.loop = true;
            ApplyBgmVolume();
        }

        public static void PlaySfx(SfxEnum id)
        {
            if (Instance == null)
            {
                Debug.LogWarning($"AudioManager is missing. Cannot play SFX: {id}.");
                return;
            }

            Instance.PlaySfxInternal(id);
        }

        public static void PlayBgm(BgmEnum id)
        {
            if (Instance == null)
            {
                Debug.LogWarning($"AudioManager is missing. Cannot play BGM: {id}.");
                return;
            }

            Instance.PlayBgmInternal(id);
        }

        public static void StopBgm()
        {
            if (Instance != null && Instance.bgmSource != null)
            {
                Instance.bgmSource.Stop();
            }
        }

        public void SetMasterVolume(float value)
        {
            masterVolume = Mathf.Clamp01(value);
            ApplyBgmVolume();
        }

        public void SetSfxVolume(float value)
        {
            sfxVolume = Mathf.Clamp01(value);
        }

        public void SetBgmVolume(float value)
        {
            bgmVolume = Mathf.Clamp01(value);
            ApplyBgmVolume();
        }

        private void PlaySfxInternal(SfxEnum id)
        {
            if (sfxSource == null || audioLibrary == null || !audioLibrary.TryGetSfxEntry(id, out AudioLibrary.SfxEntry entry))
                return;

            sfxSource.PlayOneShot(entry.clip, entry.volume * masterVolume * sfxVolume);
        }

        private void PlayBgmInternal(BgmEnum id)
        {
            if (bgmSource == null || audioLibrary == null || !audioLibrary.TryGetBgmEntry(id, out AudioLibrary.BgmEntry entry))
                return;

            currentBgmEntryVolume = entry.volume;
            bgmSource.clip = entry.clip;
            ApplyBgmVolume();
            bgmSource.Play();
        }

        private void ApplyBgmVolume()
        {
            if (bgmSource != null)
            {
                bgmSource.volume = currentBgmEntryVolume * masterVolume * bgmVolume;
            }
        }
    }
}
