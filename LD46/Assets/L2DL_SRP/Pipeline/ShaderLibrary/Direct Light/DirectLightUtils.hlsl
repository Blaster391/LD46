#ifndef L2DL_DIRECTLIGHTUTILS_INCLUDED
#define L2DL_DIRECTLIGHTUTILS_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

CBUFFER_START(UnityPerFrame)
    float4x4 unity_MatrixVP;
CBUFFER_END

CBUFFER_START(UnityPerDraw)
    float4x4 unity_ObjectToWorld;
CBUFFER_END

// Shadowmap Properties
float4x4 _CameraToWorld;
float4x4 _WorldToCamera;
float4x4 _WorldToShadow;
float4 _ProjectionParams;

TEXTURE2D(_L2DLDirectLightShadowMap);
SAMPLER(sampler_L2DLDirectLightShadowMap);

TEXTURE2D(_L2DLColorTexture);
SAMPLER(sampler_L2DLColorTexture);

TEXTURE2D(_L2DLAdditionalDataTexture);
SAMPLER(sampler_L2DLAdditionalDataTexture);

TEXTURE2D(_L2DLDepthTexture);
SAMPLER(sampler_L2DLDepthTexture);

// Values
float _backgroundDepthFromCameraNormalised;

// Common structures
struct VertexInput 
{
	float4 pos : POSITION;
    float2 uv : TEXCOORD0;
};

struct VertexOutput 
{
	float4 clipPos : SV_POSITION;
    float2 uv : TEXCOORD0;
    float3 worldPos : TEXCOORD1;
    float2 shadowUV : TEXCOORD2;
};

struct FragOutput
{
    float4 directLight : SV_Target0;
    float4 emissiveLight : SV_Target1;
};

struct ShadowMapSample
{
    float4 color;
    float3 occlusion;
    float3 remainingLight;
    float reflectivity;
    float depthNormalised;
};

// A vertex shader that's shared by all direct lights
VertexOutput DirectLightPassVertex (VertexInput input) 
{
	VertexOutput output;
	output.clipPos = float4(input.pos.xy * 2.0 - 1.0, 0.0, 1.0);
    output.uv = input.uv;

    // Flip UV for some build targets
    if(_ProjectionParams.x < 0.0)
    {
        output.uv.y = 1.0 - output.uv.y;
    }

    // Clip to world
    output.worldPos = mul(_CameraToWorld, output.clipPos).rgb;
    
    // World to shadow clip
    float4 shadowClip = mul(_WorldToShadow, float4(output.worldPos, 1));

    // Shadow clip to uv
    output.shadowUV = (shadowClip.xy + 1.0) / 2.0;
    if(_ProjectionParams.x < 0.0)
    {
        output.shadowUV.y = 1.0 - output.shadowUV.y;
    }

	return output;
}

inline bool IsWithinShadowMap(VertexOutput input)
{
    return (input.shadowUV.x > 0.0 && input.shadowUV.x <= 1.0 && input.shadowUV.y > 0.0 && input.shadowUV.y <= 1.0);
}

inline float3 GetRemainingLightColor(float3 occlusion)
{
     return clamp(float3(1, 1, 1) - occlusion, float3(0, 0, 0), float3(1, 1, 1));
}

inline ShadowMapSample SampleShadowMap(VertexOutput input)
{
    ShadowMapSample shadowMapSample;

    if(IsWithinShadowMap(input))
    {
        float4 color = SAMPLE_TEXTURE2D(_L2DLColorTexture, sampler_L2DLColorTexture, input.uv);

	    float4 shadowMapRaw = SAMPLE_TEXTURE2D(_L2DLDirectLightShadowMap, sampler_L2DLDirectLightShadowMap, input.shadowUV.xy);
        // Can add some common blurring types given defines or inputs here

        float reflectivity = SAMPLE_TEXTURE2D(_L2DLAdditionalDataTexture, sampler_L2DLAdditionalDataTexture, input.uv).r;
        float depth = 1 - SAMPLE_TEXTURE2D(_L2DLDepthTexture, sampler_L2DLDepthTexture, input.uv).r;

        shadowMapSample.color = color;
        shadowMapSample.occlusion = shadowMapRaw.rgb;
        shadowMapSample.remainingLight = GetRemainingLightColor(shadowMapSample.occlusion);
        shadowMapSample.reflectivity = reflectivity;
        shadowMapSample.depthNormalised = depth;
    }
    else
    {
        shadowMapSample.color = float4(0, 0, 0, 0);
        shadowMapSample.occlusion = float3(0, 0, 0);
        shadowMapSample.remainingLight = float3(0, 0, 0);
        shadowMapSample.reflectivity = 0;
        shadowMapSample.depthNormalised = 0;
    }
    return shadowMapSample;
}

#endif // L2DL_DIRECTLIGHTUTILS_INCLUDED