using System.Collections;
using TMPro;
using UnityEngine;

namespace UI.Dice
{
    public class DiceAnimatorController : MonoBehaviour
    {
        [Header("Targets")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform cupRoot;
        [SerializeField] private RectTransform cupContainer;
        [SerializeField] private RectTransform cupLid;
        [SerializeField] private RectTransform resultText;

        [Header("Cup Open Close")]
        [SerializeField] private Vector2 lidClosedPosition = new Vector2(0f, -34f);
        [SerializeField] private Vector2 lidOpenPosition = new Vector2(110f, 174f);
        [SerializeField] private float openDuration = 0.35f;
        [SerializeField] private float closeDuration = 0.25f;
        [SerializeField] private AnimationCurve cupCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Shake")]
        [SerializeField] private float dragMoveScale = 1f;
        [SerializeField] private float maxShakeRotation = 16f;
        [SerializeField] private float rotationPerPixel = 0.08f;

        [Header("Result Drop")]
        [SerializeField] private Vector2 resultStartPosition = new Vector2(220f, 540f);
        [SerializeField] private Vector2 resultImpactPosition = new Vector2(0f, 85f);
        [SerializeField] private Vector2 resultEndPosition = new Vector2(0f, 130f);
        [SerializeField] private float resultDropDuration = 0.32f;
        [SerializeField] private float resultReboundDuration = 0.16f;
        [SerializeField] private float resultStartScale = 1.55f;
        [SerializeField] private float resultImpactScale = 0.92f;
        [SerializeField] private float resultEndScale = 1f;
        [SerializeField] private float resultRotation = 12f;
        [SerializeField] private AnimationCurve resultDropCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve resultReboundCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Fade")]
        [SerializeField] private float fadeDuration = 0.2f;

        private TMP_Text resultLabel;

        private void Awake()
        {
            ResolveReferences();
        }

        public void ResolveReferences()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (resultText != null && resultLabel == null)
            {
                resultLabel = resultText.GetComponent<TMP_Text>();
            }
        }

        public void SetCupParts(RectTransform root, RectTransform container, RectTransform lid, RectTransform result)
        {
            cupRoot = root;
            cupContainer = container;
            cupLid = lid;
            resultText = result;
            ResolveReferences();
        }

        public void SetCanvasVisibleImmediate(bool visible)
        {
            ResolveReferences();
            if (canvasGroup == null)
                return;

            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.blocksRaycasts = visible;
            canvasGroup.interactable = visible;
        }

        public IEnumerator FadeIn()
        {
            ResolveReferences();
            if (canvasGroup == null)
                yield break;

            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            yield return FadeCanvas(0f, 1f, fadeDuration);
        }

        public IEnumerator FadeOut()
        {
            ResolveReferences();
            if (canvasGroup == null)
                yield break;

            yield return FadeCanvas(canvasGroup.alpha, 0f, fadeDuration);
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        public void SetOpenImmediate(bool open)
        {
            if (cupLid != null)
            {
                cupLid.anchoredPosition = open ? lidOpenPosition : lidClosedPosition;
            }
        }

        public IEnumerator PlayOpen()
        {
            yield return MoveLid(cupLid != null ? cupLid.anchoredPosition : lidClosedPosition, lidOpenPosition, openDuration);
        }

        public IEnumerator PlayClose()
        {
            yield return MoveLid(cupLid != null ? cupLid.anchoredPosition : lidOpenPosition, lidClosedPosition, closeDuration);
        }

        public void ApplyDragDelta(Vector2 screenDelta)
        {
            if (cupRoot == null)
                return;

            cupRoot.anchoredPosition += screenDelta * dragMoveScale;
            float rotation = Mathf.Clamp(screenDelta.x * rotationPerPixel, -maxShakeRotation, maxShakeRotation);
            cupRoot.localRotation = Quaternion.Euler(0f, 0f, rotation);
        }

        public void ResetShakeRotation()
        {
            if (cupRoot == null)
                return;

            cupRoot.localRotation = Quaternion.identity;
        }

        public void PrepareResultText(string result)
        {
            ResolveReferences();
            if (resultLabel != null)
            {
                resultLabel.text = result;
            }

            if (resultText != null)
            {
                resultText.anchoredPosition = resultStartPosition;
                resultText.localScale = Vector3.one * resultStartScale;
                resultText.localRotation = Quaternion.Euler(0f, 0f, resultRotation);
                resultText.gameObject.SetActive(false);
            }
        }

        public IEnumerator PlayResultDrop(string result)
        {
            PrepareResultText(result);
            if (resultText == null)
                yield break;

            resultText.gameObject.SetActive(true);
            yield return MoveResult(resultStartPosition, resultImpactPosition, resultStartScale, resultImpactScale, resultDropDuration, resultDropCurve);
            yield return MoveResult(resultImpactPosition, resultEndPosition, resultImpactScale, resultEndScale, resultReboundDuration, resultReboundCurve);
            resultText.anchoredPosition = resultEndPosition;
            resultText.localScale = Vector3.one * resultEndScale;
            resultText.localRotation = Quaternion.Euler(0f, 0f, resultRotation);
        }

        private IEnumerator MoveResult(Vector2 fromPosition, Vector2 toPosition, float fromScale, float toScale, float duration, AnimationCurve curve)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = duration <= 0f ? 1f : elapsed / duration;
                float eased = curve != null ? curve.Evaluate(t) : t;
                resultText.anchoredPosition = Vector2.LerpUnclamped(fromPosition, toPosition, eased);
                resultText.localScale = Vector3.one * Mathf.LerpUnclamped(fromScale, toScale, eased);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        private IEnumerator FadeCanvas(float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = duration <= 0f ? 1f : elapsed / duration;
                canvasGroup.alpha = Mathf.Lerp(from, to, t);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            canvasGroup.alpha = to;
        }

        private IEnumerator MoveLid(Vector2 from, Vector2 to, float duration)
        {
            if (cupLid == null)
                yield break;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = duration <= 0f ? 1f : elapsed / duration;
                float eased = cupCurve != null ? cupCurve.Evaluate(t) : t;
                cupLid.anchoredPosition = Vector2.LerpUnclamped(from, to, eased);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            cupLid.anchoredPosition = to;
        }
    }
}
