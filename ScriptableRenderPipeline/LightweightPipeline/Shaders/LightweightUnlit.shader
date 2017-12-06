﻿Shader "LightweightPipeline/Standard Unlit"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1, 1, 1, 1)
        _Cutoff("AlphaCutout", Range(0.0, 1.0)) = 0.5
        [Toggle] _SampleGI("SampleGI", float) = 0.0
        _BumpMap("Normal Map", 2D) = "bump" {}

        // BlendMode
        [HideInInspector] _Mode("Mode", Float) = 0.0
        [HideInInspector] _SrcBlend("Src", Float) = 1.0
        [HideInInspector] _DstBlend("Dst", Float) = 0.0
        [HideInInspector] _ZWrite("ZWrite", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "IgnoreProjectors" = "True" "RenderPipeline" = "LightweightPipe" }
        LOD 100

        Blend [_SrcBlend][_DstBlend]
        ZWrite [_ZWrite]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ UNITY_SINGLE_PASS_STEREO STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON
            #pragma multi_compile_fog
            #pragma shader_feature _SAMPLE_GI
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON

            #include "LightweightCore.cginc"
            #include "LightweightSurfaceInput.cginc"

            struct VertexInput
            {
                float4 vertex       : POSITION;
                float2 uv           : TEXCOORD0;
                float2 lightmapUV   : TEXCOORD1;
                float3 normal       : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VertexOutput
            {
                float3 uv0AndFogCoord           : TEXCOORD0; // xy: uv0, z: fogCoord
#if _SAMPLE_GI
                float4 lightmapOrVertexSH       : TEXCOORD1;
                half3 normal                    : TEXCOORD2;
    #if _NORMALMAP
                half3 tangent                   : TEXCOORD3;
                half3 binormal                  : TEXCOORD4;
    #endif
#endif
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            VertexOutput vert(VertexInput v)
            {
                VertexOutput o = (VertexOutput)0;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv0AndFogCoord.xy = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv0AndFogCoord.z = ComputeFogFactor(o.vertex.z);

#if _SAMPLE_GI
                OUTPUT_NORMAL(v, o);
                half3 normalWS = o.normal;
                OUTPUT_LIGHTMAP_UV(v.lightmapUV, unity_LightmapST, o.lightmapOrVertexSH.xy);
                OUTPUT_SH(normalWS, o.lightmapOrVertexSH);
#endif
                return o;
            }

            fixed4 frag(VertexOutput IN) : SV_Target
            {
                half2 uv = IN.uv0AndFogCoord.xy;
                half4 texColor = tex2D(_MainTex, uv);
                half3 color = texColor.rgb * _Color.rgb;
                half alpha = texColor.a * _Color.a;

                AlphaDiscard(alpha, _Cutoff);

#if _SAMPLE_GI
    #if _NORMALMAP
                half3 normalWS = TangentToWorldNormal(surfaceData.normal, IN.tangent, IN.binormal, IN.normal);
    #else
                half3 normalWS = normalize(IN.normal);
    #endif
                color += SampleGI(IN.lightmapOrVertexSH, normalWS);
#endif
                ApplyFog(color, IN.uv0AndFogCoord.z);

#ifdef _ALPHABLEND_ON
                return fixed4(color, alpha);
#else
                return fixed4(color, 1.0);
#endif
            }
            ENDCG
        }
    }
    CustomEditor "LightweightUnlitGUI"
}
