Shader "Custom/Floating Text/Stacking Damage Shader"
{
    Properties
    {
		[PerRendererData] _MainTex ("Texture", 2D) = "white" {}
		[PerRendererData] _StartTime("Start Time", Vector) = (0,0,0,0)		
		[PerRendererData] _Position("Position", Vector) = (0,0,0,1)
		_YSpeed("Y Speed", Float) = 0.2
		_ScaleSpeed("Scale Speed", Float) = 1.3
		_FadeSpeed("Fade Speed", Float) = 0.5
    }
    SubShader
    {
		Tags
		{
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
			"IgnoreProjector" = "True"
			"DisableBatching" = "True"
		}

        // No culling or depth
        Cull Off 
		ZWrite Off
		ZTest Always
		Blend One OneMinusSrcAlpha

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
				float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
				float4 color : COLOR;
            };

			float4 _StartTime;
			float4 _Position;
			float _YSpeed;
			float _ScaleSpeed;
			float _FadeSpeed;

            v2f vert (appdata v)
            {
				float elapsedTime = _Time.y - _StartTime[0];

				//Calculate scale factor. Clamp scale factor to make sure it does not get smaller than MIN_SIZE		
				const float MIN_SIZE = 1.5, MAX_SIZE = 3;			
				float scaleFactor = clamp(MIN_SIZE + elapsedTime * _ScaleSpeed, MIN_SIZE, MAX_SIZE);
				float4x4 scaleMatrix = float4x4(scaleFactor, 0, 0, 0,
											0, scaleFactor, 0, 0,
											0, 0, 1, 0,
											0, 0, 0, 1);  	

				float4x4 translationMatrix = float4x4(1, 0, 0, _Position.x * scaleFactor,
					0, 1, 0, _Position.y * scaleFactor,
					0, 0, 1, _Position.z * scaleFactor,
					0, 0, 0, 1);

				v2f o;

				//Scale and translate
				o.vertex = mul(scaleMatrix, v.vertex);
				o.vertex = mul(translationMatrix, o.vertex);				
				o.vertex.y += elapsedTime * _YSpeed; //Move up linerarly after billboard

				o.vertex = UnityObjectToClipPos(o.vertex);

				o.uv = v.uv;
				o.color = v.color * float4(1, 0.92, 0.016, 1); //yellow

				//fade alpha alpha time is up
				elapsedTime = clamp(_Time.y - _StartTime[1], 0, _Time.y);
				o.color.a = clamp(o.color.a - elapsedTime * _FadeSpeed, 0, 1);

                return o;
            }

            sampler2D _MainTex;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color ;    	
				col.rgb *= col.a;
                return col;
            }
            ENDCG
        }
    }
}
