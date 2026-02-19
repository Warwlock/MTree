#ifndef MTREE_LIGHT_DATA_FUNCTIONS
#define MTREE_LIGHT_DATA_FUNCTIONS

float3 HDMainLightDirection()
{
#if defined(HD_LIGHTING_INCLUDED)
	#if defined(SHADERPASS) && (SHADERPASS != SHADERPASS_LIGHT_TRANSPORT)
		uint lightIndex = _DirectionalShadowIndex;
		if (_DirectionalShadowIndex < 0)
		{
			if (_DirectionalLightCount == 0)
				return 0.0f;
			lightIndex = 0;
		}
		return -_DirectionalLightDatas[lightIndex].forward;
	#else
		return 0.0f;
	#endif
#endif
return 0.0f;
}

float3 HDMainLightColor()
{
#if defined(HD_LIGHTING_INCLUDED)
	#if defined(SHADERPASS) && (SHADERPASS != SHADERPASS_LIGHT_TRANSPORT)
		uint lightIndex = _DirectionalShadowIndex;
		if (_DirectionalShadowIndex < 0)
		{
			if (_DirectionalLightCount == 0)
				return 0.0f;
			lightIndex = 0;
		}
		return _DirectionalLightDatas[lightIndex].color;
	#else
		return 0.0f;
	#endif
#endif
return 0.0f;
}

float3 URPMainLightDirection()
{
#if defined(UNIVERSAL_LIGHTING_INCLUDED)
    return GetMainLight().direction;
#endif
return 0.0f;
}

float3 URPMainLightColor()
{
#if defined(UNIVERSAL_LIGHTING_INCLUDED)
    return GetMainLight().color;
#endif
return 0.0f;
}

float3 GetLightDirection()
{
#ifndef SHADERGRAPH_PREVIEW
	#if defined(UNIVERSAL_LIGHTING_INCLUDED)
		return URPMainLightDirection();
	#endif
	#if defined(HD_LIGHTING_INCLUDED)
		return HDMainLightDirection();
	#endif
	#if !defined(HD_LIGHTING_INCLUDED) && !defined(UNIVERSAL_LIGHTING_INCLUDED)
		return HDMainLightDirection();
		//return _WorldSpaceLightPos0.xyz * (1.0 - _WorldSpaceLightPos0.w);
	#endif
		return float4(0, 1, 0, 0);
#else
		return float4(0, 1, 0, 0);
#endif
}

float3 GetLightColor()
{
#ifndef SHADERGRAPH_PREVIEW
	#if defined(UNIVERSAL_LIGHTING_INCLUDED)
		return URPMainLightColor();
	#endif
	#if defined(HD_LIGHTING_INCLUDED)
		return HDMainLightColor();
	#else
		//return _MainLightColor;
		return HDMainLightColor();
	#endif
		return float4(1, 1, 1, 1);
#else
		return float4(1, 1, 1, 1);
#endif
}

#endif