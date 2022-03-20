// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/OnlyShadowsAndAtten"
{
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 200
		pass
		{
			Lighting On
			Tags { "LightMode" = "ForwardBase" }

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
				float2 uv: TEXCOORD1;
				SHADOW_COORDS(2)
			};

			v2f vert(appdata_tan v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord.xy;
				TRANSFER_VERTEX_TO_FRAGMENT(o);
				return o;
			}
			float4 frag(v2f i) : COLOR {
				float atten = LIGHT_ATTENUATION(i);
				fixed4 c = atten;
				c.rgb *= _LightColor0.rgb;
				return c;
			}
			ENDCG
		}

		// Forward additive pass (only needed if you care about more lights than 1 directional).
		// Can remove if no point/spot light support needed.
		Pass
		{
			Name "FORWARD"
			Tags { "LightMode" = "ForwardAdd" }
			ZWrite Off Blend One One

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// Include shadowing support for point/spot
			#pragma multi_compile_fwdadd_fullshadows
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"

			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 worldPos : TEXCOORD0;
				SHADOW_COORDS(1)
			};

			v2f vert (appdata_full v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos (v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				TRANSFER_SHADOW(o); // pass shadow coordinates to pixel shader
				return o;
			}

			fixed4 frag (v2f IN) : SV_Target
			{
				UNITY_LIGHT_ATTENUATION(atten, IN, IN.worldPos)
				fixed4 c = atten;
				c.rgb *= _LightColor0.rgb;
				return c;
			}

			ENDCG
		}

		// Support for casting shadows from this shader. Remove if not needed.
		UsePass "VertexLit/SHADOWCASTER"
	}
}