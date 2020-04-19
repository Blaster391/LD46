#ifndef MYRP_L2DLSPRITEPARTICLES_INCLUDED
#define MYRP_L2DLSPRITEPARTICLES_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

CBUFFER_START(UnityPerFrame)
    float4x4 unity_MatrixVP;
CBUFFER_END

CBUFFER_START(UnityPerDraw)
    float4x4 unity_ObjectToWorld;
CBUFFER_END

CBUFFER_START(Lighting)
    float _Emission;
    float _Occlusion;
    float _Reflectance;
    float _LightBleed;
CBUFFER_END

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

TEXTURE2D(_Lighting);
SAMPLER(sampler_Lighting);

// Material Color
float4 _Color;
float2 _LightingStartUV;
float2 _LightingEndUV;

struct VertexInput 
{
	float4 pos : POSITION;
    float4 color : COLOR;
    float2 uv : TEXCOORD0;
};

struct VertexOutput 
{
	float4 clipPos : SV_POSITION;
    float4 color : COLOR;
    float2 uv : TEXCOORD2;
};

struct FragOutput
{
    float4 color : SV_Target0;
    float4 occlusionReflection : SV_Target1;
    float4 emission : SV_Target2;
    float4 additionalData : SV_Target3;
};

VertexOutput L2DLSpriteParticlesPassVertex (VertexInput input) 
{
	VertexOutput output;

	float4 worldPos = mul(unity_ObjectToWorld, float4(input.pos.xyz, 1.0));
	output.clipPos = mul(unity_MatrixVP, worldPos);

    output.color = input.color * _Color;

    output.uv = input.uv;

	return output;
}

FragOutput L2DLSpriteParticlesPassFragment (VertexOutput input) 
{
    ////  Sample Inputs ////
    float4 spriteColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
    // R - emission, G - occlusion, B - reflectivity
    
	// Calculate the texture coordinates for _Lighting in case we're animating the texture
	half2 lightingMaskTexcoord = _LightingStartUV + input.uv.xy * (_LightingEndUV - _LightingStartUV);

    float4 lightingMasks = SAMPLE_TEXTURE2D(_Lighting, sampler_Lighting, lightingMaskTexcoord);
    //lightingMasks *= lightingMasks.a;

    //// Calculate outputs ////
    FragOutput output;
    
    // Everything must be pre-multiplied by the colour alpha as we're using One OneMinusSrcAlpha blending
    // http://amindforeverprogramming.blogspot.com/2013/07/why-alpha-premultiplied-colour-blending.html

    // Target 1 : Color
	output.color =  spriteColor * input.color; //float4(spriteColor.a, 0, 0, 1);//
    float alpha = output.color.a;
    output.color.rgb *= alpha;

    // Target 2 : Occlusion/Reflectivity
	float3 absorbedColours = float3(1, 1, 1) - (output.color.rgb * _LightBleed);
	output.occlusionReflection = float4(absorbedColours * lightingMasks.g * _Occlusion * alpha, alpha);
    
    // Target 3 : Emission
	output.emission = float4(output.color.rgb * lightingMasks.r * _Emission * alpha, alpha);

    // Target 4 : Currently 'Reflectance', void, void
    float reflectivity = lightingMasks.b * _Reflectance;
	output.additionalData = float4(reflectivity * alpha, 0, 0, alpha);

    return output;
}

#endif // MYRP_L2DLSPRITEPARTICLES_INCLUDED