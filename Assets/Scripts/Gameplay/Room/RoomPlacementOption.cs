using UnityEngine;

namespace Gameplay
{
    public class RoomPlacementOption
    {
        public RoomCardData data;
        public RoomDoorLayout doorLayout;

        public int DoorCount => doorLayout.Count;

        public bool HasDoor(Vector2Int direction)
        {
            return doorLayout.HasDoor(direction);
        }

        public RoomPlacementOption WithDoorLayout(RoomDoorLayout newDoorLayout)
        {
            return new RoomPlacementOption
            {
                data = data,
                doorLayout = newDoorLayout
            };
        }
    }
}
