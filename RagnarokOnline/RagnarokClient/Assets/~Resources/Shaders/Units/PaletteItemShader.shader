Shader "Custom/Sprites Item Palette"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		[PerRendererData] _Palette("Palette", 2D) = "white" {}
		[PerRendererData] _Dimensions("Dimensions", Vector) = (0,0,0,1)
		[PerRendererData] _Scale("Scale", Vector) = (1,1,1,1)
		[PerRendererData] _StartTime("Start Time", Vector) = (0,0,0,0)

		[PerRendererData] _Tint("Color", Color) = (1,1,1,1)
		_Narrow("Narrow", float) = 4
		_YSpeed("YSpeed", float) = 1.5
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

			float3 _Scale;
			float3 _CameraRotation;
			float2 _Dimensions;
			float4 _StartTime;
			float _Narrow;
			float _YSpeed;

			v2f vert(appdata_t v)
			{
				v2f o;

				o.texcoord = v.texcoord;
				o.color = v.color;

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

				//Scale the quad 
				v.vertex = mul(scaleMatrix, v.vertex);
				//Apply the parabola
				float elapsed = (_Time.y - _StartTime.x) * _YSpeed - 0.3;
				float parabola = -(_Narrow * (elapsed * elapsed) - 0.5) * 8;
				v.vertex.y = clamp(v.vertex.y + parabola, v.vertex.y, 10000);

				//original
				o.vertex = UnityObjectToClipPos(v.vertex);

				//rotate to nullify world rotation and also translate to be a straight sprite
				float isBot = 1 - clamp(v.vertex.y, 0, 1); // has 1 when it's a bottom pixel
				float botOffset = pow(_CameraRotation / 8, 2) / 16 * isBot;
				float topOffset = pow(_CameraRotation / 8, 2) / 16 * (1 -isBot);
				float4 pos = mul(rotateXMatrix, v.vertex + float4(0, botOffset + topOffset, 0, 0));

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
				float2 dimensions1 = float2(tex2D(_MainTex, float2(0, 0)).a * 255, tex2D(_MainTex, float2(1, 0)).a * 255);
				float2 dimensions = _Dimensions;

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
			}
			ENDCG
		}
	}
}
