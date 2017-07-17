Shader "Bitzawolf/Border"
{
	Properties
	{
		_FillColor ("Fill Color", Color) = (0.8, 0.282, 0.51, 1)
		_BorderColor ("Border Color", Color) = (0.3, 0.3, 0.3, 1)
		_BorderSize ("Border Size", Range(0, 1)) = 0.5
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" }
		LOD 100

		Pass
		{
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

			float4 _BorderColor;
			float _BorderSize;
			const float BORDER_ADDER = 1;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex) * (_BorderSize + BORDER_ADDER);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return _BorderColor;
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
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			fixed4 _FillColor;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return _FillColor;
			}
			ENDCG
		}
	}
}
