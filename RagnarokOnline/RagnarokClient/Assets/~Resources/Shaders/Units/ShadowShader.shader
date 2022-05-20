// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Custom/Shadow"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		[PerRendererData] _UnitFadeStartTime("Unit Fade Start Time", float) = 0
		[PerRendererData] _UnitFadeDirection("Unit Fade Direction", float) = 1
		_FadeSpeed("Fade Speed", float) = 2
	}
	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "TransparentCutout"
			"DisableBatching" = "True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Pass
		{
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

			sampler2D _MainTex;
			float3 _CameraRotation;
			float _UnitFadeStartTime;
			float _UnitFadeDirection;
			float _FadeSpeed;

			v2f vert(appdata_t v)
			{
				v2f o;
				o.texcoord = v.texcoord;
				o.color = v.color;

				//Fade alpha linearly
				float elapsedTime = _Time.y - _UnitFadeStartTime;
				float startAlpha = clamp(o.color.a * -_UnitFadeDirection, 0, o.color.a);
				o.color.a = clamp(startAlpha + elapsedTime * _FadeSpeed * _UnitFadeDirection, 0, o.color.a); 
				
				float angleX = radians(90 -_CameraRotation);
				float c1 = cos(angleX);
				float s1 = sin(angleX);
				float4x4 rotateXMatrix = float4x4(1, 0, 0, 0,
					0, c1, -s1, 0,
					0, s1, c1, 0,
					0, 0, 0, 1);

				//Calc straight to ground sprite with a small height offset
				o.vertex = mul(rotateXMatrix, v.vertex);
				o.vertex.y += _CameraRotation * 0.005 - 0.05; //From equation system -> 0.1 = x*30 + y  && 0.3 = x * 70 + y 
				o.vertex = mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, o.vertex));

				return o;
			}
			
			void frag(v2f IN, out fixed4 col : COLOR)
			{
				col = tex2D(_MainTex, IN.texcoord) * IN.color;
				col.rgb *= col.a;
			}
			ENDCG
		}
	}
}
