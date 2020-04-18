#ifndef MYRP_DIRECTIONALLIGHTPASS_INCLUDED
#define MYRP_DIRECTIONALLIGHTPASS_INCLUDED

#include "DirectLightUtils.hlsl"

CBUFFER_START(_LightBuffer)
    float4 _lightColor;
    float _lightIntensity;
CBUFFER_END

FragOutput DirectionalLightPassFragment (VertexOutput input)
{ 
    ShadowMapSample shadowMap = SampleShadowMap(input);
    
    FragOutput output;

    // For directional lights we do something different for objects in the 'background' because they're 
    // considered so far away that they won't cast shadows on other objects
    if(shadowMap.depthNormalised >= _backgroundDepthFromCameraNormalised)
    {
	    output.directLight = _lightColor * _lightIntensity /** _DirectLightMultiplier*/;
        output.emissiveLight = float4(0, 0, 0, 1);
    }
    else
    {
	    output.directLight = _lightColor * _lightIntensity * float4(shadowMap.remainingLight, 1) /** _DirectLightMultiplier*/;
        output.emissiveLight = output.directLight * shadowMap.color * shadowMap.reflectivity;
    }

    return output;
}

#endif // MYRP_DIRECTIONALLIGHTPASS_INCLUDED