/**
 * A custom skybox that creates a top-down gradient from one color into another.
 *
 * Based off Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)
 * @author Anthony 'Bitzawolf' Pepe
 */

Shader "Bitzawolf/Skybox" {
Properties {
	_SkyTint ("Sky Tint", Color) = (.5, .5, .5, 1)
	_GroundColor ("Ground", Color) = (.369, .349, .341, 1)
	_Offset ("Offset", Range(0, 1)) = 0
}

SubShader {
	Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
	Cull Off ZWrite Off
	
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
			
			float4 _SkyTint;
			float4 _GroundColor;
			float _Offset;

			fixed4 frag (v2f i) : SV_Target
			{
				return lerp(_GroundColor, _SkyTint, i.uv.y + _Offset);
			}
			ENDCG
		}
}


Fallback Off

}