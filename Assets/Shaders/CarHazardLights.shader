/*
* Copyright (C) 2016, Jaguar Land Rover
* This program is licensed under the terms and conditions of the
* Mozilla Public License, version 2.0.  The full text of the
* Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
*/

Shader "Custom/Car Hazard Lights" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
		_Shininess ("Shininess", Range (0.01, 1)) = 0.078125
		_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
		_BlinkRate ("blink rate (Hz)", Range(0.01, 3)) = 1.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 300
	
		CGPROGRAM
		#pragma surface surf BlinnPhong

		sampler2D _MainTex;
		fixed4 _Color;
		half _Shininess;
		half _BlinkRate;
		
		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
			fixed4 c = tex * _Color;
			o.Albedo = c.rgb;

			o.Emission = c.rgb * step(fmod(_Time.y * _BlinkRate, 1.0f), 0.5); 
			o.Gloss = tex.a;
			o.Alpha = c.a;
			o.Specular = _Shininess;
		}
		ENDCG
	}
	FallBack "Self-Illumin/Diffuse"
}