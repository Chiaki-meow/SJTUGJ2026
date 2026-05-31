using System.Collections.Generic;
using UnityEngine;

namespace Gameplay
{
public class BoardManager : MonoBehaviour
{
    public RoomTile roomPrefab;
    public RoomCardData startRoom;
    public List<RoomCardData> deck = new();

    public float tileSize = 1f;

    private Dictionary<Vector2Int, RoomTile> placedRooms = new();

    private void Start()
    {
        PlaceRoom(startRoom, Vector2Int.zero);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnPlayerEnterRoom(Vector2Int.zero);
        }
    }
    
    public void OnPlayerEnterRoom(Vector2Int position)
    {
        if (!placedRooms.ContainsKey(position))
            return;

        RoomTile room = placedRooms[position];

        if (room.hasExpanded)
            return;

        room.hasExpanded = true;

        int count = room.data.doorCount;

        ExpandFrom(position, count);
    }

    private void ExpandFrom(Vector2Int origin, int count)
    {
        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };

        foreach (Vector2Int dir in directions)
        {
            if (count <= 0)
                break;

            Vector2Int nextPos = origin + dir;

            if (placedRooms.ContainsKey(nextPos))
                continue;

            RoomCardData card = DrawCard();

            if (card == null)
                return;

            PlaceRoom(card, nextPos);

            count--;
        }
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

    private void PlaceRoom(RoomCardData card, Vector2Int gridPosition)
    {
        Vector3 worldPosition = new Vector3(
            gridPosition.x * tileSize,
            gridPosition.y * tileSize,
            0f
        );

        RoomTile room = Instantiate(roomPrefab, worldPosition, Quaternion.identity);
        room.Init(card, gridPosition);

        placedRooms.Add(gridPosition, room);
    }
}
}
