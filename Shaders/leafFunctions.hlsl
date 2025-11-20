//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"

#ifndef leafFuncs
#define leafFuncs

void DirectionalEquation_float( float _WindDirection, out float2 output)
{
	float d = _WindDirection * 0.0174532924;
	float xL = cos(d) + 1 / 2;
	float zL = sin(d) + 1 / 2;
	output = float2(zL,xL);
}

void If252_g478_float( int m_Switch , float3 m_Leaves , float3 m_Palm , float3 m_Grass , float3 m_None, out float3 output)
{
	float3 Output = m_None;
	if(m_Switch == 0){Output = m_Leaves;}
	if(m_Switch == 1){Output = m_Palm;}
	if(m_Switch == 2){Output = m_Grass;}
	if(m_Switch == 3){Output = m_None;}
	output = Output;
}

void HSVToRGB_float( float3 c, out float3 output)
{
	float4 K = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
	float3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
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

void If4_g459_float( float Mode , float Cull , float3 Flip , float3 Mirror , float3 None, out float3 output)
{
	float3 OUT = None;
	if(Cull == 0){
		if(Mode == 0)
			OUT = Flip;
		if(Mode == 1)
			OUT = Mirror;
		if(Mode == 2)
			OUT == None;
	}else{
		OUT = None;
	}
	output = OUT;
}

void Dither8x8Bayer_float( int x, int y, out float output)
{
	const float dither[ 64 ] = {
		1, 49, 13, 61,  4, 52, 16, 64,
		33, 17, 45, 29, 36, 20, 48, 32,
		9, 57,  5, 53, 12, 60,  8, 56,
		41, 25, 37, 21, 44, 28, 40, 24,
		3, 51, 15, 63,  2, 50, 14, 62,
		35, 19, 47, 31, 34, 18, 46, 30,
		11, 59,  7, 55, 10, 58,  6, 54,
		43, 27, 39, 23, 42, 26, 38, 22};
	int r = y * 8 + x;
	output = dither[r] / 64; // same # of instructions as pre-dividing due to compiler magic
}

void worldOutNormal_float(float3 worldTangent, float3 worldBiTangent, float3 worldNormal, float3 tanNormal, out float3 output)
{
	float3 tanToWorld0 = float3( worldTangent.x, worldBiTangent.x, worldNormal.x );
	float3 tanToWorld1 = float3( worldTangent.y, worldBiTangent.y, worldNormal.y );
	float3 tanToWorld2 = float3( worldTangent.z, worldBiTangent.z, worldNormal.z );

	output = float3(dot(tanToWorld0,tanNormal), dot(tanToWorld1,tanNormal), dot(tanToWorld2,tanNormal));
}

void GetCommFunc_float(out float4 dir, out float4 col, out float fade)
{
	#ifndef SHADERGRAPH_PREVIEW
		dir = _MainLightPosition;
		col = _MainLightColor;
	#else
		dir = float4(0, 1, 0, 0);
		col = float4(1, 1, 1, 1);
	#endif

	fade = unity_LODFade.x;
}

float _WindStrength;
float _RandomWindOffset;
float _WindPulse;
float _WindDirection;
float _WindTurbulence;

void WindProps_float(out float WindStrength,
			out float RandomWindOffset,
			out float WindPulse,
			out float WindDirection,
			out float WindTurbulence)
{
	WindStrength = _WindStrength;
	RandomWindOffset = _RandomWindOffset;
	WindPulse = _WindPulse;
	WindDirection = _WindDirection;
	WindTurbulence = _WindTurbulence;
}

void GetMatrix_float(out float4x4 objTOworld)
{
	objTOworld = GetObjectToWorldMatrix();
}

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

float2 DirectionalEquation(float _WindDirection)
{
	float d = _WindDirection * 0.0174532924;
	float xL = cos(d) + 1 / 2;
	float zL = sin(d) + 1 / 2;
	return float2(zL, xL);
}
			
float3 If252_g478(int m_Switch, float3 m_Leaves, float3 m_Palm, float3 m_Grass, float3 m_None)
{
	float3 Output = m_None;
	if (m_Switch == 0)
	{
		Output = m_Leaves;
	}
	if (m_Switch == 1)
	{
		Output = m_Palm;
	}
	if (m_Switch == 2)
	{
		Output = m_Grass;
	}
	if (m_Switch == 3)
	{
		Output = m_None;
	}
	return Output;
}
			
float3 HSVToRGB(float3 c)
{
	float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
	float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
	return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
}
			
float3 RGBToHSV(float3 c)
{
	float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
	float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
	float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
	float d = q.x - min(q.w, q.y);
	float e = 1.0e-10;
	return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}
float3 If4_g459(float Mode, float Cull, float3 Flip, float3 Mirror, float3 None)
{
	float3 OUT = None;
	if (Cull == 0)
	{
		if (Mode == 0)
			OUT = Flip;
		if (Mode == 1)
			OUT = Mirror;
		if (Mode == 2)
			OUT == None;
	}
	else
	{
		OUT = None;
	}
	return OUT;
}
			
inline float Dither8x8Bayer(int x, int y)
{
	const float dither[64] =
	{
		1, 49, 13, 61, 4, 52, 16, 64,
			33, 17, 45, 29, 36, 20, 48, 32,
			 9, 57, 5, 53, 12, 60, 8, 56,
			41, 25, 37, 21, 44, 28, 40, 24,
			 3, 51, 15, 63, 2, 50, 14, 62,
			35, 19, 47, 31, 34, 18, 46, 30,
			11, 59, 7, 55, 10, 58, 6, 54,
			43, 27, 39, 23, 42, 26, 38, 22
	};
	int r = y * 8 + x;
	return dither[r] / 64; // same # of instructions as pre-dividing due to compiler magic
}
#endif