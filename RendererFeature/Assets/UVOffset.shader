Shader "MyEffect/UVOffset"
{
    Properties
    {
        [MainTexture] _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off 
        ZWrite Off
        ZTest Always
        Tags { "RenderType"="UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex ZoomBlurVert
            #pragma fragment ZoomBlurFrag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes
            {
                float4 vertex: POSITION;
                float2 uv: TEXCOORD0;
            };

            struct Varyings
            {
                float4 pos: SV_POSITION;
                float2 uv: TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float _Strength;
            float2 _OffsetDir;
            CBUFFER_END

            TEXTURE2D(_MainTex);    SAMPLER(sampler_MainTex);
            
            Varyings ZoomBlurVert(Attributes v)
            {
                Varyings o;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
                o.pos = vertexInput.positionCS;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 ZoomBlurFrag(Varyings i): SV_Target
            {
                float r = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv).r;
                float g = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + _OffsetDir * _Strength).g;
                float b = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv - _OffsetDir * _Strength).b;
                return float4(r,g,b,1);
            }
            ENDHLSL
        }
    }
}
