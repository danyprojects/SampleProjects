
Shader "Custom/Sprites Monster Palette ZWrite"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		[PerRendererData] _Dimensions("Dimensions", Vector) = (0,0,0,1)
		[PerRendererData] _Scale("Scale", Vector) = (1,1,1,1)
		[PerRendererData] _Offset("Offset", Vector) = (0,0,0,1)
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent+1"
			"RenderType" = "TransparentCutout"
			"IgnoreProjector" = "True"
			"DisableBatching" = "True"
		}

		Pass
		{
			Blend Off 
			ZWrite On
			ColorMask 0

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

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
			};

			float3 _Offset;
			float3 _CameraRotation;
			float3 _Scale;
			float2 _Dimensions;

			v2f vert(appdata_t v)
			{
				v2f o;

				float isMirrored = clamp(abs(_Offset.z - 1), 0, 1);
				o.texcoord = float2(abs(1 * isMirrored - v.texcoord.x), v.texcoord.y);

				float dimY = (_Dimensions.y / 6.25);
				float4x4 scaleMatrix = float4x4(_Scale.x * (_Dimensions.x / 6.25), 0, 0, 0,
					0, _Scale.y * dimY, 0, 0,
					0, 0, 1, 0,
					0, 0, 0, 1);

				float4x4 scaleDimensionsMatrix = float4x4(_Dimensions.x / 6.25, 0, 0, 0,
					0, _Dimensions.y / 6.25, 0, 0,
					0, 0, 1, 0,
					0, 0, 0, 1);

				float angleX = radians(-_CameraRotation);
				float c = cos(angleX);
				float s = sin(angleX);
				float4x4 rotateXMatrix = float4x4(1, 0, 0, 0,
					0, c, -s, 0,
					0, s, c, 0,
					0, 0, 0, 1);

				//Offset has the position offset
				_Offset.z = 0;
				_Offset.y = -_Offset.y; //RO has them inverted
				_Offset = mul(rotateXMatrix, _Offset);
				float4x4 trans = float4x4(1, 0, 0, _Offset.x,
					0, 1, 0, _Offset.y,
					0, 0, 1, _Offset.z,
					0, 0, 0, 1);

				//original
				o.vertex = UnityObjectToClipPos(mul(scaleMatrix, v.vertex));

				//Remove offset from world
				float4 pos = mul(trans, mul(scaleDimensionsMatrix, v.vertex));

				//Make sprite straight and taller							
				float isBot = 1 - clamp(v.vertex.y, 0, 1); // has 1 when it's a bottom pixel
				float botOffset = pow(_CameraRotation / 16, 2) / 4 * isBot;
				float topOffset = pow(_CameraRotation / 8, 2) * 1.5 * (1 - isBot);
				pos = mul(rotateXMatrix, pos + float4(0, botOffset + topOffset, 0, 0));

				//calculate it in clip position
				pos = UnityObjectToClipPos(pos);

				//Put new depth
				o.vertex.z = pos.z;
				//o.vertex = pos;
				return o;
			}

			sampler2D _MainTex;

			void frag(v2f IN, out fixed4 col : COLOR)
			{
				if(tex2D(_MainTex, IN.texcoord).b <= 0.05)
					discard;

				col = float4(0, 0, 0, 1);
			}

			ENDCG
		}

	}
}
