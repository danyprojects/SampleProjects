Shader "Custom/Cast Circle Shader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Limit("Limit", float ) = 1
		_Variation("Variation", float) = 1
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

        // No culling or depth
        Cull Off 
		ZWrite Off
		ZTest LEqual
		Blend SrcAlpha OneMinusSrcColor, Zero Zero
		//Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
				float4 color : COLOR;
            };

			float _Speed;
			float _Limit;
			float _Variation;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
              
				float sinX = sin(_Time.a);
				float cosX = cos(_Time.a);
				float sinY = sin(_Time.a);
				float2x2 rotationMatrix = float2x2(cosX, -sinX, sinY, cosX);

				v.uv.xy -= float2(0.5, 0.5);
				o.uv.xy = mul(v.uv.xy, rotationMatrix);
				o.uv.xy += float2(0.5, 0.5);

				o.color = float4(0, 0, 0, (_Limit + _Variation * _SinTime.a) /255 );

                return o;
            }

            sampler2D _MainTex;
			fixed4 _Tint;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);    
				col.a = i.color.a;
				col.rgb *= i.color.a;
                return col;
            }
            ENDCG
        }
    }
}
