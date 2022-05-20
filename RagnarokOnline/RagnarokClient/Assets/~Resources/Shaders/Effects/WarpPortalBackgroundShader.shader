Shader "Custom/Warp Portal Background Shader"
{
    Properties
    {
		[PerRendererData] _MainTex ("Alpha texture", 2D) = "white" {}
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
		Blend SrcAlpha OneMinusSrcAlpha, Zero Zero
		
		Pass
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
				float4 color : COLOR;
			};


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
				o.pos = UnityObjectToClipPos(input.pos);
				return o;
			}

			sampler2D _MainTex;

			half4 frag(v2p o) : COLOR
			{
				float4 mainTexColor = tex2D(_MainTex, o.uv0) * o.color;
				return mainTexColor;
			}
			ENDCG
		}
	}
}

