Shader "Hidden/BranchEditor/BranchEditor_EnhancedNormal" {
	Properties{
		_Cutoff("CutOff", Range(0,1)) = 0.3
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_BumpMap("Normal Map", 2D) = "bump" {}
		_Rotation("Axis",float) = (-90,0,0)
	}
		SubShader{
			Tags{ "RenderType" = "Opaque" }
			Cull off
			CGPROGRAM

			#pragma surface surf Lambert vertex:vert
			#pragma target 3.0
			#include "UnityCG.cginc"
			sampler2D _MainTex;
			sampler2D _BumpMap;
			float _Cutoff;
			float3 _Rotation;

			struct Input {
				float2 uv_MainTex;
				fixed4 color : COLOR;
				float3 worldNormal;
				INTERNAL_DATA
			};

			float4x4 RotationMatrix() {
				float radX = radians(_Rotation.x);
				float radY = radians(_Rotation.y);
				float radZ = radians(_Rotation.z);
				float sinX = sin(radX);
				float cosX = cos(radX);
				float sinY = sin(radY);
				float cosY = cos(radY);
				float sinZ = sin(radZ);
				float cosZ = cos(radZ);

				return float4x4(
						cosY * cosZ,	cosX * sinZ + sinX * sinY * cosZ,	sinX * sinZ - cosX * sinY * cosZ,	0,
						-cosY * sinZ,	cosX * cosZ - sinX * sinY * sinZ,	sinX * cosZ + cosX * sinY * sinZ,	0,
						sinY,			-sinX * cosY,						cosX * cosY,						0,
						0, 			0, 								0, 								1
					);
			}
			void vert(inout appdata_full v, out Input o)
			{
				UNITY_INITIALIZE_OUTPUT(Input, o);
				v.tangent = mul(RotationMatrix(),v.tangent);
				v.normal = mul(RotationMatrix(),v.normal);
			}
			void surf(Input IN, inout SurfaceOutput o) {
				fixed4 n = tex2D(_BumpMap, IN.uv_MainTex);
				fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
				o.Normal = UnpackNormalDXT5nm(n);

				clip(c.a - _Cutoff);
				float3 worldN = WorldNormalVector(IN,o.Normal);
				worldN = normalize(worldN.xyz);
				o.Albedo = float3(float2(-worldN.x, worldN.y)*0.5 + 0.5,1);

				o.Alpha = 1;
			}

			ENDCG
		}
			FallBack "Diffuse"
}