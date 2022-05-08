Shader "Custom/ShadowReceiver2Shader" {
    SubShader {
		LOD 200
		pass {
			Tags {
				"LightMode" = "ForwardBase"
			}
			CGINCLUDE
			#include "UnityCG.cginc"
			ENDCG

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase

			struct v2f {
				float4 pos: SV_POSITION;
				float2 uv: TEXCOORD0;
			};

			v2f vert(appdata_tan v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord.xy;
				return o;
			}
			float4 frag(v2f i) : COLOR {
				fixed4 c = 1.0f;
				c.rgb = 0.0f;
				return c;
			}
			ENDCG
		}

		Pass {
			Name "FORWARD"
			Tags {
				"LightMode" = "ForwardAdd"
			}
			ZWrite Off
			Blend One One

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile_fwdadd_fullshadows
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"

			struct v2f {
				float4 pos : SV_POSITION;
				float3 worldPos : TEXCOORD0;
				SHADOW_COORDS(1)
			};

			v2f vert(appdata_full v) {
				v2f o;
				o.pos = UnityObjectToClipPos (v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				TRANSFER_SHADOW(o);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target {
				UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos)
				fixed4 c = atten;
				c.rgb *= _LightColor0.rgb;
				return c;
			}

			ENDCG
		}

		UsePass "VertexLit/SHADOWCASTER"
	}
}