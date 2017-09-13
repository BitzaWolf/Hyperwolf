/**
 * Water shader creates a perfectly reflective surface and allows a moving
 * bumpmap to simulate animated water.
 */

Shader "Bitzawolf/Water"
{
	Properties
	{
		_Color("Color", Color) = (1, 1, 1, 1)
		_SpecularRamp("Specular Ramp", 2D) = "white" {}
		_BumpMap("Bump Map", 2D) = "bump" {}
		//_Cube ("Reflection Cubemap", Cube) = "_Skybox" {}
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" }

		CGPROGRAM
		#pragma surface surf Lambert

		struct Input
		{
			float2 uv_MainTex;
			//float3 worldRefl;
		};

		/*
		struct SurfaceOutput
		{
			fixed3 Albedo;      // base (diffuse or specular) color
			fixed3 Normal;      // tangent space normal, if written
			half3 Emission;
			half Metallic;      // 0=non-metal, 1=metal
			half Smoothness;    // 0=rough, 1=smooth
			half Occlusion;     // occlusion (default 1)
			fixed Alpha;        // alpha for transparencies
		};*/

		sampler2D _SpecularRamp;
		sampler2D _BumpMap;
		samplerCUBE _Cube;
		fixed4 _Color;

		void surf(Input IN, inout SurfaceOutput o)
		{
			//fixed4 reflection = texCUBE(_Cube, IN.worldRefl);
			o.Albedo = _Color.rgb; //(_Color * reflection).rgb;
			o.Normal = tex2D(_BumpMap, IN.uv_MainTex);
		}

		ENDCG
	}
}
