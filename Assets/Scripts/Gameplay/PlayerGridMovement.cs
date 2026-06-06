using UnityEngine;

namespace Gameplay
{
    public class PlayerGridMovement : MonoBehaviour
    {
        public BoardManager boardManager;
        public InGameManager inGameManager;
        public GameFlowManager gameFlowManager;
        public Vector2Int gridPosition;
        public bool enterStartingRoomOnStart;
        public int playerSortingOrder = 10;

        private void Awake()
        {
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
                if (!boardManager.TryDrawAndPlaceRoom(gridPosition, direction, out RoomCard placedRoom))
                    return;

                nextPosition = placedRoom.gridPosition;
            }

            gridPosition = nextPosition;
            SnapToGridPosition();
            EnterCurrentRoom();
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
            transform.position = boardManager.GridToWorldPosition(gridPosition);
        }
    }
}
