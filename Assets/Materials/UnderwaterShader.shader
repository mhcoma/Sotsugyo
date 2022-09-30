Shader "Unlit/UnderwaterShader" {
	Properties {
		_Power("Radius", Range(0, 255)) = 1
		_Color("Color", Color) = (1, 1, 1, 1)
		_NoiseMap ("Noise Map", 2D) = "white" {}
	}

	Category {
		Tags{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Opaque"
		}
	
		SubShader {
			GrabPass {
				Tags{
					"LightMode" = "Always"
				}
			}

			Pass {
				Tags{
					"LightMode" = "Always"
				}

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#include "UnityCG.cginc"

				struct appdata_t {
					float4 vertex : POSITION;
					float2 texcoord: TEXCOORD0;
				};

				struct v2f {
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
					float4 uvgrab : TEXCOORD1;
				};

				sampler2D _NoiseMap;
				float4 _NoiseMap_ST;

				v2f vert(appdata_t v) {
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					#if UNITY_UV_STARTS_AT_TOP
						float scale = -1.0;
					#else
						float scale = 1.0;
					#endif
					o.uvgrab.xy = (float2(o.vertex.x, o.vertex.y*scale) + o.vertex.w) * 0.5;
					o.uvgrab.zw = o.vertex.zw;
					o.uv = TRANSFORM_TEX(v.texcoord, _NoiseMap);
					return o;
				}

				sampler2D _GrabTexture;
				float4 _GrabTexture_TexelSize;
				float _Power;
				float4 _Color;


				half4 frag(v2f i) : COLOR {
					fixed time = floor(_Time.x * 256) / 64;
					fixed power = _Power * 0.0625;
					i.uv.x *= _ScreenParams.z / 1;
					i.uv.y *= _ScreenParams.y / _ScreenParams.x;
					fixed noise_x = (
						tex2D(_NoiseMap,
							float2(i.uv.x + time, i.uv.y + time)
						).r - 0.5
					) * power;
					fixed noise_y = (
						tex2D(_NoiseMap,
							float2(i.uv.y + time, i.uv.y + time)
						).r - 0.5
					) * power;
					half4 result = tex2D(
						_GrabTexture,
						float2(
							i.uvgrab.x + noise_x,
							i.uvgrab.y + noise_y
						)
					);

					half4 color_result = lerp(result, _Color, _Color.a);

					return color_result;
				}
				ENDCG
			}
		}
	}
}