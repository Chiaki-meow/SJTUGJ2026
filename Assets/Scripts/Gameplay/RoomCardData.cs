using UnityEngine;
using UnityEngine.Serialization;

namespace Gameplay
{
    [CreateAssetMenu(menuName = "Gameplay/Room Card")]
    public class RoomCardData : ScriptableObject
    {
        public string roomName;
        public Sprite sprite;

        [Range(0, 4)]
        public int doorCount = 1;

        private void OnValidate()
        {
            doorCount = Mathf.Clamp(doorCount, 1, 4);

            if (string.IsNullOrWhiteSpace(roomName))
            {
                roomName = name;
            }
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

                parsedDoorCount = Mathf.Clamp(assetName[i] - '0', 0, 4);
                return true;
            }

            return false;
        }
#endif
    }
}
