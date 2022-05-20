// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "VertexColor" 
{
    Properties 
	{
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
	}
		SubShader
		{
			Tags{ "Queue" = "Transparent" "IgnoreProjector" = "true" "ForceNoShadowCasting" = "true"}

			Blend[_SrcBlend][_DstBlend]
			ZWrite[_ZWrite]
			Cull Off
			Blend SrcAlpha OneMinusSrcAlpha

			Pass
			{
				CGPROGRAM
				#pragma target 3.0
				#pragma vertex vert             
				#pragma fragment frag 

				#include "UnityCG.cginc"
				#include "AutoLight.cginc"
				#include "Lighting.cginc"

				sampler2D _MainTex;
				float4 _MainTex_ST;

			struct vertInput 
			{
				float4 pos : POSITION;
				float4 color : COLOR;
				float2 texcoord0 : TEXCOORD0;
			};

			struct vertOutput 
			{
				float4 pos : SV_POSITION;
				float4 color : COLOR;
				float2 uv0 : TEXCOORD0;
			};

			vertOutput vert(vertInput input) 
			{
				vertOutput o;
				o.pos = UnityObjectToClipPos(input.pos);
				o.color = input.color;
				o.uv0 = input.texcoord0;
				return o;
			}

			half4 frag(vertOutput output) : COLOR
			{
				float3 mainTexColor = tex2D(_MainTex,TRANSFORM_TEX(output.uv0, _MainTex));
				float3 result = lerp(mainTexColor, output.color.rgb, output.color.a);
				return half4(result, 1);
			}
			ENDCG
		}
	}
}    