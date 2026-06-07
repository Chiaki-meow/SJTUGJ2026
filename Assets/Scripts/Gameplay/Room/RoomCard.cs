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
        public bool isPreview;
        public RoomDoorLayout doorLayout;
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
            Init(newData, newGridPosition, RoomDoorLayout.FromData(newData));
        }

        public void Init(RoomCardData newData, Vector2Int newGridPosition, RoomDoorLayout newDoorLayout)
        {
            data = newData;
            gridPosition = newGridPosition;
            doorLayout = newDoorLayout;
            remainingDoors = doorLayout.Count;
            hasResolvedEvent = false;
            isPreview = false;

            RefreshVisuals();

            if (data != null)
            {
                gameObject.name = data.roomName;
            }
        }

        public void InitPreview(Vector2Int newGridPosition)
        {
            data = null;
            gridPosition = newGridPosition;
            doorLayout = default;
            remainingDoors = 0;
            hasResolvedEvent = true;
            isPreview = true;

            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = null;
            }

            SetImageEnabled(roomImage, false);
            SetImageEnabled(wallImage, false);
            SetDoorVisible(upDoorImage, false);
            SetDoorVisible(leftDoorImage, false);
            SetDoorVisible(downDoorImage, false);
            SetDoorVisible(rightDoorImage, false);
            SetIconVisible(chestIcon, false);
            SetIconVisible(eventIcon, false);
            SetIconVisible(omenIcon, false);

            Transform next = transform.Find("next");
            if (next != null)
            {
                next.gameObject.SetActive(true);
            }

            gameObject.name = "next";
        }

        public bool HasDoor(Vector2Int direction)
        {
            return doorLayout.HasDoor(direction);
        }

        private void RefreshVisuals()
        {
            if (spriteRenderer != null && data != null)
            {
                spriteRenderer.sprite = data.sprite;
            }

            if (roomImage != null && data != null)
            {
                if (data.sprite != null)
                {
                    roomImage.sprite = data.sprite;
                }

                roomImage.enabled = true;
                roomImage.preserveAspect = true;
            }

            if (wallImage != null && data != null)
            {
                wallImage.sprite = data.wallSprite;
                wallImage.enabled = data.wallSprite != null;
            }

            Transform next = transform.Find("next");
            if (next != null)
            {
                next.gameObject.SetActive(false);
            }

            SetDoorVisible(upDoorImage, data != null && doorLayout.up);
            SetDoorVisible(leftDoorImage, data != null && doorLayout.left);
            SetDoorVisible(downDoorImage, data != null && doorLayout.down);
            SetDoorVisible(rightDoorImage, data != null && doorLayout.right);
            RefreshCategoryIcons();
        }

        private void SetImageEnabled(Image image, bool enabled)
        {
            if (image != null)
            {
                image.enabled = enabled;
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
