Shader "Hidden/BranchEditor/BranchEditor_Leafs"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Cutoff("Cutoff", Range(0, 1)) = 0.5
		_ColorVariation("Hue Shift", Range(0, 1)) = 0.1
		_HueShift("Hue Shift", Range(0, 1)) = 0
		_Saturation("_Saturation", Range(0, 2)) = 1
		_Value("_Saturation", Range(0, 2)) = 1
	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" }
			LOD 100

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
					float4 col : COLOR;
				};

				struct v2f
				{
					float2 uv : TEXCOORD0;
					float4 vertex : SV_POSITION;
					float4 col : COLOR;
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				float _Cutoff;
				float _HueShift;
				float _Saturation;
				float _ColorVariation;
				float _Value;

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					o.col = v.col;
					return o;
				}

				float3 rgb2hsv(float3 c) {
					float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
					float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
					float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));

					float d = q.x - min(q.w, q.y);
					float e = 1.0e-10;
					return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
				}


				float3 hsv2rgb(float3 c) {
					c = float3(c.x, clamp(c.yz, 0.0, 1.0));
					float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
					float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
					return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
				}

				fixed4 frag(v2f i) : SV_Target
				{
					fixed4 col = tex2D(_MainTex, i.uv);
					float3 hsv = rgb2hsv(col.rgb);
					hsv.x += (i.col.g - 0.5) * _ColorVariation + _HueShift;
					hsv.g *= _Saturation;
					hsv.b *= _Value;
					col.rgb = hsv2rgb(hsv);
					clip(col.a - _Cutoff);
					return col;
				}
				ENDCG
			}
		}
}