Shader "Ogxd/Grid" {
	// Modified by RobotoSkunk
	// Credits to Ogxd: https://github.com/ogxd/grid-shader-unity

	Properties {
		[Header(Colors and texture)]
		_MainColor("Main Color", Color) = (1, 1, 1, 1)
		_SecondaryColor("Secondary Color", Color) = (1, 1, 1, 0.2)
		_BackgroundColor("Background Color", Color) = (0, 0, 0, 0)
		_MaskTexture("Texture", 2D) = "white" {}

		[Header(Grid)]
		_Scale("Scale", Range(0.1, 2)) = 1
		_GraduationScale("Graduation Scale", Float) = 1
		_Thickness("Lines Thickness", Range(0.0001, 0.01)) = 0.005
	}
	SubShader {
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 100

		ZWrite On // We need to write in depth to avoid tearing issues
		Blend SrcAlpha OneMinusSrcAlpha

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ UNITY_SINGLE_PASS_STEREO STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON
			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f {
				float2 uv : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
				float4 vertex : SV_POSITION;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _GridTexture;
			float4 _GridTexture_ST;

			sampler2D _MaskTexture;
			float4 _MaskTexture_ST;

			float _Scale;
			float _GraduationScale;

			float _Thickness;
			float _SecondaryFadeInSpeed;

			fixed4 _MainColor;
			fixed4 _SecondaryColor;
			fixed4 _BackgroundColor;

			v2f vert(appdata v) {
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.vertex = UnityObjectToClipPos(v.vertex);

				float2 worldXY = mul(unity_ObjectToWorld, v.vertex).xy;

				// UVs for mask texture
				o.uv1 = TRANSFORM_TEX(v.uv1, _MaskTexture);
				o.uv = TRANSFORM_TEX(worldXY, _MaskTexture) / _GraduationScale;

				return o;
			}

			fixed4 frag(v2f i) : SV_Target {
				fixed4 col = _MainColor;
				fixed4 maskCol = tex2D(_MaskTexture, i.uv1);

				float localScale = 1 / _Scale;

				float2 pos;

				pos.x = floor(frac((i.uv.x - 0.5 * _Thickness) * localScale) + _Thickness * localScale);
				pos.y = floor(frac((i.uv.y - 0.5 * _Thickness) * localScale) + _Thickness * localScale);

				if (pos.x != 1 && pos.y != 1) {
					pos.x = floor(frac((i.uv.x - 0.5 * _Thickness) * 10.0 * localScale) + _Thickness * 10.0 * localScale);
					pos.y = floor(frac((i.uv.y - 0.5 * _Thickness) * 10.0 * localScale) + _Thickness * 10.0 * localScale);

					if (pos.x == 1 || pos.y == 1) col = _SecondaryColor;
					else col = _BackgroundColor;
				}

				// Apply mask multiplying by its alpha
				col.a *= maskCol.a;
				return col;
			}

			ENDCG
		}
	}
}
