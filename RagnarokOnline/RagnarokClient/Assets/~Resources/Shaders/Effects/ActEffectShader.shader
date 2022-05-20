
Shader "Custom/Effect Sprite Renderer"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		[PerRendererData] _Palette("Palette", 2D) = "white" {}
		[PerRendererData] _Dimensions("Dimensions", Vector) = (0,0,0,1)
		[PerRendererData] _Position("Position", Vector) = (0,0,0,1)
		[PerRendererData] _Scale("Scale", Vector) = (0,0,0,1)
		[PerRendererData] _Rotation("Rotation", Float) = 1
		[PerRendererData] _Tint("Color", Color) = (1,1,1,1)
		[Toggle(BILLBOARD)] _Billboard("Billboard", Float) = 0
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
				Blend One OneMinusSrcAlpha, Zero Zero
				Lighting Off
				ZWrite Off
				ZTest Always
				Cull Off

				CGPROGRAM
				#pragma shader_feature BILLBOARD
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

				static const float pi = 3.141592653589793238462;

				float4 _MainTex_TexelSize;
				float3 _Position;
				float3 _Scale;
				float _Rotation;

				v2f vert(appdata_t v)
				{
					v2f o;
					o.texcoord = v.texcoord;
					o.color = v.color;

					float rad = _Rotation * (pi / 180);
					float s = sin(rad);
					float c = cos(rad);
					float4x4 rotMatrix = float4x4(c, s, 0, 0,
						-s, c, 0, 0,
						0, 0, 1, 0,
						0, 0, 0, 1);

					float4x4 translationMatrix = float4x4(1, 0, 0, _Position.x,
						0, 1, 0, _Position.y,
						0, 0, 1, _Position.z, //z doesn't scaling
						0, 0, 0, 1);

					float4x4 scaleMatrix = float4x4(_Scale.x, 0, 0, 0,
						0, _Scale.y, 0, 0,
						0, 0, 1, 0,
						0, 0, 0, 1);

					//apply rotation and scale outside world coordinates
					o.vertex = mul(scaleMatrix, v.vertex);
					o.vertex = mul(rotMatrix, o.vertex);
					o.vertex = mul(translationMatrix, o.vertex);

					#ifdef BILLBOARD
						float3 vpos = mul((float3x3)unity_ObjectToWorld, o.vertex.xyz);
						float4 worldCoord = float4(unity_ObjectToWorld._m03, unity_ObjectToWorld._m13, unity_ObjectToWorld._m23, 1);
						o.vertex = mul(UNITY_MATRIX_V, worldCoord) + float4(vpos, 0);

						o.vertex = mul(UNITY_MATRIX_P, float4(o.vertex.xyz, 1));
					#else
						o.vertex = UnityObjectToClipPos(o.vertex);
					#endif

					return o;
				}

				sampler2D _MainTex;
				sampler2D _Palette;
				float2 _Dimensions;
				fixed4 _Tint;

				float4 ColorFromPalette(fixed4 color, float alpha)
				{
					float4 palColor = tex2D(_Palette, float2(color.r, color.g));
					palColor.a = alpha;
					return palColor;
				}

				void frag(v2f IN, out fixed4 col : COLOR)
				{
					float2 texelSize = (1 / _Dimensions.xy) * 0.48;
					float2 uv_pixels = IN.texcoord * _Dimensions.xy;
					float4 uv_min_max = float4((floor(uv_pixels) - texelSize) / _Dimensions.xy, (ceil(uv_pixels) - texelSize) / _Dimensions.xy);

					float2 uv_frac = frac(uv_pixels);

					float4 texelA = ColorFromPalette(tex2D(_MainTex, uv_min_max.xy), tex2D(_MainTex, uv_min_max.xy).b);
					float4 texelB = ColorFromPalette(tex2D(_MainTex, uv_min_max.xw), tex2D(_MainTex, uv_min_max.xw).b);
					float4 texelC = ColorFromPalette(tex2D(_MainTex, uv_min_max.zy), tex2D(_MainTex, uv_min_max.zy).b);
					float4 texelD = ColorFromPalette(tex2D(_MainTex, uv_min_max.zw), tex2D(_MainTex, uv_min_max.zw).b);

					float4 bilinear = lerp(lerp(texelA, texelB, uv_frac.y), lerp(texelC, texelD, uv_frac.y), uv_frac.x) * IN.color;

					float alpha = ceil(texelA.a * texelB.a * texelC.a * texelD.a);
					float4 outline = float4(bilinear.x * alpha, bilinear.y * alpha, bilinear.z * alpha, 1);
					bilinear.rgb = lerp(bilinear.rgb, outline.rgb, 0.3);

					bilinear.rgb *= IN.color.a;
					col = bilinear * _Tint;
				}
				ENDCG
			}
		}
}