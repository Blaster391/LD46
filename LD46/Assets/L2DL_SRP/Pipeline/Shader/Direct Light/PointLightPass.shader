Shader "L2DL/PointLightPass"
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
			#pragma fragment PointLightPassFragment

			#include "../../ShaderLibrary/Direct Light/PointLightPass.hlsl"

			ENDHLSL
        }
    }
}
