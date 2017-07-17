Shader "Bitzawolf/UIPanel"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Skew ("Skew amount (Degrees)", Range(-1.49, 1.49)) = 30
		_Border ("Border Size", Float) = 1
		_BorderColor ("Border Color", Color) = (0, 0, 0, 1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
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

			float _Border;
			float4 _BorderColor;
			float _Skew;
			
			v2f vert (appdata v)
			{
				v2f o;
				float4x4 transform = float4x4(
						1, tan(_Skew), 0, 0,
						0, 1, 0, 0,
						0, 0, 1, 0,
						0, 0, 0, 1);
				float4 transformed = mul(transform, v.vertex);
				o.vertex = UnityObjectToClipPos(transformed * _Border);
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
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
			};

			sampler2D _MainTex;
			float _Skew;
			
			v2f vert (appdata v)
			{
				v2f o;
				float4x4 transform = float4x4(
						1, tan(_Skew), 0, 0,
						0, 1, 0, 0,
						0, 0, 1, 0,
						0, 0, 0, 1);
				float4 transformed = mul(transform, v.vertex);
				o.vertex = UnityObjectToClipPos(transformed);
				o.uv = v.uv;
				o.color = v.color;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return i.color;
			}
			ENDCG
		}
	}
}
