Shader "Hidden/AttributeVFX/EdgeDizzinessBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Intensity ("Intensity", Range(0, 1)) = 0
        _EdgeWidth ("Edge Width", Range(0.01, 1)) = 0.45
        _BlurRadius ("Blur Radius", Range(0, 12)) = 4
        _DistortAmount ("Distort Amount", Range(0, 0.08)) = 0.015
        _TintColor ("Tint Color", Color) = (0.45, 0.55, 0.9, 1)
        _TimeScale ("Time Scale", Range(0, 8)) = 2
    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _Intensity;
            float _EdgeWidth;
            float _BlurRadius;
            float _DistortAmount;
            float4 _TintColor;
            float _TimeScale;

            fixed4 frag(v2f_img i) : SV_Target
            {
                float2 uv = i.uv;
                float2 centered = uv * 2.0 - 1.0;
                float radial = saturate(length(centered));
                float edgeMask = smoothstep(1.0 - _EdgeWidth, 1.0, radial) * _Intensity;

                float wave = sin((centered.x * 17.0 + centered.y * 11.0) + _Time.y * _TimeScale) * 0.5 + 0.5;
                float2 direction = radial > 0.0001 ? centered / radial : float2(0.0, 0.0);
                float2 tangent = float2(-direction.y, direction.x);
                float2 distortion = tangent * ((wave - 0.5) * _DistortAmount * edgeMask);
                float2 sampleUv = uv + distortion;

                float2 blurStep = _MainTex_TexelSize.xy * _BlurRadius * edgeMask;
                fixed4 col = tex2D(_MainTex, sampleUv) * 0.36;
                col += tex2D(_MainTex, sampleUv + blurStep * float2(1, 0)) * 0.12;
                col += tex2D(_MainTex, sampleUv + blurStep * float2(-1, 0)) * 0.12;
                col += tex2D(_MainTex, sampleUv + blurStep * float2(0, 1)) * 0.12;
                col += tex2D(_MainTex, sampleUv + blurStep * float2(0, -1)) * 0.12;
                col += tex2D(_MainTex, sampleUv + blurStep * float2(0.707, 0.707)) * 0.08;
                col += tex2D(_MainTex, sampleUv + blurStep * float2(-0.707, 0.707)) * 0.08;

                float3 tinted = lerp(col.rgb, col.rgb * _TintColor.rgb, edgeMask * 0.55);
                tinted *= 1.0 - edgeMask * 0.18;
                return fixed4(tinted, col.a);
            }
            ENDCG
        }
    }

    Fallback Off
}
