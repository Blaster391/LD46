Shader "My Pipeline/L2DLSpriteStandard"
{
    Properties
    {
		[PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
		[PerRendererData] _InstancedColor ("Instanced Color", Color) = (1,1,1,1)
		[PerRendererData] _InstancedAlpha ("Instance Alpha", Range(0.0, 1.0)) = 1.0
		[PerRendererData] _SpriteReplacementTexture ("ActualSprite", 2D) = "white" {}
    }

    SubShader
    {
        Pass
        {
			Name "L2DLSpriteStandard"

			Tags
			{ 
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent" 
				"PreviewType" = "Plane"
				"CanUseSpriteAtlas" = "True"
			}

			ZWrite Off
			Blend One OneMinusSrcAlpha

			HLSLPROGRAM

			#pragma target 3.5

			#pragma multi_compile_instancing
			#pragma multi_compile_local _ PIXELSNAP_ON

			#pragma vertex SpriteVert
			#pragma fragment SpriteFrag

			#include "../ShaderLibrary/L2DLSpriteStandard.hlsl"

			ENDHLSL
        }
    }
}
