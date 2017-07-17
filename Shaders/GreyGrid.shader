Shader "Bitzawolf/GreyGrid" {
	Properties {
		_MainTex ("Texture", 2D) = "Black" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }

		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows
		#pragma vertex vert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct AppData {
			float4 vertex : POSITION;
			float3 normal : NORMAL;
		};

		struct Input {
			float3 position;
            float3 normal;
		};

		Input vert(inout AppData app)
		{
			Input o;
			o.position = app.vertex;
			o.normal = app.normal;
			return o;
		}

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_CBUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_CBUFFER_END

		void surf (Input IN, inout SurfaceOutputStandard o) {
			float3 blending = abs(IN.normal);
			blending = normalize(max(blending, 0.00001));
			float b = (blending.x + blending.y + blending.z);
			blending /= float3(b, b, b);

			float4 xaxis = tex2D(_MainTex, IN.position.yz);
			float4 yaxis = tex2D(_MainTex, IN.position.xz);
			float4 zaxis = tex2D(_MainTex, IN.position.xy);
			float4 c = xaxis * blending.x + yaxis * blending.y + zaxis * blending.z;

			// get world xyz position of pixel.
			// if pos divisible by 5, then color it thick black
			// if pos divisible by 1
			
			o.Albedo = c.rgb;
			o.Metallic = 0;
			o.Smoothness = 0;
			o.Alpha = 1;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
