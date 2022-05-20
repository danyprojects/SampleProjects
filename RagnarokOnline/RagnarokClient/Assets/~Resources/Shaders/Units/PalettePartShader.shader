﻿Shader "Custom/Sprites Character Part Palette"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		[PerRendererData] _Palette("Palette", 2D) = "white" {}
		[PerRendererData] _Dimensions("Dimensions", Vector) = (0,0,0,1)
		[PerRendererData] _Scale("Scale", Vector) = (1,1,1,1)

		[PerRendererData] _Tint("Color", Color) = (1,1,1,1)
		[PerRendererData] _VColor("VColor", Color) = (1,1,1,1)
		[PerRendererData] _Offset("Offset", Vector) = (0,0,0,1)
		[PerRendererData] _UnitFadeStartTime("Unit Fade Start Time", float) = 0
		[PerRendererData] _UnitFadeDirection("Unit Fade Direction", float) = 1
		_FadeSpeed("Fade Speed", float) = 2

		[Toggle(IS_BODY)] _IsBody("IsBody", Float) = 0
		[Toggle(IS_WEAP_OR_SHIELD)] _IsWeapOrShield("IsWeapOrShield", Float) = 0
		[Toggle(IS_HEAD_OR_HEADGEAR)] _IsHeadOrHeadgear("IsHeadOrHeadgear", Float) = 0
	}

		SubShader
		{
			Tags
			{
				"Queue" = "Transparent"
				"RenderType" = "TransparentCutout"
				"IgnoreProjector" = "True"
				"DisableBatching" = "True"
			}

			Pass
			{
				Blend One OneMinusSrcAlpha
				Lighting Off
				ZWrite Off
				ZTest LEqual

				CGPROGRAM
				#pragma shader_feature IS_BODY IS_WEAP_OR_SHIELD IS_HEAD_OR_HEADGEAR
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"	

				struct appdata_t
				{
					float4 vertex   : POSITION;
					float2 texcoord : TEXCOORD0;
					float4 color : COLOR;
				};

				struct v2f
				{
					float4 vertex   : SV_POSITION;
					half2 texcoord  : TEXCOORD0;
					float4 color : COLOR;
				};

				float4 _MainTex_TexelSize;
				float4 _VColor;
				float3 _Offset;
				float3 _Scale;
				float3 _CameraRotation;
				float2 _Dimensions;
				float _UnitFadeStartTime;
				float _UnitFadeDirection;
				float _FadeSpeed;

				v2f vert(appdata_t v)
				{
					v2f o;

					float isMirrored = clamp(abs(_Offset.z - 1), 0, 1);
					o.texcoord = float2(abs(1 * isMirrored - v.texcoord.x), v.texcoord.y);

					o.color = _VColor;
					//Fade alpha linearly
					float elapsedTime = _Time.y - _UnitFadeStartTime;
					float startAlpha = clamp(o.color.a * -_UnitFadeDirection, 0, o.color.a);
					o.color.a = clamp(startAlpha + elapsedTime * _FadeSpeed * _UnitFadeDirection, 0, o.color.a);

					//To remove the camera rotation
					float angleX = radians(-_CameraRotation);
					float c = cos(angleX);
					float s = sin(angleX);
					float4x4 rotateXMatrix = float4x4(1, 0, 0, 0,
						0, c, -s, 0,
						0, s, c, 0,
						0, 0, 0, 1);

					//Scales the object with grf scaling too
					float4x4 scaleMatrix = float4x4(_Scale.x * (_Dimensions.x / 6.25), 0, 0, 0,
						0, _Scale.y * (_Dimensions.y / 6.25), 0, 0,
						0, 0, 1, 0,
						0, 0, 0, 1);

					//Scales the object only with dimensions
					float4x4 scaleDimensionsMatrix = float4x4(_Dimensions.x / 6.25, 0, 0, 0,
						0, _Dimensions.y / 6.25, 0, 0,
						0, 0, 1, 0,
						0, 0, 0, 1);

					//Offset has the position offset in x,y and mirror in z
					_Offset.z = 0;
					#ifdef IS_BODY
					_Offset.y = -_Offset.y;
					#endif
					_Offset = mul(rotateXMatrix, _Offset);
					float4x4 trans = float4x4(1, 0, 0, _Offset.x,
						0, 1, 0, _Offset.y,
						0, 0, 1, _Offset.z,
						0, 0, 0, 1);

					//original
					o.vertex = UnityObjectToClipPos(mul(scaleMatrix, v.vertex));

					//Remove offset from world
					float4 pos = mul(trans, mul(scaleDimensionsMatrix, v.vertex));

					//rotate to nullify world rotation and also translate to be a straight sprite
					float isBot = 1 - clamp(v.vertex.y, 0, 1); // has 1 when it's a bottom pixel

					float botOffset = 0, topOffset = 0;
					#ifdef IS_HEAD_OR_HEADGEAR
						botOffset = pow(_CameraRotation / 8, 2) / 3.2 * isBot;
						topOffset = pow(_CameraRotation / 8, 2) / 2 * (1 - isBot);
					#elif IS_BODY
						botOffset = pow(_CameraRotation / 16, 2) / 4 * isBot;
						topOffset = pow(_CameraRotation / 8, 2) / 1.5 * (1 - isBot);
					#elif IS_WEAP_OR_SHIELD
						botOffset = pow(_CameraRotation / 8, 2) / 8 * isBot;
						topOffset = pow(_CameraRotation / 8, 2) / 3 * (1 - isBot);
					#endif

					pos = mul(rotateXMatrix, pos + float4(0, botOffset + topOffset, 0, 0));

					//calculate it in clip position
					pos = UnityObjectToClipPos(pos);

					//Put new depth
					o.vertex.z = pos.z;
					//o.vertex = pos;
					return o;
				}

				sampler2D _MainTex;
				sampler2D _Palette;
				fixed4 _Tint;

				float4 ColorFromPalette(fixed4 color)
				{
					float4 palColor = tex2D(_Palette, float2(color.r, color.g));
					palColor.a = color.b;
					return palColor;
				}

				void frag(v2f IN, out fixed4 col : COLOR)
				{
					//Dimensions are stored in pixel 0,0 and 1,0. Need texel size to go to pixel 1,0
					float2 dimensions = float2(tex2D(_MainTex, float2(0, 0)).a * 255, tex2D(_MainTex, float2(1, 0)).a * 255);
					float2 dimensions1 = _Dimensions;

					float2 texelSize = (1 / dimensions.xy) * 0.48;
					float2 uv_pixels = IN.texcoord * dimensions.xy;
					float4 uv_min_max = float4((floor(uv_pixels) - texelSize) / dimensions.xy, (ceil(uv_pixels) - texelSize) / dimensions.xy);

					float2 uv_frac = frac(uv_pixels);

					float4 texelA = ColorFromPalette(tex2D(_MainTex, uv_min_max.xy));
					float4 texelB = ColorFromPalette(tex2D(_MainTex, uv_min_max.xw));
					float4 texelC = ColorFromPalette(tex2D(_MainTex, uv_min_max.zy));
					float4 texelD = ColorFromPalette(tex2D(_MainTex, uv_min_max.zw));

					float4 bilinear = lerp(lerp(texelA, texelB, uv_frac.y), lerp(texelC, texelD, uv_frac.y), uv_frac.x) * IN.color;

					float alpha = ceil(texelA.a * texelB.a * texelC.a * texelD.a);
					float4 outline = float4(bilinear.x * alpha, bilinear.y * alpha, bilinear.z * alpha, 1);
					bilinear.rgb = lerp(bilinear.rgb, outline.rgb, 0.3);

					bilinear.rgb *= IN.color.a;
					col = bilinear * _Tint;

					//Remove this later. This is for debugging purposes
					if (abs(dimensions.x - dimensions1.x) > 0.005 || abs(dimensions.y - dimensions1.y) > 0.005)
						col = fixed4(1, 192 / 255, 203 / 255, 1);
				}
				ENDCG
			}
		}
}
