// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/CoolGround"
{
    Properties
    {
        _MainTex ("Dirt", 2D) = "white" {}
        _GrassTex ("Grass", 2D) = "white" {}
    }
    SubShader
    {
        Tags {
            "LightMode"="ForwardBase"
            "RenderType"="Opaque"
        }
        Pass
        {
        CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc" // for UnityObjectToWorldNormal
            #include "UnityLightingCommon.cginc" // for _LightColor0

            sampler2D _GrassTex;
            float4 _GrassTex_ST;
            sampler2D _MainTex;
            float4 _MainTex_ST;

            struct appdata
            {
                float4 vertex    : POSITION;  // The vertex position in model space.
                float3 normal    : NORMAL;    // The vertex normal in model space.
                float4 texcoord0 : TEXCOORD0; // The first UV coordinate.
                float4 texcoord1 : TEXCOORD1; // The first UV coordinate.
            };

            struct v2f
            {
                float2 uv     : TEXCOORD0;
                float2 uv2    : TEXCOORD1;
                fixed4 diff   : COLOR0; // diffuse lighting color
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord0, _MainTex);
                o.uv2 = TRANSFORM_TEX(v.texcoord1, _GrassTex);

                half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz)); // dot product between normal and light direction for standard diffuse (Lambert) lighting
                o.diff = nl * _LightColor0; // factor in the light color
                o.diff.rgb += ShadeSH9(half4(worldNormal,1)); // factor in ambient lighting and light probes
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col;

                col = lerp(tex2D(_MainTex, i.uv), tex2D(_GrassTex, i.uv), i.uv2.x);
            //  col = i.uv2.x < 0.5 ? tex2D(_MainTex, i.uv) : tex2D(_GrassTex, i.uv);
                col *= i.diff;
                return col;
            }

        ENDCG
        }
    }
}
