using System;
using UnityEngine;

namespace Gameplay
{
    public enum Phase2State
    {
        NotStarted,
        Active,
        Completed
    }

    public enum Phase2Route
    {
        None,
        BloodyKnife,
        PatientLetter
    }

    public class Phase2Director : MonoBehaviour
    {
        public BoardManager boardManager;
        public InGameManager inGameManager;
        public PlayerInventory playerInventory;
        public PlayerStateManager playerStateManager;

        [Header("Key Content")]
        public RoomCardData deanOfficeRoomCard;
        public Vector2Int deanOfficeGridPosition = new Vector2Int(3, 3);
        public ItemData bloodyKnifeItem;
        public ItemData patientLetterItem;
        public ItemData patientDiaryItem;

        [Header("Patience")]
        public int bloodyKnifeInitialPatience = 3;
        public int patientLetterInitialPatience = 7;

        private Phase2State state = Phase2State.NotStarted;
        private Phase2Route currentRoute = Phase2Route.None;
        private int currentPatience;
        private int difficultyBonus;
        private int successfulDeanChecks;
        private RoomCard deanOfficeRoom;
        private RoomCard phase2TriggerRoom;

        public Phase2State State => state;
        public Phase2Route CurrentRoute => currentRoute;
        public int CurrentPatience => currentPatience;
        public int DifficultyBonus => difficultyBonus;
        public int SuccessfulDeanChecks => successfulDeanChecks;
        public RoomCard DeanOfficeRoom => deanOfficeRoom;
        public bool IsActive => state == Phase2State.Active;

        public event Action<Phase2Route> OnPhase2Started;
        public event Action<int> OnPatienceChanged;
        public event Action<int> OnDifficultyBonusChanged;
        public event Action<bool> OnPhase2Completed;

        private void Start()
        {
            ResolveReferences();
            SubscribeInGameEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeInGameEvents();
        }

        public void TriggerTruthRevealFailed()
        {
            BeginPhase2(ResolveRouteFromInventory());
        }

        public bool BeginPhase2(Phase2Route route)
        {
            if (state != Phase2State.NotStarted || route == Phase2Route.None)
                return false;

            ResolveReferences();

            currentRoute = route;
            state = Phase2State.Active;
            currentPatience = GetInitialPatience(route);
            difficultyBonus = 0;
            successfulDeanChecks = 0;
            phase2TriggerRoom = inGameManager != null ? inGameManager.CurrentRoom : null;
            PlaceDeanOfficeRoom();
            OnPhase2Started?.Invoke(currentRoute);
            OnPatienceChanged?.Invoke(currentPatience);
            OnDifficultyBonusChanged?.Invoke(difficultyBonus);
            return true;
        }

        public Phase2Route ResolveRouteFromInventory()
        {
            ResolveReferences();

            if (playerInventory != null && bloodyKnifeItem != null && playerInventory.Contains(bloodyKnifeItem))
                return Phase2Route.BloodyKnife;

            if (playerInventory != null && patientLetterItem != null && playerInventory.Contains(patientLetterItem))
                return Phase2Route.PatientLetter;

            return Phase2Route.None;
        }

        public int GetCheckTarget(int baseTarget)
        {
            return baseTarget + difficultyBonus;
        }

        public bool ShouldSkipFirstPatientLetterCheck()
        {
            return currentRoute == Phase2Route.PatientLetter
                && playerInventory != null
                && patientDiaryItem != null
                && playerInventory.Contains(patientDiaryItem);
        }

        public bool RegisterDeanCheckSuccess(int requiredSuccessCount = 2)
        {
            if (state != Phase2State.Active)
                return false;

            successfulDeanChecks++;

            if (successfulDeanChecks < requiredSuccessCount)
                return false;

            CompletePhase2(true);
            return true;
        }

        public void RegisterDeanCheckFailure(int damage = 1)
        {
            if (state != Phase2State.Active)
                return;

            ResolveReferences();

            if (playerStateManager != null)
            {
                playerStateManager.Damage(damage);

                if (playerStateManager.IsDead)
                {
                    CompletePhase2(false);
                }
            }
        }

        public void CompletePhase2(bool isWin)
        {
            if (state == Phase2State.Completed)
                return;

            state = Phase2State.Completed;
            OnPhase2Completed?.Invoke(isWin);

            if (inGameManager != null)
            {
                inGameManager.EndGame(isWin);
            }
        }

        private int GetInitialPatience(Phase2Route route)
        {
            switch (route)
            {
                case Phase2Route.BloodyKnife:
                    return Mathf.Max(0, bloodyKnifeInitialPatience);
                case Phase2Route.PatientLetter:
                    return Mathf.Max(0, patientLetterInitialPatience);
                default:
                    return 0;
            }
        }

        private void PlaceDeanOfficeRoom()
        {
            if (boardManager == null || deanOfficeRoomCard == null)
                return;

            Vector2Int targetPosition = GetDeanOfficePosition();
            boardManager.TryPlaceFixedRoom(deanOfficeRoomCard, targetPosition, out deanOfficeRoom);
        }

        private Vector2Int GetDeanOfficePosition()
        {
            if (boardManager == null || phase2TriggerRoom == null)
                return deanOfficeGridPosition;

            Vector2Int origin = phase2TriggerRoom.gridPosition;
            Vector2Int[] directions =
            {
                Vector2Int.right,
                Vector2Int.up,
                Vector2Int.left,
                Vector2Int.down
            };

            for (int i = 0; i < directions.Length; i++)
            {
                Vector2Int position = origin + directions[i];
                if (!boardManager.HasRoom(position) && CanConnectToDeanOffice(directions[i]))
                {
                    return position;
                }
            }

            return deanOfficeGridPosition;
        }

        private void HandleRoomResolved(RoomCard room)
        {
            if (state != Phase2State.Active || room == null || room == deanOfficeRoom)
                return;

            if (room == phase2TriggerRoom)
            {
                phase2TriggerRoom = null;
                return;
            }

            if (currentPatience > 0)
            {
                currentPatience--;
                OnPatienceChanged?.Invoke(currentPatience);
                return;
            }

            difficultyBonus++;
            OnDifficultyBonusChanged?.Invoke(difficultyBonus);
        }

        private bool CanConnectToDeanOffice(Vector2Int direction)
        {
            return phase2TriggerRoom != null
                && phase2TriggerRoom.data != null
                && deanOfficeRoomCard != null
                && phase2TriggerRoom.data.HasDoor(direction)
                && deanOfficeRoomCard.HasDoor(GetOppositeDirection(direction));
        }

        private static Vector2Int GetOppositeDirection(Vector2Int direction)
        {
            if (direction == Vector2Int.up)
                return Vector2Int.down;

            if (direction == Vector2Int.down)
                return Vector2Int.up;

            if (direction == Vector2Int.left)
                return Vector2Int.right;

            if (direction == Vector2Int.right)
                return Vector2Int.left;

            return Vector2Int.zero;
        }

        private void ResolveReferences()
        {
            if (boardManager == null)
            {
                boardManager = FindObjectOfType<BoardManager>();
            }

            if (inGameManager == null)
            {
                inGameManager = InGameManager.Instance != null ? InGameManager.Instance : FindObjectOfType<InGameManager>();
            }

            if (playerInventory == null)
            {
                playerInventory = inGameManager != null && inGameManager.playerInventory != null
                    ? inGameManager.playerInventory
                    : FindObjectOfType<PlayerInventory>();
            }

            if (playerStateManager == null)
            {
                playerStateManager = PlayerStateManager.Instance != null ? PlayerStateManager.Instance : FindObjectOfType<PlayerStateManager>();
            }
        }

        private void SubscribeInGameEvents()
        {
            if (inGameManager != null)
            {
                inGameManager.OnRoomResolved += HandleRoomResolved;
            }
        }

        private void UnsubscribeInGameEvents()
        {
            if (inGameManager != null)
            {
                inGameManager.OnRoomResolved -= HandleRoomResolved;
            }
        }
    }
}
