using Gameplay;
using UnityEngine;

namespace UI.AttributeVFX
{
    public class AttributeChangeVFXController : MonoBehaviour
    {
        [SerializeField] private PlayerStateManager playerStateManager;
        [SerializeField] private AttributeChangeVFXView view;

        private int lastPhysical;
        private int lastMental;
        private int lastHealth;
        private bool hasSnapshot;
        private bool subscribed;

        private void OnEnable()
        {
            ResolveReferences();
            Subscribe();
            TakeSnapshot();
        }

        private void Start()
        {
            ResolveReferences();
            Subscribe();
            TakeSnapshot();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void ResolveReferences()
        {
            if (playerStateManager == null)
            {
                playerStateManager = PlayerStateManager.Instance != null ? PlayerStateManager.Instance : FindObjectOfType<PlayerStateManager>();
            }

            if (view == null)
            {
                view = GetComponent<AttributeChangeVFXView>();
            }
        }

        private void Subscribe()
        {
            if (subscribed || playerStateManager == null)
                return;

            playerStateManager.OnStateChanged += HandleStateChanged;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || playerStateManager == null)
                return;

            playerStateManager.OnStateChanged -= HandleStateChanged;
            subscribed = false;
        }

        private void TakeSnapshot()
        {
            if (playerStateManager == null)
                return;

            lastPhysical = playerStateManager.Physical;
            lastMental = playerStateManager.Mental;
            lastHealth = playerStateManager.Health;
            hasSnapshot = true;
        }

        private void HandleStateChanged()
        {
            if (playerStateManager == null || view == null)
                return;

            if (!hasSnapshot)
            {
                TakeSnapshot();
                return;
            }

            int physicalDrop = lastPhysical - playerStateManager.Physical;
            int mentalDrop = lastMental - playerStateManager.Mental;
            int healthDrop = lastHealth - playerStateManager.Health;

            if (mentalDrop > 0)
            {
                view.PlayMentalDrop(mentalDrop);
            }

            if (physicalDrop > 0)
            {
                view.PlayPhysicalDrop(physicalDrop);
            }

            if (healthDrop > 0)
            {
                view.PlayHealthDrop(healthDrop);
            }

            TakeSnapshot();
        }
    }
}
