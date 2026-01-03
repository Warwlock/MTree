#ifndef MTREE_WIND_DEFINITIONS
#define MTREE_WIND_DEFINITIONS

float _WindStrength;
float _RandomWindOffset;
float _WindPulse;
float _WindDirection;
float _WindTurbulence;

#endif

#ifndef MTREE_LEAF_FUNCTIONS
#define MTREE_LEAF_FUNCTIONS

void WindPropsLeaf_float(float3 pos, out float WindStrength,
			out float RandomWindOffset,
			out float WindPulse,
			out float WindDirection,
			out float WindTurbulence)
{
	WindStrength = _WindStrength * _GlobalWindInfluence;
	RandomWindOffset = _RandomWindOffset;
	WindPulse = _WindPulse;
	WindDirection = _WindDirection;
	WindTurbulence = _WindTurbulence * _GlobalTurbulenceInfluence;
}

#endif