Shader "Custom/SpriteShader"
{
	Properties
	{
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_NormalMap ("Normal Map", 2D) = "bump" {}
		_SpecMap ("Specular Map", 2D) = "black" {}
		_NoiseMap ("Noise Texture", 2D) = "black" {}
		_ShadowRender ("Shadow Render Texture", 2D) = "black" {}
		_NoisePower ("Noise Power", float) = 0
	}
	SubShader
	{
		Tags {
			"RenderType" = "TransparentCutout"
			"Queue" = "Transparent"
		}
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Blinnphong alpha:fade

		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _NormalMap;
		sampler2D _SpecMap;
		sampler2D _NoiseMap;
		sampler2D _ShadowRender;

		struct SurfaceOutputSprite {
			fixed3 Albedo;
			fixed3 Normal;
			fixed3 Emission;
			half Specular;
			fixed Gloss;
			fixed Alpha;
			fixed Shadow;
		};

		struct Input
		{
			float2 uv_MainTex;
			float2 uv_NormalMap;
			float2 uv_SpecMap;
			float2 uv_NoiseMap;
			float4 screenPos;
		};

		float _NoisePower;

		UNITY_INSTANCING_BUFFER_START(Props)
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputSprite o)
		{
			float3 screen_uv = (IN.screenPos.rgb) / IN.screenPos.a;
			o.Normal = UnpackNormal(tex2D(_NormalMap, IN.uv_NormalMap));
			fixed4 color = tex2D(_MainTex, IN.uv_MainTex);
			o.Specular = tex2D(_SpecMap, IN.uv_SpecMap);
			fixed4 shadow = tex2D(_ShadowRender, screen_uv);

			fixed noise = tex2D(_NoiseMap,
				float2(IN.uv_NoiseMap.x, IN.uv_NoiseMap.y - _Time.y)
			).r;

			noise = (noise > _NoisePower) ? 1 : ((noise > _NoisePower - 0.1) ? 0.5 : 0);

			fixed3 noise_color = fixed3(_NoisePower * 0.5, 1, 1);

			o.Albedo = (noise < 1 ? 0 : color.rgb);
			o.Shadow = shadow;
			o.Alpha = color.a * (noise > 0 ? 1 : 0);
			o.Emission = (noise < 1 ? noise_color : 0);
		}

		float4 LightingBlinnphong(SurfaceOutputSprite s, float3 light_dir, float3 view_dir, float atten) {
			float4 final;

			float3 diff_color;
			float n_dot_l = DotClamped(s.Normal, light_dir);
			diff_color = n_dot_l * s.Albedo * _LightColor0.rgb * atten * s.Shadow;
			diff_color = n_dot_l * _LightColor0.rgb * atten * s.Shadow;

			float3 half_vector = normalize(light_dir + view_dir);
			float3 specular = pow(saturate(dot(half_vector, s.Normal)), 100) * _LightColor0.rgb * s.Shadow * s.Specular;

			final.rgb = diff_color.rgb + specular + s.Emission;
			final.a = s.Alpha;
			return final;
		}

		ENDCG
	}
}
