using UnityEngine;
using UnityEngine.EventSystems;

namespace Gameplay
{
    public class MainMapDragController : MonoBehaviour, IBeginDragHandler, IDragHandler, IScrollHandler
    {
        [SerializeField] private RectTransform viewRoot;
        [SerializeField] private Canvas canvas;
        [SerializeField] private float dragScale = 1f;
        [SerializeField] private float scrollZoomStep = 0.15f;
        [SerializeField] private float minZoom = 0.5f;
        [SerializeField] private float maxZoom = 2f;
        [SerializeField] private float zoomSmoothTime = 0.08f;

        private float targetZoom = 1f;
        private float zoomVelocity;

        public Vector2 ViewOffset => viewRoot != null ? viewRoot.anchoredPosition : Vector2.zero;
        public float Zoom => viewRoot != null ? viewRoot.localScale.x : targetZoom;

        private void Awake()
        {
            if (canvas == null)
            {
                canvas = GetComponentInParent<Canvas>();
            }

            if (viewRoot != null)
            {
                targetZoom = viewRoot.localScale.x;
            }
        }

        private void Update()
        {
            if (viewRoot == null)
                return;

            float currentZoom = viewRoot.localScale.x;
            float nextZoom = Mathf.SmoothDamp(currentZoom, targetZoom, ref zoomVelocity, zoomSmoothTime);
            viewRoot.localScale = new Vector3(nextZoom, nextZoom, 1f);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            CacheCanvasIfNeeded();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (viewRoot == null)
                return;

            float scaleFactor = canvas != null && canvas.scaleFactor > 0f ? canvas.scaleFactor : 1f;
            viewRoot.anchoredPosition += eventData.delta / scaleFactor * dragScale;
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (viewRoot == null || Mathf.Approximately(eventData.scrollDelta.y, 0f))
                return;

            targetZoom = Mathf.Clamp(targetZoom + eventData.scrollDelta.y * scrollZoomStep, minZoom, maxZoom);
        }

        private void CacheCanvasIfNeeded()
        {
            if (canvas == null)
            {
                canvas = GetComponentInParent<Canvas>();
            }
        }
    }
}
