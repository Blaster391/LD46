Shader "L2DL/Direct Light/Shadow Map Rendering/DirectionalLightShadowMapRender"
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
			#pragma fragment ShadowMapRenderFragment
			#include "../../../ShaderLibrary/Direct Light/Shadow Map Rendering/DirectionalLightShadowMapRender.hlsl"
			ENDHLSL
        }
    }
}
