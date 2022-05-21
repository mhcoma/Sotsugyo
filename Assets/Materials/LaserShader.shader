Shader "Custom/LaserShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_NoiseMap ("Noise Texture", 2D) = "black" {}
	}
	SubShader {
		Tags {
			"RenderType" = "TransparentCutout"
			"Queue" = "Overlay"
		}
		LOD 200

		CGPROGRAM
		#pragma surface surf Lambert alpha:fade

		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _NoiseMap;

		struct Input {
			float2 uv_MainTex;
			float2 uv_NoiseMap;
		};

		fixed4 _Color;
		UNITY_INSTANCING_BUFFER_START(Props)
		
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutput o) {
			fixed noise = tex2D(_NoiseMap,
				float2(IN.uv_NoiseMap.x - _Time.w * 2, IN.uv_NoiseMap.y - _SinTime.w * 4)
			).r;

			fixed4 c = tex2D (_MainTex, IN.uv_MainTex + noise.r + 0.5) * _Color;

			o.Emission = c.rgb;
			o.Alpha = c.a;
		}

		ENDCG
	}
	// FallBack "Diffuse"
}
