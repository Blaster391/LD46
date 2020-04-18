#ifndef L2DL_GENERATEPOLYPHASEMIPMAPCOMPUTEPROPERTIES_INCLUDED
#define L2DL_GENERATEPOLYPHASEMIPMAPCOMPUTEPROPERTIES_INCLUDED

RWTexture2D<float4> LowerMipTex;
float2 LowerMipTexSize;
float2 LowerMipTexelUVSize;
RWTexture2D<float4> HigherMipTex;
float2 HigherMipTexSize;
float2 HigherMipTexelUVSize;

float isFirstMip;

float standardFilterAv(float value1, float value2)
{
	return 0.5 * (value1 + value2);
}

float standardFilterMax(float value1, float value2)
{
	return max(value1, value2);
}

float polyFilterAv(uint2 texel, float value1, float value2, float value3, int whIndex)
{
	float2 uv = texel * HigherMipTexelUVSize;
	float n = LowerMipTexelUVSize[whIndex] * HigherMipTexSize[whIndex];
	float x = (uv[whIndex] - LowerMipTexelUVSize[whIndex] / 2) * n;
	float w0 = n - x;
	float w1 = n;
	float w2 = LowerMipTexelUVSize[whIndex] + x;

	return w0 * value1 + w1 * value2 + w2 * value3;
}

float polyFilterMax(uint2 texel, float value1, float value2, float value3, int whIndex)
{
	float2 uv = texel * HigherMipTexelUVSize;
	float n = LowerMipTexelUVSize[whIndex] * HigherMipTexSize[whIndex];
	float x = (uv[whIndex] - LowerMipTexelUVSize[whIndex] / 2) * n;
	float w0 = n - x;
	float w1 = n;
	float w2 = LowerMipTexelUVSize[whIndex] + x;

	return max(w1 * value2, (w0 * value1 + w2 * value3)); 
    // We weren't using any of the calculated values, so now we are and I've no idea if that's correct
}

// float4 version
float4 standardFilterAv(float4 value1, float4 value2)
{
	return 0.5 * (value1 + value2);
}

float4 polyFilterAv(uint2 texel, float4 value1, float4 value2, float4 value3, int whIndex)
{
	float2 uv = texel * HigherMipTexelUVSize;
	float n = LowerMipTexelUVSize[whIndex] * HigherMipTexSize[whIndex];
	float x = (uv[whIndex] - LowerMipTexelUVSize[whIndex] / 2) * n;
	float w0 = n - x;
	float w1 = n;
	float w2 = LowerMipTexelUVSize[whIndex] + x;

	return w0 * value1 + w1 * value2 + w2 * value3;
}

#endif // L2DL_GENERATEPOLYPHASEMIPMAPCOMPUTEPROPERTIES_INCLUDED