// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "UI/GrayscaleTransparent"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)

		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255

		_ColorMask("Color Mask", Float) = 15

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

		GrabPass { "_RagnarokScreenBackground" }

		Stencil
		{
			Ref[_Stencil]
			Comp[_StencilComp]
			Pass[_StencilOp]
			ReadMask[_StencilReadMask]
			WriteMask[_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest[unity_GUIZTestMode]
		Blend One Zero
		ColorMask[_ColorMask]

		Pass
		{
			Name "Default"
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			#pragma multi_compile_local _ UNITY_UI_CLIP_RECT
			#pragma multi_compile_local _ UNITY_UI_ALPHACLIP

			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord  : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
				float4 grabPos : TEXTCOORD2;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _MainTex;
			fixed4 _Color;
			fixed4 _TextureSampleAdd;
			float4 _ClipRect;
			float4 _MainTex_ST;

			v2f vert(appdata_t v)
			{
				v2f OUT;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
				OUT.worldPosition = v.vertex;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

				OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

				OUT.color = v.color;
				OUT.grabPos = ComputeGrabScreenPos(OUT.vertex);
				return OUT;
			}

			sampler2D _RagnarokScreenBackground;

			fixed4 frag(v2f IN) : SV_Target
			{
				half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd);
				color.a *= IN.color.a * _Color.a; //only apply alpha

				#ifdef UNITY_UI_CLIP_RECT
				color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
				#endif

				#ifdef UNITY_UI_ALPHACLIP
				clip(color.a - 0.001);
				#endif

				if (color.a == 0)
				{
					discard;
				}
				else
				{
					half4 bgcolor = tex2Dproj(_RagnarokScreenBackground, IN.grabPos);

					color.r = (color.r * color.a) + (bgcolor.r * (1 - color.a));
					color.g = (color.g * color.a) + (bgcolor.g * (1 - color.a));
					color.b = (color.b * color.a) + (bgcolor.b * (1 - color.a));

					half4 colorOld = color * _Color;
					color.rgb = dot(color.rgb, float3(0.33, 0.33, 0.34));

					color.r = clamp(color.r, 99.0 / 255, 1);
					color.g = clamp(color.g, 99.0 / 255, 1);
					color.b = clamp(color.b, 99.0 / 255, 1);

					/*Arguments:
						red_scale (206,0,0,X)
						green_scale(0,206,0,X)
						blue_scale (0,0,206,X)
						yellow_scale(0,0,49,X)
						purple_scale(0,49,0,X)
						grey_scale(0,0,0,X)
						original(255,255,255,X)
					*/

					//check if we want to have red green or blue scale instead
					color.r = IN.color.r + (1 - ceil(IN.color.r)) * color.r;
					color.g = IN.color.g + (1 - ceil(IN.color.g)) * color.g;
					color.b = IN.color.b + (1 - ceil(IN.color.b)) * color.b;

					//check if we discard grayscale or not
					float flag = floor((IN.color.r + IN.color.g + IN.color.b) / 3.0);
					color.rgb = (1 - flag)*color.rgb + flag * colorOld.rgb;

					// Apply tint, alpha was aplied before already
					color.r *= _Color.r;
					color.g *= _Color.g;
					color.b *= _Color.b;
				}

				return color;
			}
		ENDCG
		}
	}
}
