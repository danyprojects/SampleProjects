Shader "Custom/Fill Bar Shader"
{
    Properties
    {
		[PerRendererData] _StartTime("Start Time", Vector) = (0,2,0,0)
		[PerRendererData] _ProgressColor("Progress color", Color) = (0,1,0,1)

		_OutlineColor("Outline color", Color) = (0,0,0,1)
		_OutlineWidth("Outlines width", Range(0.0, 2.0)) = 1.1
		_BackgroundColor("Background color", Color) = (0,0,0,1)
		_Scale("Scale", Vector) = (0,0,0,0)

    }
    SubShader
    {
		Tags
		{
			"Queue" = "Overlay"
			"RenderType" = "TransparentCutout"
			"IgnoreProjector" = "True"
			"DisableBatching" = "True"
		}

        // No culling or depth
        Cull Off 
		ZWrite Off
		ZTest Always
		Blend One Zero, Zero Zero

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
                float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 outline : TEXCOORD1;
				float2 progress : TEXCOORD2;
            };

			float _OutlineWidth;
			float4 _Scale;
			float4 _StartTime;

			v2f vert(appdata v)
			{
				float outlineX = v.vertex.x * _OutlineWidth;
				float outlineY = v.vertex.y * _OutlineWidth;

				v.vertex.x *= _Scale.x;
				v.vertex.y *= _Scale.y;
				v.vertex.xy += float2(outlineX, outlineY);

				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.outline = float2(_OutlineWidth / _Scale.x, _OutlineWidth / _Scale.y);


				float elapsed = clamp(_Time.y - _StartTime.x, 0, _StartTime.y); //startTime.y is duration
				o.progress = float2(lerp(0, 1, elapsed / _StartTime.y), 0);

                return o;
            }

			float4 _BackgroundColor;
			float4 _ProgressColor;
			float4 _OutlineColor;

			fixed4 frag(v2f i) : SV_Target
			{
				float hasOutline = 1 - ceil(i.uv.x - i.outline.x) + ceil(i.uv.x - 1 + i.outline.x);
				hasOutline += 1 - ceil(i.uv.y - i.outline.y) + ceil(i.uv.y - 1 + i.outline.y);
				hasOutline = clamp(hasOutline, 0, 1.0);
				float4 col = hasOutline * _OutlineColor;

				float hasProgress = ceil(i.progress.x - i.uv.x) * (1 - hasOutline);
				col += _ProgressColor * hasProgress;

				float hasBackground = ceil(i.uv.x - i.progress.x) * (1 - hasOutline);
				col += _BackgroundColor * hasBackground;
				return col;
            }
            ENDCG
        }
    }
}
