Shader "Custom/SpriteShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_NormalMap ("Normal Map", 2D) = "bump" {}
		_SpecMap ("Specular Map", 2D) = "black" {}
		_NoiseMap ("Noise Texture", 2D) = "black" {}
		_ShadowRender ("Shadow Render Texture", 2D) = "black" {}
		_ShadowRender2 ("Shadow Render Texture", 2D) = "black" {}
		_NoisePower ("Noise Power", float) = 0

		_Metallic ("Metallic", float) = 0
		_Smoothness ("Smoothness", float) = 0
	}

	CGINCLUDE
	#define BINORMAL_PER_FRAGMENT
	ENDCG

	SubShader {
		Pass {
			Tags {
				"LightMode" = "ForwardBase"
				"Queue" = "AlphaTest"
				"IgnoreProjector" = "True"
				"RenderType" = "TransparentCutout"
				"PreviewType" = "Plane"
				"CanUseSpriteAtlas" = "True"
			}

			Cull Off
			ZWrite On

			CGPROGRAM

			// #pragma multi_compile _ SHADOWS_SCREEN
			#pragma multi_compile _ VERTEXLIGHT_ON

			#pragma vertex vert
			#pragma fragment frag

			#define FORWARD_BASE_PASS

			#include "SpriteLight.cginc"

			ENDCG
		}

		Pass {
			Tags {
				"LightMode" = "ForwardAdd"
				"Queue" = "AlphaTest"
				"IgnoreProjector" = "True"
				"RenderType" = "TransparentCutout"
				"PreviewType" = "Plane"
				"CanUseSpriteAtlas" = "True"
			}

			Blend One OneMinusSrcColor
			ZWrite On

			CGPROGRAM

			#pragma target 3.0

			#pragma multi_compile_fwdadd

			#pragma vertex vert
			#pragma fragment frag
			#include "SpriteLight.cginc"
			ENDCG
		}
	}
}
