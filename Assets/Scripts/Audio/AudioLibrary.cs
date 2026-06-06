using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Gameplay
{
    [CreateAssetMenu(menuName = "Gameplay/Audio/Audio Library")]
    public class AudioLibrary : ScriptableObject
    {
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

        [FormerlySerializedAs("entries")]
        public List<SfxEntry> sfxEntries = new List<SfxEntry>();
        public List<BgmEntry> bgmEntries = new List<BgmEntry>();

        private Dictionary<SfxEnum, SfxEntry> sfxLookup;
        private Dictionary<BgmEnum, BgmEntry> bgmLookup;

        public bool TryGetSfxEntry(SfxEnum id, out SfxEntry entry)
        {
            entry = null;

            if (id == SfxEnum.None)
                return false;

            if (sfxLookup == null)
            {
                BuildSfxLookup();
            }

            return sfxLookup.TryGetValue(id, out entry)
                && entry != null
                && entry.clip != null;
        }

        public bool TryGetBgmEntry(BgmEnum id, out BgmEntry entry)
        {
            entry = null;

            if (id == BgmEnum.None)
                return false;

            if (bgmLookup == null)
            {
                BuildBgmLookup();
            }

            return bgmLookup.TryGetValue(id, out entry)
                && entry != null
                && entry.clip != null;
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
            if (sfxEntries == null)
            {
                sfxEntries = new List<SfxEntry>();
            }

            if (bgmEntries == null)
            {
                bgmEntries = new List<BgmEntry>();
            }

            AddMissingSfxEntries();
            AddMissingBgmEntries();
            ClampVolumes();

            sfxLookup = null;
            bgmLookup = null;
        }

        private void AddMissingSfxEntries()
        {
            Array values = Enum.GetValues(typeof(SfxEnum));

            for (int i = 0; i < values.Length; i++)
            {
                SfxEnum id = (SfxEnum)values.GetValue(i);

                if (id == SfxEnum.None || HasSfxEntry(id))
                    continue;

                sfxEntries.Add(new SfxEntry { id = id });
            }
        }

        private void AddMissingBgmEntries()
        {
            Array values = Enum.GetValues(typeof(BgmEnum));

            for (int i = 0; i < values.Length; i++)
            {
                BgmEnum id = (BgmEnum)values.GetValue(i);

                if (id == BgmEnum.None || HasBgmEntry(id))
                    continue;

                bgmEntries.Add(new BgmEntry { id = id });
            }
        }

        private void ClampVolumes()
        {
            for (int i = 0; i < sfxEntries.Count; i++)
            {
                SfxEntry entry = sfxEntries[i];

                if (entry != null)
                {
                    entry.volume = Mathf.Clamp01(entry.volume);
                }
            }

            for (int i = 0; i < bgmEntries.Count; i++)
            {
                BgmEntry entry = bgmEntries[i];

                if (entry != null)
                {
                    entry.volume = Mathf.Clamp01(entry.volume);
                }
            }
        }

        private bool HasSfxEntry(SfxEnum id)
        {
            for (int i = 0; i < sfxEntries.Count; i++)
            {
                SfxEntry entry = sfxEntries[i];

                if (entry != null && entry.id == id)
                    return true;
            }

            return false;
        }

        private bool HasBgmEntry(BgmEnum id)
        {
            for (int i = 0; i < bgmEntries.Count; i++)
            {
                BgmEntry entry = bgmEntries[i];

                if (entry != null && entry.id == id)
                    return true;
            }

            return false;
        }
    }
}
