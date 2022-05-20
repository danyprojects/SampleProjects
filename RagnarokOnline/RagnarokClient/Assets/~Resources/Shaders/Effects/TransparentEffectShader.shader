Shader "Custom/Effect Shader Transparent"
{
    Properties
    {
		[PerRendererData] _MainTex ("Texture", 2D) = "white" {}
		[PerRendererData] _VertexColor("Color", Color) = (1,1,1,1)
		[PerRendererData] _Tint("Color", Color) = (1,1,1,1)
		[PerRendererData] _Rotation("Rotation", Float) = 1
		[PerRendererData] _Position("Position", Vector) = (0,0,0,1)
		[Toggle(BILLBOARD)] _Billboard("Billboard", Float) = 0
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

		Lighting Off
		ZWrite Off
		ZTest Always
		Cull Off
		Blend SrcAlpha One, Zero Zero

        Pass
        {
            CGPROGRAM
			#pragma shader_feature BILLBOARD
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
            };

			static const float pi = 3.141592653589793238462;

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float4 _Tint;
			float4 _VertexColor;
			float _Rotation;
			float3 _Position;

            v2f vert (appdata v)
            {
                v2f o;			
				o.uv = v.uv;

				float rad = _Rotation * (pi/180);
				float s = sin(rad);
				float c = cos(rad);
				float4x4 rotMatrix = float4x4(	c, s, 0, 0,
											   -s, c, 0, 0,
												0, 0, 1, 0,
												0, 0, 0, 1);		

				float4x4 translationMatrix = float4x4(1, 0, 0, _Position.x,
													  0, 1, 0, _Position.y,
													  0, 0, 1, _Position.z,
													  0, 0, 0, 1);

				o.vertex = mul(rotMatrix, v.vertex);
				o.vertex = mul(translationMatrix, o.vertex);

				#ifdef BILLBOARD
					float3 vpos = mul((float3x3)unity_ObjectToWorld, o.vertex.xyz);
					float4 worldCoord = float4(unity_ObjectToWorld._m03, unity_ObjectToWorld._m13, unity_ObjectToWorld._m23, 1);
					o.vertex = mul(UNITY_MATRIX_V, worldCoord) + float4(o.vertex.xyz, 0);

					o.vertex = mul(UNITY_MATRIX_P, o.vertex);
				#else
					o.vertex = UnityObjectToClipPos(o.vertex);
				#endif

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv) * _VertexColor;
                return col * _Tint;
            }
            ENDCG
        }
    }
}
