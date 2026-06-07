using UnityEngine;

namespace Gameplay
{
    public struct RoomDoorLayout
    {
        public bool up;
        public bool left;
        public bool down;
        public bool right;

        public int Count
        {
            get
            {
                int count = 0;
                if (up)
                    count++;
                if (left)
                    count++;
                if (down)
                    count++;
                if (right)
                    count++;
                return count;
            }
        }

        public bool HasDoor(Vector2Int direction)
        {
            if (direction == Vector2Int.up)
                return up;

            if (direction == Vector2Int.left)
                return left;

            if (direction == Vector2Int.down)
                return down;

            if (direction == Vector2Int.right)
                return right;

            return false;
        }

        public void SetDoor(Vector2Int direction, bool value)
        {
            if (direction == Vector2Int.up)
            {
                up = value;
            }
            else if (direction == Vector2Int.left)
            {
                left = value;
            }
            else if (direction == Vector2Int.down)
            {
                down = value;
            }
            else if (direction == Vector2Int.right)
            {
                right = value;
            }
        }

        public RoomDoorLayout RotatedClockwise()
        {
            return new RoomDoorLayout
            {
                up = left,
                right = up,
                down = right,
                left = down
            };
        }

        public bool HasSameDoors(RoomDoorLayout other)
        {
            return up == other.up
                && left == other.left
                && down == other.down
                && right == other.right;
        }

        public static RoomDoorLayout FromData(RoomCardData data)
        {
            if (data == null)
                return default;

            return new RoomDoorLayout
            {
                up = data.doorUp,
                left = data.doorLeft,
                down = data.doorDown,
                right = data.doorRight
            };
        }
    }
}
