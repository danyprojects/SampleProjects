Shader "Custom/Floating Text/Test Shader"
{
    Properties
    {
		[PerRendererData] _MainTex ("Texture", 2D) = "white" {}
		_Tint("Tint", Color) = (0,0,0,0)
        _Offset("Offset", Float) = 0
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
            float _Offset;

            v2f vert (appdata v)
            {
				//Calculate scale factor. Clamp scale factor to make sure it does not get smaller than MIN_SIZE		
				const float SCALE_FACTOR = 2.1;
				float4x4 scaleMatrix = float4x4(SCALE_FACTOR, 0, 0, 0,
					0, SCALE_FACTOR, 0, 0,
					0, 0, 1, 0,
					0, 0, 0, 1);

				v2f o;

				//Get world position from object position and scale x in world coordinates
				//This will move and "translate" it to left / right, spacing out the numbers
				//o.vertex = mul(unity_ObjectToWorld, v.vertex);
				o.vertex = mul(v.vertex, scaleMatrix);

                o.vertex.x += _Offset;

				//make object into clip position
                float3 vpos = mul((float3x3)unity_ObjectToWorld, o.vertex.xyz);
                float4 worldCoord = float4(unity_ObjectToWorld._m03, unity_ObjectToWorld._m13, unity_ObjectToWorld._m23, 1);
                float4 viewPos = mul(UNITY_MATRIX_V, worldCoord) + float4(vpos, 0);
                o.vertex = mul(UNITY_MATRIX_P, viewPos);

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
