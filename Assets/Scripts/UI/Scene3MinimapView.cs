using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay
{
    public class Scene3MinimapView : MonoBehaviour
    {
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private PlayerGridMovement playerMovement;
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private RoomCard roomPrefab;
        [SerializeField] private Image playerMarker;
        [SerializeField] private float padding = 18f;
        [SerializeField] private float playerMarkerSize = 18f;
        [SerializeField] private float previewAlpha = 0.35f;

        private readonly List<RoomCard> spawnedRooms = new();
        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            if (boardManager == null)
            {
                boardManager = FindObjectOfType<BoardManager>();
            }

            if (playerMovement == null)
            {
                playerMovement = FindObjectOfType<PlayerGridMovement>();
            }

            if (roomPrefab == null && boardManager != null)
            {
                roomPrefab = boardManager.roomPrefab;
            }

            if (boardManager != null)
            {
                boardManager.RoomsChanged += Refresh;
            }

            EnsureContentRoot();
            EnsurePlayerMarker();
            Refresh();
        }

        private void OnDisable()
        {
            if (boardManager != null)
            {
                boardManager.RoomsChanged -= Refresh;
            }
        }

        private void LateUpdate()
        {
            RefreshPlayerMarker();
        }

        public void Refresh()
        {
            EnsureContentRoot();
            ClearRooms();

            if (boardManager == null || roomPrefab == null || contentRoot == null)
                return;

            BoundsInt bounds = CalculateBounds();
            Vector2 size = rectTransform != null ? rectTransform.rect.size : new Vector2(296f, 296f);
            float mapWidth = Mathf.Max(1, bounds.size.x) * boardManager.uiTileSize;
            float mapHeight = Mathf.Max(1, bounds.size.y) * boardManager.uiTileSize;
            float availableWidth = Mathf.Max(1f, size.x - padding * 2f);
            float availableHeight = Mathf.Max(1f, size.y - padding * 2f);
            float scale = Mathf.Min(availableWidth / mapWidth, availableHeight / mapHeight);

            contentRoot.localScale = new Vector3(scale, scale, 1f);
            Vector2 center = boardManager.GridToUiAnchoredPosition(new Vector2Int(bounds.xMin, bounds.yMin))
                + new Vector2((bounds.size.x - 1) * boardManager.uiTileSize * 0.5f, (bounds.size.y - 1) * boardManager.uiTileSize * 0.5f);
            contentRoot.anchoredPosition = -center * scale;

            SpawnRooms(boardManager.PreviewRooms, true);
            SpawnRooms(boardManager.PlacedRooms, false);
            RefreshPlayerMarker();
        }

        private void SpawnRooms(IEnumerable<RoomCard> rooms, bool isPreview)
        {
            foreach (RoomCard sourceRoom in rooms)
            {
                if (sourceRoom == null)
                    continue;

                RoomCard room = Instantiate(roomPrefab, contentRoot);
                RectTransform roomTransform = room.GetComponent<RectTransform>();
                if (roomTransform != null)
                {
                    roomTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    roomTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    roomTransform.pivot = new Vector2(0.5f, 0.5f);
                    roomTransform.anchoredPosition = boardManager.GridToUiAnchoredPosition(sourceRoom.gridPosition);
                    roomTransform.localScale = Vector3.one;
                }

                if (isPreview || sourceRoom.isPreview)
                {
                    room.InitPreview(sourceRoom.gridPosition);
                    SetRoomAlpha(room, previewAlpha);
                }
                else
                {
                    room.Init(sourceRoom.data, sourceRoom.gridPosition, sourceRoom.doorLayout);
                }

                DisableRaycasts(room.transform);
                spawnedRooms.Add(room);
            }
        }

        private BoundsInt CalculateBounds()
        {
            bool hasRoom = false;
            int minX = 0;
            int maxX = 0;
            int minY = 0;
            int maxY = 0;

            AddRoomBounds(boardManager.PlacedRooms, ref hasRoom, ref minX, ref maxX, ref minY, ref maxY);
            AddRoomBounds(boardManager.PreviewRooms, ref hasRoom, ref minX, ref maxX, ref minY, ref maxY);

            if (!hasRoom)
            {
                return new BoundsInt(0, 0, 0, 1, 1, 1);
            }

            return new BoundsInt(minX, minY, 0, maxX - minX + 1, maxY - minY + 1, 1);
        }

        private void AddRoomBounds(IEnumerable<RoomCard> rooms, ref bool hasRoom, ref int minX, ref int maxX, ref int minY, ref int maxY)
        {
            foreach (RoomCard room in rooms)
            {
                if (room == null)
                    continue;

                Vector2Int position = room.gridPosition;
                if (!hasRoom)
                {
                    minX = maxX = position.x;
                    minY = maxY = position.y;
                    hasRoom = true;
                    continue;
                }

                minX = Mathf.Min(minX, position.x);
                maxX = Mathf.Max(maxX, position.x);
                minY = Mathf.Min(minY, position.y);
                maxY = Mathf.Max(maxY, position.y);
            }
        }

        private void RefreshPlayerMarker()
        {
            if (playerMarker == null || playerMovement == null || boardManager == null)
                return;

            RectTransform markerTransform = playerMarker.rectTransform;
            markerTransform.SetParent(contentRoot, false);
            markerTransform.anchoredPosition = boardManager.GridToUiAnchoredPosition(playerMovement.gridPosition);
            markerTransform.sizeDelta = new Vector2(playerMarkerSize, playerMarkerSize);
            markerTransform.SetAsLastSibling();
        }

        private void EnsureContentRoot()
        {
            if (contentRoot != null)
                return;

            Transform existing = transform.Find("MinimapContent");
            if (existing != null)
            {
                contentRoot = existing.GetComponent<RectTransform>();
                return;
            }

            GameObject content = new GameObject("MinimapContent", typeof(RectTransform));
            content.transform.SetParent(transform, false);
            contentRoot = content.GetComponent<RectTransform>();
            contentRoot.anchorMin = new Vector2(0.5f, 0.5f);
            contentRoot.anchorMax = new Vector2(0.5f, 0.5f);
            contentRoot.pivot = new Vector2(0.5f, 0.5f);
            contentRoot.sizeDelta = Vector2.zero;
        }

        private void EnsurePlayerMarker()
        {
            if (playerMarker != null)
                return;

            GameObject marker = new GameObject("PlayerMarker", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            marker.transform.SetParent(contentRoot, false);
            playerMarker = marker.GetComponent<Image>();
            playerMarker.color = Color.red;
            playerMarker.raycastTarget = false;
        }

        private void ClearRooms()
        {
            for (int i = 0; i < spawnedRooms.Count; i++)
            {
                if (spawnedRooms[i] != null)
                {
                    Destroy(spawnedRooms[i].gameObject);
                }
            }

            spawnedRooms.Clear();
        }

        private void DisableRaycasts(Transform root)
        {
            Graphic[] graphics = root.GetComponentsInChildren<Graphic>(true);
            for (int i = 0; i < graphics.Length; i++)
            {
                graphics[i].raycastTarget = false;
            }
        }

        private void SetRoomAlpha(RoomCard room, float alpha)
        {
            Graphic[] graphics = room.GetComponentsInChildren<Graphic>(true);
            for (int i = 0; i < graphics.Length; i++)
            {
                Color color = graphics[i].color;
                color.a = alpha;
                graphics[i].color = color;
            }
        }
    }
}
