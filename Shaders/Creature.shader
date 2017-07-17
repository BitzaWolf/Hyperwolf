/**
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
		_Color ("Main Color", Color) = (.5,.5,.5,0)
		_BorderColor ("Border Color", Color) = (0.3, 0.3, 0.3, 1)
		_BorderSize ("Border Size", Range(0, 1)) = 0
		_MainTex ("Texture", 2D) = "Black" {}
	}

	CGINCLUDE
	#include "UnityCG.cginc"

	struct appdata
	{
		float4 vertex : POSITION;
		float4 normal : NORMAL;
	};

	struct v2f
	{
		float4 pos : POSITION;
		float4 color : COLOR;
	};

	uniform float _BorderSize;
	uniform fixed4 _BorderColor;

	v2f vert(appdata v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);

		float3 norm = mul((float3x3)UNITY_MATRIX_IT_MV, v.normal);
		float2 offset = TransformViewToProjection(norm.xy);

		o.pos.xy += offset * o.pos.z * _BorderSize;
		o.color = _BorderColor;
		return o;
	}
	ENDCG

	SubShader
	{
		Tags { "Queue"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			Name "BASE"
			ZWrite On
			ZTest LEqual

			Material {
				Diffuse [_Color]
				Ambient [_Color]
			}
			Lighting Off
			SetTexture [_MainTex] {
				ConstantColor [_Color]
				Combine texture * constant
			}
		}
	}

	Fallback "Diffuse"
}
