Shader "Bitzawolf/AntiAliasing"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_BlurDist ("Blur Distance", Range(0, 1)) = 0.2
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

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
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			float _BlurDist;

			fixed4 frag (v2f i) : SV_Target
			{
				float step = _BlurDist / 100;
				float4 up = float4(0, step, 0, 0);
				float4 right = float4(step, 0, 0, 0);
				fixed4 col = tex2D(_MainTex, i.uv + up - right) * 0.0625 +
						tex2D(_MainTex, i.uv + up) * 0.125 +
						tex2D(_MainTex, i.uv + up + right) * 0.0625 +
						tex2D(_MainTex, i.uv - right) * 0.125 +
						tex2D(_MainTex, i.uv) * 0.25 +
						tex2D(_MainTex, i.uv + right) * 0.125 +
						tex2D(_MainTex, i.uv - up - right) * 0.0625 +
						tex2D(_MainTex, i.uv - up) * 0.125 +
						tex2D(_MainTex, i.uv - up + right) * 0.0625;
				return col;
			}
			ENDCG
		}
	}
}
