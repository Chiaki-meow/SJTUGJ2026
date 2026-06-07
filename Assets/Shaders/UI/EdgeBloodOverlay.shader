Shader "UI/AttributeVFX/EdgeBloodOverlay"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _Intensity ("Intensity", Range(0, 1)) = 0
        _EdgeWidth ("Edge Width", Range(0.01, 1)) = 0.35
        _EdgeMask ("Edge Mask", Vector) = (1, 1, 1, 1)
        _BloodColor ("Blood Color", Color) = (0.55, 0.02, 0.01, 1)
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _NoiseScale ("Noise Scale", Range(0.5, 8)) = 2
        _DripAmount ("Drip Amount", Range(0, 1)) = 0.15
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            fixed4 _Color;
            fixed4 _BloodColor;
            float _Intensity;
            float _EdgeWidth;
            float4 _EdgeMask;
            float _NoiseScale;
            float _DripAmount;
            float4 _ClipRect;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;
                float leftEdge = (1.0 - smoothstep(0.0, _EdgeWidth, uv.x)) * _EdgeMask.x;
                float rightEdge = (1.0 - smoothstep(0.0, _EdgeWidth, 1.0 - uv.x)) * _EdgeMask.y;
                float topEdge = (1.0 - smoothstep(0.0, _EdgeWidth, 1.0 - uv.y)) * _EdgeMask.z;
                float bottomEdge = (1.0 - smoothstep(0.0, _EdgeWidth, uv.y)) * _EdgeMask.w;
                float edge = saturate(max(max(leftEdge, rightEdge), max(topEdge, bottomEdge)));

                float verticalDrip = saturate((1.0 - uv.y) * _DripAmount + edge);
                float2 noiseUv = float2(uv.x, uv.y + _Time.y * 0.03) * _NoiseScale;
                float noise = tex2D(_NoiseTex, noiseUv).r;
                float breakup = smoothstep(0.15, 0.95, noise + edge * 0.55 + verticalDrip * 0.18);
                float alpha = saturate(edge * breakup * _Intensity * _BloodColor.a);

                fixed4 color = _BloodColor;
                color.a = alpha * IN.color.a;
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                return color;
            }
            ENDCG
        }
    }
}
