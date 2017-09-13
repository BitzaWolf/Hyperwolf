Shader "Bitzawolf/Tree"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}
		_Tint ("Tint", Color) = (1, 1, 1, 1)
		_TreeRamp ("Tree Ramp", 2D) = "white" {}
		_Amount ("Border Size", Range(0, 1)) = 0.5
		_BorderColor ("Border Color", Color) = (0, 0, 0, 1)
	}

	// Normal surface lighting
	SubShader
	{

		// Create border around the object
		Pass
		{
			Name "TreeBorder"
			Tags { "RenderType" = "Transparent" }
			LOD 200
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
			ZTest Always
			ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
 
			struct v2f {
				float4 pos	: POSITION;
			};

			float _Amount;
			fixed4 _BorderColor;
 
			v2f vert (appdata_full v)
			{
				v2f o;
				float3 vert = v.vertex + v.normal * _Amount;
				o.pos = UnityObjectToClipPos(vert);   
				return o;
			}
 
			half4 frag(v2f i) : COLOR
			{
				return _BorderColor;
			}
			ENDCG   
		}	
		
		CGPROGRAM
		#pragma surface surf Custom fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _TreeRamp;
		float4 _Tint;

		struct Input
		{
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o)
		{
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb * _Tint;
			o.Alpha = 1;
		}

		half4 LightingCustom (SurfaceOutput s, half3 lightDir, half atten)
		{
			half NdotL = dot(s.Normal, lightDir);
			half val = (NdotL + 1) / 2;
			half4 ramp = tex2D(_TreeRamp, float2(0.5, val));
			half4 c;
			c.rgb = s.Albedo * _LightColor0.rgb * ramp;
			c.a = s.Alpha;
			return c;
		}

		ENDCG
	}

	FallBack "Diffuse"
}
