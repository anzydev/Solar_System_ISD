Shader "SolarSystem/AtmosphereRim"
{
    Properties { _Color("Atmosphere",Color)=(0.15,0.5,1,1) _Power("Rim Power",Range(1,8))=3 _Intensity("Intensity",Range(0,4))=1.2 }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Blend One One
        ZWrite Off
        Cull Front
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _Color; float _Power; float _Intensity;
            CBUFFER_END
            struct A{float4 p:POSITION;float3 n:NORMAL;};
            struct V{float4 p:SV_POSITION;float3 n:TEXCOORD0;float3 v:TEXCOORD1;};
            V vert(A i){V o;float3 w=TransformObjectToWorld(i.p.xyz);o.p=TransformWorldToHClip(w);o.n=TransformObjectToWorldNormal(i.n);o.v=GetWorldSpaceViewDir(w);return o;}
            half4 frag(V i):SV_Target{float rim=pow(1-saturate(dot(normalize(i.n),normalize(i.v))),_Power);return half4(_Color.rgb*rim*_Intensity,rim);}
            ENDHLSL
        }
    }
}