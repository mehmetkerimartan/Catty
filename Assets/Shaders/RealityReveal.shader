Shader "Custom/RealityReveal"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Top Color", Color) = (0.6,0.15,0.1,1)
        _BottomColorHeaven ("Bottom Color Heaven", Color) = (0.9,0.9,0.9,1)
        _BottomColorHell ("Bottom Color Hell", Color) = (0.1,0.02,0.02,1)
        _FadeStart ("Fade Start Y", Float) = 0
        _FadeEnd ("Fade End Y", Float) = -10
        _EdgeColor ("Edge Color", Color) = (0.1,0.05,0.05,1)
        _EdgeThickness ("Edge Thickness", Range(0.001, 0.05)) = 0.01
        _EdgeWidth ("Tear Edge Width", Range(0.5, 4.0)) = 2.0
        _TearEdgeColor ("Tear Edge Glow", Color) = (1,0.3,0.1,1)
        
        _FogStart ("Fog Start Distance", Float) = 10
        _FogEnd ("Fog End Distance", Float) = 30
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 viewDirWS : TEXCOORD3;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _BottomColorHeaven;
                float4 _BottomColorHell;
                float _FadeStart;
                float _FadeEnd;
                float4 _EdgeColor;
                float _EdgeThickness;
                float _EdgeWidth;
                float4 _TearEdgeColor;
                float _FogStart;
                float _FogEnd;
            CBUFFER_END
            
            float _TearRadius;
            float3 _TearCenter;
            float _RealityBlend;
            float3 _FogColorHeaven;
            float3 _FogColorHell;

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceViewDir(output.positionWS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float dist = distance(input.positionWS, _TearCenter);
                
                float2 dir = input.positionWS.xz - _TearCenter.xz;
                float angle = atan2(dir.y, dir.x);
                
                float wave1 = sin(angle * 8.0 + _Time.y * 5.0) * 0.5;
                float wave2 = sin(angle * 12.0 - _Time.y * 7.0) * 0.3;
                float totalWave = wave1 + wave2;
                
                float adjustedRadius = _TearRadius + totalWave;
                
                if (dist > adjustedRadius || _TearRadius < 0.1)
                {
                    discard;
                }
                
                /* Dynamic bottom color based on reality state */
                half3 bottomColor = lerp(_BottomColorHeaven.rgb, _BottomColorHell.rgb, _RealityBlend);
                
                /* Vertical gradient fade */
                float fadeT = saturate((input.positionWS.y - _FadeEnd) / (_FadeStart - _FadeEnd));
                half3 baseColor = lerp(bottomColor, _Color.rgb, fadeT);
                
                /* Edge detection using fresnel */
                float3 viewDir = normalize(input.viewDirWS);
                float3 normal = normalize(input.normalWS);
                float fresnel = 1.0 - saturate(dot(viewDir, normal));
                float edge = smoothstep(1.0 - _EdgeThickness * 10, 1.0, fresnel);
                
                half3 finalColor = lerp(baseColor, _EdgeColor.rgb, edge * 0.5);
                
                /* Tear edge glow */
                float edgeDist = adjustedRadius - dist;
                if (edgeDist < _EdgeWidth)
                {
                    float edgeFactor = 1.0 - (edgeDist / _EdgeWidth);
                    float smoothEdge = edgeFactor * edgeFactor;
                    float pulse = 0.8 + 0.2 * sin(_Time.y * 10.0);
                    finalColor = lerp(finalColor, _TearEdgeColor.rgb, smoothEdge * 0.6);
                    finalColor += _TearEdgeColor.rgb * smoothEdge * pulse * 0.5;
                }
                
                /* Distance fog from player */
                float fogDist = distance(input.positionWS.xz, _TearCenter.xz);
                float fogFactor = saturate((fogDist - _FogStart) / (_FogEnd - _FogStart));
                half3 fogColor = lerp(_FogColorHeaven, _FogColorHell, _RealityBlend);
                finalColor = lerp(finalColor, fogColor, fogFactor);
                
                return half4(finalColor, 1);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}
