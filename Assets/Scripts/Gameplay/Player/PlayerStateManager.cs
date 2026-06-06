using System;
using UnityEngine;

namespace Gameplay
{
    public class PlayerStateManager : MonoBehaviour
    {
        public static PlayerStateManager Instance { get; private set; }

        [SerializeField] private int physical = 3;
        [SerializeField] private int mental = 3;
        [SerializeField] private int maxHealth = 3;
        [SerializeField] private int health = 3;

        public int Physical => physical;
        public int Mental => mental;
        public int MaxHealth => maxHealth;
        public int Health => health;
        public bool IsDead => health <= 0;

        public event Action OnStateChanged;
        public event Action OnDied;

        private bool hasNotifiedDeath;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            ClampState();
            hasNotifiedDeath = IsDead;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public int GetStatValue(CharacterStat stat)
        {
            switch (stat)
            {
                case CharacterStat.Physical:
                    return physical;
                case CharacterStat.Mental:
                    return mental;
                case CharacterStat.Health:
                    return health;
                default:
                    return 0;
            }
        }

        public void ApplyStatChange(CharacterStat stat, int delta)
        {
            switch (stat)
            {
                case CharacterStat.Physical:
                    SetPhysical(physical + delta);
                    break;
                case CharacterStat.Mental:
                    SetMental(mental + delta);
                    break;
                case CharacterStat.Health:
                    ChangeHealth(delta);
                    break;
            }
        }

        public void SetPhysical(int value)
        {
            int clampedValue = Mathf.Max(0, value);
            if (physical == clampedValue)
                return;

            physical = clampedValue;
            NotifyStateChanged();
        }

        public void SetMental(int value)
        {
            int clampedValue = Mathf.Max(0, value);
            if (mental == clampedValue)
                return;

            mental = clampedValue;
            NotifyStateChanged();
        }

        public void SetMaxHealth(int value, bool keepCurrentHealthRatio = false)
        {
            int clampedValue = Mathf.Max(1, value);
            if (maxHealth == clampedValue)
                return;

            float healthRatio = maxHealth > 0 ? health / (float)maxHealth : 1f;
            maxHealth = clampedValue;
            health = keepCurrentHealthRatio ? Mathf.Clamp(Mathf.RoundToInt(maxHealth * healthRatio), 0, maxHealth) : Mathf.Min(health, maxHealth);
            NotifyStateChanged();
            CheckDeath();
        }

        public void Heal(int amount)
        {
            if (amount <= 0)
                return;

            ChangeHealth(amount);
        }

        public void Damage(int amount)
        {
            if (amount <= 0)
                return;

            ChangeHealth(-amount);
        }

        public void ChangeHealth(int delta)
        {
            if (delta == 0)
                return;

            int clampedValue = Mathf.Clamp(health + delta, 0, maxHealth);
            if (health == clampedValue)
                return;

            health = clampedValue;
            NotifyStateChanged();
            CheckDeath();
        }

        public void ResetState(int newPhysical, int newMental, int newMaxHealth)
        {
            physical = Mathf.Max(0, newPhysical);
            mental = Mathf.Max(0, newMental);
            maxHealth = Mathf.Max(1, newMaxHealth);
            health = maxHealth;
            hasNotifiedDeath = false;
            NotifyStateChanged();
        }

        private void ClampState()
        {
            physical = Mathf.Max(0, physical);
            mental = Mathf.Max(0, mental);
            maxHealth = Mathf.Max(1, maxHealth);
            health = Mathf.Clamp(health, 0, maxHealth);
        }

        private void NotifyStateChanged()
        {
            OnStateChanged?.Invoke();
        }

        private void CheckDeath()
        {
            if (!IsDead || hasNotifiedDeath)
                return;

            hasNotifiedDeath = true;
            OnDied?.Invoke();
        }
    }
}
