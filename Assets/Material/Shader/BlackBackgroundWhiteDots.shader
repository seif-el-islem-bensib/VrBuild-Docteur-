Shader "Custom/BlackBackgroundWhiteDots"
{
    Properties
    {
        _DotSpacing("Dot Spacing", Float) = 50
        _DotSize("Dot Size (Density)", Float) = 2
        _MainColor("Dot Color", Color) = (1,1,1,1)
        _BackgroundColor("Background Color", Color) = (0,0,0,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float _DotSpacing;
            float _DotSize;
            float4 _MainColor;
            float4 _BackgroundColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv * 100; // Scale UVs for spacing control
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 gridUV = fmod(i.uv, _DotSpacing);

                // Center of each grid cell
                float2 center = _DotSpacing / 2.0;
                float dist = distance(gridUV, center);

                // If within dot radius, show dot color
                if (dist < _DotSize)
                    return _MainColor;

                return _BackgroundColor;
            }
            ENDCG
        }
    }
}
