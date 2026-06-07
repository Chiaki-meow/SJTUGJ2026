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

        private RectTransform rectTransform;
        private bool isWaitingForRoomSelection;
        private readonly System.Collections.Generic.List<RoomCardData> roomChoices = new();

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
            if (isWaitingForRoomSelection)
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

                if (roomSelectionHandler != null && boardManager.GetRoomChoices(roomChoices, boardManager.roomChoiceCount, gridPosition, direction) > 0)
                {
                    isWaitingForRoomSelection = true;
                    Vector2Int selectedDirection = direction;
                    roomSelectionHandler.ShowRoomSelection(roomChoices, selectedCard => HandleRoomSelected(selectedDirection, selectedCard));
                    return;
                }

                if (!boardManager.TryDrawAndPlaceRoom(gridPosition, direction, out RoomCard placedRoom))
                    return;

                nextPosition = placedRoom.gridPosition;
            }

            gridPosition = nextPosition;
            SnapToGridPosition();
            EnterCurrentRoom();
        }

        private void HandleRoomSelected(Vector2Int direction, RoomCardData selectedCard)
        {
            isWaitingForRoomSelection = false;

            if (selectedCard == null)
                return;

            if (inGameManager != null && !inGameManager.CanPlayerAct)
                return;

            if (!boardManager.TryPlaceSelectedRoom(gridPosition, direction, selectedCard, out RoomCard placedRoom))
            {
                ShowPlacementFailed("选择的房间无法与当前门对齐。");
                return;
            }

            gridPosition = placedRoom.gridPosition;
            SnapToGridPosition();
            EnterCurrentRoom();
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

        private void SnapToGridPosition()
        {
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = boardManager != null
                    ? boardManager.GridToUiAnchoredPosition(gridPosition)
                    : uiOrigin + new Vector2(gridPosition.x * uiTileSize, gridPosition.y * uiTileSize);
                return;
            }

            transform.position = boardManager.GridToWorldPosition(gridPosition);
        }
    }
}
