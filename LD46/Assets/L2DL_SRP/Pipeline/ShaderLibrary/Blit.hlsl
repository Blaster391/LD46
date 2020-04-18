#ifndef MYRP_BLIT_INCLUDED
#define MYRP_BLIT_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

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

VertexOutput BlitPassVertex (VertexInput input) 
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

float4 BlitPassFragment (VertexOutput input) : SV_TARGET 
{
    return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
}

#endif // MYRP_BLIT_INCLUDED