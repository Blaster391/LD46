Shader "L2DL/SpotLightPass"
{
    Properties
    {
    }

    SubShader
    {
        Pass
        {
			Blend One One
			CULL Off
			ZTest Always
			ZWrite Off

			HLSLPROGRAM

			#pragma target 3.5

			#pragma vertex DirectLightPassVertex
			#pragma fragment SpotLightPassFragment

			#include "../../ShaderLibrary/Direct Light/SpotLightPass.hlsl"

			ENDHLSL
        }
    }
}
