#ifndef L2DL_DIRECTIONALLIGHTSHADOWMAPRENDER_INCLUDED
#define L2DL_DIRECTIONALLIGHTSHADOWMAPRENDER_INCLUDED

#include "../../Blit.hlsl"

TEXTURE2D(_OcclusionMap);
float4 _OcclusionMap_TexelSize;
SAMPLER(sampler_OcclusionMap);

int _SamplesPerPixel;
float _WorldDistPerStep;

float4 ShadowMapRenderFragment (VertexOutput input) : SV_TARGET 
{
	// So...
	// Given our uv position we loop a certain number of times which needs to be passed in...
	// In this case we're going from (x, 1) to our UV
	// Depending on how big a uv jump we have per sample we'll sample from a different mip level

	// 1   0.8  0.6  0.4  0.2  0.0
	// |    |    |    |    |    |
	// 
	// |     0.6      |
	//  x  x x  x x  x    
	// |

	// pixels for a mip level = 2^mipLevel
	// 2^3 = 8 pixels
	// mip level from pixels = 
	// 2 = 3 root 8
	
	float2 uvStep = float2(0, (1.f - input.uv.y) / _SamplesPerPixel);
	float2 uvStart = float2(input.uv.x, 1 - uvStep.y / 2.f);

	float2 pixelsPerStep = uvStep * _OcclusionMap_TexelSize.zw;
	float maxPixelsPerStep = max(pixelsPerStep.x, pixelsPerStep.y);

	float mipLevel = log10(maxPixelsPerStep) / 0.30103;//log10(2);

	float3 occlusionTotal = float3(0, 0, 0);

	for(int i = 0; i < _SamplesPerPixel; ++i)
	{
		float3 occlusion = SAMPLE_TEXTURE2D_LOD(_OcclusionMap, sampler_OcclusionMap, uvStart - uvStep * i, mipLevel).rgb;
		occlusionTotal += (float3(1, 1, 1) - occlusionTotal) * occlusion * _WorldDistPerStep;

        if(any(occlusion))
        {
            //occlusionTotal = float3(1, 0, 0);
        }
	}

	
    return float4(occlusionTotal, 1);
}

#endif // L2DL_DIRECTIONALLIGHTSHADOWMAPRENDER_INCLUDED