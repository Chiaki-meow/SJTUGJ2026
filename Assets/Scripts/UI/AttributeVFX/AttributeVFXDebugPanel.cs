using Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.AttributeVFX
{
    public class AttributeVFXDebugPanel : MonoBehaviour
    {
        [SerializeField] private PlayerStateManager playerStateManager;
        [SerializeField] private Button damageHealthButton;
        [SerializeField] private Button reduceMentalButton;
        [SerializeField] private Button reducePhysicalButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private TMP_Text stateText;

        private bool hasListeners;

        private void OnEnable()
        {
            ResolveReferences();
            AddListeners();
            RefreshStateText();
        }

        private void Start()
        {
            ResolveReferences();
            AddListeners();
            RefreshStateText();
        }

        private void OnDisable()
        {
            RemoveListeners();
        }

        private void OnDestroy()
        {
            RemoveListeners();
        }

        private void ResolveReferences()
        {
            if (playerStateManager == null)
            {
                playerStateManager = PlayerStateManager.Instance != null ? PlayerStateManager.Instance : FindObjectOfType<PlayerStateManager>();
            }
        }

        private void AddListeners()
        {
            if (hasListeners)
                return;

            if (damageHealthButton != null)
            {
                damageHealthButton.onClick.AddListener(DamageHealth);
            }

            if (reduceMentalButton != null)
            {
                reduceMentalButton.onClick.AddListener(ReduceMental);
            }

            if (reducePhysicalButton != null)
            {
                reducePhysicalButton.onClick.AddListener(ReducePhysical);
            }

            if (resetButton != null)
            {
                resetButton.onClick.AddListener(ResetState);
            }

            if (playerStateManager != null)
            {
                playerStateManager.OnStateChanged += RefreshStateText;
            }

            hasListeners = true;
        }

        private void RemoveListeners()
        {
            if (!hasListeners)
                return;

            if (damageHealthButton != null)
            {
                damageHealthButton.onClick.RemoveListener(DamageHealth);
            }

            if (reduceMentalButton != null)
            {
                reduceMentalButton.onClick.RemoveListener(ReduceMental);
            }

            if (reducePhysicalButton != null)
            {
                reducePhysicalButton.onClick.RemoveListener(ReducePhysical);
            }

            if (resetButton != null)
            {
                resetButton.onClick.RemoveListener(ResetState);
            }

            if (playerStateManager != null)
            {
                playerStateManager.OnStateChanged -= RefreshStateText;
            }

            hasListeners = false;
        }

        private void DamageHealth()
        {
            if (playerStateManager != null)
            {
                playerStateManager.Damage(1);
            }
        }

        private void ReduceMental()
        {
            if (playerStateManager != null)
            {
                playerStateManager.ApplyStatChange(CharacterStat.Mental, -1);
            }
        }

        private void ReducePhysical()
        {
            if (playerStateManager != null)
            {
                playerStateManager.ApplyStatChange(CharacterStat.Physical, -1);
            }
        }

        private void ResetState()
        {
            if (playerStateManager != null)
            {
                playerStateManager.ResetState(3, 3, 3);
            }
        }

        private void RefreshStateText()
        {
            if (stateText == null || playerStateManager == null)
                return;

            stateText.text = $"Physical: {playerStateManager.Physical}\nMental: {playerStateManager.Mental}\nHealth: {playerStateManager.Health}/{playerStateManager.MaxHealth}";
        }
    }
}
