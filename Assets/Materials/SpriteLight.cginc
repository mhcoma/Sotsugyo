#if !defined(__SRITE_LIGHT__)
#define __SRITE_LIGHT__

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

	float4 screen_pos : TEXCOORD7;
	float2 noise_uv : TEXCOORD8;
};

float4 _Color;
float _Metallic;
float _Smoothness;

#include "Utils.cginc"

sampler2D _MainTex;
float4 _MainTex_ST;

sampler2D _NormalMap;
float4 _NormalMap_ST;

sampler2D _SpecMap;
float4 _SpecMap_ST;

sampler2D _ShadowRender;
float4 _ShadowRender_ST;

sampler2D _ShadowRender2;
float4 _ShadowRender2_ST;

sampler2D _NoiseMap;
float4 _NoiseMap_ST;

float _NoisePower;



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

	o.screen_pos = ComputeScreenPos(o.pos);
	o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
	o.noise_uv = TRANSFORM_TEX(v.texcoord, _NoiseMap);

	TRANSFER_SHADOW(o);

	compute_vertex_light_color(o);

	return o;
}

fixed4 frag(v2f i) : SV_Target {
	i.normal = create_normal(_NormalMap, i.uv, i);

	float2 screen_uv = (i.screen_pos.xy) / i.screen_pos.w;

	fixed4 shadow_texture;
	
	#if defined(FORWARD_BASE_PASS)
		shadow_texture = tex2D(_ShadowRender, screen_uv);
	#else
		shadow_texture = tex2D(_ShadowRender2, screen_uv);
	#endif

	half4 main_texture = tex2D(_MainTex, i.uv);

	float3 view_dir = normalize(_WorldSpaceCameraPos - i.world_pos);

	UnityLight light = create_light(i);
	UnityIndirect indirect = create_indirect(i, view_dir);

	float3 specular_tint;
	float omr;
	float metalic = _Metallic;
	half4 emission;
	fixed noise = tex2D(_NoiseMap,
		float2(i.noise_uv.x, i.noise_uv.y - _Time.y)
	).r;

	noise = (noise > _NoisePower) ? 1 : ((noise > _NoisePower - 0.1) ? 0.5 : 0);

	fixed3 noise_color = fixed3(_NoisePower * 0.5, 1, 1);

	half3 albedo = main_texture.rgb * _Color.rgb;
	albedo = DiffuseAndSpecularFromMetallic(
		albedo, metalic, specular_tint, omr
	);
	
	float alpha = main_texture.a * (noise > 0 ? 1 : 0);

	emission = half4((noise < 1 ? noise_color : 0), 1);

	light.color *= shadow_texture.rgb;

	float specular_level = tex2D(_SpecMap, i.uv).rgb;
	clip(alpha - 0.5);

	fixed4 result = UNITY_BRDF_PBS(
		albedo, specular_tint,
		omr, specular_level,
		i.normal, view_dir,
		light, indirect
	) + emission;

	result = make_retro(result);
	return result;
}

#endif