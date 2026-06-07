using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Gameplay
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        public AudioLibrary audioLibrary;
        public AudioSource sfxSource;

        [Serializable]
        public class SfxEntry
        {
            public SfxEnum id;
            public AudioClip clip;

            [Range(0f, 1f)]
            public float volume = 1f;
        }

        [Serializable]
        public class BgmEntry
        {
            public BgmEnum id;
            public AudioClip clip;

            [Range(0f, 1f)]
            public float volume = 1f;
        }

        public List<BgmEntry> bgmEntries = new List<BgmEntry>();
        public List<SfxEntry> sfxEntries = new List<SfxEntry>();

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
        private Dictionary<SfxEnum, SfxEntry> sfxLookup;
        private Dictionary<BgmEnum, BgmEntry> bgmLookup;

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
                GameObject sfxObject = new GameObject("SFX");
                sfxObject.transform.SetParent(transform, false);
                sfxSource = sfxObject.AddComponent<AudioSource>();
            }

            if (bgmSource == null)
            {
                GameObject bgmObject = new GameObject("BGM");
                bgmObject.transform.SetParent(transform, false);
                bgmSource = bgmObject.AddComponent<AudioSource>();
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
            if (sfxSource == null)
                return;

            if (TryGetSfxEntry(id, out SfxEntry entry))
            {
                sfxSource.PlayOneShot(entry.clip, entry.volume * masterVolume * sfxVolume);
            }
        }

        private void PlayBgmInternal(BgmEnum id)
        {
            if (bgmSource == null)
                return;

            if (TryGetBgmEntry(id, out BgmEntry entry))
            {
                currentBgmEntryVolume = entry.volume;
                bgmSource.clip = entry.clip;
                ApplyBgmVolume();
                bgmSource.Play();
            }
        }

        private bool TryGetSfxEntry(SfxEnum id, out SfxEntry entry)
        {
            entry = null;

            if (id == SfxEnum.None)
                return false;

            if (sfxLookup == null)
            {
                BuildSfxLookup();
            }

            if (sfxLookup.TryGetValue(id, out entry) && entry != null && entry.clip != null)
                return true;

            if (audioLibrary != null && audioLibrary.TryGetSfxEntry(id, out AudioLibrary.SfxEntry libraryEntry))
            {
                entry = new SfxEntry
                {
                    id = libraryEntry.id,
                    clip = libraryEntry.clip,
                    volume = libraryEntry.volume
                };
                return true;
            }

            return false;
        }

        private bool TryGetBgmEntry(BgmEnum id, out BgmEntry entry)
        {
            entry = null;

            if (id == BgmEnum.None)
                return false;

            if (bgmLookup == null)
            {
                BuildBgmLookup();
            }

            if (bgmLookup.TryGetValue(id, out entry) && entry != null && entry.clip != null)
                return true;

            if (audioLibrary != null && audioLibrary.TryGetBgmEntry(id, out AudioLibrary.BgmEntry libraryEntry))
            {
                entry = new BgmEntry
                {
                    id = libraryEntry.id,
                    clip = libraryEntry.clip,
                    volume = libraryEntry.volume
                };
                return true;
            }

            return false;
        }

        private void BuildSfxLookup()
        {
            sfxLookup = new Dictionary<SfxEnum, SfxEntry>();

            if (sfxEntries == null)
                return;

            for (int i = 0; i < sfxEntries.Count; i++)
            {
                SfxEntry entry = sfxEntries[i];

                if (entry == null || entry.id == SfxEnum.None || sfxLookup.ContainsKey(entry.id))
                    continue;

                sfxLookup.Add(entry.id, entry);
            }
        }

        private void BuildBgmLookup()
        {
            bgmLookup = new Dictionary<BgmEnum, BgmEntry>();

            if (bgmEntries == null)
                return;

            for (int i = 0; i < bgmEntries.Count; i++)
            {
                BgmEntry entry = bgmEntries[i];

                if (entry == null || entry.id == BgmEnum.None || bgmLookup.ContainsKey(entry.id))
                    continue;

                bgmLookup.Add(entry.id, entry);
            }
        }

        private void OnValidate()
        {
            masterVolume = Mathf.Clamp01(masterVolume);
            sfxVolume = Mathf.Clamp01(sfxVolume);
            bgmVolume = Mathf.Clamp01(bgmVolume);

            if (sfxEntries == null)
            {
                sfxEntries = new List<SfxEntry>();
            }

            if (bgmEntries == null)
            {
                bgmEntries = new List<BgmEntry>();
            }

            sfxLookup = null;
            bgmLookup = null;
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
