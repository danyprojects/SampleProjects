Shader "Custom/Floating Text/Miss Text Shader"
{
    Properties
    {
		[PerRendererData] _MainTex ("Texture", 2D) = "white" {}
		[PerRendererData] _StartTime("Start Time", Vector) = (0,0,0,0)
		_Tint("Tint", Color) = (0,0,0,0)
		_YSpeed("Y Speed", Float) = 0.2
		_FadeDelay("Fade Delay", Float) = 1
		_FixedFade("Fixed Fade", Float) = 0.25
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
			float4 _Tint;
			float _YSpeed;
			float _FadeDelay;
			float _FixedFade;
			float _FadeSpeed;

            v2f vert (appdata v)
            {
				float elapsedTime = _Time.y - _StartTime[0];
	
                v2f o;		

				//scale and translate
				o.vertex =  v.vertex;
				o.vertex.y += elapsedTime * _YSpeed; //Move up linerarly after billboard
							
				//make it into clip position
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
