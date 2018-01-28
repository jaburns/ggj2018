// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/Blobbo" {
    Properties{
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0
        _NoiseTex("Noise", 2D) = "white" {}
        _Scale("Scale", Float) = 0.1
        _TimeScale("Time Scale", Float) = 1.0
        _EmissionColor("Emission", Color) = (0,0,0,0)
        _WorldTimeScaleOffset("WorldTimeScaleOffset", Float) = 0
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200
        
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _NoiseTex;

        struct Input {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        float _Scale;
        float _Offset;
        float _TimeScale;
        fixed4 _Color;
        float4 _EmissionColor;
        float _WorldTimeScaleOffset;

        #define PI 3.14159265358979
        #define TWO_PI (2*3.14159265358979)

        void vert (inout appdata_full v) 
        {
            float theta = atan2(v.vertex.y, v.vertex.x) + PI;
            float phi = atan2(length(float2(v.vertex.x, v.vertex.y)), v.vertex.z) + PI;

            float4 lookupA = tex2Dlod(_NoiseTex, float4(theta, phi, 0, 0) / TWO_PI);
            float4 lookupB = tex2Dlod(_NoiseTex, float4(phi + 11, theta + 7, 0, 0) / TWO_PI);

            float3 world = mul(unity_ObjectToWorld, v.vertex).xyz;
            float time = _Time.y * _TimeScale + (world.x + world.y) * _WorldTimeScaleOffset;

            float offset = lookupA.r * sin(time) + lookupB.r * cos(time);
            v.vertex += float4(v.normal * offset * _Scale, 0);
        }

        void surf (Input IN, inout SurfaceOutputStandard o) {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb + _EmissionColor;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
