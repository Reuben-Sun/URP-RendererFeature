Shader "MyEffect/ZoomBlur"
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
            int _FocusPower;
            int _FocusDetail;
            float2 _FocusScreenPos;
            float _ReferenceRes;
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
                float2 p = _FocusScreenPos + _ScreenParams.xy / 2;
                float2 mousePos = p.xy / _ScreenParams.xy;
                float2 focus = i.uv - mousePos;
                float aspectX = _ScreenParams.x / _ReferenceRes;
                float3 finColor = 0;
                for(int i = 0; i < _FocusDetail; i++)
                {
                    float power = 1.0 - _FocusPower * (1.0 / _ScreenParams.x * aspectX) * float(i);
                    finColor += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, focus * power + mousePos).rgb;
                }
                finColor *= 1.0 / float(_FocusDetail);
                return float4(finColor, 1);
            }
            ENDHLSL
        }
    }
}
