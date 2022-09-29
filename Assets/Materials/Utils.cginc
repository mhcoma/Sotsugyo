#if !defined(__UTILS__)
#define __UTILS__

float3 create_binormal(float3 normal, float3 tangent, float sign) {
	return cross(normal, tangent) * (sign * unity_WorldTransformParams.w);
}

void compute_vertex_light_color(inout v2f i) {
	#if defined(VERTEXLIGHT_ON)
		i.vertex_light_color = Shade4PointLights(
			unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
			unity_LightColor[0].rgb, unity_LightColor[1].rgb,
			unity_LightColor[2].rgb, unity_LightColor[3].rgb,
			unity_4LightAtten0, i.world_pos, i.normal
		);
	#endif
}

UnityLight create_light(v2f i) {
	UnityLight light;
	#if defined(POINT) || defined(POINT_COOKIE)  || defined(SPOT)
		light.dir = normalize(_WorldSpaceLightPos0.xyz - i.world_pos);
	#else
		light.dir = _WorldSpaceLightPos0.xyz;
	#endif

	UNITY_LIGHT_ATTENUATION(attenuation, i, i.world_pos);

	light.color = _LightColor0.rgb * attenuation;
	
	if (light.color.r > 1) { light.color /= light.color.r; }
	if (light.color.g > 1) { light.color /= light.color.g; }
	if (light.color.b > 1) { light.color /= light.color.b; }

	light.ndotl = DotClamped(i.normal, light.dir);
	return light;
}

float3 box_projection(float3 direction, float3 position, float4 cubemap_position, float3 box_min, float3 box_max) {
	#if UNITY_SPECCUBE_BOX_PROJECTION
		UNITY_BRANCH
		if (cubemap_position.w > 0) {
			float3 factors = ((direction > 0 ? box_max : box_min) - position) / direction;
			float scalar = min(min(factors.x, factors.y), factors.z);
			direction = direction * scalar + (position - cubemap_position);
		}
	#endif
	return direction;
}

UnityIndirect create_indirect(v2f i, float3 view_dir) {
	UnityIndirect indirect;
	indirect.diffuse = 0;
	indirect.specular = 0;
	#if defined(VERTEXLIGHT_ON)
		indirect.diffuse = i.vertex_light_color;
	#endif

	#if defined(FORWARD_BASE_PASS)
		indirect.diffuse += max(0, ShadeSH9(float4(i.normal, 1)));
		float3 reflection_dir = reflect(-view_dir, i.normal);
		Unity_GlossyEnvironmentData env_data;
		env_data.roughness = 1 - _Smoothness;
		env_data.reflUVW = box_projection(
			reflection_dir, i.world_pos,
			unity_SpecCube0_ProbePosition,
			unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax
		);
		float3 probe0 = Unity_GlossyEnvironment(
			UNITY_PASS_TEXCUBE(unity_SpecCube0), unity_SpecCube0_HDR, env_data
		);
		env_data.reflUVW = box_projection(
			reflection_dir, i.world_pos,
			unity_SpecCube1_ProbePosition,
			unity_SpecCube1_BoxMin, unity_SpecCube1_BoxMax
		);
		#if UNITY_SPECCUBE_BLENDING
			float interpolator = unity_SpecCube0_BoxMin.w;
			UNITY_BRANCH
			if (interpolator < 0.99999) {
				float3 probe1 = Unity_GlossyEnvironment(
					UNITY_PASS_TEXCUBE_SAMPLER(unity_SpecCube1, unity_SpecCube0), unity_SpecCube1_HDR, env_data
				);
				indirect.specular = lerp(probe1, probe0, unity_SpecCube0_BoxMin.w);
			}
			else {
				indirect.specular = probe0;
			}
		#else
			indirect.specular = probe0;
		#endif
	#endif

	return indirect;
}

float3 create_normal(sampler2D tex, float2 uv, v2f i) {
	float3 tangent_space_normal_1 = normalize(UnpackNormal(tex2D(tex, uv)));

	#if defined(BINORMAL_PER_FRAGMENT)
		float3 binormal = create_binormal(i.normal, i.tangent.xyz, i.tangent.w);
	#else
		float3 binormal = i.binormal;
	#endif

	return normalize(
		tangent_space_normal_1.x * i.tangent +
		tangent_space_normal_1.y * binormal +
		tangent_space_normal_1.z * i.normal
	);
}

fixed4 make_retro(fixed4 input) {
	fixed retro_coef = 8;
	return floor(input * retro_coef) / retro_coef;
}

#endif