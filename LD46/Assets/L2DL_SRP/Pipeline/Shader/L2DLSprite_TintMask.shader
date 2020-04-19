Shader "L2DL/L2DLSpriteTintMask"
{
    Properties
    {
		[PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
		[PerRendererData] _Lighting ("Lighting Data", 2D) = "white" {}
		_TintMask ("Tint Mask", 2D) = "white" {}
		_Color ("Color", Color) = (1, 1, 1, 1)
		[HideInInspector] _LightingStartUV ("Lighting Start UV", Vector) = (0, 0, 0, 0)
		[HideInInspector] _LightingEndUV ("Lighting End UV", Vector) = (1, 1, 1, 1)
    }

    SubShader
    {
        Pass
        {
			Name "L2DLSpriteTintMask"

			Tags
			{ 
				"Queue" = "Transparent"
				"RenderType"="Transparent" 
				"PreviewType"="Plane"
				"CanUseSpriteAtlas"="True"
			}

			ZWrite On
			Blend One OneMinusSrcAlpha
			Cull Off 

			HLSLPROGRAM

			#pragma target 3.5

			#pragma vertex L2DLSpriteTintMaskPassVertex
			#pragma fragment L2DLSpriteTintMaskPassFragment

			#include "../ShaderLibrary/L2DLSprite_TintMask.hlsl"

			ENDHLSL
        }
    }
}
