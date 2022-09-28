#if !defined(__GOO_LIGHT__)
#define __GOO_LIGHT__

#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

struct v2f {
	float4 pos : SV_POSITION;
	float2 uv : TEXCOORD0;
	float3 normal : TEXCOORD1;

	#if defined(BINORMAL_PER_FRAGMENT)
		float4 tangent : TEXCOORD2;
	#else
		float3 tangent : TEXCOORD2;
		float3 binormal : TEXCOORD3;
	#endif

	float3 world_pos : TEXCOORD4;

	SHADOW_COORDS(5)

	#if defined(VERTEXLIGHT_ON)
		float3 vertex_light_color : TEXCOORD6;
	#endif
};

float4 _Color;
float _Metallic;
float _Smoothness;

#include "Utils.cginc"

sampler2D _MainTex;
float4 _MainTex_ST;

sampler2D _NormalMap1;
float4 _NormalMap1_ST;

sampler2D _NormalMap2;
float4 _NormalMap2_ST;

sampler2D _NoiseMap;
float4 _NoiseMap_ST;

v2f vert(appdata_full v){
	v2f o;
	o.pos = UnityObjectToClipPos(v.vertex);
	o.world_pos = mul(unity_ObjectToWorld, v.vertex);
	half3 worldNormal = UnityObjectToWorldNormal(v.normal);
	o.normal = worldNormal;

	#if defined(BINORMAL_PER_FRAGMENT)
		o.tangent = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);
	#else
		o.tangent = UnityObjectToWorldDir(v.tangent.xyz);
		o.binormal = create_binormal(i.normal, i.tangent, v.tangent.w);
	#endif

	o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);

	TRANSFER_SHADOW(o);

	compute_vertex_light_color(o);
	return o;
}

fixed4 frag(v2f i) : SV_Target {

	fixed time = int(_Time.x * 128) / 16.0f;

	i.normal = (
		create_normal(
			_NormalMap1,
			float2(i.uv.x + time, i.uv.y),
			i
		) +
		create_normal(
			_NormalMap2,
			float2(i.uv.x, i.uv.y + time),
			i
		)
	) / 2.0f;


	fixed noise = int(tex2D(_NoiseMap,
		float2(i.uv.x + time, i.uv.y - time)
	).r * 16) / 16.0f;

	half3 albedo = tex2D(_MainTex, i.uv + noise).rgb * _Color.rgb;

	float3 view_dir = normalize(_WorldSpaceCameraPos - i.world_pos);

	UnityLight light = create_light(i);
	UnityIndirect indirect = create_indirect(i, view_dir);

	float3 specular_tint;
	float omr;
	float metalic = _Metallic;

	albedo = DiffuseAndSpecularFromMetallic(
		albedo, metalic, specular_tint, omr
	);

	float specular_level = 1.0f;

	fixed4 result = UNITY_BRDF_PBS(
		albedo, specular_tint,
		omr, specular_level,
		i.normal, view_dir,
		light, indirect
	);
	
	result = make_retro(result);
	return result;
}

#endif