Shader "Custom/Character Info Text Mesh" {

	Properties{
		_FaceTex("Face Texture", 2D) = "white" {}
		_FaceUVSpeedX("Face UV Speed X", Range(-5, 5)) = 0.0
		_FaceUVSpeedY("Face UV Speed Y", Range(-5, 5)) = 0.0
		_FaceColor("Face Color", Color) = (1,1,1,1)
		_FaceDilate("Face Dilate", Range(-1,1)) = 0

		_OutlineColor("Outline Color", Color) = (0,0,0,1)
		_OutlineTex("Outline Texture", 2D) = "white" {}
		_OutlineUVSpeedX("Outline UV Speed X", Range(-5, 5)) = 0.0
		_OutlineUVSpeedY("Outline UV Speed Y", Range(-5, 5)) = 0.0
		_OutlineWidth("Outline Thickness", Range(0, 1)) = 0
		_OutlineSoftness("Outline Softness", Range(0,1)) = 0

		_BumpMap("Normal map", 2D) = "bump" {}
		_BumpOutline("Bump Outline", Range(0,1)) = 0
		_BumpFace("Bump Face", Range(0,1)) = 0

		_ReflectFaceColor("Reflection Color", Color) = (0,0,0,1)
		_ReflectOutlineColor("Reflection Color", Color) = (0,0,0,1)
		_Cube("Reflection Cubemap", Cube) = "black" { /* TexGen CubeReflect */ }
		_EnvMatrixRotation("Texture Rotation", vector) = (0, 0, 0, 0)

		_GlowColor("Color", Color) = (0, 1, 0, 0.5)
		_GlowOffset("Offset", Range(-1,1)) = 0
		_GlowInner("Inner", Range(0,1)) = 0.05
		_GlowOuter("Outer", Range(0,1)) = 0.05
		_GlowPower("Falloff", Range(1, 0)) = 0.75

		_WeightNormal("Weight Normal", float) = 0
		_WeightBold("Weight Bold", float) = 0.5

		_ShaderFlags("Flags", float) = 0
		_ScaleRatioA("Scale RatioA", float) = 1
		_ScaleRatioB("Scale RatioB", float) = 1
		_ScaleRatioC("Scale RatioC", float) = 1

		_MainTex("Font Atlas", 2D) = "white" {}
		_TextureWidth("Texture Width", float) = 512
		_TextureHeight("Texture Height", float) = 512
		_GradientScale("Gradient Scale", float) = 5.0
		_ScaleX("Scale X", float) = 1.0
		_ScaleY("Scale Y", float) = 1.0
		_PerspectiveFilter("Perspective Correction", Range(0, 1)) = 0.875
		_Sharpness("Sharpness", Range(-1,1)) = 0

		_VertexOffsetX("Vertex OffsetX", float) = 0
		_VertexOffsetY("Vertex OffsetY", float) = 0

		_MaskCoord("Mask Coordinates", vector) = (0, 0, 32767, 32767)
		_ClipRect("Clip Rect", vector) = (-32767, -32767, 32767, 32767)
		_MaskSoftnessX("Mask SoftnessX", float) = 0
		_MaskSoftnessY("Mask SoftnessY", float) = 0

		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255

		_ColorMask("Color Mask", Float) = 15
	}

	SubShader{

		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}

		Stencil
		{
			Ref[_Stencil]
			Comp[_StencilComp]
			Pass[_StencilOp]
			ReadMask[_StencilReadMask]
			WriteMask[_StencilWriteMask]
		}

		Cull[_CullMode]
		ZWrite Off
		Lighting Off
		Fog { Mode Off }
		ZTest always
		Blend One OneMinusSrcAlpha
		ColorMask[_ColorMask]

		Pass {
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex VertShader
			#pragma fragment PixShader
			#pragma shader_feature __ GLOW_ON

			#pragma multi_compile __ UNITY_UI_CLIP_RECT
			#pragma multi_compile __ UNITY_UI_ALPHACLIP

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"
			#include "TMPro_Properties.cginc"
			#include "TMPro.cginc"

			struct vertex_t {
				UNITY_VERTEX_INPUT_INSTANCE_ID
				float4	position		: POSITION;
				float3	normal			: NORMAL;
				fixed4	color : COLOR;
				float2	texcoord0		: TEXCOORD0;
				float2	texcoord1		: TEXCOORD1;
			};


			struct pixel_t {
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
				float4	position		: SV_POSITION;
				fixed4	color : COLOR;
				float2	atlas			: TEXCOORD0;		// Atlas
				float4	param			: TEXCOORD1;		// alphaClip, scale, bias, weight
				float4	mask			: TEXCOORD2;		// Position in object space(xy), pixel Size(zw)
				float3	viewDir			: TEXCOORD3;

				float4 textures			: TEXCOORD5;
			};

			// Used by Unity internally to handle Texture Tiling and Offset.
			float4 _FaceTex_ST;
			float4 _OutlineTex_ST;

			pixel_t VertShader(vertex_t input)
			{
				pixel_t output;

				UNITY_INITIALIZE_OUTPUT(pixel_t, output);
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input,output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				float bold = step(input.texcoord1.y, 0);

				float4 vert = input.position;
				vert.x += _VertexOffsetX;
				vert.y += _VertexOffsetY;

				float4 vPosition = UnityObjectToClipPos(vert);

				float2 pixelSize = vPosition.w;
				pixelSize /= float2(_ScaleX, _ScaleY) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));
				float scale = rsqrt(dot(pixelSize, pixelSize));
				scale *= abs(input.texcoord1.y) * _GradientScale * (_Sharpness + 1);
				if (UNITY_MATRIX_P[3][3] == 0) scale = lerp(abs(scale) * (1 - _PerspectiveFilter), scale, abs(dot(UnityObjectToWorldNormal(input.normal.xyz), normalize(WorldSpaceViewDir(vert)))));

				float weight = lerp(_WeightNormal, _WeightBold, bold) / 4.0;
				weight = (weight + _FaceDilate) * _ScaleRatioA * 0.5;

				float bias = (.5 - weight) + (.5 / scale);

				float alphaClip = (1.0 - _OutlineWidth * _ScaleRatioA - _OutlineSoftness * _ScaleRatioA);

			#if GLOW_ON
				alphaClip = min(alphaClip, 1.0 - _GlowOffset * _ScaleRatioB - _GlowOuter * _ScaleRatioB);
			#endif

				alphaClip = alphaClip / 2.0 - (.5 / scale) - weight;
									

				// Generate UV for the Masking Texture
				float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);
				float2 maskUV = (vert.xy - clampedRect.xy) / (clampedRect.zw - clampedRect.xy);

				// Support for texture tiling and offset
				float2 textureUV = UnpackUV(input.texcoord1.x);
				float2 faceUV = TRANSFORM_TEX(textureUV, _FaceTex);
				float2 outlineUV = TRANSFORM_TEX(textureUV, _OutlineTex);


				output.position = vPosition;
				output.color = input.color;
				output.atlas = input.texcoord0;
				output.param = float4(alphaClip, scale, bias, weight);
				output.mask = half4(vert.xy * 2 - clampedRect.xy - clampedRect.zw, 0.25 / (0.25 * half2(_MaskSoftnessX, _MaskSoftnessY) + pixelSize.xy));
				output.viewDir = mul((float3x3)_EnvMatrix, _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, vert).xyz);
				output.textures = float4(faceUV, outlineUV);

				return output;
			}


			fixed4 PixShader(pixel_t input) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(input);

				float c = tex2D(_MainTex, input.atlas).a;

				float	scale = input.param.y;
				float	bias = input.param.z;
				float	weight = input.param.w;
				float	sd = (bias - c) * scale;

				float outline = (_OutlineWidth * _ScaleRatioA) * scale;
				float softness = (_OutlineSoftness * _ScaleRatioA) * scale;

				half4 faceColor = _FaceColor;
				half4 outlineColor = _OutlineColor;

				faceColor.rgb *= input.color.rgb;

				faceColor *= tex2D(_FaceTex, input.textures.xy + float2(_FaceUVSpeedX, _FaceUVSpeedY) * _Time.y);
				outlineColor *= tex2D(_OutlineTex, input.textures.zw + float2(_OutlineUVSpeedX, _OutlineUVSpeedY) * _Time.y);

				faceColor = GetColor(sd, faceColor, outlineColor, outline, softness);

			#if GLOW_ON
				float4 glowColor = GetGlowColor(sd, scale);
				faceColor.rgb += glowColor.rgb * glowColor.a;
			#endif

				// Alternative implementation to UnityGet2DClipping with support for softness.
				#if UNITY_UI_CLIP_RECT
					half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(input.mask.xy)) * input.mask.zw);
					faceColor *= m.x * m.y;
				#endif

				#if UNITY_UI_ALPHACLIP
					clip(faceColor.a - 0.001);
				#endif

				return faceColor * input.color.a;
				}

				ENDCG
			}
		}
	CustomEditor "TMPro.EditorUtilities.TMP_SDFShaderGUI"
}
