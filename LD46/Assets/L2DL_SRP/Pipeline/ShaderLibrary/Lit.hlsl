#ifndef MYRP_Lit_INCLUDED
#define MYRP_Lit_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

CBUFFER_START(UnityPerFrame)
    float4x4 unity_MatrixVP;
CBUFFER_END

CBUFFER_START(UnityPerDraw)
    float4x4 unity_ObjectToWorld;
CBUFFER_END

#define MAX_VISIBLE_LIGHTS 4

CBUFFER_START(_LightBuffer)
    float4 _visibleLightColors[MAX_VISIBLE_LIGHTS];
    float4 _visibleLightDirectionsOrPositions[MAX_VISIBLE_LIGHTS];
    float4 _visibleLightAttenuations[MAX_VISIBLE_LIGHTS];
    float4 _visibleLightSpotDirections[MAX_VISIBLE_LIGHTS];
CBUFFER_END

// Instancing support
#define UNITY_MATRIX_M unity_ObjectToWorld

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

UNITY_INSTANCING_BUFFER_START(PerInstance)
    UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
UNITY_INSTANCING_BUFFER_END(PerInstance)

struct VertexInput 
{
	float4 pos : POSITION;
    float3 normal : NORMAL;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VertexOutput 
{
	float4 clipPos : SV_POSITION;
    float3 normal : TEXCOORD0;
    float3 worldPos : TEXCOORD1;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

float3 DiffuseLighting(int index, float3 normal, float3 worldPos)
{
    float3 color = _visibleLightColors[index].rgb;
    float4 directionOrPosition = _visibleLightDirectionsOrPositions[index];
    float4 attenuation = _visibleLightAttenuations[index];
    float3 spotDirection = _visibleLightSpotDirections[index].xyz;

    float3 lightVector = directionOrPosition.xyz - (worldPos * directionOrPosition.w); // 1 for position, 0 for direction
    float3 direction = normalize(lightVector);
    float3 diffuse = saturate(dot(normal, direction));

    float rangeFade = dot(lightVector, lightVector) * attenuation.x; // d^2 * 1/r^2
    rangeFade = saturate(1.0 - rangeFade * rangeFade); // 1 - (d^2 / r^2)^2
    rangeFade *= rangeFade; // (1 - (d^2 / r^2)^2)^2

    float spotFade = dot(spotDirection, direction);
    spotFade = saturate(spotFade * attenuation.z + attenuation.w);
    spotFade *= spotFade;

    float distanceSqr = max(dot(lightVector, lightVector), 0.00001);
    diffuse *= spotFade * rangeFade / distanceSqr;
    return diffuse * color;
}

VertexOutput LitPassVertex (VertexInput input) 
{
	VertexOutput output;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);

	float4 worldPos = mul(UNITY_MATRIX_M, float4(input.pos.xyz, 1.0));
	output.clipPos = mul(unity_MatrixVP, worldPos);

    output.normal = mul((float3x3)UNITY_MATRIX_M, input.normal);

    output.worldPos = worldPos.xyz;

	return output;
}

float4 LitPassFragment (VertexOutput input) : SV_TARGET 
{
    UNITY_SETUP_INSTANCE_ID(input);
	float4 albedo = UNITY_ACCESS_INSTANCED_PROP(PerInstance, _Color);

    float3 diffuseLight = 0;
    for(int i = 0; i < MAX_VISIBLE_LIGHTS; i++)
    {
        diffuseLight += DiffuseLighting(i, input.normal, input.worldPos);
    }

    float3 color = diffuseLight * albedo.rgb;

    return float4(color, 1);
}

#endif // MYRP_Lit_INCLUDED