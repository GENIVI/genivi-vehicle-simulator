/*
* Copyright (C) 2016, Jaguar Land Rover
* This program is licensed under the terms and conditions of the
* Mozilla Public License, version 2.0.  The full text of the
* Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
*/

Shader "Custom/Car HeadLights" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
	
		CGPROGRAM
		#pragma surface surf Lambert

		sampler2D _MainTex;
		fixed4 _Color;
		float _LightsOn;

		struct Input {
			float2 uv_MainTex;
			float2 uv_Illum;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
			fixed4 c = tex * _Color;
			o.Albedo = c.rgb;
			o.Emission = c.rgb * _LightsOn;
			o.Alpha = c.a;
		}
		ENDCG
	} 
	FallBack "Self-Illumin/VertexLit"
}
