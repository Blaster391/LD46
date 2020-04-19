Shader "L2DL/L2DLSpriteParticles"
{
    Properties
    {
		[PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
		[PerRendererData] _Lighting ("Lighting Data", 2D) = "white" {}
		_Color ("Color", Color) = (1, 1, 1, 1)
		[HideInInspector] _LightingStartUV ("Lighting Start UV", Vector) = (0, 0, 0, 0)
		[HideInInspector] _LightingEndUV ("Lighting End UV", Vector) = (1, 1, 1, 1)
    }

    SubShader
    {
        Pass
        {
			Name "L2DLSprite"

			Tags
			{ 
				"Queue" = "Transparent"
				"RenderType"="Transparent" 
				"PreviewType"="Plane"
				"CanUseSpriteAtlas"="True"
			}

			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off 

			HLSLPROGRAM

			#pragma target 3.5

			#pragma vertex L2DLSpriteParticlesPassVertex
			#pragma fragment L2DLSpriteParticlesPassFragment

			#include "../ShaderLibrary/L2DLSprite_Particles.hlsl"

			ENDHLSL
        }
    }
}
