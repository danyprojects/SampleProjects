// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "UI/SpritePaletteTransparent"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		[PerRendererData] _AlphaTex("Palette", 2D) = "white" {}

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
			float4 _MainTex_TexelSize;

			v2f vert(appdata_t v)
			{
				v2f OUT;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
				OUT.worldPosition = v.vertex;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

				OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

				OUT.color = float4(v.color.r * 255, v.color.g * 255, 0, v.color.a);
				OUT.grabPos = ComputeGrabScreenPos(OUT.vertex);
				return OUT;
			}

			sampler2D _RagnarokScreenBackground;
			sampler2D _AlphaTex;

			float4 ColorFromPalette(fixed4 color)
			{
				float4 palColor = tex2D(_AlphaTex, float2(color.r, color.g));
				palColor.a = color.b;
				return palColor;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				float2 dimensions = float2(tex2D(_MainTex, float2(0.05f, 0)).a * 255, tex2D(_MainTex, float2(0.95f, 0)).a * 255);
				float2 dimensions2 = float2(IN.color.r, IN.color.g);

				float2 texelSize = (1 / dimensions.xy) * 0.48;
				float2 uv_pixels = IN.texcoord * dimensions.xy;
				float4 uv_min_max = float4((floor(uv_pixels) - texelSize) / dimensions.xy, (ceil(uv_pixels) - texelSize) / dimensions.xy);

				float2 uv_frac = frac(uv_pixels);

				float4 texelA = ColorFromPalette(tex2D(_MainTex, uv_min_max.xy));
				float4 texelB = ColorFromPalette(tex2D(_MainTex, uv_min_max.xw));
				float4 texelC = ColorFromPalette(tex2D(_MainTex, uv_min_max.zy));
				float4 texelD = ColorFromPalette(tex2D(_MainTex, uv_min_max.zw));

				float4 bilinear = lerp(lerp(texelA, texelB, uv_frac.y), lerp(texelC, texelD, uv_frac.y), uv_frac.x);

				float alpha = ceil(texelA.a * texelB.a * texelC.a * texelD.a);
				float4 outline = float4(bilinear.x * alpha, bilinear.y * alpha, bilinear.z * alpha, 1);
				bilinear.rgb = lerp(bilinear.rgb, outline.rgb, 0.3);

				half4 color = bilinear;
				color.a *= IN.color.a;
				color*= _Color;

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
				}


				if (abs(dimensions.x - dimensions2.x) > 0.005 || abs(dimensions.y - dimensions2.y) > 0.005)
					color = fixed4(1, 192 / 255, 203 / 255, 1);

				return color;
			}
		ENDCG
		}
	}
}
