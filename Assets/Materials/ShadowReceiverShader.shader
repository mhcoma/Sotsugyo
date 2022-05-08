Shader "Custom/ShadowReceiverShader" {
	SubShader {
		LOD 200
		pass {
			Lighting On
			Tags {
				"LightMode" = "ForwardBase"
				"RenderType" = "Opaque"
			}

			CGINCLUDE
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#include "Lighting.cginc"
			ENDCG

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase

			struct v2f {
				float4 pos: SV_POSITION;
				float2 uv: TEXCOORD0;
				float4 world_pos: TEXCOORD1;
				SHADOW_COORDS(2)
			};

			v2f vert(appdata_tan v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord.xy;
				o.world_pos = mul(unity_ObjectToWorld, v.vertex);
				TRANSFER_VERTEX_TO_FRAGMENT(o);
				TRANSFER_SHADOW(o);
				return o;
			}
			float4 frag(v2f i) : COLOR {
				UNITY_LIGHT_ATTENUATION(atten, i, i.world_pos);
				fixed4 c = atten;
				c.rgb *= _LightColor0.rgb;
				return c;
			}
			ENDCG
		}
	}
	FallBack "Legacy Shaders/VertexLit"
}