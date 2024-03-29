Shader "PolyTerrain/Splatmap/Lightmap-FirstPass"
{
    Properties
    {
        _Control ("SplatMap (RGBA)", 2D) = "red" {}
        _Splat0 ("Layer 0 (R)", 2D) = "white" {}
        _Splat1 ("Layer 1 (G)", 2D) = "white" {}
        _Splat2 ("Layer 2 (B)", 2D) = "white" {}
        _Splat3 ("Layer 3 (A)", 2D) = "white" {}
        _BaseMap ("BaseMap (RGB)", 2D) = "white" {}
    }

    Category
    {
        // Fragment program, 4 splats per pass
        SubShader
        {
            Tags
            {
                "SplatCount" = "4"
                "Queue" = "Geometry-100"
                "RenderType" = "Opaque"
            }
            LOD 100
            Pass
            {
                Tags
                {
                    "LightMode" = "ForwardBase"
                }
                CGPROGRAM
                #pragma vertex simplevert
                #pragma fragment simplefrag
                #pragma fragmentoption ARB_fog_exp2
                #pragma fragmentoption ARB_precision_hint_fastest
                #pragma multi_compile LIGHTMAP_ON LIGHTMAP_OFF
                #pragma multi_compile_fog
                #define TEXTURECOUNT 4

                #include "UnityCG.cginc"

                struct appdata_lightmap
                {
                    float4 vertex : POSITION;
                    float3 normal : NORMAL;
                    float2 texcoord : TEXCOORD0;
                    float2 texcoord1 : TEXCOORD1;
                };

                struct v2f_vertex
                {
                    float4 pos : SV_POSITION;
                    UNITY_FOG_COORDS(3)
                    float4 uv[3] : TEXCOORD0;
                };

                uniform sampler2D _Control;
                uniform float4 _Control_ST;

                uniform sampler2D _Splat0, _Splat1, _Splat2, _Splat3, _BaseMap;
                uniform float4 _Splat0_ST, _Splat1_ST, _Splat2_ST, _Splat3_ST;

                v2f_vertex simplevert(appdata_lightmap v)
                {
                    v2f_vertex o;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.uv[0].xy = TRANSFORM_TEX(v.texcoord.xy, _Control);
                    #ifdef LIGHTMAP_ON
                    o.uv[0].zw = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                    #else
	                o.uv[0].zw = half2(0,0);
                    #endif
                    o.uv[1].xy = TRANSFORM_TEX(v.texcoord.xy, _Splat0);
                    o.uv[1].zw = TRANSFORM_TEX(v.texcoord.xy, _Splat1);
                    o.uv[2].xy = TRANSFORM_TEX(v.texcoord.xy, _Splat2);
                    o.uv[2].zw = TRANSFORM_TEX(v.texcoord.xy, _Splat3);
                    UNITY_TRANSFER_FOG(o,o.pos);
                    return o;
                }

                float4 simplefrag(v2f_vertex i) : COLOR
                {
                    half4 splat_control = tex2D(_Control, i.uv[0].xy);
                    half4 splat_color = tex2D(_BaseMap, i.uv[0].xy);
                    splat_color += splat_control.r * tex2D(_Splat0, i.uv[1].xy);
                    splat_color += splat_control.g * tex2D(_Splat1, i.uv[1].zw);
                    splat_color += splat_control.b * tex2D(_Splat2, i.uv[2].xy);
                    splat_color += splat_control.a * tex2D(_Splat3, i.uv[2].zw);
                    #ifdef LIGHTMAP_ON
                    splat_color.rgb *= DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.uv[0].zw));
                    #endif

                    UNITY_APPLY_FOG(i.fogCoord, splat_color);
                    return splat_color;
                }
                ENDCG
            }
        }
    }

    // Fallback to base map
    Fallback "Hidden/TerrainEngine/Splatmap/Lightmap-BaseMap"
}