using System;
using System.Collections;
using System.Collections.Generic;
using Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Dice
{
    public class DicePanelView : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IPointerClickHandler
    {
        [Header("Header")]
        [SerializeField] private Image attributeIconImage;
        [SerializeField] private TMP_Text checkText;
        [SerializeField] private TMP_Text requirementText;
        [SerializeField] private TMP_Text instructionText;
        [SerializeField] private Sprite physicalIcon;
        [SerializeField] private Sprite mentalIcon;
        [SerializeField] private Color physicalColor = new Color(0.85f, 0.35f, 0.35f, 1f);
        [SerializeField] private Color mentalColor = new Color(0.35f, 0.55f, 0.95f, 1f);
        [SerializeField] private Color omenColor = new Color(0.7f, 0.55f, 0.95f, 1f);
        [SerializeField] private Color customColor = new Color(0.85f, 0.85f, 0.85f, 1f);

        [Header("Cup")]
        [SerializeField] private RectTransform cupRoot;
        [SerializeField] private RectTransform diceSpawnRoot;
        [SerializeField] private DiceItemView diceItemPrefab;
        [SerializeField] private DiceAnimatorController animatorController;

        [Header("Interaction")]
        [SerializeField] private float initialOpenHoldSeconds = 0.7f;
        [SerializeField] private float requiredShakeDistance = 360f;
        [SerializeField] private float diceShakeMoveScale = 0.35f;
        [SerializeField] private float maxDiceShakeOffset = 90f;
        [SerializeField] private float diceShakeRotationPerPixel = 0.28f;
        [SerializeField] private float diceMovementPadding = 42f;
        [SerializeField] private string shakePromptText = "按住骰盅并用力拖动";
        [SerializeField] private string shakeAgainText = "再用力一点";
        [SerializeField] private string clickToCloseText = "点击任意位置继续";

        private readonly List<DiceItemView> diceViews = new List<DiceItemView>();
        private readonly List<Vector2> diceBasePositions = new List<Vector2>();
        private const float DefaultDiceSize = 50f;
        private const float DefaultDiceAreaSize = 360f;
        private const float DiceLayoutPadding = 14f;
        private const float DiceSeparationPadding = 8f;
        private const float MinDiceVisualSize = 24f;
        private global::DiceManager diceManager;
        private Action<global::DiceCheckResultModel> onResultConfirmed;
        private Coroutine playRoutine;
        private bool isWaitingForShake;
        private bool isDraggingCup;
        private bool isDismissReady;
        private bool hasRerolledAfterDrag;
        private float shakeDistance;
        private Vector2 previousDragPosition;

        private void Awake()
        {
            ResolveReferences();
            if (animatorController != null)
            {
                animatorController.SetCanvasVisibleImmediate(false);
            }
        }

        private void OnDisable()
        {
            if (diceManager != null)
            {
                diceManager.onDiceRolled -= HandleDiceRolled;
            }
        }

        public void PlayCheck(global::DiceCheckType checkType, int diceCount, int difficulty, global::DiceCompareRule compareRule, bool allowModifier, Action<global::DiceCheckResultModel> resultConfirmed)
        {
            ResolveReferences();
            gameObject.SetActive(true);

            if (playRoutine != null)
            {
                StopCoroutine(playRoutine);
                playRoutine = null;
            }

            if (diceManager != null)
            {
                diceManager.onDiceRolled -= HandleDiceRolled;
            }

            ClearDiceViews();
            isWaitingForShake = false;
            isDraggingCup = false;
            isDismissReady = false;
            shakeDistance = 0f;
            hasRerolledAfterDrag = false;
            onResultConfirmed = resultConfirmed;
            diceManager = new global::DiceManager();
            diceManager.onDiceRolled += HandleDiceRolled;

            RefreshHeader(checkType, difficulty);
            SetInstruction(string.Empty);
            if (animatorController != null)
            {
                animatorController.ResetShakeRotation();
                animatorController.SetOpenImmediate(true);
                animatorController.PrepareResultText(string.Empty);
            }

            diceManager.StartCheck(checkType, diceCount, difficulty, compareRule, allowModifier);
            AudioManager.PlaySfx(SfxEnum.DiceRoll);
            playRoutine = StartCoroutine(PlayIntroRoutine());
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!isWaitingForShake || cupRoot == null)
                return;

            if (!RectTransformUtility.RectangleContainsScreenPoint(cupRoot, eventData.position, eventData.pressEventCamera))
                return;

            isDraggingCup = true;
            previousDragPosition = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDraggingCup || !isWaitingForShake)
                return;

            Vector2 delta = eventData.position - previousDragPosition;
            previousDragPosition = eventData.position;
            shakeDistance += delta.magnitude;
            RerollDiceOnceAfterDrag();

            if (animatorController != null)
            {
                animatorController.ApplyDragDelta(delta);
            }

            MoveDiceDuringShake(delta);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isDraggingCup)
                return;

            isDraggingCup = false;
            if (animatorController != null)
            {
                animatorController.ResetShakeRotation();
            }

            if (isWaitingForShake && shakeDistance >= requiredShakeDistance)
            {
                isWaitingForShake = false;
                playRoutine = StartCoroutine(PlayResultRoutine());
            }
            else if (isWaitingForShake)
            {
                SetInstruction(shakeAgainText);
                shakeDistance = 0f;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isDismissReady || diceManager == null)
                return;

            global::DiceCheckResultModel result = diceManager.ConfirmCheckResult();
            diceManager.onDiceRolled -= HandleDiceRolled;
            diceManager = null;
            isDismissReady = false;
            onResultConfirmed?.Invoke(result);
            onResultConfirmed = null;
            StartCoroutine(CloseRoutine());
        }

        private IEnumerator PlayIntroRoutine()
        {
            if (animatorController != null)
            {
                yield return animatorController.FadeIn();
                animatorController.SetOpenImmediate(true);
            }

            yield return new WaitForSecondsRealtime(initialOpenHoldSeconds);

            if (animatorController != null)
            {
                yield return animatorController.PlayClose();
            }

            SetInstruction(shakePromptText);
            isWaitingForShake = true;
            shakeDistance = 0f;
        }

        private IEnumerator PlayResultRoutine()
        {
            SetInstruction(string.Empty);
            if (animatorController != null)
            {
                yield return animatorController.PlayOpen();
            }

            global::DiceCheckResultModel previewResult = diceManager.GenerateCheckResult();
            string resultText = previewResult.isSuccess ? "Success!" : "Failure!";
            if (animatorController != null)
            {
                yield return animatorController.PlayResultDrop(resultText);
            }

            SetInstruction(clickToCloseText);
            isDismissReady = true;
        }

        private IEnumerator CloseRoutine()
        {
            if (animatorController != null)
            {
                yield return animatorController.FadeOut();
            }

            ClearDiceViews();
            gameObject.SetActive(false);
        }

        private void HandleDiceRolled(List<global::DiceModel> diceList, int totalValue)
        {
            if (diceItemPrefab == null || diceSpawnRoot == null || diceList == null)
                return;

            if (diceViews.Count != diceList.Count)
            {
                ClearDiceViews();
                DisableSpawnRootLayout();
                for (int i = 0; i < diceList.Count; i++)
                {
                    DiceItemView diceView = Instantiate(diceItemPrefab, diceSpawnRoot);
                    diceView.gameObject.SetActive(true);
                    diceViews.Add(diceView);
                }

                ArrangeDiceViews();
            }

            for (int i = 0; i < diceList.Count; i++)
            {
                diceViews[i].SetValue(diceList[i].GetCurrentValue());
            }
        }

        private void MoveDiceDuringShake(Vector2 dragDelta)
        {
            for (int i = 0; i < diceViews.Count; i++)
            {
                DiceItemView diceView = diceViews[i];
                if (diceView == null)
                    continue;

                RectTransform rectTransform = diceView.transform as RectTransform;
                if (rectTransform == null || i >= diceBasePositions.Count)
                    continue;

                float direction = i % 2 == 0 ? 1f : -1f;
                Vector2 offset = rectTransform.anchoredPosition - diceBasePositions[i];
                Vector2 impulse = new Vector2(dragDelta.x * direction, dragDelta.y * (1f - 0.12f * i));
                offset = Vector2.ClampMagnitude(offset + impulse * diceShakeMoveScale, maxDiceShakeOffset);
                rectTransform.anchoredPosition = ClampDicePosition(diceBasePositions[i] + offset, rectTransform);
                rectTransform.localRotation = Quaternion.Euler(0f, 0f, rectTransform.localEulerAngles.z + dragDelta.x * diceShakeRotationPerPixel * direction);
            }

            ResolveDiceOverlaps();
        }

        private void ResolveDiceOverlaps()
        {
            for (int iteration = 0; iteration < 2; iteration++)
            {
                for (int i = 0; i < diceViews.Count; i++)
                {
                    RectTransform first = diceViews[i] != null ? diceViews[i].transform as RectTransform : null;
                    if (first == null)
                        continue;

                    for (int j = i + 1; j < diceViews.Count; j++)
                    {
                        RectTransform second = diceViews[j] != null ? diceViews[j].transform as RectTransform : null;
                        if (second == null)
                            continue;

                        float minimumDistance = (GetDiceDiameter(first) + GetDiceDiameter(second)) * 0.5f + DiceSeparationPadding;
                        Vector2 difference = second.anchoredPosition - first.anchoredPosition;
                        float distance = difference.magnitude;
                        if (distance >= minimumDistance)
                            continue;

                        Vector2 direction = distance > 0.001f ? difference / distance : GetFallbackSeparationDirection(i, j);
                        Vector2 correction = direction * ((minimumDistance - distance) * 0.5f);
                        first.anchoredPosition = ClampDicePosition(first.anchoredPosition - correction, first);
                        second.anchoredPosition = ClampDicePosition(second.anchoredPosition + correction, second);
                    }
                }
            }
        }

        private void ArrangeDiceViews()
        {
            diceBasePositions.Clear();
            int count = diceViews.Count;
            if (count == 0)
                return;

            Vector2 areaSize = GetDiceMovementAreaSize();
            Vector2Int grid = CalculateDiceGrid(count, areaSize);
            float cellWidth = areaSize.x / grid.x;
            float cellHeight = areaSize.y / grid.y;
            float diceSize = GetPrefabDiceSize();
            float targetDiceSize = Mathf.Min(diceSize, Mathf.Max(MinDiceVisualSize, Mathf.Min(cellWidth, cellHeight) - DiceLayoutPadding));
            float diceScale = diceSize > 0f ? targetDiceSize / diceSize : 1f;

            for (int i = 0; i < count; i++)
            {
                DiceItemView diceView = diceViews[i];
                if (diceView == null)
                {
                    diceBasePositions.Add(Vector2.zero);
                    continue;
                }

                RectTransform rectTransform = diceView.transform as RectTransform;
                Vector2 basePosition = CalculateGridDiceBasePosition(i, grid, cellWidth, cellHeight);
                diceBasePositions.Add(basePosition);
                if (rectTransform != null)
                {
                    rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    rectTransform.localScale = Vector3.one * diceScale;
                    rectTransform.anchoredPosition = ClampDicePosition(basePosition, rectTransform);
                    rectTransform.localRotation = Quaternion.identity;
                }
            }
        }

        private Vector2Int CalculateDiceGrid(int count, Vector2 areaSize)
        {
            int bestColumns = 1;
            int bestRows = count;
            float bestCellSize = 0f;

            for (int columns = 1; columns <= count; columns++)
            {
                int rows = Mathf.CeilToInt(count / (float)columns);
                float cellSize = Mathf.Min(areaSize.x / columns, areaSize.y / rows);
                if (cellSize > bestCellSize)
                {
                    bestCellSize = cellSize;
                    bestColumns = columns;
                    bestRows = rows;
                }
            }

            return new Vector2Int(bestColumns, bestRows);
        }

        private Vector2 CalculateGridDiceBasePosition(int index, Vector2Int grid, float cellWidth, float cellHeight)
        {
            int row = index / grid.x;
            int column = index % grid.x;
            int itemsInRow = Mathf.Min(grid.x, diceViews.Count - row * grid.x);
            float rowWidth = (itemsInRow - 1) * cellWidth;
            float x = column * cellWidth - rowWidth * 0.5f;
            float y = (grid.y - 1) * cellHeight * 0.5f - row * cellHeight;
            return new Vector2(x, y);
        }

        private Vector2 GetDiceAreaSize()
        {
            if (diceSpawnRoot == null)
                return Vector2.one * DefaultDiceAreaSize;

            Rect rect = diceSpawnRoot.rect;
            float width = rect.width > 0f ? rect.width : DefaultDiceAreaSize;
            float height = rect.height > 0f ? rect.height : DefaultDiceAreaSize;
            return new Vector2(width, height);
        }

        private Vector2 GetDiceMovementAreaSize()
        {
            Vector2 areaSize = GetDiceAreaSize();
            float padding = Mathf.Max(0f, diceMovementPadding) * 2f;
            float minimumSize = Mathf.Max(MinDiceVisualSize, GetPrefabDiceSize());
            return new Vector2(Mathf.Max(minimumSize, areaSize.x - padding), Mathf.Max(minimumSize, areaSize.y - padding));
        }

        private float GetPrefabDiceSize()
        {
            if (diceItemPrefab == null)
                return DefaultDiceSize;

            RectTransform rectTransform = diceItemPrefab.transform as RectTransform;
            if (rectTransform == null)
                return DefaultDiceSize;

            float size = Mathf.Max(rectTransform.rect.width, rectTransform.rect.height);
            if (size <= 0f)
            {
                size = Mathf.Max(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y);
            }

            return size > 0f ? size : DefaultDiceSize;
        }

        private float GetDiceDiameter(RectTransform rectTransform)
        {
            if (rectTransform == null)
                return DefaultDiceSize;

            float size = Mathf.Max(rectTransform.rect.width, rectTransform.rect.height);
            if (size <= 0f)
            {
                size = Mathf.Max(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y);
            }

            return (size > 0f ? size : DefaultDiceSize) * Mathf.Max(rectTransform.localScale.x, rectTransform.localScale.y);
        }

        private Vector2 ClampDicePosition(Vector2 position, RectTransform rectTransform)
        {
            Vector2 areaSize = GetDiceMovementAreaSize();
            float halfDiceSize = GetDiceDiameter(rectTransform) * 0.5f;
            float maxX = Mathf.Max(0f, areaSize.x * 0.5f - halfDiceSize);
            float maxY = Mathf.Max(0f, areaSize.y * 0.5f - halfDiceSize);
            return new Vector2(Mathf.Clamp(position.x, -maxX, maxX), Mathf.Clamp(position.y, -maxY, maxY));
        }

        private Vector2 GetFallbackSeparationDirection(int firstIndex, int secondIndex)
        {
            float angle = (firstIndex * 97f + secondIndex * 53f) * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }

        private void DisableSpawnRootLayout()
        {
            if (diceSpawnRoot == null)
                return;

            LayoutGroup layoutGroup = diceSpawnRoot.GetComponent<LayoutGroup>();
            if (layoutGroup != null)
            {
                layoutGroup.enabled = false;
            }
        }

        private void RerollDiceOnceAfterDrag()
        {
            if (diceManager == null || hasRerolledAfterDrag)
                return;

            AudioManager.PlaySfx(SfxEnum.DiceRoll);
            diceManager.RollAllDices();
            hasRerolledAfterDrag = true;
        }

        private void RefreshHeader(global::DiceCheckType checkType, int difficulty)
        {
            if (checkText != null)
            {
                checkText.text = GetCheckLabel(checkType);
            }

            if (requirementText != null)
            {
                requirementText.text = difficulty.ToString();
            }

            if (attributeIconImage != null)
            {
                attributeIconImage.sprite = GetCheckIcon(checkType);
                attributeIconImage.color = GetCheckColor(checkType);
            }
        }

        private string GetCheckLabel(global::DiceCheckType checkType)
        {
            switch (checkType)
            {
                case global::DiceCheckType.Physical:
                    return "物理检定";
                case global::DiceCheckType.Mental:
                    return "精神检定";
                case global::DiceCheckType.Omen:
                    return "预兆检定";
                default:
                    return "检定";
            }
        }

        private Sprite GetCheckIcon(global::DiceCheckType checkType)
        {
            switch (checkType)
            {
                case global::DiceCheckType.Physical:
                    return physicalIcon;
                case global::DiceCheckType.Mental:
                    return mentalIcon;
                default:
                    return null;
            }
        }

        private Color GetCheckColor(global::DiceCheckType checkType)
        {
            switch (checkType)
            {
                case global::DiceCheckType.Physical:
                    return physicalColor;
                case global::DiceCheckType.Mental:
                    return mentalColor;
                case global::DiceCheckType.Omen:
                    return omenColor;
                default:
                    return customColor;
            }
        }

        private void SetInstruction(string text)
        {
            if (instructionText != null)
            {
                instructionText.text = text;
            }
        }

        private void ClearDiceViews()
        {
            for (int i = 0; i < diceViews.Count; i++)
            {
                if (diceViews[i] != null)
                {
                    Destroy(diceViews[i].gameObject);
                }
            }

            diceViews.Clear();
            diceBasePositions.Clear();
        }

        private void ResolveReferences()
        {
            if (animatorController == null)
            {
                animatorController = GetComponent<DiceAnimatorController>();
            }
        }
    }
}
