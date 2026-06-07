using UnityEngine;

namespace Gameplay
{
    [CreateAssetMenu(menuName = "Gameplay/Room Card")]
    public class RoomCardData : ScriptableObject
    {
        public string roomName;
        public Sprite sprite;
        public Sprite wallSprite;
        public RoomEventData eventData;
        public bool doorUp = true;
        public bool doorLeft;
        public bool doorDown;
        public bool doorRight;

        [Range(1, 4)]
        public int doorCount = 1;

        private void OnValidate()
        {
            int count = GetDoorCount();
            if (count == 0)
            {
                doorUp = true;
                count = 1;
            }

            doorCount = Mathf.Clamp(count, 1, 4);

            if (string.IsNullOrWhiteSpace(roomName))
            {
                roomName = name;
            }
        }

        public bool HasDoor(Vector2Int direction)
        {
            if (direction == Vector2Int.up)
                return doorUp;

            if (direction == Vector2Int.left)
                return doorLeft;

            if (direction == Vector2Int.down)
                return doorDown;

            if (direction == Vector2Int.right)
                return doorRight;

            return false;
        }

        public int GetDoorCount()
        {
            int count = 0;
            if (doorUp)
                count++;
            if (doorLeft)
                count++;
            if (doorDown)
                count++;
            if (doorRight)
                count++;
            return count;
        }

#if UNITY_EDITOR
        [ContextMenu("Fill From Asset Name")]
        public void FillFromAssetName()
        {
            roomName = name;

            if (TryParseDoorCount(name, out int parsedDoorCount))
            {
                doorCount = parsedDoorCount;
            }
        }

        private static bool TryParseDoorCount(string assetName, out int parsedDoorCount)
        {
            parsedDoorCount = 0;

            for (int i = assetName.Length - 1; i >= 0; i--)
            {
                if (!char.IsDigit(assetName[i]))
                    continue;

                parsedDoorCount = Mathf.Clamp(assetName[i] - '0', 1, 4);
                return true;
            }

            return false;
        }
#endif
    }
}
