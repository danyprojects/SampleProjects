Shader "Custom/Cast Lock On Shader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [PerRendererData] _StartTime("Start Time", Vector) = (0,0,0,0)

        _Speed("Speed", float ) = 1
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

            static const float pi = 3.141592653589793238462;

            float3 _CameraRotation;
			float _Speed;
			float _Limit;
			float _Variation;
            float4 _StartTime;

            v2f vert (appdata v)
            {
                v2f o;
                o.uv = v.uv;
                 
                //Get the rotation matrixes
                float angleX = radians(_CameraRotation - 90);
                float c = cos(angleX);
                float s = sin(angleX);

                float4x4 rotateXMatrix = float4x4(1, 0, 0, 0,
                    0, c, -s, 0,
                    0, s, c, 0,
                    0, 0, 0, 1);

                float angleZ = radians(_Time.y * _Speed);
                c = cos(angleZ);
                s = sin(angleZ);

                float4x4 rotateZMatrix = float4x4(c, -s, 0, 0,
                    s, c, 0, 0,
                    0, 0, 1, 0,
                    0, 0, 0, 1);


                //Apply scale
                float elapsedTime = _Time.y - _StartTime.x;
                v.vertex *= clamp(2 - elapsedTime * 3, 1, 2);

                v.vertex.z -= 0.01;

                //Apply Z rotation to rotate image
                v.vertex = mul(v.vertex, rotateZMatrix);

                //Remove camera rotation
                v.vertex = mul(v.vertex, rotateXMatrix);

                o.vertex = UnityObjectToClipPos(v.vertex);

				o.color = float4(1, (sin(_Time.a + pi) + 1) / 2, (sin(_Time.a + pi) + 1) / 2, 1);

                return o;
            }

            sampler2D _MainTex;

            fixed4 frag (v2f i) : SV_Target
            {
                return tex2D(_MainTex, i.uv) * i.color;
            }
            ENDCG
        }
        
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

            static const float pi = 3.141592653589793238462;

            float3 _CameraRotation;
            float _Speed;
            float _Limit;
            float _Variation;
            float4 _StartTime;

            v2f vert(appdata v)
            {
                v2f o;
                o.uv = v.uv;

                //Get the rotation matrixes
                float angleX = radians(_CameraRotation - 90);
                float c = cos(angleX);
                float s = sin(angleX);

                float4x4 rotateXMatrix = float4x4(1, 0, 0, 0,
                    0, c, -s, 0,
                    0, s, c, 0,
                    0, 0, 0, 1);

                float angleZ = radians(_Time.y * _Speed);
                c = cos(angleZ + 180);
                s = sin(angleZ + 180);

                float4x4 rotateZMatrix = float4x4(c, -s, 0, 0,
                    s, c, 0, 0,
                    0, 0, 1, 0,
                    0, 0, 0, 1);


                //Apply scale
                float elapsedTime = _Time.y - _StartTime.x;
                v.vertex *= clamp(2 - elapsedTime * 3, 1, 2);

                v.vertex.z -= 0.01;

                //Apply Z rotation to rotate image
                v.vertex = mul(v.vertex, rotateZMatrix);

                //Remove camera rotation
                v.vertex = mul(v.vertex, rotateXMatrix);

                o.vertex = UnityObjectToClipPos(v.vertex);

                o.color = float4(1, (sin(_Time.a + pi) + 1) / 2, (sin(_Time.a + pi) + 1) / 2, 1);

                return o;
            }

            sampler2D _MainTex;
            fixed4 _Tint;

            fixed4 frag(v2f i) : SV_Target
            {
                return tex2D(_MainTex, i.uv) * i.color;
            }
            ENDCG
        }
    }
}
