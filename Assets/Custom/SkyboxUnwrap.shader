Shader "Custom/SkyboxUnwrap"
{
    Properties
    {
        _SkyboxTex ("Skybox Texture", 2D) = "white" {}
        _FOV ("Field of View", Float) = 60.0
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
            #include "UnityCG.cginc"

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

            sampler2D _SkyboxTex;
            float _FOV;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Simple equirectangular unwrap (360 to 2D)
                float2 uv = i.uv;
                uv.x = uv.x * 2.0 * UNITY_PI; 
                uv.y = (uv.y - 0.5) * UNITY_PI; 

                float3 dir;
                dir.x = cos(uv.y) * sin(uv.x);
                dir.y = sin(uv.y);
                dir.z = cos(uv.y) * cos(uv.x);

                // Convert direction to UV for equirectangular skybox
                float2 skyUV;
                skyUV.x = atan2(dir.z, dir.x) / (2.0 * UNITY_PI) + 0.5;
                skyUV.y = asin(dir.y) / UNITY_PI + 0.5;

                fixed4 col = tex2D(_SkyboxTex, skyUV);
                return col;
            }
            ENDCG
        }
    }
    Fallback "Diffuse"
}