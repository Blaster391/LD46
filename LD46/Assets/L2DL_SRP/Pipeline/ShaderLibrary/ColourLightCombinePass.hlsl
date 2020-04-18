#ifndef MYRP_COLOURLIGHTCOMBINEPASS_INCLUDED
#define MYRP_COLOURLIGHTCOMBINEPASS_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

TEXTURE2D(_Color);
SAMPLER(sampler_Color);

TEXTURE2D(_DirectLight);
SAMPLER(sampler_DirectLight);

TEXTURE2D(_IndirectLight);
SAMPLER(sampler_IndirectLight);
int _IndirectLightMip;

float4 _ProjectionParams;

struct VertexInput 
{
	float4 pos : POSITION;
    float2 uv : TEXCOORD0;
};

struct VertexOutput 
{
	float4 clipPos : SV_POSITION;
    float2 uv : TEXCOORD0;
};

VertexOutput ColourLightCombinePassVertex (VertexInput input) 
{
	VertexOutput output;
	output.clipPos = float4(input.pos.xy * 2.0 - 1.0, 0.0, 1.0);
    output.uv = input.uv;
    if(_ProjectionParams.x < 0.0)
    {
        output.uv.y = 1.0 - output.uv.y;
    }
	return output;
}

float4 ColourLightCombinePassFragment (VertexOutput input) : SV_TARGET 
{
    return SAMPLE_TEXTURE2D(_Color, sampler_Color, input.uv) * (SAMPLE_TEXTURE2D(_DirectLight, sampler_DirectLight, input.uv) + SAMPLE_TEXTURE2D_LOD(_IndirectLight, sampler_IndirectLight, input.uv, _IndirectLightMip));
}

#endif // MYRP_COLOURLIGHTCOMBINEPASS_INCLUDED