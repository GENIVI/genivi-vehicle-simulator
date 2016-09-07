/*
* Copyright (C) 2016, Jaguar Land Rover
* This program is licensed under the terms and conditions of the
* Mozilla Public License, version 2.0.  The full text of the
* Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
*/

Shader "Custom/BuildingWindowLights" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_LightsTex ("Lights texture", 2D) = "black" {}
		_LightColor("Light Color", Color) = (1,1,1,1)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert

		sampler2D _MainTex;
		sampler2D _LightsTex;
		half4 _LightColor;
		float _LightsOn;

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
			half4 cLight = tex2D(_LightsTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
			o.Emission = cLight.rgb * _LightColor.rgb * _LightsOn;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
