using UnityEngine;

namespace Gameplay
{
    public class RoomTile : MonoBehaviour
    {
        public RoomCardData data;
        public Vector2Int gridPosition;
        public bool hasExpanded;

        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Init(RoomCardData newData, Vector2Int newGridPosition)
        {
            data = newData;
            gridPosition = newGridPosition;
            hasExpanded = false;

            if (spriteRenderer != null && data != null)
            {
                spriteRenderer.sprite = data.sprite;
            }

            gameObject.name = data.roomName;
        }
    }
}