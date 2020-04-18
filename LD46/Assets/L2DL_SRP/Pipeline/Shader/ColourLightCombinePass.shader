Shader "L2DL/ColorLightCombinePass"
{
    Properties
    {
    }

    SubShader
    {
        Pass
        {
			CULL Off
			ZTest Always
			ZWrite Off

			HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex ColourLightCombinePassVertex
			#pragma fragment ColourLightCombinePassFragment
			#include "../ShaderLibrary/ColourLightCombinePass.hlsl"
			ENDHLSL
        }
    }
}
