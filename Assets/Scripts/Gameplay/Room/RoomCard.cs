using UnityEngine;

namespace Gameplay
{
    public class RoomCard : MonoBehaviour
    {
        public RoomCardData data;
        public Vector2Int gridPosition;
        public int remainingDoors;
        public bool hasResolvedEvent;

        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Init(RoomCardData newData, Vector2Int newGridPosition)
        {
            data = newData;
            gridPosition = newGridPosition;
            remainingDoors = data != null ? data.doorCount : 0;
            hasResolvedEvent = false;

            if (spriteRenderer != null && data != null)
            {
                spriteRenderer.sprite = data.sprite;
            }

            gameObject.name = data.roomName;
        }
    }
}
