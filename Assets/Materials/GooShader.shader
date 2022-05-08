Shader "Unlit/GooShader"
{
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
		_NormalMap1 ("Normal Map 1", 2D) = "bump" {}
		_NormalMap2 ("Normal Map 2", 2D) = "bump" {}
		_NoiseMap ("Noise Map", 2D) = "white" {}

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
			}

			CGPROGRAM

			#pragma multi_compile _ SHADOWS_SCREEN
			#pragma multi_compile _ VERTEXLIGHT_ON

			#pragma vertex vert
			#pragma fragment frag

			#define FORWARD_BASE_PASS

			#include "GooLight.cginc"
			ENDCG
		}

		Pass {
			Tags {
				"LightMode" = "ForwardAdd"
			}

			Blend One One
			ZWrite Off

			CGPROGRAM

			#pragma target 3.0

			#pragma multi_compile_fwdadd_fullshadows

			#pragma vertex vert
			#pragma fragment frag
			#include "GooLight.cginc"
			ENDCG
		}

		Pass {
			Tags {
				"LightMode" = "ShadowCaster"
			}

			CGPROGRAM

			#pragma multi_compile_shadowcaster
			
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			#include "MapShadow.cginc"

			ENDCG
		}
	}
}
