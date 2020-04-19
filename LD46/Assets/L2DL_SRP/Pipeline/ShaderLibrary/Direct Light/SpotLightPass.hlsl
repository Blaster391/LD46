#ifndef L2DL_SPOTLIGHTPASS_INCLUDED
#define L2DL_SPOTLIGHTPASS_INCLUDED

#include "DirectLightUtils.hlsl"

CBUFFER_START(_LightBuffer)
    float4 _lightColor;
    float2 _lightPosition;
    float2 _lightDirection;
    float _lightIntensity;
    float _lightAttenuation;
    float2 _lightFade;
    float _maxIntensityOutput;
    float _startingRange;
CBUFFER_END

FragOutput SpotLightPassFragment (VertexOutput input)
{
    ShadowMapSample shadowMap = SampleShadowMap(input);

    float2 lightVector = input.worldPos.xy - _lightPosition;
    float rangeFade = dot(lightVector, lightVector) * _lightAttenuation;
    rangeFade = saturate(1.0 - rangeFade * rangeFade);
    rangeFade *= rangeFade;

    float2 lightDirection = normalize(lightVector);
    float spotFade = dot(lightDirection, _lightDirection);
    spotFade = saturate(spotFade * _lightFade.x + _lightFade.y);
    spotFade *= spotFade;

    float distanceSqr = max(dot(lightVector, lightVector), 0.00001);

    if(distanceSqr < _startingRange * _startingRange)
    {
        spotFade = 0;
    }

    float attenuation = spotFade * rangeFade / distanceSqr;
    
    FragOutput output;
	output.directLight = _lightColor * clamp(_lightIntensity * attenuation, 0, _maxIntensityOutput) * float4(shadowMap.remainingLight, 1) /** _DirectLightMultiplier*/;
    output.emissiveLight = output.directLight * shadowMap.color * shadowMap.reflectivity;
    return output;
}

#endif // L2DL_SPOTLIGHTPASS_INCLUDED