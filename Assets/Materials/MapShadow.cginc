#if !defined(__MAP_SHADOW__)
#define __MAP_SHADOW__

#include "UnityCG.cginc"

#if defined(SHADOWS_CUBE)
	struct v2f_shaodw {
		float4 pos : SV_POSITION;
		float3 light_vec : TEXCOORD0;
	};

	v2f_shaodw vert(appdata_full v) {
		v2f_shaodw o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.light_vec = mul(unity_ObjectToWorld, v.vertex).xyz - _LightPositionRange.xyz;
		return o;
	}

	float4 frag(v2f_shaodw i) : SV_TARGET {
		float depth = (length(i.light_vec) + unity_LightShadowBias.x) * _LightPositionRange.w;
		return UnityEncodeCubeShadowDepth(depth);
	}
#else
	float4 vert(appdata_full v) : SV_POSITION {
		float4 vertex = UnityClipSpaceShadowCasterPos(v.vertex.xyz, v.normal);
		return UnityApplyLinearShadowBias(vertex);
	}

	half4 frag() : SV_Target {
		return 0;
	}
#endif

#endif