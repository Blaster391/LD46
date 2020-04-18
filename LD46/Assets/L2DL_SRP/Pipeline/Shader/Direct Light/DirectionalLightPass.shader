Shader "L2DL/DirectionalLightPass"
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
			#pragma fragment DirectionalLightPassFragment

			#include "../../ShaderLibrary/Direct Light/DirectionalLightPass.hlsl"

			ENDHLSL
        }
    }
}
