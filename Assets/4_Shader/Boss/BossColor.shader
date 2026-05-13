Shader "Custom/BossColor"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _CenterOffset("Center Offset", Vector) = (0, 0, 0, 0)
        _TintColor ("Tint Color", Color) = (1,1,1,1)
        _Strength ("Tint Strength", Range(0,1)) = 0

        _EdgeStrength ("Edge Strength", Range(0,2)) = 0.5
        _EdgeMin ("Edge Min", Range(0,1)) = 0.2
        _EdgeMax ("Edge Max", Range(0,1)) = 0.7
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR; // SpriteRenderer 색
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float4 color       : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _TintColor;
            float2 _CenterOffset;
            float _Strength;

            float _EdgeStrength;
            float _EdgeMin;
            float _EdgeMax;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.color = IN.color;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                half4 multColor = IN.color;
                clip(texColor.a - 0.2);
                
                float2 centeredUV = IN.uv - 0.5 + _CenterOffset;
                float dist = length(centeredUV) * 2.0;

                float edgeMask = smoothstep(_EdgeMin, _EdgeMax, dist);
                float edgeFactor = edgeMask * _EdgeStrength;
                half4 tinted = lerp(texColor, _TintColor, _Strength * edgeFactor);

                return tinted;
            }

            ENDHLSL
        }
    }
}