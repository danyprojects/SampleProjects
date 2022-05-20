Shader "Custom/Floating Text/Act Text Shader"
{
    Properties
    {
		[PerRendererData] _MainTex ("Texture", 2D) = "white" {}
		[PerRendererData] _Scale("Scale", Vector) = (0,0,0,1)
		_Tint("Tint", Color) = (0,0,0,0)
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

			float4 _Tint;
			float4 _Scale;

            v2f vert (appdata v)
            {
				//Calculate scale factor. Clamp scale factor to make sure it does not get smaller than MIN_SIZE	
				float4x4 scaleMatrix = float4x4(_Scale.x, 0, 0, 0,
					0, _Scale.y, 0, 0,
					0, 0, 1, 0,
					0, 0, 0, 1);

				v2f o;

				//Scale vertices
				o.vertex = mul(scaleMatrix, v.vertex);

				o.vertex = UnityObjectToClipPos(o.vertex);

				o.uv = v.uv;
				o.color = v.color * _Tint; 

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
