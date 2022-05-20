Shader "Custom/Floating Text/Floating Damage Shader"
{
    Properties
    {
		[PerRendererData] _MainTex ("Texture", 2D) = "white" {}
		[PerRendererData] _StartTime("Start Time", Vector) = (0,0,0,0)
		[PerRendererData] _Position("Position", Vector) = (0,0,0,1)
		_Tint("Tint", Color) = (0,0,0,0)
		_XSpeed("X Speed", Float) = 0.1
		_YSpeed("Y Speed", Float) = 0.2
		_Narrow("Narrow", Float) = 3
		_FadeSpeed("Fade Speed", Float) = 0.5
		_FixedFade("Fixed fade", Float) = 0.15
		_FadeDelay("Fade delay", Float) = 0.5
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
			float _XSpeed;
			float _YSpeed;
			float _Narrow;
			float _FadeSpeed;
			float _FixedFade;
			float _StartScale;
			float4 _Tint;
			float _FadeDelay;

            v2f vert (appdata v)
            {
				float elapsedTime = _Time.y - _StartTime[0];

				const float SCALE_DURATION = 0.500;

				//Calculate scale factor. Clamp scale factor to make sure it does not get smaller than MIN_SIZE		
				const float MIN_SIZE = 3, MAX_SIZE = 5; 
				float scaleFactor = clamp(-(((MAX_SIZE - MIN_SIZE) / SCALE_DURATION) * elapsedTime) + MIN_SIZE, MIN_SIZE, MAX_SIZE);
				float4x4 scaleMatrix = float4x4(scaleFactor, 0, 0, 0,
											0, scaleFactor, 0, 0,
											0, 0, 1, 0,
											0, 0, 0, 1);

				float4x4 translationMatrix = float4x4(1, 0, 0, _Position.x * scaleFactor,
					0, 1, 0, _Position.y * scaleFactor,
					0, 0, 1, _Position.z * scaleFactor,
					0, 0, 0, 1);

                v2f o;		
				
				//scale and translate
				o.vertex = mul(scaleMatrix, v.vertex);				
				o.vertex = mul(translationMatrix, o.vertex);

				//Create the parabola translation
				o.vertex.x -= elapsedTime * _XSpeed; //x moves linearly
				const float SHIFT_RIGHT = 2;
				o.vertex.y -= (_Narrow * (elapsedTime*elapsedTime) - SHIFT_RIGHT * elapsedTime) * _YSpeed;

				o.vertex = UnityObjectToClipPos(o.vertex);

				o.uv = v.uv;
				o.color = v.color * _Tint;

				//fade alpha
				o.color.a = clamp(pow(_FadeSpeed, elapsedTime - _FadeDelay) - _FixedFade, 0, 1);

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
