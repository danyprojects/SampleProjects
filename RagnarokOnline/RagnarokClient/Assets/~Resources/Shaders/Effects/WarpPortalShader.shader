
Shader "Custom/Warp Portal Shader"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}

		[PerRendererData] _Tint("Tint", Color) = (0.685,0.6920612,0.9056604,0.9019608)
		[PerRendererData] _BottomWidth("Bottom Width", float) = 3
		[PerRendererData] _TopWidth("Top Width", float) = 4
		[PerRendererData] _Height("Height",  float) = 5
		[PerRendererData] _UnitFadeStartTime("Unit Fade Start Time", float) = 0
		[PerRendererData] _UnitFadeDirection("Unit Fade Direction", float) = 1
		_FadeSpeed("Fade Speed", float) = 2
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
		Blend SrcAlpha One, Zero Zero

		Pass //PASS 1
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"	

			struct IN
			{
				float4 pos : POSITION;
				float2 texcoord0 : TEXCOORD0;
				float4 color : COLOR;
			};

			struct v2p
			{
				float4 pos : SV_POSITION;
				float2 uv0 : TEXCOORD0;
				float2 factor : TEXCOORD3;
				float4 color : COLOR;
			};

			float _BottomWidth;
			float _TopWidth;
			float _Height;
			float _RotateSpeed;
			float _UnitFadeStartTime;
			float _UnitFadeDirection;
			float _FadeSpeed;

			v2p vert(IN input)
			{
				v2p o;

				o.color = input.color;

				//Fade alpha linearly
				float elapsedTime = _Time.y - _UnitFadeStartTime;
				float startAlpha = clamp(o.color.a * -_UnitFadeDirection, 0, o.color.a);
				o.color.a = clamp(startAlpha + elapsedTime * _FadeSpeed * _UnitFadeDirection, 0, o.color.a);
				o.uv0 = input.texcoord0;

				//y for this is always either 0 or 1. Let's use it as a flag
				float isTop = input.pos.y;
				float isBot = 1 - isTop;
				float time = (_Time.y % 3.5);
				o.factor.x = 3.5 - time;
				o.factor.y = cos(o.factor.x / 2);
				float xScale = 0.3 + _TopWidth * isTop + _BottomWidth * isBot;
				float yScale = 0.3 + _TopWidth * isTop + _BottomWidth * isBot;

				float4x4 scaleMatrix = float4x4(xScale * o.factor.x, 0, 0, 0,
					0, 1 * _Height - clamp(time * 0.75, 0, 2), 0, 0,
					0, 0, yScale * o.factor.x, 0,
					0, 0, 0, 1);
				
				input.pos = mul(scaleMatrix, input.pos);
				o.pos = UnityObjectToClipPos(input.pos);
				return o; 
			}

			sampler2D _MainTex;
			float4 _Tint;

			half4 frag(v2p o) : COLOR
			{
				float4 mainTexColor = tex2D(_MainTex, o.uv0) * o.color;	
				mainTexColor *= o.factor.y;
				return mainTexColor * _Tint; 
			}
			ENDCG
		}

		Pass //PASS 2
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"	

			struct IN
			{
				float4 pos : POSITION;
				float2 texcoord0 : TEXCOORD0;
				float4 color : COLOR;
			};

			struct v2p
			{
				float4 pos : SV_POSITION;
				float2 uv0 : TEXCOORD0;
				float2 factor : TEXCOORD3;
				float4 color : COLOR;
			};

			float _BottomWidth;
			float _TopWidth;
			float _Height;
			float _UnitFadeStartTime;
			float _UnitFadeDirection;
			float _FadeSpeed;

			v2p vert(IN input)
			{
				v2p o;
				o.color = input.color;

				//Fade alpha linearly
				float elapsedTime = _Time.y - _UnitFadeStartTime;
				float startAlpha = clamp(o.color.a * -_UnitFadeDirection, 0, o.color.a);
				o.color.a = clamp(startAlpha + elapsedTime * _FadeSpeed * _UnitFadeDirection, 0, o.color.a);

				o.uv0 = input.texcoord0;

				//y for this is always either 0 or 1. Let's use it as a flag
				float isTop = input.pos.y;
				float isBot = 1 - isTop;
				float time = ((_Time.y + 0.7) % 3.5);
				o.factor.x = 3.5 - time;
				o.factor.y = clamp(cos(o.factor.x / 2) *1.5, 0 ,1);
				float xScale = 0.3 + _TopWidth * isTop + _BottomWidth * isBot;
				float yScale = 0.3 + _TopWidth * isTop + _BottomWidth * isBot;

				float4x4 scaleMatrix = float4x4(xScale * o.factor.x, 0, 0, 0,
					0, 1 * _Height - clamp(time * 0.75, 0, 2), 0, 0,
					0, 0, yScale * o.factor.x, 0,
					0, 0, 0, 1);

				input.pos = mul(scaleMatrix, input.pos);
				o.pos = UnityObjectToClipPos(input.pos);
				return o;
			}

			sampler2D _MainTex;
			float4 _Tint;

			half4 frag(v2p o) : COLOR
			{
				float4 mainTexColor = tex2D(_MainTex, o.uv0) * o.color;
				mainTexColor *= o.factor.y;
				return mainTexColor * _Tint;
			}
			ENDCG
		}

		Pass //PASS 3
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"	

			struct IN
			{
				float4 pos : POSITION;
				float2 texcoord0 : TEXCOORD0;
				float4 color : COLOR;
			};

			struct v2p
			{
				float4 pos : SV_POSITION;
				float2 uv0 : TEXCOORD0;
				float2 factor : TEXCOORD3;
				float4 color : COLOR;
			};

			float _BottomWidth;
			float _TopWidth;
			float _Height;
			float _UnitFadeStartTime;
			float _UnitFadeDirection;
			float _FadeSpeed;

			v2p vert(IN input)
			{
				v2p o;

				o.color = input.color;

				//Fade alpha linearly
				float elapsedTime = _Time.y - _UnitFadeStartTime;
				float startAlpha = clamp(o.color.a * -_UnitFadeDirection, 0, o.color.a);
				o.color.a = clamp(startAlpha + elapsedTime * _FadeSpeed * _UnitFadeDirection, 0, o.color.a);

				o.uv0 = input.texcoord0;

				//y for this is always either 0 or 1. Let's use it as a flag
				float isTop = input.pos.y;
				float isBot = 1 - isTop;
				float time = ((_Time.y + 1.4) % 3.5);
				o.factor.x = 3.5 - time;
				o.factor.y = clamp(cos(o.factor.x / 2) *1.5, 0, 1);
				float xScale = 0.3 + _TopWidth * isTop + _BottomWidth * isBot;
				float yScale = 0.3 + _TopWidth * isTop + _BottomWidth * isBot;

				float4x4 scaleMatrix = float4x4(xScale * o.factor.x, 0, 0, 0,
					0, 1 * _Height - clamp(time * 0.75, 0, 2), 0, 0,
					0, 0, yScale * o.factor.x, 0,
					0, 0, 0, 1);

				input.pos = mul(scaleMatrix, input.pos);
				o.pos = UnityObjectToClipPos(input.pos);
				return o;
			}

			sampler2D _MainTex;
			float4 _Tint;

			half4 frag(v2p o) : COLOR
			{
				float4 mainTexColor = tex2D(_MainTex, o.uv0) * o.color;
				mainTexColor *= o.factor.y;
				return mainTexColor * _Tint;
			}
			ENDCG
		}

		Pass //PASS 4
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"	

			struct IN
			{
				float4 pos : POSITION;
				float2 texcoord0 : TEXCOORD0;
				float4 color : COLOR;
			};

			struct v2p
			{
				float4 pos : SV_POSITION;
				float2 uv0 : TEXCOORD0;
				float2 factor : TEXCOORD3;
				float4 color : COLOR;
			};

			float _BottomWidth;
			float _TopWidth;
			float _Height;
			float _UnitFadeStartTime;
			float _UnitFadeDirection;
			float _FadeSpeed;

			v2p vert(IN input)
			{
				v2p o;
				o.color = input.color;

				//Fade alpha linearly
				float elapsedTime = _Time.y - _UnitFadeStartTime;
				float startAlpha = clamp(o.color.a * -_UnitFadeDirection, 0, o.color.a);
				o.color.a = clamp(startAlpha + elapsedTime * _FadeSpeed * _UnitFadeDirection, 0, o.color.a);

				o.uv0 = input.texcoord0;

				//y for this is always either 0 or 1. Let's use it as a flag
				float isTop = input.pos.y;
				float isBot = 1 - isTop;
				float time = ((_Time.y + 2.1) % 3.5);
				o.factor.x = 3.5 - time;
				o.factor.y = clamp(cos(o.factor.x / 2) *1.5, 0, 1);
				float xScale = 0.3 + _TopWidth * isTop + _BottomWidth * isBot;
				float yScale = 0.3 + _TopWidth * isTop + _BottomWidth * isBot;

				float4x4 scaleMatrix = float4x4(xScale * o.factor.x, 0, 0, 0,
					0, 1 * _Height - clamp(time * 0.75, 0, 2), 0, 0,
					0, 0, yScale * o.factor.x, 0,
					0, 0, 0, 1);

				input.pos = mul(scaleMatrix, input.pos);
				o.pos = UnityObjectToClipPos(input.pos);
				return o;
			}

			sampler2D _MainTex;
			float4 _Tint;

			half4 frag(v2p o) : COLOR
			{
				float4 mainTexColor = tex2D(_MainTex, o.uv0) * o.color;
				mainTexColor *= o.factor.y;
				return mainTexColor * _Tint;
			}
			ENDCG
		}

		Pass //PASS 5
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"	

			struct IN
			{
				float4 pos : POSITION;
				float2 texcoord0 : TEXCOORD0;
				float4 color : COLOR;
			};

			struct v2p
			{
				float4 pos : SV_POSITION;
				float2 uv0 : TEXCOORD0;
				float2 factor : TEXCOORD3;
				float4 color : COLOR;
			};

			float _BottomWidth;
			float _TopWidth;
			float _Height;
			float _UnitFadeStartTime;
			float _UnitFadeDirection;
			float _FadeSpeed;

			v2p vert(IN input)
			{
				v2p o;
				o.color = input.color;

				//Fade alpha linearly
				float elapsedTime = _Time.y - _UnitFadeStartTime;
				float startAlpha = clamp(o.color.a * -_UnitFadeDirection, 0, o.color.a);
				o.color.a = clamp(startAlpha + elapsedTime * _FadeSpeed * _UnitFadeDirection, 0, o.color.a);

				o.uv0 = input.texcoord0;

				//y for this is always either 0 or 1. Let's use it as a flag
				float isTop = input.pos.y;
				float isBot = 1 - isTop;
				float time = (_Time.y + 2.8) % 3.5;
				o.factor.x = 3.5 - time;
				o.factor.y = clamp(cos(o.factor.x / 2) *1.5, 0, 1);
				float xScale = 0.3 + _TopWidth * isTop + _BottomWidth * isBot;
				float yScale = 0.3 + _TopWidth * isTop + _BottomWidth * isBot;

				float4x4 scaleMatrix = float4x4(xScale * o.factor.x, 0, 0, 0,
					0, 1 * _Height - clamp(time * 0.75, 0, 2), 0, 0,
					0, 0, yScale * o.factor.x, 0,
					0, 0, 0, 1);

				input.pos = mul(scaleMatrix, input.pos);
				o.pos = UnityObjectToClipPos(input.pos);
				return o;
			}

			sampler2D _MainTex;
			float4 _Tint;

			half4 frag(v2p o) : COLOR
			{
				float4 mainTexColor = tex2D(_MainTex, o.uv0) * o.color;
				mainTexColor *= o.factor.y;
				return mainTexColor * _Tint;
			}
			ENDCG
		}
	}
}