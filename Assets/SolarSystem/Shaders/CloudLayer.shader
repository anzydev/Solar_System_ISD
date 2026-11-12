Shader "SolarSystem/CloudLayer"
{
    Properties { _MainTex("Clouds",2D)="white"{} _Tint("Tint",Color)=(1,1,1,0.5) _Opacity("Opacity",Range(0,1))=0.45 }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST, _Tint; float _Opacity;
            CBUFFER_END
            struct A { float4 p:POSITION; float2 uv:TEXCOORD0; };
            struct V { float4 p:SV_POSITION; float2 uv:TEXCOORD0; };
            V vert(A i){V o;o.p=TransformObjectToHClip(i.p.xyz);o.uv=TRANSFORM_TEX(i.uv,_MainTex);return o;}
            half4 frag(V i):SV_Target{half4 c=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);return half4(c.rgb*_Tint.rgb,c.r*_Opacity*_Tint.a);}
            ENDHLSL
        }
    }
}