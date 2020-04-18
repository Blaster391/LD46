#ifndef UNITY_SPRITES_INCLUDED
#define UNITY_SPRITES_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

CBUFFER_START(UnityPerFrame)
    float4x4 unity_MatrixVP;
CBUFFER_END

CBUFFER_START(UnityPerDraw)
    float4x4 unity_ObjectToWorld;
CBUFFER_END

// Instancing support
#define UNITY_MATRIX_M unity_ObjectToWorld

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

//UNITY_INSTANCING_BUFFER_START(PerDrawSprite)
    //UNITY_DEFINE_INSTANCED_PROP(float4, unity_SpriteRendererColorArray)
    //UNITY_DEFINE_INSTANCED_PROP(float2, unity_SpriteFlipArray)
//UNITY_INSTANCING_BUFFER_END(PerDrawSprite)

//#define _RendererColor  UNITY_ACCESS_INSTANCED_PROP(PerDrawSprite, unity_SpriteRendererColorArray)
//#define _Flip           UNITY_ACCESS_INSTANCED_PROP(PerDrawSprite, unity_SpriteFlipArray)

UNITY_INSTANCING_BUFFER_START(PerDrawSprite)
    UNITY_DEFINE_INSTANCED_PROP(float4, _InstancedColor)
    UNITY_DEFINE_INSTANCED_PROP(float, _InstancedAlpha)
UNITY_INSTANCING_BUFFER_END(PerDrawSprite)

// Material Color.
//float4 _Color;

struct appdata_t
{
    float4 vertex   : POSITION;
    float4 color    : COLOR;
    float2 texcoord : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
    float4 vertex   : SV_POSITION;
    float4 color    : COLOR;
    float2 texcoord : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

inline float4 UnityObjectToClipPos(in float4 vert)
{
	return mul(unity_MatrixVP, mul(UNITY_MATRIX_M, float4(vert.xyz, 1.0)));
}

v2f SpriteVert(appdata_t IN)
{
    v2f OUT;

    UNITY_SETUP_INSTANCE_ID (IN);
    UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

    OUT.vertex = IN.vertex;//UnityFlipSprite(IN.vertex, _Flip);
    OUT.vertex = UnityObjectToClipPos(OUT.vertex);
    OUT.texcoord = IN.texcoord;
    OUT.color = UNITY_ACCESS_INSTANCED_PROP(PerDrawSprite, _InstancedColor); //IN.color * _Color * _RendererColor;
    OUT.color.rgb *= UNITY_ACCESS_INSTANCED_PROP(PerDrawSprite, _InstancedAlpha); //IN.color * _Color * _RendererColor;

    #ifdef PIXELSNAP_ON
    OUT.vertex = UnityPixelSnap (OUT.vertex);
    #endif

    return OUT;
}

TEXTURE2D(_SpriteReplacementTexture);
SAMPLER(sampler_SpriteReplacementTexture);

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

float4 SpriteFrag(v2f IN) : SV_Target0
{
    UNITY_SETUP_INSTANCE_ID(IN);

    float4 spriteColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.texcoord);
    spriteColor.rgb *= spriteColor.a;
    return spriteColor * IN.color;

    //return IN.color;
}

#endif // UNITY_SPRITES_INCLUDED

//#ifndef MYRP_L2DLSPRITESTANDARD_INCLUDED
//#define MYRP_L2DLSPRITESTANDARD_INCLUDED

//#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

//CBUFFER_START(UnityPerFrame)
//    float4x4 unity_MatrixVP;
//CBUFFER_END

//CBUFFER_START(UnityPerDraw)
//    float4x4 unity_ObjectToWorld;
//CBUFFER_END

//// Instancing support
//#define UNITY_MATRIX_M unity_ObjectToWorld

//#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

//TEXTURE2D(_MainTex);
//SAMPLER(sampler_MainTex);

//UNITY_INSTANCING_BUFFER_START(UnityInstancing_PerDrawSprite)
//    UNITY_DEFINE_INSTANCED_PROP(float4, _RendererColor)
//UNITY_INSTANCING_BUFFER_END(UnityInstancing_PerDrawSprite)


//struct VertexInput 
//{
//	float4 pos : POSITION;
//    float4 color : COLOR;
//    float2 uv : TEXCOORD0;
//    UNITY_VERTEX_INPUT_INSTANCE_ID
//};

//struct VertexOutput 
//{
//	float4 clipPos : SV_POSITION;
//    float3 worldPos : TEXCOORD1;
//    float4 color : COLOR;
//    float2 uv : TEXCOORD2;
//    UNITY_VERTEX_INPUT_INSTANCE_ID
//};

//VertexOutput L2DLSpriteStandardPassVertex (VertexInput input) 
//{
//	VertexOutput output;
    
//    UNITY_SETUP_INSTANCE_ID(input);
//    UNITY_TRANSFER_INSTANCE_ID(input, output);

//	float4 worldPos = mul(UNITY_MATRIX_M, float4(input.pos.xyz, 1.0));
//	output.clipPos = mul(unity_MatrixVP, worldPos);
//    output.worldPos = worldPos.xyz;

//    output.color = input.color;

//    output.uv = input.uv;

//	return output;
//}

//float4 L2DLSpriteStandardPassFragment (VertexOutput input) : SV_TARGET0
//{
//    UNITY_SETUP_INSTANCE_ID(input);

//    float4 spriteColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

//    return spriteColor * UNITY_ACCESS_INSTANCED_PROP(UnityInstancing_PerDrawSprite, _RendererColor) * spriteColor.a;
//}

//#endif // MYRP_L2DLSPRITE_INCLUDED