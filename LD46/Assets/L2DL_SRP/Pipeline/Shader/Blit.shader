Shader "My Pipeline/Blit"
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
			#pragma vertex BlitPassVertex
			#pragma fragment BlitPassFragment
			#include "../ShaderLibrary/Blit.hlsl"
			ENDHLSL
        }
    }
}
