Shader "Custom/Map Object Shader"
{
	Properties
	{
		 [PerRendererData]  _MainTex("Albedo (RGB)", 2D) = "white" {}
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

		CGPROGRAM
		#pragma surface surf Lambert
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input
		{
			float2 uv_MainTex: TEXCOORD0;
		};

		float4 _AmbientLightColor;
		float _AmbientLighIntensity;
		float3 _DiffuseDirection;
		float4 _DiffuseColor;
		float _DiffuseIntensity;

		void surf(Input IN, inout SurfaceOutput o)
		{
			float4 mainTexColor = tex2D(_MainTex, IN.uv_MainTex);

			if (mainTexColor.a <= 0.4)
				discard;

			float4 normals = float4(1, 1, 1, 1); // todo, proper calculation of normals
			float4 diffuse = saturate(dot(_DiffuseDirection, normals));
			float4 lightColor = (_AmbientLightColor * _AmbientLighIntensity) + (diffuse *_DiffuseColor);

			mainTexColor.rgb *= clamp(lightColor, 0, 1); // Apply light color

			o.Albedo = mainTexColor.rgb;
			o.Alpha = mainTexColor.a;
		}
		ENDCG
		
	}
}
