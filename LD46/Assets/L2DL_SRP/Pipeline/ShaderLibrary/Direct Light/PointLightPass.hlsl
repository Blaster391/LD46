#ifndef MYRP_POINTLIGHTPASS_INCLUDED
#define MYRP_POINTLIGHTPASS_INCLUDED

#include "DirectLightUtils.hlsl"

CBUFFER_START(_LightBuffer)
    float4 _lightColor;
    float _lightIntensity;
    float _lightAttenuation;
    float2 _lightPosition;
CBUFFER_END

FragOutput PointLightPassFragment (VertexOutput input)
{ 
    ShadowMapSample shadowMap = SampleShadowMap(input);

    float2 lightVector = input.worldPos.xy - _lightPosition;
    float rangeFade = dot(lightVector, lightVector) * _lightAttenuation;
    rangeFade = saturate(1.0 - rangeFade * rangeFade);
    rangeFade *= rangeFade;

    float distanceSqr = max(dot(lightVector, lightVector), 0.00001);

    float attenuation = rangeFade / distanceSqr;

    FragOutput output;
	output.directLight = _lightColor * _lightIntensity * attenuation * float4(shadowMap.remainingLight, 1) /** _DirectLightMultiplier*/;
    output.emissiveLight = output.directLight * shadowMap.color * shadowMap.reflectivity;
    return output;
}

#endif // MYRP_POINTLIGHTPASS_INCLUDED