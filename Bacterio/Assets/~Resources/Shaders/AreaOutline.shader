Shader "Bacterio/AreaOutline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineThickness("Outline Thickness", float) = 0
        _OutlineColor("Outline Color", Color) = (0,0,0,0)
        _Tint("Tint", Color) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }

        Cull Off
        ZWrite Off
        ZTest LEqual
        Blend SrcAlpha OneMinusSrcAlpha

        //Pass 1
        Pass
        {
            Stencil
            {
                Ref 1
                Comp NotEqual
                Pass replace
            }

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
            sampler2D _MainTex;
            float _OutlineThickness;

            v2f vert (appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.uv = v.uv;
                return o;
            }


            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color * _Tint;
                return col;
            }
            ENDCG
        }

        //Outline pass
        Pass
        {
            Stencil
            {
                Ref 0
                Comp equal
                Pass invert
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            float _OutlineThickness;

            v2f vert(appdata v)
            {
                v2f o;

                v.vertex *= _OutlineThickness;
                o.vertex = UnityObjectToClipPos(v.vertex);

                return o;
            }

            sampler2D _MainTex;
            float4 _OutlineColor;

            fixed4 frag(v2f i) : SV_Target
            {
                return _OutlineColor;
            }
            ENDCG
        }
    }
}
