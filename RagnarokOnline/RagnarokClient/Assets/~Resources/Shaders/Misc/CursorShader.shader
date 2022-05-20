Shader "Custom/Cursor Shader"
{
	Properties
	{
		[PerRendererData] _MainTex("Albedo (RGB)", 2D) = "white" {}
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "true"
			"ForceNoShadowCasting" = "true"
		}

		ZWrite Off
		ZTest LEqual
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert             
			#pragma fragment frag 

			sampler2D _MainTex;

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

			v2p vert(IN input)
			{
				v2p o;
								
				o.pos = UnityObjectToClipPos(input.pos);
				o.uv0 = input.texcoord0;

				return o;
			}

			half4 frag(v2p o) : COLOR
			{
				float4 mainTexColor = tex2D(_MainTex, o.uv0);
				mainTexColor.rgb *= float3(0,1,0);
				mainTexColor.rgb *= mainTexColor.a;
				return mainTexColor;
			}
			ENDCG
		}
	}
}
