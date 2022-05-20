
Shader "Custom/Sprites Item Palette ZWrite"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		[PerRendererData] _Dimensions("Dimensions", Vector) = (0,0,0,1)
		[PerRendererData] _Scale("Scale", Vector) = (1,1,1,1)
		[PerRendererData] _StartTime("Start Time", Vector) = (0,0,0,0)

		_Narrow("Narrow", float) = 4
		_YSpeed("YSpeed", float) = 1.5
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

			float3 _CameraRotation;
			float3 _Scale;
			float2 _Dimensions;
			float4 _StartTime;
			float _Narrow;
			float _YSpeed;

			v2f vert(appdata_t v)
			{
				v2f o;

				o.texcoord = v.texcoord;

				float dimY = (_Dimensions.y / 6.25);
				float4x4 scaleMatrix = float4x4(_Scale.x * (_Dimensions.x / 6.25), 0, 0, 0,
					0, _Scale.y * dimY, 0, 0,
					0, 0, 1, 0,
					0, 0, 0, 1);

				float angleX = radians(-_CameraRotation);
				float c = cos(angleX);
				float s = sin(angleX);
				float4x4 rotateXMatrix = float4x4(1, 0, 0, 0,
					0, c, -s, 0,
					0, s, c, 0,
					0, 0, 0, 1);
							
				//Scale the quad
				v.vertex = mul(scaleMatrix, v.vertex);

				//Apply parabola
				float elapsed = (_Time.y - _StartTime.x) * _YSpeed - 0.3;
				float parabola = -(_Narrow * (elapsed * elapsed) - 0.5) * 8;
				v.vertex.y = clamp(v.vertex.y + parabola, v.vertex.y, 10000);

				//original
				o.vertex = UnityObjectToClipPos(v.vertex);

				//Make sprite straight and taller for depth calculation				
				float isBot = 1 - clamp(v.vertex.y, 0, 1); // has 1 when it's a bottom pixel
				float botOffset = pow(_CameraRotation / 8, 2) / 16 * isBot;
				float topOffset = pow(_CameraRotation / 8, 2) / 16 * (1 - isBot);
				float4 pos = mul(rotateXMatrix, v.vertex + float4(0, botOffset + topOffset, 0, 0));

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
