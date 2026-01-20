Shader "Custom/OutlineOverride"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (1, 0.5, 0, 1)
        _OutlineWidth ("Outline Width", Range(0.0, 0.1)) = 0.02
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry-1" }
        
        Pass
        {
            Name "OutlineOverride"
            Cull Front
            ZWrite Off
            ZTest Always
            ColorMask RGB
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float _OutlineWidth;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // Use smooth normals based on vertex position for broken normals
                float3 smoothNormal = normalize(input.positionOS.xyz);
                
                // Blend between mesh normal and position-based normal
                float3 expandDir = normalize(lerp(input.normalOS, smoothNormal, 0.5));
                
                float3 expandedPos = input.positionOS.xyz + expandDir * _OutlineWidth;
                output.positionCS = TransformObjectToHClip(expandedPos);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }
    }
    FallBack Off
}
