using UnityEngine;

namespace UI.AttributeVFX
{
    [RequireComponent(typeof(Camera))]
    public class AttributePostProcessController : MonoBehaviour
    {
        [SerializeField] private Material dizzinessMaterial;
        [SerializeField, Range(0f, 1f)] private float intensity;
        [SerializeField, Range(0.01f, 1f)] private float edgeWidth = 0.45f;
        [SerializeField, Range(0f, 12f)] private float blurRadius = 4f;
        [SerializeField, Range(0f, 0.08f)] private float distortAmount = 0.015f;
        [SerializeField] private Color tintColor = new Color(0.45f, 0.55f, 0.9f, 1f);
        [SerializeField, Range(0f, 8f)] private float timeScale = 2f;

        private Material runtimeMaterial;

        public void SetIntensity(float value)
        {
            intensity = Mathf.Clamp01(value);
        }

        public void SetEdgeWidth(float value)
        {
            edgeWidth = Mathf.Clamp(value, 0.01f, 1f);
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Material material = GetRuntimeMaterial();
            if (material == null || intensity <= 0.001f)
            {
                Graphics.Blit(source, destination);
                return;
            }

            material.SetFloat("_Intensity", intensity);
            material.SetFloat("_EdgeWidth", edgeWidth);
            material.SetFloat("_BlurRadius", blurRadius);
            material.SetFloat("_DistortAmount", distortAmount);
            material.SetColor("_TintColor", tintColor);
            material.SetFloat("_TimeScale", timeScale);
            Graphics.Blit(source, destination, material);
        }

        private Material GetRuntimeMaterial()
        {
            if (runtimeMaterial != null)
                return runtimeMaterial;

            if (dizzinessMaterial == null)
                return null;

            runtimeMaterial = new Material(dizzinessMaterial);
            runtimeMaterial.hideFlags = HideFlags.HideAndDontSave;
            return runtimeMaterial;
        }

        private void OnDestroy()
        {
            if (runtimeMaterial == null)
                return;

            if (Application.isPlaying)
            {
                Destroy(runtimeMaterial);
            }
            else
            {
                DestroyImmediate(runtimeMaterial);
            }
        }
    }
}
