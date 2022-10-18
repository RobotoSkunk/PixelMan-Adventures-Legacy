Shader "Sprites/RobotoSkunk/Outline2D"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0

	    // Value to determine if outlining is enabled and outline color + size.
		[PerRendererData] _Outline("Outline", Float) = 0
		[PerRendererData] _OutlineColor("Outline Color", Color) = (1,1,1,1)
		[PerRendererData] _OutlineSize("Outline Size", int) = 1
		[PerRendererData] _OutlineSensitivity("Outline Sensitivity", float) = 0.5
	}

	SubShader {
		Tags {
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#pragma shader_feature ETC1_EXTERNAL_ALPHA
			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex   : SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord  : TEXCOORD0;
			};

			fixed4 _Color;
			float _Outline;
			fixed4 _OutlineColor;
			int _OutlineSize;
			float _OutlineSensitivity;

			v2f vert(appdata_t IN) {
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;

				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap(OUT.vertex);
				#endif

				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _AlphaTex;
			float4 _MainTex_TexelSize;

			fixed4 SampleSpriteTexture(float2 uv) {
				fixed4 color = tex2D(_MainTex, uv);
				return color;
			}

			fixed4 frag(v2f IN) : SV_Target {
				fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;

				//If outline mode is enabled and pixel does not exists, try to draw outline
				if (_Outline > 0 && c.a < _OutlineSensitivity) {
					float4 col = float4(0, 0, 0, 0);

					[unroll(16)]
					for (int i = 1; i < _OutlineSize + 1; i++) {
						fixed4 pixelUp    = tex2D(_MainTex, IN.texcoord + fixed2(0, i * _MainTex_TexelSize.y));
						fixed4 pixelDown  = tex2D(_MainTex, IN.texcoord - fixed2(0, i * _MainTex_TexelSize.y));
						fixed4 pixelLeft  = tex2D(_MainTex, IN.texcoord - fixed2(i * _MainTex_TexelSize.x, 0));
						fixed4 pixelRight = tex2D(_MainTex, IN.texcoord + fixed2(i * _MainTex_TexelSize.x, 0));

						if (pixelUp.a > 0) col += pixelUp;
						else if (pixelDown.a > 0) col += pixelDown;
						else if (pixelLeft.a > 0) col += pixelLeft;
						else if (pixelRight.a > 0) col += pixelRight;
					}

					// if (alpha != 0) c.rgba = _OutlineColor;
					if (col.a > 0) c.rgba = col * _OutlineColor;
				}

				c.rgb *= c.a;
				return c;
			}
			ENDCG
		}
	}
}
