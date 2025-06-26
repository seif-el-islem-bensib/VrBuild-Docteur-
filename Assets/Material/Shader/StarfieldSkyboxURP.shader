Shader "URP/StaticStarfieldSkybox"
{
    Properties
    {
        _StarColor("Star Color", Color) = (1,1,1,1)
        _BackgroundColor("Background Color", Color) = (0,0,0,1)
        _Density("Star Density", Float) = 40
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" }
        Cull Off
        ZWrite Off

        Pass
        {
            Name "SkyboxPass"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 dirWS : TEXCOORD0;
            };

            float4 _StarColor;
            float4 _BackgroundColor;
            float _Density;

            // Hash function to pseudo-randomly distribute stars
            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.dirWS = normalize(mul((float3x3)UNITY_MATRIX_M, IN.positionOS)); // Get world direction
                OUT.positionHCS = TransformWorldToHClip(OUT.dirWS * 1000); // Push skybox far away
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.dirWS.xy * _Density;

                float star = smoothstep(0.98, 1.0, hash21(floor(uv)));
                return lerp(_BackgroundColor, _StarColor, star);
            }

            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}