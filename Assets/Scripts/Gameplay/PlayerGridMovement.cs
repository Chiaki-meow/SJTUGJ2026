using System.Collections;
using UnityEngine;

namespace Gameplay
{
    public class PlayerGridMovement : MonoBehaviour
    {
        public BoardManager boardManager;
        public InGameManager inGameManager;
        public GameFlowManager gameFlowManager;
        public RoomSelectionHandler roomSelectionHandler;
        public Vector2Int gridPosition;
        public bool enterStartingRoomOnStart;
        public int playerSortingOrder = 10;
        public Vector2 uiOrigin;
        public float uiTileSize = 503f;
        public float moveAnimationDuration = 0.25f;

        private RectTransform rectTransform;
        private bool isWaitingForRoomSelection;
        private bool isMoving;
        private Coroutine moveRoutine;
        private RoomPlacementOption pendingPlacementOption;
        private Vector2Int pendingPlacementOrigin;
        private Vector2Int pendingPlacementDirection;
        private readonly System.Collections.Generic.List<RoomPlacementOption> roomChoices = new();

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = playerSortingOrder;
            }
        }

        private void Start()
        {
            if (boardManager == null)
            {
                boardManager = FindObjectOfType<BoardManager>();
            }

            if (inGameManager == null)
            {
                inGameManager = InGameManager.Instance != null ? InGameManager.Instance : FindObjectOfType<InGameManager>();
            }

            if (gameFlowManager == null)
            {
                gameFlowManager = FindObjectOfType<GameFlowManager>();
            }

            if (roomSelectionHandler == null)
            {
                roomSelectionHandler = boardManager != null && boardManager.roomSelectionHandler != null
                    ? boardManager.roomSelectionHandler
                    : FindObjectOfType<RoomSelectionHandler>(true);
            }

            if (boardManager == null)
            {
                Debug.LogError("PlayerGridMovement needs a BoardManager in the scene.", this);
                enabled = false;
                return;
            }

            SnapToGridPosition();

            if (enterStartingRoomOnStart)
            {
                EnterCurrentRoom();
            }
        }

        private void Update()
        {
            if (isWaitingForRoomSelection || isMoving)
                return;

            if (inGameManager != null && !inGameManager.CanPlayerAct)
                return;

            if (inGameManager == null && gameFlowManager != null && !gameFlowManager.CanMove)
                return;

            Vector2Int direction = ReadMoveDirection();

            if (direction == Vector2Int.zero)
                return;

            TryMove(direction);
        }

        private Vector2Int ReadMoveDirection()
        {
            if (Input.GetKeyDown(KeyCode.W))
                return Vector2Int.up;

            if (Input.GetKeyDown(KeyCode.D))
                return Vector2Int.right;

            if (Input.GetKeyDown(KeyCode.S))
                return Vector2Int.down;

            if (Input.GetKeyDown(KeyCode.A))
                return Vector2Int.left;

            return Vector2Int.zero;
        }

        private void TryMove(Vector2Int direction)
        {
            Vector2Int nextPosition = gridPosition + direction;

            if (!boardManager.HasRoom(nextPosition))
            {
                if (!boardManager.CanPlaceRoom(gridPosition, direction, out string failureReason))
                {
                    ShowPlacementFailed(failureReason);
                    return;
                }

                if (roomSelectionHandler != null && boardManager.GetRoomOptions(roomChoices, boardManager.roomChoiceCount, gridPosition, direction) > 0)
                {
                    isWaitingForRoomSelection = true;
                    Vector2Int selectedDirection = direction;
                    roomSelectionHandler.ShowRoomSelection(roomChoices, selectedOption => HandleRoomSelected(selectedDirection, selectedOption));
                    return;
                }

                if (!boardManager.TryDrawAndPlaceRoom(gridPosition, direction, out RoomCard placedRoom))
                    return;

                nextPosition = placedRoom.gridPosition;
            }

            MoveToGridPosition(nextPosition);
        }

        private void HandleRoomSelected(Vector2Int direction, RoomPlacementOption selectedOption)
        {
            isWaitingForRoomSelection = false;

            if (selectedOption == null)
                return;

            if (inGameManager != null && !inGameManager.CanPlayerAct)
            {
                isWaitingForRoomSelection = false;
                return;
            }

            Vector2Int requiredDoor = GetOppositeDirection(direction);
            roomSelectionHandler.ShowRoomRotation(selectedOption, requiredDoor, rotatedOption => HandleRoomRotationConfirmed(direction, rotatedOption));
        }

        private void HandleRoomRotationConfirmed(Vector2Int direction, RoomPlacementOption selectedOption)
        {
            isWaitingForRoomSelection = false;

            if (selectedOption == null)
                return;

            if (inGameManager != null && !inGameManager.CanPlayerAct)
                return;

            if (!boardManager.CanPlaceSelectedRoom(gridPosition, direction, selectedOption, out _))
            {
                ShowPlacementFailed("选择的房间无法与当前门对齐。");
                return;
            }

            pendingPlacementOrigin = gridPosition;
            pendingPlacementDirection = direction;
            pendingPlacementOption = selectedOption;
            MoveToGridPosition(gridPosition + direction);
        }

        private void ShowPlacementFailed(string message)
        {
            if (roomSelectionHandler != null)
            {
                isWaitingForRoomSelection = true;
                roomSelectionHandler.ShowPlacementFailed(message, () => isWaitingForRoomSelection = false);
            }
        }

        private void EnterCurrentRoom()
        {
            if (inGameManager != null)
            {
                inGameManager.EnterRoom(gridPosition);
            }
            else if (gameFlowManager != null)
            {
                gameFlowManager.EnterRoom(gridPosition);
            }
            else
            {
                RoomCard room = boardManager.GetRoom(gridPosition);
                if (room != null)
                {
                    room.hasResolvedEvent = true;
                }
            }
        }

        private void MoveToGridPosition(Vector2Int nextPosition)
        {
            if (rectTransform == null || moveAnimationDuration <= 0f)
            {
                gridPosition = nextPosition;
                SnapToGridPosition();
                ResolvePendingPlacement();
                EnterCurrentRoom();
                return;
            }

            if (moveRoutine != null)
            {
                StopCoroutine(moveRoutine);
            }

            Vector2 startPosition = GetUiAnchoredPosition(gridPosition);
            Vector2 targetPosition = GetUiAnchoredPosition(nextPosition);
            gridPosition = nextPosition;
            moveRoutine = StartCoroutine(MoveToGridPositionRoutine(startPosition, targetPosition));
        }

        private IEnumerator MoveToGridPositionRoutine(Vector2 startPosition, Vector2 targetPosition)
        {
            isMoving = true;
            float elapsed = 0f;

            while (elapsed < moveAnimationDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / moveAnimationDuration);
                float easedT = t * t * (3f - 2f * t);
                rectTransform.anchoredPosition = Vector2.LerpUnclamped(startPosition, targetPosition, easedT);
                yield return null;
            }

            rectTransform.anchoredPosition = targetPosition;
            isMoving = false;
            moveRoutine = null;
            ResolvePendingPlacement();
            EnterCurrentRoom();
        }

        private void ResolvePendingPlacement()
        {
            if (pendingPlacementOption == null)
                return;

            if (!boardManager.TryPlaceSelectedRoom(pendingPlacementOrigin, pendingPlacementDirection, pendingPlacementOption, out _))
            {
                ShowPlacementFailed("选择的房间无法放置。可能已被占用或门未对齐。");
            }

            pendingPlacementOption = null;
        }

        private void SnapToGridPosition()
        {
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = GetUiAnchoredPosition(gridPosition);
                return;
            }

            transform.position = boardManager.GridToWorldPosition(gridPosition);
        }

        private Vector2 GetUiAnchoredPosition(Vector2Int targetGridPosition)
        {
            return boardManager != null
                ? boardManager.GridToUiAnchoredPosition(targetGridPosition)
                : uiOrigin + new Vector2(targetGridPosition.x * uiTileSize, targetGridPosition.y * uiTileSize);
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
    }
}
