using System.Collections.Generic;
using UnityEngine;

namespace Gameplay
{
    public class BoardManager : MonoBehaviour
    {
        public RoomCard roomPrefab;
        public RoomCardData startRoom;
        public List<RoomCardData> deck = new();

        public float tileSize = 3f;

        private Dictionary<Vector2Int, RoomCard> placedRooms = new();

        private void Awake()
        {
            if (roomPrefab == null)
            {
                Debug.LogError("BoardManager needs a room prefab.", this);
                return;
            }

            if (startRoom == null)
            {
                Debug.LogError("BoardManager needs a start room.", this);
                return;
            }

            PlaceRoom(startRoom, Vector2Int.zero);
        }

        public RoomCard GetRoom(Vector2Int position)
        {
            placedRooms.TryGetValue(position, out RoomCard room);
            return room;
        }

        public bool TryDrawAndPlaceRoom(Vector2Int origin, Vector2Int direction, out RoomCard placedRoom)
        {
            placedRoom = null;

            RoomCard originRoom = GetRoom(origin);
            Vector2Int nextPosition = origin + direction;

            if (originRoom == null)
            {
                Debug.LogWarning($"Cannot place room from {origin}: no origin room.");
                return false;
            }

            if (originRoom.remainingDoors <= 0)
            {
                Debug.LogWarning($"Cannot place room from {origin}: no remaining doors.", originRoom);
                return false;
            }

            if (placedRooms.ContainsKey(nextPosition))
            {
                Debug.LogWarning($"Cannot place room at {nextPosition}: position is already occupied.");
                return false;
            }

            RoomCardData card = DrawCard();

            if (card == null)
            {
                Debug.LogWarning("Cannot place room: deck is empty.", this);
                return false;
            }

            placedRoom = PlaceRoom(card, nextPosition);
            originRoom.remainingDoors--;

            return placedRoom != null;
        }

        public bool TryPlaceFixedRoom(RoomCardData card, Vector2Int gridPosition, out RoomCard placedRoom)
        {
            placedRoom = null;

            if (card == null)
            {
                Debug.LogWarning("Cannot place fixed room: room data is null.", this);
                return false;
            }

            if (roomPrefab == null)
            {
                Debug.LogWarning("Cannot place fixed room: room prefab is null.", this);
                return false;
            }

            if (placedRooms.ContainsKey(gridPosition))
            {
                Debug.LogWarning($"Cannot place fixed room at {gridPosition}: position is already occupied.", this);
                return false;
            }

            placedRoom = PlaceRoom(card, gridPosition);
            return placedRoom != null;
        }

        public bool HasRoom(Vector2Int position)
        {
            return placedRooms.ContainsKey(position);
        }

        public Vector3 GridToWorldPosition(Vector2Int gridPosition)
        {
            return new Vector3(
                gridPosition.x * tileSize,
                gridPosition.y * tileSize,
                0f
            );
        }

        private RoomCard PlaceRoom(RoomCardData card, Vector2Int gridPosition)
        {
            Vector3 worldPosition = GridToWorldPosition(gridPosition);

            RoomCard room = Instantiate(roomPrefab, worldPosition, Quaternion.identity);
            room.Init(card, gridPosition);

            placedRooms.Add(gridPosition, room);
            return room;
        }

        private RoomCardData DrawCard()
        {
            if (deck.Count == 0)
                return null;

            int index = Random.Range(0, deck.Count);
            RoomCardData card = deck[index];
            deck.RemoveAt(index);

            return card;
        }
    }
}
