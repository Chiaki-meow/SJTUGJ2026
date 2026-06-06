using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UI.AttributeVFX
{
    [System.Flags]
    public enum BloodOverlayEdges
    {
        None = 0,
        Left = 1,
        Right = 2,
        Top = 4,
        Bottom = 8,
        All = Left | Right | Top | Bottom
    }

    public class AttributeChangeVFXView : MonoBehaviour
    {
        [SerializeField] private AttributePostProcessController postProcessController;
        [SerializeField] private Image bloodOverlayImage;

        [Header("Mental Drop")]
        [SerializeField] private float mentalBaseIntensity = 0.75f;
        [SerializeField, Range(0.01f, 1f)] private float mentalEdgeWidth = 0.52f;
        [SerializeField] private float mentalFadeInDuration = 0.12f;
        [SerializeField] private float mentalFadeOutDuration = 0.65f;

        [Header("Physical Drop")]
        [SerializeField] private float physicalBaseIntensity = 0.45f;
        [SerializeField, Range(0.01f, 1f)] private float physicalEdgeWidth = 0.38f;
        [SerializeField] private BloodOverlayEdges physicalEdges = BloodOverlayEdges.All;
        [SerializeField] private float physicalFadeInDuration = 0.08f;
        [SerializeField] private float physicalFadeOutDuration = 0.75f;

        [Header("Health Drop")]
        [SerializeField] private float healthBaseIntensity = 0.75f;
        [SerializeField, Range(0.01f, 1f)] private float healthEdgeWidth = 0.38f;
        [SerializeField] private BloodOverlayEdges healthEdges = BloodOverlayEdges.Top;
        [SerializeField] private float healthFadeInDuration = 0.08f;
        [SerializeField] private float healthFadeOutDuration = 0.95f;

        private Material bloodRuntimeMaterial;
        private Coroutine mentalRoutine;
        private Coroutine bloodRoutine;

        private void Awake()
        {
            ResolveReferences();

            if (bloodOverlayImage != null && bloodOverlayImage.material != null)
            {
                bloodRuntimeMaterial = new Material(bloodOverlayImage.material);
                bloodRuntimeMaterial.hideFlags = HideFlags.HideAndDontSave;
                bloodOverlayImage.material = bloodRuntimeMaterial;
            }

            SetMentalIntensity(0f);
            SetBloodIntensity(0f);
        }

        private void ResolveReferences()
        {
            if (postProcessController == null)
            {
                Camera mainCamera = Camera.main;
                postProcessController = mainCamera != null ? mainCamera.GetComponent<AttributePostProcessController>() : FindObjectOfType<AttributePostProcessController>();
            }

            if (bloodOverlayImage == null)
            {
                bloodOverlayImage = GetComponentInChildren<Image>();
            }
        }

        public void PlayMentalDrop(float amount)
        {
            float targetIntensity = CalculateTargetIntensity(mentalBaseIntensity, amount);
            RestartRoutine(ref mentalRoutine, AnimateMental(targetIntensity, mentalEdgeWidth, mentalFadeInDuration, mentalFadeOutDuration));
        }

        public void PlayPhysicalDrop(float amount)
        {
            float targetIntensity = CalculateTargetIntensity(physicalBaseIntensity, amount);
            RestartRoutine(ref bloodRoutine, AnimateBlood(targetIntensity, physicalEdgeWidth, physicalEdges, physicalFadeInDuration, physicalFadeOutDuration));
        }

        public void PlayHealthDrop(float amount)
        {
            float targetIntensity = CalculateTargetIntensity(healthBaseIntensity, amount);
            RestartRoutine(ref bloodRoutine, AnimateBlood(targetIntensity, healthEdgeWidth, healthEdges, healthFadeInDuration, healthFadeOutDuration));
        }

        private static float CalculateTargetIntensity(float baseIntensity, float amount)
        {
            return Mathf.Clamp01(baseIntensity + Mathf.Max(0f, amount - 1f) * 0.12f);
        }

        private void RestartRoutine(ref Coroutine routine, IEnumerator enumerator)
        {
            if (routine != null)
            {
                StopCoroutine(routine);
            }

            routine = StartCoroutine(enumerator);
        }

        private IEnumerator AnimateMental(float targetIntensity, float edgeWidth, float fadeInDuration, float fadeOutDuration)
        {
            SetMentalEdgeWidth(edgeWidth);
            yield return AnimateValue(SetMentalIntensity, 0f, targetIntensity, fadeInDuration);
            yield return AnimateValue(SetMentalIntensity, targetIntensity, 0f, fadeOutDuration);
            mentalRoutine = null;
        }

        private IEnumerator AnimateBlood(float targetIntensity, float edgeWidth, BloodOverlayEdges edges, float fadeInDuration, float fadeOutDuration)
        {
            SetBloodEdgeWidth(edgeWidth);
            SetBloodEdges(edges);
            yield return AnimateValue(SetBloodIntensity, 0f, targetIntensity, fadeInDuration);
            yield return AnimateValue(SetBloodIntensity, targetIntensity, 0f, fadeOutDuration);
            bloodRoutine = null;
        }

        private IEnumerator AnimateValue(System.Action<float> setter, float from, float to, float duration)
        {
            if (duration <= 0f)
            {
                setter(to);
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                setter(Mathf.Lerp(from, to, Smooth01(t)));
                yield return null;
            }

            setter(to);
        }

        private static float Smooth01(float value)
        {
            return value * value * (3f - 2f * value);
        }

        private void SetMentalEdgeWidth(float value)
        {
            if (postProcessController != null)
            {
                postProcessController.SetEdgeWidth(value);
            }
        }

        private void SetMentalIntensity(float value)
        {
            if (postProcessController != null)
            {
                postProcessController.SetIntensity(value);
            }
        }

        private void SetBloodEdgeWidth(float value)
        {
            if (bloodRuntimeMaterial != null)
            {
                bloodRuntimeMaterial.SetFloat("_EdgeWidth", Mathf.Clamp(value, 0.01f, 1f));
            }
        }

        private void SetBloodEdges(BloodOverlayEdges edges)
        {
            if (bloodRuntimeMaterial != null)
            {
                bloodRuntimeMaterial.SetVector("_EdgeMask", ToEdgeMask(edges));
            }
        }

        private static Vector4 ToEdgeMask(BloodOverlayEdges edges)
        {
            return new Vector4(
                (edges & BloodOverlayEdges.Left) != 0 ? 1f : 0f,
                (edges & BloodOverlayEdges.Right) != 0 ? 1f : 0f,
                (edges & BloodOverlayEdges.Top) != 0 ? 1f : 0f,
                (edges & BloodOverlayEdges.Bottom) != 0 ? 1f : 0f);
        }

        private void SetBloodIntensity(float value)
        {
            if (bloodRuntimeMaterial != null)
            {
                bloodRuntimeMaterial.SetFloat("_Intensity", value);
            }
        }

        private void OnDisable()
        {
            if (mentalRoutine != null)
            {
                StopCoroutine(mentalRoutine);
                mentalRoutine = null;
            }

            if (bloodRoutine != null)
            {
                StopCoroutine(bloodRoutine);
                bloodRoutine = null;
            }

            SetMentalIntensity(0f);
            SetBloodIntensity(0f);
        }

        private void OnDestroy()
        {
            if (bloodRuntimeMaterial == null)
                return;

            if (Application.isPlaying)
            {
                Destroy(bloodRuntimeMaterial);
            }
            else
            {
                DestroyImmediate(bloodRuntimeMaterial);
            }
        }
    }
}
