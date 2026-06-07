using UnityEngine;
using UnityEngine.UI;

namespace Gameplay
{
    public class RoomCard : MonoBehaviour
    {
        public RoomCardData data;
        public Vector2Int gridPosition;
        public int remainingDoors;
        public bool hasResolvedEvent;
        public Image roomImage;
        public Image wallImage;
        public Image upDoorImage;
        public Image leftDoorImage;
        public Image downDoorImage;
        public Image rightDoorImage;
        public GameObject chestIcon;
        public GameObject eventIcon;
        public GameObject omenIcon;

        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            if (roomImage == null)
            {
                roomImage = GetChildImage("room", GetComponent<Image>());
            }

            if (wallImage == null)
            {
                wallImage = GetChildImage("wall", null);
            }

            if (upDoorImage == null)
            {
                upDoorImage = GetChildImage("link1", null);
            }

            if (leftDoorImage == null)
            {
                leftDoorImage = GetChildImage("link2", null);
            }

            if (downDoorImage == null)
            {
                downDoorImage = GetChildImage("link3", null);
            }

            if (rightDoorImage == null)
            {
                rightDoorImage = GetChildImage("link4", null);
            }

            if (chestIcon == null)
            {
                chestIcon = GetChildGameObject("chest");
            }

            if (eventIcon == null)
            {
                eventIcon = GetChildGameObject("event");
            }

            if (omenIcon == null)
            {
                omenIcon = GetChildGameObject("inspire");
            }
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

            if (roomImage != null && data != null)
            {
                roomImage.sprite = data.sprite;
                roomImage.preserveAspect = true;
            }

            if (wallImage != null && data != null)
            {
                wallImage.sprite = data.wallSprite;
                wallImage.enabled = data.wallSprite != null;
            }

            SetDoorVisible(upDoorImage, data != null && data.doorUp);
            SetDoorVisible(leftDoorImage, data != null && data.doorLeft);
            SetDoorVisible(downDoorImage, data != null && data.doorDown);
            SetDoorVisible(rightDoorImage, data != null && data.doorRight);
            RefreshCategoryIcons();

            if (data != null)
            {
                gameObject.name = data.roomName;
            }
        }

        private Image GetChildImage(string childName, Image fallback)
        {
            Transform child = transform.Find(childName);
            return child != null ? child.GetComponent<Image>() : fallback;
        }

        private GameObject GetChildGameObject(string childName)
        {
            Transform child = transform.Find(childName);
            return child != null ? child.gameObject : null;
        }

        private void SetDoorVisible(Image doorImage, bool isVisible)
        {
            if (doorImage != null)
            {
                doorImage.gameObject.SetActive(isVisible);
            }
        }

        private void RefreshCategoryIcons()
        {
            RoomEventCategory category = data != null && data.eventData != null ? data.eventData.category : RoomEventCategory.Event;
            SetIconVisible(chestIcon, category == RoomEventCategory.ItemGain);
            SetIconVisible(eventIcon, category == RoomEventCategory.Event);
            SetIconVisible(omenIcon, category == RoomEventCategory.Omen);
        }

        private void SetIconVisible(GameObject icon, bool isVisible)
        {
            if (icon != null)
            {
                icon.SetActive(isVisible);
            }
        }
    }
}
