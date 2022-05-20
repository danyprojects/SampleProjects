Shader "Custom/MapShader"
{
    Properties
    {
		 [PerRendererData] _MainTex ("Albedo (RGB)", 2D) = "white" {}
    }

	SubShader
	{
		Tags
		{ 
			"Queue" = "Geometry" 
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
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"	

			struct appdata
			{
				float4 pos : POSITION;
				float4 color : COLOR;
				float2 uv: TEXCOORD0;
				float2 uv2 : TEXCOORD1;
				float3 normal : NORMAL;
			};

			struct v2p
			{
				float4 pos : SV_POSITION;
				float4 color : COLOR;
				float2 uv: TEXCOORD0;
				float2 uv2 : TEXCOORD1;
				float3 normal : NORMAL;
			};

			sampler2D _MainTex;
			sampler2D _ColorMap;

			v2p vert(appdata input)
			{
				v2p o;

				o.pos = UnityObjectToClipPos(input.pos);
				o.color = input.color;
				o.uv = input.uv;
				o.uv2 = input.uv2;
				o.normal = input.normal;

				return o;
			}

			float4 _AmbientLightColor;
			float _AmbientLighIntensity;
			float3 _DiffuseDirection;
			float4 _DiffuseColor;
			float _DiffuseIntensity;

			half4 frag(v2p o) : COLOR
			{
				float4 mainTexColor = tex2D(_MainTex, o.uv);
				float4 lightmap = tex2D(_ColorMap, o.uv2);

				//Pipeline should be tile color -> light color -> OPTIONAL[ shadow -> lightmap] -> OPTIONAL [fog]
				//for now we have no fog and no optional

				//UNITY_LIGHTMODEL_AMBIENT 
				mainTexColor *= o.color; // tile color 

				float4 diffuse = max(dot(_DiffuseDirection, o.normal), 0.1);
				float4 lightColor = (_AmbientLightColor * _AmbientLighIntensity) + (diffuse *_DiffuseColor);

				lightColor *= lightmap.a;

				mainTexColor.rgb *= clamp(lightColor, 0,1); // Apply light color
				mainTexColor.rgb += lightmap.rgb;

				return mainTexColor;
			}
			ENDCG
		}
	}
}
