Shader "SolarSystem/PlanetSurface"
{
    Properties
    {
        _BaseMap ("Surface", 2D) = "white" {}
        _NormalMap ("Normal", 2D) = "bump" {}
        _NormalStrength ("Normal Strength", Range(0,2)) = 1
        _NightMap ("Night Lights", 2D) = "black" {}
        _SpecMap ("Specular Map", 2D) = "white" {}
        _NightStrength ("Night Strength", Range(0,4)) = 1.5
        _CloudMap ("Clouds", 2D) = "black" {}
        _CloudStrength ("Cloud Strength", Range(0,1)) = 0
        _Smoothness ("Smoothness", Range(0,1)) = 0.35
        _Specular ("Specular", Range(0,1)) = 0.25
        _AtmosphereColor ("Atmosphere Color", Color) = (0.12,0.48,1,1)
        _AtmosphereStrength ("Atmosphere Strength", Range(0,3)) = 0.7
        _AtmospherePower ("Atmosphere Power", Range(1,8)) = 3
        _SunPosition ("Sun Position", Vector) = (0,0,0,1)
        _Ambient ("Ambient", Range(0,1)) = 0.045
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Name "PlanetForward"
            Tags { "LightMode"="UniversalForward" }
            Cull Back ZWrite On
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.5
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_NormalMap); SAMPLER(sampler_NormalMap);
            TEXTURE2D(_NightMap); SAMPLER(sampler_NightMap);
            TEXTURE2D(_SpecMap); SAMPLER(sampler_SpecMap);
            TEXTURE2D(_CloudMap); SAMPLER(sampler_CloudMap);
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST, _AtmosphereColor, _SunPosition;
                float _NormalStrength, _NightStrength, _CloudStrength, _Smoothness;
                float _Specular, _AtmosphereStrength, _AtmospherePower, _Ambient;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; float4 tangentOS:TANGENT; float2 uv:TEXCOORD0; };
            struct Varyings { float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; float3 normalWS:TEXCOORD1; float4 tangentWS:TEXCOORD2; float2 uv:TEXCOORD3; };
            Varyings Vert(Attributes v)
            {
                Varyings o;
                VertexPositionInputs p = GetVertexPositionInputs(v.positionOS.xyz);
                VertexNormalInputs n = GetVertexNormalInputs(v.normalOS, v.tangentOS);
                o.positionCS=p.positionCS; o.positionWS=p.positionWS; o.normalWS=n.normalWS;
                o.tangentWS=float4(n.tangentWS,v.tangentOS.w*GetOddNegativeScale());
                o.uv=TRANSFORM_TEX(v.uv,_BaseMap);
                return o;
            }
            half4 Frag(Varyings i):SV_Target
            {
                float3 g=normalize(i.normalWS), t=normalize(i.tangentWS.xyz);
                float3 b=normalize(cross(g,t)*i.tangentWS.w);
                float3 nts=UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap,sampler_NormalMap,i.uv),_NormalStrength);
                float3 n=normalize(t*nts.x+b*nts.y+g*nts.z);
                float4 a=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv);
                float3 l=normalize(_SunPosition.xyz-i.positionWS);
                float ndl=saturate(dot(n,l));
                float day=smoothstep(-0.16,0.18,dot(g,l));
                float3 v=normalize(_WorldSpaceCameraPos-i.positionWS);
                float3 h=normalize(l+v);
                float specMask=SAMPLE_TEXTURE2D(_SpecMap,sampler_SpecMap,i.uv).r;
                float spec=pow(saturate(dot(n,h)),exp2(2.0+_Smoothness*9.0))*_Specular*ndl*lerp(.2,1.0,specMask);
                float3 c=a.rgb*(_Ambient+ndl*1.12)+spec.xxx;
                float3 night=SAMPLE_TEXTURE2D(_NightMap,sampler_NightMap,i.uv).rgb;
                c+=night*_NightStrength*(1.0-day);
                float clouds=SAMPLE_TEXTURE2D(_CloudMap,sampler_CloudMap,i.uv).r;
                c=lerp(c,c+clouds*0.34,_CloudStrength*day);
                float rim=pow(1.0-saturate(dot(g,v)),_AtmospherePower);
                c+=_AtmosphereColor.rgb*rim*_AtmosphereStrength*(0.35+ndl*0.65);
                return half4(c,1);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}