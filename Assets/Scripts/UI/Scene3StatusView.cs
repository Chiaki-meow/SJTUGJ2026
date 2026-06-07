using Gameplay;
using TMPro;
using UnityEngine;

namespace UI
{
    public class Scene3StatusView : MonoBehaviour
    {
        public PlayerStateManager playerStateManager;
        public InGameManager inGameManager;
        public TMP_Text physicalText;
        public TMP_Text mentalText;
        public TMP_Text healthText;
        public TMP_Text turnText;
        public TMP_Text omenText;
        public TMP_Text phaseText;

        private void OnEnable()
        {
            ResolveReferences();
            Subscribe();
            RefreshAll();
        }

        private void Start()
        {
            ResolveReferences();
            RefreshAll();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void ResolveReferences()
        {
            if (playerStateManager == null)
            {
                playerStateManager = PlayerStateManager.Instance != null ? PlayerStateManager.Instance : FindObjectOfType<PlayerStateManager>();
            }

            if (inGameManager == null)
            {
                inGameManager = InGameManager.Instance != null ? InGameManager.Instance : FindObjectOfType<InGameManager>();
            }
        }

        private void Subscribe()
        {
            if (playerStateManager != null)
            {
                playerStateManager.OnStateChanged += RefreshPlayerState;
            }

            if (inGameManager != null)
            {
                inGameManager.OnTurnCountChanged += RefreshTurn;
                inGameManager.OnOmenCountChanged += RefreshOmen;
                inGameManager.OnPhaseChanged += RefreshPhase;
            }
        }

        private void Unsubscribe()
        {
            if (playerStateManager != null)
            {
                playerStateManager.OnStateChanged -= RefreshPlayerState;
            }

            if (inGameManager != null)
            {
                inGameManager.OnTurnCountChanged -= RefreshTurn;
                inGameManager.OnOmenCountChanged -= RefreshOmen;
                inGameManager.OnPhaseChanged -= RefreshPhase;
            }
        }

        private void RefreshAll()
        {
            RefreshPlayerState();
            if (inGameManager != null)
            {
                RefreshTurn(inGameManager.TurnCount);
                RefreshOmen(inGameManager.OmenCount);
                RefreshPhase(inGameManager.Phase);
            }
        }

        private void RefreshPlayerState()
        {
            if (playerStateManager == null)
                return;

            SetText(physicalText, playerStateManager.Physical.ToString());
            SetText(mentalText, playerStateManager.Mental.ToString());
            SetText(healthText, $"{playerStateManager.Health}/{playerStateManager.MaxHealth}");
        }

        private void RefreshTurn(int value)
        {
            SetText(turnText, $"当前回合： {value + 1}");
        }

        private void RefreshOmen(int value)
        {
            SetText(omenText, $"预兆 {value}");
        }

        private void RefreshPhase(InGamePhase phase)
        {
            SetText(phaseText, phase.ToString());
        }

        private static void SetText(TMP_Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }
    }
}
