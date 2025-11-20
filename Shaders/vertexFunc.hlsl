#include "leafFunctions.hlsl"

void VertexCustomFunc_float(float3 worldPos, float3 vertCol, float3 worldNorm, out float3 output)
{
	int m_Switch252_g478 = _WindModeLeaves;
	float3 VAR_VertexPosition21_g478 = worldPos;
	float3 break109_g478 = VAR_VertexPosition21_g478;
	float VAR_WindStrength43_g478 = (_WindStrength * _GlobalWindInfluence);
	float4 transform37_g478 = mul(GetObjectToWorldMatrix(), float4(0, 0, 0, 1));
	float2 appendResult38_g478 = (float2(transform37_g478.x, transform37_g478.z));
	float dotResult2_g479 = dot(appendResult38_g478, float2(12.9898, 78.233));
	float lerpResult8_g479 = lerp(0.8, ((_RandomWindOffset / 2.0) + 0.9), frac((sin(dotResult2_g479) * 43758.55)));
	float VAR_RandomTime16_g478 = ((_TimeParameters.x * 0.05) * lerpResult8_g479);
	float FUNC_Turbulence36_g478 = (sin(((VAR_RandomTime16_g478 * 40.0) - (VAR_VertexPosition21_g478.z / 15.0))) * 0.5);
	float VAR_WindPulse274_g478 = _WindPulse;
	float FUNC_Angle73_g478 = (VAR_WindStrength43_g478 * (1.0 + sin(((((VAR_RandomTime16_g478 * 2.0) + FUNC_Turbulence36_g478) - (VAR_VertexPosition21_g478.z / 50.0)) - (vertCol.r / 20.0)))) * sqrt(vertCol.r) * 0.2 * VAR_WindPulse274_g478);
	float VAR_SinA80_g478 = sin(FUNC_Angle73_g478);
	float VAR_CosA78_g478 = cos(FUNC_Angle73_g478);
	float _WindDirection164_g478 = _WindDirection;
	float2 localDirectionalEquation164_g478 = DirectionalEquation(_WindDirection164_g478);
	float2 break165_g478 = localDirectionalEquation164_g478;
	float VAR_xLerp83_g478 = break165_g478.x;
	float lerpResult118_g478 = lerp(break109_g478.x, ((break109_g478.y * VAR_SinA80_g478) + (break109_g478.x * VAR_CosA78_g478)), VAR_xLerp83_g478);
	float3 break98_g478 = VAR_VertexPosition21_g478;
	float3 break105_g478 = VAR_VertexPosition21_g478;
	float VAR_zLerp95_g478 = break165_g478.y;
	float lerpResult120_g478 = lerp(break105_g478.z, ((break105_g478.y * VAR_SinA80_g478) + (break105_g478.z * VAR_CosA78_g478)), VAR_zLerp95_g478);
	float3 appendResult122_g478 = (float3(lerpResult118_g478, ((break98_g478.y * VAR_CosA78_g478) - (break98_g478.z * VAR_SinA80_g478)), lerpResult120_g478));
	float3 FUNC_vertexPos123_g478 = appendResult122_g478;
	float3 break236_g478 = FUNC_vertexPos123_g478;
	half FUNC_SinFunction195_g478 = sin(((VAR_RandomTime16_g478 * 200.0 * (0.2 + vertCol.g)) + (vertCol.g * 10.0) + FUNC_Turbulence36_g478 + (VAR_VertexPosition21_g478.z / 2.0)));
	float VAR_GlobalWindTurbulence194_g478 = (_WindTurbulence * _GlobalTurbulenceInfluence);
	float3 appendResult237_g478 = (float3(break236_g478.x, (break236_g478.y + (FUNC_SinFunction195_g478 * vertCol.b * (FUNC_Angle73_g478 + (VAR_WindStrength43_g478 / 200.0)) * VAR_GlobalWindTurbulence194_g478)), break236_g478.z));
	float3 OUT_Leafs_Standalone244_g478 = appendResult237_g478;
	float3 m_Leaves252_g478 = OUT_Leafs_Standalone244_g478;
	float3 ase_worldNormal = worldNorm;
	float3 normalizedWorldNormal = normalize(ase_worldNormal);
	float3 appendResult234_g478 = (float3((normalizedWorldNormal.x * vertCol.g), (normalizedWorldNormal.y / vertCol.r), (normalizedWorldNormal.z * vertCol.g)));
	float3 OUT_Palm_Standalone243_g478 = (((FUNC_SinFunction195_g478 * vertCol.b * (FUNC_Angle73_g478 + (VAR_WindStrength43_g478 / 200.0)) * VAR_GlobalWindTurbulence194_g478) * appendResult234_g478) + FUNC_vertexPos123_g478);
	float3 m_Palm252_g478 = OUT_Palm_Standalone243_g478;
	float3 break221_g478 = FUNC_vertexPos123_g478;
	float temp_output_202_0_g478 = (FUNC_SinFunction195_g478 * vertCol.b * (FUNC_Angle73_g478 + (VAR_WindStrength43_g478 / 200.0)));
	float lerpResult203_g478 = lerp(0.0, temp_output_202_0_g478, VAR_xLerp83_g478);
	float lerpResult196_g478 = lerp(0.0, temp_output_202_0_g478, VAR_zLerp95_g478);
	float3 appendResult197_g478 = (float3((break221_g478.x + lerpResult203_g478), break221_g478.y, (break221_g478.z + lerpResult196_g478)));
	float3 OUT_Grass_Standalone245_g478 = appendResult197_g478;
	float3 m_Grass252_g478 = OUT_Grass_Standalone245_g478;
	float3 m_None252_g478 = FUNC_vertexPos123_g478;
	float3 localIf252_g478 = If252_g478(m_Switch252_g478, m_Leaves252_g478, m_Palm252_g478, m_Grass252_g478, m_None252_g478);
	float3 OUT_Leafs262_g478 = localIf252_g478;
	float3 temp_output_5_0_g478 = mul(GetWorldToObjectMatrix(), float4(OUT_Leafs262_g478, 0.0)).xyz;
	float3 OUT_VertexPos261 = temp_output_5_0_g478;
	
	output = OUT_VertexPos261;
}