Shader "URP/StarfieldOnMeshURP"
{
    Properties
    {
        _StarColor("Star Color", Color) = (1,1,1,1)
        _BackgroundColor("Background Color", Color) = (0,0,0,1)
        _Speed("Star Movement Speed", Float) = 0.1
        _Density("Star Density", Float) = 80
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Background" }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Cull Front

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            float4 _StarColor;
            float4 _BackgroundColor;
            float _Speed;
            float _Density;

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionHCS = TransformWorldToHClip(worldPos);
                OUT.worldPos = worldPos;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
{
    float3 dir = normalize(IN.worldPos);
    float star = 0;

    // Layer 1: dense small stars
    float2 uv1 = dir.xy * (_Density);
    uv1 += frac(_Time.y * float2(4.8, 9.6));
    star += 0.5 * smoothstep(0.99, 1.0, hash21(floor(uv1)));

    // Layer 2: medium stars
    float2 uv2 = dir.xy * (_Density * 0.5);
    uv2 += frac(_Time.y * float2(-0.7, 1.1));
    star += 0.3 * smoothstep(0.95, 1.0, hash21(floor(uv2)));

    // Layer 3: large sparse stars
    float2 uv3 = dir.xy * (_Density * 0.25);
    uv3 += frac(_Time.y * float2(0.94, -0.5));
    star += 0.2 * smoothstep(0.98, 1.0, hash21(floor(uv3)));

    // Twinkle modulation
    float twinkle = 0.8 + 0.2 * sin(dot(dir.xy * _Density, float2(12.989, 78.233)) + _Time.y * 3.0);
    star *= twinkle;

    // Clamp star brightness to max 1
    star = saturate(star);

    return lerp(_BackgroundColor, _StarColor, star);
}


            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
