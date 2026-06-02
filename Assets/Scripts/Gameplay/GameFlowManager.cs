using UnityEngine;

namespace Gameplay
{
    public class GameFlowManager : MonoBehaviour
    {
        public BoardManager boardManager;
        public RoomEventHandler roomEventHandler;

        public bool CanMove { get; private set; } = true;

        private RoomCard currentRoom;

        private void Awake()
        {
            if (boardManager == null)
            {
                boardManager = FindObjectOfType<BoardManager>();
            }

            if (boardManager == null)
            {
                Debug.LogError("GameFlowManager needs a BoardManager in the scene.", this);
                enabled = false;
                CanMove = false;
                return;
            }

            if (roomEventHandler == null)
            {
                roomEventHandler = FindObjectOfType<RoomEventHandler>();
            }
        }

        public void EnterRoom(Vector2Int gridPosition)
        {
            if (!CanMove)
                return;

            RoomCard room = boardManager.GetRoom(gridPosition);

            if (room == null || room.hasResolvedEvent)
                return;

            currentRoom = room;
            CanMove = false;

            RoomEventData eventData = room.data != null ? room.data.eventData : null;

            if (roomEventHandler != null)
            {
                roomEventHandler.HandleRoomEvent(room, eventData, ResolveCurrentRoom);
            }
            else
            {
                ResolveCurrentRoom();
            }
        }

        private void ResolveCurrentRoom()
        {
            if (currentRoom == null)
            {
                CanMove = true;
                return;
            }

            currentRoom.hasResolvedEvent = true;

            currentRoom = null;
            CanMove = true;
        }
    }
}
