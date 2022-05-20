
Shader "Custom/Cylinder 1 Pass Shader"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		[PerRendererData] _StartTime("Start Time", Vector) = (0,0,0,0)
		[PerRendererData] _Tint("Tint", Color) = (1,1,1,1)
		[PerRendererData] _BottomWidth("Bottom Width", Vector) = (1,1,1,1)
		[PerRendererData] _TopWidth("Top Width", Vector) = (1,1,1,1)
		[PerRendererData] _MinHeight("Min Height", Vector) = (1,1,1,1)
		[PerRendererData] _MaxHeight("Max Height", Vector) = (1,1,1,1)
		[PerRendererData] _HeightSpeed("Height Speed", Vector) = (1,1,1,1)
		[PerRendererData] _RotateSpeed("Rotate Speed", Vector) = (1,1,1,1)

		[Toggle(REMOVE_CAMERA_ROTATION)] _RemoveCameraRotation("RemoveCameraRotation", Float) = 0
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

		Cull Off
		ZWrite Off
		ZTest LEqual
		Blend SrcAlpha OneMinusSrcAlpha, Zero Zero

		Pass //PASS 1
		{
			CGPROGRAM
			#pragma shader_feature REMOVE_CAMERA_ROTATION
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"	

			struct IN
			{
				float4 pos : POSITION;
				float2 texcoord0 : TEXCOORD0;
			};

			struct v2p
			{
				float4 pos : SV_POSITION;
				float2 uv0 : TEXCOORD0;
			};

			float4 _BottomWidth;
			float4 _TopWidth;
			float4 _MinHeight;
			float4 _MaxHeight;
			float4 _HeightSpeed;
			float4 _RotateSpeed;
			float3 _CameraRotation;
			float4 _StartTime;

			v2p vert(IN input)
			{
				v2p o;

				o.uv0 = input.texcoord0;

				//rotation for Y axis
				float c2 = cos(_Time.y * _RotateSpeed[3]);
				float s2 = sin(_Time.y * _RotateSpeed[3]);
				float4x4 rotate = float4x4(c2, 0, s2, 0,
					0, 1, 0, 0,
					-s2, 0, c2, 0,
					0, 0, 0, 1);

				//y for this is always either 0 or 1. Let's use it as a flag
				float isTop = input.pos.y;
				float isBot = 1 - isTop;
				float4x4 scaleMatrix = float4x4(1 + _TopWidth[0] * isTop + _BottomWidth[0] * isBot, 0, 0, 0,
					0, 1 * clamp(_MinHeight[0] + (_Time.y - _StartTime[0]) * _HeightSpeed[0], 0, _MaxHeight[0]), 0, 0,
					0, 0,1 + _TopWidth[0] * isTop + _BottomWidth[0] * isBot, 0,
					0, 0, 0, 1);

				input.pos = mul(scaleMatrix, input.pos);
				input.pos = mul(rotate, input.pos);

			#ifdef REMOVE_CAMERA_ROTATION
				//Rotate on X axis to remove camera rotation
				float angleX = radians(-_CameraRotation);
				float c1 = cos(angleX);
				float s1 = sin(angleX);
				float4x4 rotateXMatrix = float4x4(1, 0, 0, 0,
					0, c1, -s1, 0,
					0, s1, c1, 0,
					0, 0, 0, 1);
				input.pos = mul(rotateXMatrix, input.pos);
			#endif

				o.pos = UnityObjectToClipPos(input.pos);
				return o;
			}

			sampler2D _MainTex;
			float4 _Tint;

			half4 frag(v2p o) : COLOR
			{
				float4 mainTexColor = tex2D(_MainTex, o.uv0);
				float elapsed = clamp(_Time.y - (_StartTime[0] + _StartTime[3] - _StartTime[1]), 0, 32672);
				_Tint.a = clamp(_Tint.a - (elapsed / (_StartTime[1] + 0.001)) * _Tint.a, 0, 1);
				return mainTexColor * _Tint;
			}
			ENDCG
		}
	}
}