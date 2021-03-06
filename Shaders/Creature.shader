﻿/**
 * This shader is used for the player wolf and other creatures in the game.
 * This shader uses a flat, shadeless approach to meshes, but allows them to cast
 * shadows. This gives creatures a funny 2D cutout look, despite being 3D objects
 * and casting 3D shadows.
 * 
 * TODO add a border around creatures.
 * Based on http://wiki.unity3d.com/index.php/Silhouette-Outlined_Diffuse
 * @author Anthony 'Bitzawolf' Pepe
 */

Shader "Bitzawolf/Creature"
{
	Properties
	{
		_BorderColor ("Border Color", Color) = (0.3, 0.3, 0.3, 1)
		_BorderSize ("Border Size", Range(0, 1)) = 0
		_MainTex ("Texture", 2D) = "Black" {}
		_LightRamp ("Light Ramp", 2D) = "White" {}
	}

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

			float _BorderSize;
			fixed4 _BorderColor;
 
			v2f vert (appdata_full v)
			{
				v2f o;
				float3 vert = v.vertex + v.normal * _BorderSize;
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
		sampler2D _LightRamp;

		struct Input
		{
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o)
		{
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Alpha = 1;
		}

		half4 LightingCustom (SurfaceOutput s, half3 lightDir, half atten)
		{
			half NdotL = dot(s.Normal, lightDir);
			half val = (NdotL + 1) / 2;
			half4 ramp = tex2D(_LightRamp, float2(0.5, val));
			half4 c;
			c.rgb = s.Albedo * _LightColor0.rgb * ramp;
			c.a = s.Alpha;
			return c;
		}

		ENDCG
	}

	Fallback "Diffuse"
}
