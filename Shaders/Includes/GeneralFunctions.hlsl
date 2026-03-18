#include "LightDataFunctions.hlsl"

#ifndef MTREE_GENERAL_FUNCTIONS
#define MTREE_GENERAL_FUNCTIONS

#ifndef MTREE_WIND_DEFINITIONS
#define MTREE_WIND_DEFINITIONS
float _WindStrength;
float _RandomWindOffset;
float _WindPulse;
float _WindDirection;
float _WindTurbulence;
float _WindStrengthBillboard;
#endif

float2 DirectionalEquation(float _WindDirection)
{
	float d = _WindDirection * 0.0174532924;
	float xL = cos(d) + 1 / 2;
	float zL = sin(d) + 1 / 2;
	return float2(zL, xL);
}

void HSVToRGB_float(float3 c, out float3 output)
{
	float4 K = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
	float3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
	output = c.z * lerp( K.xxx, saturate( p - K.xxx ), c.y );
}

void HSVToRGB_half(half3 c, out half3 output)
{
	half4 K = half4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
	half3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
	output = c.z * lerp( K.xxx, saturate( p - K.xxx ), c.y );
}

void RGBToHSV_float(float3 c, out float3 output)
{
	float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
	float4 p = lerp( float4( c.bg, K.wz ), float4( c.gb, K.xy ), step( c.b, c.g ) );
	float4 q = lerp( float4( p.xyw, c.r ), float4( c.r, p.yzx ), step( p.x, c.r ) );
	float d = q.x - min( q.w, q.y );
	float e = 1.0e-10;
	output = float3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

void RGBToHSV_half(half3 c, out half3 output)
{
	half4 K = half4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
	half4 p = lerp( half4( c.bg, K.wz ), half4( c.gb, K.xy ), step( c.b, c.g ) );
	half4 q = lerp( half4( p.xyw, c.r ), half4( c.r, p.yzx ), step( p.x, c.r ) );
	half d = q.x - min( q.w, q.y );
	half e = 1.0e-10;
	output = half3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

void GetMainLightDirection_float(out float3 dir)
{
	dir = GetLightDirection();
}

void GetMainLightDirection_half(out float3 dir)
{
	dir = GetLightDirection();
}

void GetMainLightColor_float(out float3 col)
{
	col = GetLightColor();
}

void GetMainLightColor_half(out half3 col)
{
	col = GetLightColor();
}

///////////////////////// Wind Strength /////////////////////////
void WindPropsStrength_float(out float WindStrength)
{
	WindStrength = _WindStrength;
}

void WindPropsStrength_half(out half WindStrength)
{
	WindStrength = _WindStrength;
}

///////////////////////// Random Offset /////////////////////////
void WindPropsRandomOffset_float(out float RandomOffset)
{
	RandomOffset = _RandomWindOffset;
}

void WindPropsRandomOffset_half(out half RandomOffset)
{
	RandomOffset = _RandomWindOffset;
}

///////////////////////// Wind Pulse /////////////////////////
void WindPropsWindPulse_float(out float WindPulse)
{
	WindPulse = _WindPulse;
}

void WindPropsWindPulse_half(out half WindPulse)
{
	WindPulse = _WindPulse;
}

///////////////////////// Wind Direction /////////////////////////
void WindPropsWindDirection_float(out float2 WindDirection)
{
	WindDirection = DirectionalEquation(_WindDirection);
}

void WindPropsWindDirection_half(out half2 WindDirection)
{
	WindDirection = DirectionalEquation(_WindDirection);
}

///////////////////////// Wind Turbulence /////////////////////////
void WindPropsWindTurbulence_float(out float WindTurbulence)
{
	WindTurbulence = _WindTurbulence;
}

void WindPropsWindTurbulence_half(out half WindTurbulence)
{
	WindTurbulence = _WindTurbulence;
}

///////////////////// Wind Strength Billboard /////////////////////
void WindPropsStrengthBillboard_float(out float WindStrengthBillboard)
{
	WindStrengthBillboard = _WindStrengthBillboard;
}

void WindPropsStrengthBillboard_half(out half WindStrengthBillboard)
{
	WindStrengthBillboard = _WindStrengthBillboard;
}

//////////////////////////// Switches ////////////////////////////
void LeafTypeSwitch_float(int m_Switch , float3 m_Leaves , float3 m_Palm , float3 m_Grass , float3 m_None, out float3 output)
{
	float3 Output = m_None;
	if(m_Switch == 0){Output = m_Leaves;}
	if(m_Switch == 1){Output = m_Palm;}
	if(m_Switch == 2){Output = m_Grass;}
	if(m_Switch == 3){Output = m_None;}
	output = Output;
}

void LeafTypeSwitch_half(int m_Switch , float3 m_Leaves , float3 m_Palm , float3 m_Grass , float3 m_None, out float3 output)
{
	float3 Output = m_None;
	if(m_Switch == 0){Output = m_Leaves;}
	if(m_Switch == 1){Output = m_Palm;}
	if(m_Switch == 2){Output = m_Grass;}
	if(m_Switch == 3){Output = m_None;}
	output = Output;
}

void CullNormalSwitch_float(float m_Mode , float m_Cull , float3 m_Flip , float3 m_Mirror , float3 m_None, out float3 output)
{
	float3 OUT = m_None;
	if(m_Cull == 0){
		if(m_Mode == 0)
			OUT = m_Flip;
		if(m_Mode == 1)
			OUT = m_Mirror;
		if(m_Mode == 2)
			OUT == m_None;
	}else{
		OUT = m_None;
	}
	output = OUT;
}

void CullNormalSwitch_half(half m_Mode , half m_Cull , half3 m_Flip , half3 m_Mirror , half3 m_None, out half3 output)
{
	half3 OUT = m_None;
	if(m_Cull == 0){
		if(m_Mode == 0)
			OUT = m_Flip;
		if(m_Mode == 1)
			OUT = m_Mirror;
		if(m_Mode == 2)
			OUT == m_None;
	}else{
		OUT = m_None;
	}
	output = OUT;
}

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#endif