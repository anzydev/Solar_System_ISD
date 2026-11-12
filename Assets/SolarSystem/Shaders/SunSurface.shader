Shader "SolarSystem/SunSurface"
{
    Properties { _MainTex("Solar Surface",2D)="white"{} _Tint("Solar Color",Color)=(1,0.78,0.42,1) _Intensity("Emission",Range(0,8))=2.5 }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" "RenderType"="Opaque" }
        Pass
        {
            Name "Sun"
            Tags { "LightMode"="UniversalForward" }
            Cull Back ZWrite On
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST, _Tint; float _Intensity;
            CBUFFER_END
            struct A{float4 p:POSITION;float3 n:NORMAL;float2 uv:TEXCOORD0;};
            struct V{float4 p:SV_POSITION;float2 uv:TEXCOORD0;float3 n:TEXCOORD1;float3 w:TEXCOORD2;};
            V vert(A i){V o;VertexPositionInputs p=GetVertexPositionInputs(i.p.xyz);o.p=p.positionCS;o.uv=TRANSFORM_TEX(i.uv,_MainTex);o.n=TransformObjectToWorldNormal(i.n);o.w=p.positionWS;return o;}
            half4 frag(V i):SV_Target
            {
                float3 tex=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv).rgb;
                float3 view=normalize(_WorldSpaceCameraPos-i.w);
                float rim=pow(1-saturate(dot(normalize(i.n),view)),2.5);
                float3 color=tex*_Tint.rgb*_Intensity;
                color += _Tint.rgb*(0.12+rim*0.45);
                return half4(color,1);
            }
            ENDHLSL
        }
    }
}