using System.Collections.Generic;
using UnityEngine;

namespace Gameplay
{
    public class BoardManager : MonoBehaviour
    {
        public RoomCard roomPrefab;
        public RoomCardData startRoom;
        public List<RoomCardData> deck = new();

        public RoomSelectionHandler roomSelectionHandler;
        [Range(1, 3)] public int roomChoiceCount = 3;

        public float tileSize = 3f;
        public RectTransform uiRoomParent;
        public Vector2 uiOrigin;
        public float uiTileSize = 503f;

        private Dictionary<Vector2Int, RoomCard> placedRooms = new();
        private readonly List<RoomCardData> candidateRooms = new();

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

            if (!CanPlaceFromOrigin(origin, direction, out string failureReason))
            {
                Debug.LogWarning(failureReason, this);
                return false;
            }

            RoomCard originRoom = GetRoom(origin);
            Vector2Int nextPosition = origin + direction;
            RoomCardData card = DrawCard(origin, direction);

            if (card == null)
            {
                Debug.LogWarning("Cannot place room: deck is empty.", this);
                return false;
            }

            placedRoom = PlaceRoom(card, nextPosition);
            if (placedRoom != null)
            {
                placedRoom.remainingDoors = Mathf.Max(0, placedRoom.remainingDoors - 1);
                originRoom.remainingDoors--;
            }

            return placedRoom != null;
        }

        public bool TryPlaceSelectedRoom(Vector2Int origin, Vector2Int direction, RoomCardData card, out RoomCard placedRoom)
        {
            placedRoom = null;

            if (!CanPlaceFromOrigin(origin, direction, out string failureReason))
            {
                Debug.LogWarning(failureReason, this);
                return false;
            }

            RoomCard originRoom = GetRoom(origin);
            Vector2Int nextPosition = origin + direction;

            if (card == null)
            {
                Debug.LogWarning("Cannot place room: selected card is null.", this);
                return false;
            }

            if (!card.HasDoor(GetOppositeDirection(direction)))
            {
                Debug.LogWarning($"Cannot place room: selected card {card.name} has no matching entrance.", this);
                return false;
            }

            if (!deck.Remove(card))
            {
                Debug.LogWarning($"Cannot place room: selected card {card.name} is no longer in deck.", this);
                return false;
            }

            placedRoom = PlaceRoom(card, nextPosition);
            if (placedRoom != null)
            {
                placedRoom.remainingDoors = Mathf.Max(0, placedRoom.remainingDoors - 1);
                originRoom.remainingDoors--;
            }

            return placedRoom != null;
        }

        public int GetRoomChoices(List<RoomCardData> results, int count, Vector2Int origin, Vector2Int direction)
        {
            if (results == null)
                return 0;

            results.Clear();

            if (deck.Count == 0 || count <= 0 || !CanPlaceFromOrigin(origin, direction, out _))
                return 0;

            FillCandidates(direction);

            if (candidateRooms.Count == 0)
                return 0;

            int attempts = candidateRooms.Count * 2;

            while (results.Count < count && attempts > 0)
            {
                attempts--;
                RoomCardData card = candidateRooms[Random.Range(0, candidateRooms.Count)];
                if (card != null && !results.Contains(card))
                {
                    results.Add(card);
                }
            }

            return results.Count;
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

        public bool CanPlaceRoom(Vector2Int origin, Vector2Int direction, out string failureReason)
        {
            if (!CanPlaceFromOrigin(origin, direction, out failureReason))
                return false;

            FillCandidates(direction);
            if (candidateRooms.Count == 0)
            {
                failureReason = "随机池里没有能与这扇门对齐的房间。";
                return false;
            }

            failureReason = string.Empty;
            return true;
        }

        public Vector3 GridToWorldPosition(Vector2Int gridPosition)
        {
            return new Vector3(
                gridPosition.x * tileSize,
                gridPosition.y * tileSize,
                0f
            );
        }

        public Vector2 GridToUiAnchoredPosition(Vector2Int gridPosition)
        {
            return uiOrigin + new Vector2(
                gridPosition.x * uiTileSize,
                gridPosition.y * uiTileSize
            );
        }

        private RoomCard PlaceRoom(RoomCardData card, Vector2Int gridPosition)
        {
            RoomCard room;

            if (uiRoomParent != null)
            {
                room = Instantiate(roomPrefab, uiRoomParent);
                RectTransform rectTransform = room.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    rectTransform.anchoredPosition = GridToUiAnchoredPosition(gridPosition);
                    rectTransform.localScale = Vector3.one;
                }
            }
            else
            {
                Vector3 worldPosition = GridToWorldPosition(gridPosition);
                room = Instantiate(roomPrefab, worldPosition, Quaternion.identity);
            }

            room.Init(card, gridPosition);
            placedRooms.Add(gridPosition, room);
            return room;
        }

        private RoomCardData DrawCard(Vector2Int origin, Vector2Int direction)
        {
            FillCandidates(direction);

            if (candidateRooms.Count == 0)
                return null;

            int index = Random.Range(0, candidateRooms.Count);
            RoomCardData card = candidateRooms[index];
            deck.Remove(card);

            return card;
        }

        private void FillCandidates(Vector2Int direction)
        {
            candidateRooms.Clear();
            Vector2Int requiredDoor = GetOppositeDirection(direction);

            for (int i = 0; i < deck.Count; i++)
            {
                RoomCardData card = deck[i];
                if (card != null && card.HasDoor(requiredDoor))
                {
                    candidateRooms.Add(card);
                }
            }
        }

        private bool CanPlaceFromOrigin(Vector2Int origin, Vector2Int direction, out string failureReason)
        {
            RoomCard originRoom = GetRoom(origin);
            Vector2Int nextPosition = origin + direction;

            if (originRoom == null)
            {
                failureReason = "当前格子没有房间。";
                return false;
            }

            if (originRoom.remainingDoors <= 0)
            {
                failureReason = "当前房间没有剩余可连接的门。";
                return false;
            }

            if (originRoom.data == null || !originRoom.data.HasDoor(direction))
            {
                failureReason = "当前方向没有门，无法放置房间。";
                return false;
            }

            if (placedRooms.ContainsKey(nextPosition))
            {
                failureReason = "目标位置已经有房间。";
                return false;
            }

            failureReason = string.Empty;
            return true;
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
