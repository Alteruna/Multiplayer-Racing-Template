Shader "Custom/WaterShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Color2 ("Color2", Color) = (1,1,1,1)
        _NormalTex1 ("Normal texture 1", 2D) = "bump" {}
        _NormalTex2 ("Normal texture 2", 2D) = "bump" {}
        _NoiseTex ("Noise texture", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        _Scale ("Noise scale", Range(0.01, 0.1)) = 0.03
        _Amplitude ("Amplitude", Range(0.01, 0.1)) = 0.015
        _Speed ("Speed", Range(0.0001, 0.3)) = 0.15
        _NormalStrength ("Normal Strength", Range(0, 1)) = 0.5
        _SoftFactor("Soft Factor", Range(0.01, 3.0)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "ForceNoShadowCasting" = "True"}
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows alpha vertex:vert

        #pragma target 3.0

        sampler2D _NormalTex1;
        sampler2D _NormalTex2;
        sampler2D _NoiseTex;
        sampler2D _CameraDepthTexture;

        float _Scale;
        float _Amplitude;
        float _Speed;
        float _NormalStrength;
        float _SoftFactor;

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        fixed4 _Color2;

        struct Input
        {
            float2 uv_NormalTex1;
            float4 screenPos;
            float eyeDepth;
        };

        void vert(inout appdata_full v, out Input o)
        {
            float2 NoiseUV = float2((v.texcoord.xy + _Time * _Speed) * _Scale);
            float NoiseValue = tex2Dlod(_NoiseTex, float4(NoiseUV, 0, 0)).x * _Amplitude;

            v.vertex = v.vertex + float4(0, NoiseValue, 0, 0);

            UNITY_INITIALIZE_OUTPUT(Input, o);
            COMPUTE_EYEDEPTH(o.eyeDepth);
        }


        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = _Color;
            
            o.Smoothness = _Glossiness;


            //Calculate Depth
            float rawZ = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(IN.screenPos));
            float sceneZ = LinearEyeDepth(rawZ);
            float partZ = IN.eyeDepth;

            float fadeRaw = _SoftFactor * (sceneZ - partZ);
            float fade = saturate(fadeRaw);
            float fade1 = saturate(fadeRaw-1);
            float fadeEdge = saturate(0.08-fadeRaw);
            //fade *= fade;
            o.Alpha = fade1;
            
            o.Metallic = _Metallic * fade;

            //Normal Textures
            float normalUVX = IN.uv_NormalTex1.x + sin(_Time * _Speed) * 5;
            float normalUVY = IN.uv_NormalTex1.y + sin(_Time * _Speed) * 5;

            float2 normalUV1 = float2(normalUVX, IN.uv_NormalTex1.y);
            float2 normalUV2 = float2(IN.uv_NormalTex1.x, normalUVY);

            o.Normal = UnpackNormal((tex2D(_NormalTex1, normalUV1) + tex2D(_NormalTex2, normalUV2)) * _NormalStrength * fade);
            fadeEdge *= (o.Normal.y+1)*64;
            o.Albedo = lerp(_Color2.rgb, c, fade - fadeEdge*10000*_Color2.a);
        }
        ENDCG
    }
    FallBack "Diffuse"
}