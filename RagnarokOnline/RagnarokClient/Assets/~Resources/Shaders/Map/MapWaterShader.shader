Shader "Custom/MapWaterShader"
{
    Properties
    {
        [PerRendererData] _MainTex ("Albedo (RGB)", 2D) = "white" {}

		[PerRendererData] _WaveHeight("Wave Height", float) = 1
		[PerRendererData] _WavePitch("Wave Pitch", float) = 50
		[PerRendererData] _WaveSpeed("Wave Speed", float) = 2

		_Opacity("Opacity", float) = 0.5
    }

	SubShader
	{
		Tags
		{ 
			"Queue" = "Transparent" 
			"IgnoreProjector" = "true"
			"ForceNoShadowCasting" = "true"
		}

		ZWrite On
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

			float _WaveHeight;
			float _WavePitch;
			float _WaveSpeed;

			const float PI = 3.14159265;

			v2p vert(IN input)
			{
				v2p o;

				float x = input.pos.x % 2.0;
				float y = input.pos.y % 2.0;
				float diff = x < 1.0 ? (y < 1.0 ? 1.0 : -1.0) : 0.0;
				float waterOffset = (_Time.y * _WaveSpeed) % 360 - 180;
				float height = sin((waterOffset + 0.5 * _WavePitch * (input.pos.x + input.pos.z + diff)) ) * _WaveHeight;

				input.pos.y -= height;

				o.pos = UnityObjectToClipPos(input.pos);
				o.uv0 = input.texcoord0;

				return o;
			}

			float _Opacity;

			half4 frag(v2p o) : COLOR
			{
				float4 mainTexColor = tex2D(_MainTex, o.uv0);
				mainTexColor.a = _Opacity;
				mainTexColor.rgb *= _Opacity;
				return mainTexColor;
			}
			ENDCG
		}
	}
}
